using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Simple Twitch PubSub client using HTTP polling for channel points.
/// Since Unity IL2CPP doesn't support WebSockets reliably, this uses
/// the Twitch Helix API to poll for channel point redemptions.
/// </summary>
public class SimpleTwitchPubSubClient : MonoBehaviour
{
    private const string HELIX_BASE_URL = "https://api.twitch.tv/helix";
    private const float POLL_INTERVAL = 2.0f; // Poll every 2 seconds

    private string _clientId;
    private string _accessToken;
    private string _broadcasterId;
    private bool _isPolling;
    private Coroutine _pollCoroutine;

    // Events
    public event Action<TwitchChannelPointsEvent> OnChannelPointsRedeemed;

    /// <summary>
    /// Initialize the PubSub client with Twitch credentials
    /// </summary>
    public void Initialize(string clientId, string accessToken, string broadcasterId)
    {
        _clientId = clientId;
        _accessToken = accessToken;
        _broadcasterId = broadcasterId;

        LogAspera.Info($"SimpleTwitchPubSubClient initialized for broadcaster {_broadcasterId}");
    }

    /// <summary>
    /// Start polling for channel point redemptions
    /// </summary>
    public void StartPolling()
    {
        if (_isPolling)
        {
            LogAspera.Warning("PubSub polling already started");
            return;
        }

        _isPolling = true;
        _pollCoroutine = StartCoroutine(PollChannelPoints());
        LogAspera.Info("Started PubSub polling for channel points");
    }

    /// <summary>
    /// Stop polling for channel point redemptions
    /// </summary>
    public void StopPolling()
    {
        if (!_isPolling)
            return;

        _isPolling = false;
        if (_pollCoroutine != null)
        {
            StopCoroutine(_pollCoroutine);
            _pollCoroutine = null;
        }
        LogAspera.Info("Stopped PubSub polling");
    }

    private IEnumerator PollChannelPoints()
    {
        while (_isPolling)
        {
            yield return FetchChannelPointRedemptions();
            yield return new WaitForSeconds(POLL_INTERVAL);
        }
    }

    private IEnumerator FetchChannelPointRedemptions()
    {
        string url = $"{HELIX_BASE_URL}/channel_points/custom_rewards/redemptions";
        var parameters = new Dictionary<string, string>
        {
            ["broadcaster_id"] = _broadcasterId,
            ["status"] = "UNFULFILLED", // Only get unfulfilled redemptions
            ["first"] = "20" // Get up to 20 recent redemptions
        };

        url += "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={UnityWebRequest.EscapeURL(kvp.Value)}"));

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Client-ID", _clientId);
            request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessRedemptionsResponse(request.downloadHandler.text);
            }
            else
            {
                LogAspera.Error($"Failed to fetch channel point redemptions: {request.error}");
            }
        }
    }

    private void ProcessRedemptionsResponse(string jsonResponse)
    {
        try
        {
            // Parse the JSON response
            var response = JsonUtility.FromJson<ChannelPointsResponse>(jsonResponse);

            if (response?.data != null)
            {
                foreach (var redemption in response.data)
                {
                    // Create channel points event
                    var channelPointsEvent = new TwitchChannelPointsEvent
                    {
                        RedemptionId = redemption.id,
                        UserId = redemption.user_id,
                        UserName = redemption.user_login,
                        UserDisplayName = redemption.user_name,
                        RewardId = redemption.reward.id,
                        RewardTitle = redemption.reward.title,
                        RewardCost = redemption.reward.cost,
                        RewardPrompt = redemption.reward.prompt,
                        UserInput = redemption.user_input,
                        RedeemedAt = DateTime.Parse(redemption.redeemed_at)
                    };

                    // Trigger event
                    OnChannelPointsRedeemed?.Invoke(channelPointsEvent);

                    // Mark as fulfilled (optional - you might want to handle this differently)
                    StartCoroutine(FulfillRedemption(redemption.id));
                }
            }
        }
        catch (Exception ex)
        {
            LogAspera.Error($"Failed to process channel points response: {ex.Message}");
        }
    }

    private IEnumerator FulfillRedemption(string redemptionId)
    {
        string url = $"{HELIX_BASE_URL}/channel_points/custom_rewards/redemptions";
        var parameters = new Dictionary<string, string>
        {
            ["id"] = redemptionId,
            ["broadcaster_id"] = _broadcasterId
        };

        url += "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={UnityWebRequest.EscapeURL(kvp.Value)}"));

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.SetRequestHeader("Client-ID", _clientId);
            request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            string jsonBody = "{\"status\": \"FULFILLED\"}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                LogAspera.Warning($"Failed to fulfill redemption {redemptionId}: {request.error}");
            }
        }
    }

    private void OnDestroy()
    {
        StopPolling();
    }

    // JSON response classes
    [Serializable]
    private class ChannelPointsResponse
    {
        public ChannelPointRedemption[] data;
    }

    [Serializable]
    private class ChannelPointRedemption
    {
        public string id;
        public string user_id;
        public string user_login;
        public string user_name;
        public string user_input;
        public ChannelPointReward reward;
        public string redeemed_at;
    }

    [Serializable]
    private class ChannelPointReward
    {
        public string id;
        public string title;
        public string prompt;
        public int cost;
    }
}