using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PerAspera.Core;

namespace PerAspera.GameAPI.TwitchIntegration
{
    /// <summary>
    /// IL2CPP-compatible Twitch PubSub client using HTTP polling for channel points.
    /// Uses async/await instead of Unity coroutines for IL2CPP compatibility.
    /// </summary>
    public class SimpleTwitchPubSubClient
    {
        private const string HELIX_BASE_URL = "https://api.twitch.tv/helix";
        private const int POLL_INTERVAL_MS = 2000; // Poll every 2 seconds

        private readonly LogAspera _log = new LogAspera("TwitchPubSub");

        private string _clientId;
        private string _accessToken;
        private string _broadcasterId;
        private bool _isPolling;
        private CancellationTokenSource? _cancellationTokenSource;

        // Events
        public event Action<TwitchChannelPointsEvent>? OnChannelPointsRedeemed;

        /// <summary>
        /// Initialize the PubSub client with Twitch credentials
        /// </summary>
        public void Initialize(string clientId, string accessToken, string broadcasterId)
        {
            _clientId = clientId;
            // Strip "oauth:" prefix if present (Twitch API expects just the token for Bearer auth)
            _accessToken = accessToken.StartsWith("oauth:") ? accessToken.Substring(6) : accessToken;
            _broadcasterId = broadcasterId;

            _log.Info($"SimpleTwitchPubSubClient initialized for broadcaster {_broadcasterId}");
        }

        /// <summary>
        /// Start polling for channel point redemptions asynchronously
        /// </summary>
        public async Task StartPollingAsync()
        {
            if (_isPolling)
            {
                _log.Warning("PubSub polling already started");
                return;
            }

            _isPolling = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await PollChannelPointsAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                _log.Error($"Error in polling loop: {ex.Message}");
            }
            finally
            {
                _isPolling = false;
            }
        }

        /// <summary>
        /// Stop polling for channel point redemptions
        /// </summary>
        public void StopPolling()
        {
            if (!_isPolling)
                return;

            _isPolling = false;
            _cancellationTokenSource?.Cancel();
            _log.Info("Stopped PubSub polling");
        }

        /// <summary>
        /// Main polling loop using async/await instead of Unity coroutines
        /// </summary>
        private async Task PollChannelPointsAsync(CancellationToken cancellationToken)
        {
            _log.Info("Started PubSub polling for channel points");

            while (_isPolling && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await FetchChannelPointRedemptionsAsync(cancellationToken);
                    await Task.Delay(POLL_INTERVAL_MS, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error($"Error in polling iteration: {ex.Message}");
                    await Task.Delay(5000, cancellationToken); // Wait 5 seconds on error
                }
            }

            _log.Info("PubSub polling loop ended");
        }

        /// <summary>
        /// Fetch channel point redemptions using HttpClient instead of UnityWebRequest
        /// </summary>
        private async Task FetchChannelPointRedemptionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Client-ID", _clientId);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                // Debug logging
                _log.Info($"Making API call with Client-ID: {_clientId}, Broadcaster: {_broadcasterId}");
                _log.Info($"Authorization header: Bearer {_accessToken.Substring(0, 10)}...");

                var parameters = new Dictionary<string, string>
                {
                    ["broadcaster_id"] = _broadcasterId,
                    ["status"] = "UNFULFILLED",
                    ["first"] = "20"
                };

                var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var url = $"{HELIX_BASE_URL}/channel_points/custom_rewards/redemptions?{queryString}";

                _log.Info($"API URL: {url}");

                var response = await httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                    _log.Info($"API Response: {jsonResponse}");
                    ProcessRedemptionsResponse(jsonResponse);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _log.Error($"Failed to fetch channel point redemptions: {response.StatusCode} - {response.ReasonPhrase}");
                    _log.Error($"Error details: {errorContent}");
                    
                    // If Bad Request with "Missing required parameter reward_id", it means we need to specify which reward to monitor
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && errorContent.Contains("reward_id"))
                    {
                        _log.Info("Channel points API requires a specific reward_id. To enable channel points:");
                        _log.Info("1. Go to your Twitch dashboard: https://dashboard.twitch.tv/");
                        _log.Info("2. Navigate to Channel Points");
                        _log.Info("3. Create at least one channel point reward");
                        _log.Info("4. The mod will then be able to monitor redemptions for that reward");
                        _log.Info("Alternatively, you can disable channel points in twitch_config.json by setting EnableChannelPoints to false");
                        // Stop polling since this will keep failing
                        _isPolling = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error fetching channel point redemptions: {ex.Message}");
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

                    // Mark as fulfilled asynchronously
                    _ = FulfillRedemptionAsync(redemption.id, CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to process channel points response: {ex.Message}");
        }
    }

        /// <summary>
        /// Mark a redemption as fulfilled using HttpClient
        /// </summary>
        private async Task FulfillRedemptionAsync(string redemptionId, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Client-ID", _clientId);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var parameters = new Dictionary<string, string>
                {
                    ["id"] = redemptionId,
                    ["broadcaster_id"] = _broadcasterId
                };

                var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var url = $"{HELIX_BASE_URL}/channel_points/custom_rewards/redemptions?{queryString}";

                var payload = new StringContent("{\"status\": \"FULFILLED\"}", Encoding.UTF8, "application/json");

                var response = await httpClient.PatchAsync(url, payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _log.Warning($"Failed to fulfill redemption {redemptionId}: {response.StatusCode} - {response.ReasonPhrase}");
                }
                else
                {
                    _log.Info($"Successfully fulfilled redemption {redemptionId}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error fulfilling redemption {redemptionId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            StopPolling();
            _cancellationTokenSource?.Dispose();
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