using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;
using PerAspera.Core;

namespace PerAspera.GameAPI.TwitchIntegration
{
    /// <summary>
    /// Simple Twitch PubSub client using HTTP polling for channel points.
    /// Since Unity IL2CPP doesn't support WebSockets reliably, this uses
    /// the Twitch Helix API to poll for channel point redemptions.
    /// </summary>
    public class SimpleTwitchPubSubClient : MonoBehaviour
{
    private const string HELIX_BASE_URL = "https://api.twitch.tv/helix";
    private const float POLL_INTERVAL = 2.0f; // Poll every 2 seconds

    private readonly LogAspera _log = new LogAspera("TwitchPubSub");

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

        _log.Info($"SimpleTwitchPubSubClient initialized for broadcaster {_broadcasterId}");
    }

    /// <summary>
    /// Start polling for channel point redemptions
    /// </summary>
    public void StartPolling()
    {
        if (_isPolling)
        {
            _log.Warning("PubSub polling already started");
            return;
        }

        _isPolling = true;
        _pollCoroutine = StartCoroutine(PollChannelPoints());
        _log.Info("Started PubSub polling for channel points");
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
        _log.Info("Stopped PubSub polling");
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

        url += "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        using (UnityWebRequest request = new UnityWebRequest(url, "GET", new DownloadHandlerBuffer(), null))
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
                _log.Error($"Failed to fetch channel point redemptions: {request.error}");
            }
        }
    }

    private void ProcessRedemptionsResponse(string jsonResponse)
    {
        try
        {
            // Parse the JSON response
            var response = JsonSerializer.Deserialize<ChannelPointsResponse>(jsonResponse);

            if (response?.data != null)
            {
                foreach (var redemption in response.data)
                {
                    // Create channel points event
                    var channelPointsEvent = new TwitchChannelPointsEvent
                    {
                        RewardId = redemption.reward.id,
                        RewardTitle = redemption.reward.title,
                        Cost = redemption.reward.cost,
                        UserId = redemption.user_id,
                        UserName = redemption.user_login,
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
            _log.Error($"Failed to process channel points response: {ex.Message}");
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

        url += "?" + string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH", new DownloadHandlerBuffer(), new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{\"status\": \"FULFILLED\"}"))))
        {
            request.SetRequestHeader("Client-ID", _clientId);
            request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                _log.Warning($"Failed to fulfill redemption {redemptionId}: {request.error}");
            }
        }
    }

    private void OnDestroy()
    {
        StopPolling();
    }

    // Event data class
    public class TwitchChannelPointsEvent
    {
        public string RewardId { get; set; }
        public string RewardTitle { get; set; }
        public int Cost { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserInput { get; set; }
        public DateTime RedeemedAt { get; set; }
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
}