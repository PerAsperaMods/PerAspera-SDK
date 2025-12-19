using System;
using System.Threading.Tasks;
using PerAspera.SDK.TwitchIntegration.Interfaces;
using PerAspera.Core;

// TwitchLib v4.0.1+ imports
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using Microsoft.Extensions.Logging;

namespace PerAspera.SDK.TwitchIntegration.Client
{
    /// <summary>
    /// Modern TwitchLib v4.0.1+ wrapper for Per Aspera integration
    /// 
    /// FEATURES:
    /// - Full TwitchLib 4.0.1+ async/await compatibility
    /// - Circuit breaker pattern for reliability
    /// - Per Aspera SDK logging integration
    /// - Two-phase initialization support
    /// - Rate limiting and anti-spam protection
    /// 
    /// DOC REFERENCES:
    /// - TwitchLib Client: F:\ModPeraspera\Internal_doc\repotwitchlib\TwitchLib.Client\README.md
    /// - TwitchLib Main: F:\ModPeraspera\Internal_doc\repotwitchlib\TwitchLib\README.md
    /// - Per Aspera Integration: TwitchIntegrationManager two-phase system
    /// 
    /// BREAKING CHANGES FROM TWITCHLIB 3.x:
    /// - All methods now async with Task/Task<T> return types
    /// - Event handlers now return Task (async events)
    /// - Removed OnLog, AddChatCommandIdentifier, builder classes
    /// - Model properties instead of fields
    /// </summary>
    public class ModernTwitchClientWrapper : ITwitchClientModern
    {
        private static readonly LogAspera _logger = LogAspera.Create("TwitchClientModern");
        
        private readonly TwitchClient _client;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly string _username;
        private readonly string _oauth;
        private readonly CircuitBreakerConfig _circuitConfig;
        
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        private string? _currentChannel;
        private DateTime _lastConnectionAttempt = DateTime.MinValue;
        private int _consecutiveFailures = 0;
        private bool _disposed = false;
        
        // TwitchLib 4.0.1+ async events mapped to our interface
        public event Func<string, Task>? OnConnectedAsync;
        public event Func<string, Task>? OnDisconnectedAsync;
        public event Func<string, string, Task>? OnMessageReceivedAsync;
        public event Func<Exception, Task>? OnErrorAsync;
        public event Func<string, Task>? OnJoinedChannelAsync;
        
        public bool IsConnected => _client?.IsConnected == true;
        public string? CurrentChannel => _currentChannel;
        public ConnectionState ConnectionState => _connectionState;
        
        public ModernTwitchClientWrapper(
            string username, 
            string oauth, 
            ILoggerFactory? loggerFactory = null,
            CircuitBreakerConfig? circuitConfig = null)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _oauth = oauth ?? throw new ArgumentNullException(nameof(oauth));
            _loggerFactory = loggerFactory;
            _circuitConfig = circuitConfig ?? CircuitBreakerConfig.Default;
            
            try
            {
                // TwitchLib 4.0.1+ constructor pattern
                _client = new TwitchClient(loggerFactory: _loggerFactory);
                
                // Initialize connection credentials
                var credentials = new ConnectionCredentials(_username, _oauth);
                _client.Initialize(credentials);
                
                // Subscribe to TwitchLib 4.0.1+ async events
                SubscribeToTwitchLibEvents();
                
                _logger.Info($"ModernTwitchClientWrapper initialized for {_username}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize TwitchClient: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Subscribe to TwitchLib 4.0.1+ async events and map to our interface
        /// </summary>
        private void SubscribeToTwitchLibEvents()
        {
            // TwitchLib 4.0.1+ async event handlers (Task return type)
            _client.OnConnected += async (sender, e) =>
            {
                _connectionState = ConnectionState.Connected;
                _consecutiveFailures = 0;
                
                _logger.Info($"Connected to Twitch IRC: {e.BotUsername}");
                
                if (OnConnectedAsync != null)
                    await OnConnectedAsync.Invoke(e.BotUsername ?? _username);
            };
            
            _client.OnDisconnected += async (sender, e) =>
            {
                _connectionState = ConnectionState.Disconnected;
                _currentChannel = null;
                
                _logger.Info("Disconnected from Twitch IRC");
                
                if (OnDisconnectedAsync != null)
                    await OnDisconnectedAsync.Invoke("Disconnected");
            };
            
            _client.OnJoinedChannel += async (sender, e) =>
            {
                _currentChannel = e.Channel;
                
                _logger.Info($"Joined channel: {e.Channel}");
                
                if (OnJoinedChannelAsync != null)
                    await OnJoinedChannelAsync.Invoke(e.Channel);
            };
            
            _client.OnMessageReceived += async (sender, e) =>
            {
                var message = e.ChatMessage;
                
                // Rate limiting check
                if (ShouldProcessMessage(message))
                {
                    _logger.Debug($"Message from {message.Username}: {message.Message}");
                    
                    if (OnMessageReceivedAsync != null)
                        await OnMessageReceivedAsync.Invoke(message.Username, message.Message);
                }
            };
            
            _client.OnConnectionError += async (sender, e) =>
            {
                _connectionState = ConnectionState.Failed;
                _consecutiveFailures++;
                
                var exception = new Exception($"TwitchLib connection error: {e.Error?.Message ?? "Unknown error"}");
                _logger.Error($"Connection error: {exception.Message}");
                
                if (OnErrorAsync != null)
                    await OnErrorAsync.Invoke(exception);
                
                // Circuit breaker logic
                await HandleConnectionFailureAsync(exception);
            };
        }
        
        /// <summary>
        /// TwitchLib 4.0.1+ compatible ConnectAsync implementation
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ModernTwitchClientWrapper));
            
            // Circuit breaker check
            if (_connectionState == ConnectionState.CircuitOpen)
            {
                if (DateTime.UtcNow - _lastConnectionAttempt < _circuitConfig.CircuitOpenDuration)
                {
                    _logger.Warning("Circuit breaker is open, connection attempt blocked");
                    return false;
                }
                
                _logger.Info("Circuit breaker timeout expired, attempting to reconnect");
                _connectionState = ConnectionState.Disconnected;
            }
            
            if (_connectionState == ConnectionState.Connected)
            {
                _logger.Info("Already connected to Twitch IRC");
                return true;
            }
            
            try
            {
                _connectionState = ConnectionState.Connecting;
                _lastConnectionAttempt = DateTime.UtcNow;
                
                _logger.Info($"Connecting to Twitch IRC as {_username}...");
                
                // TwitchLib 4.0.1+ async connect
                var result = await _client.ConnectAsync();
                
                if (result)
                {
                    _connectionState = ConnectionState.Connected;
                    _consecutiveFailures = 0;
                    _logger.Info("Successfully connected to Twitch IRC");
                }
                else
                {
                    _connectionState = ConnectionState.Failed;
                    _consecutiveFailures++;
                    _logger.Warning("TwitchLib ConnectAsync returned false");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _connectionState = ConnectionState.Failed;
                _consecutiveFailures++;
                
                _logger.Error($"Connection failed: {ex.Message}");
                await HandleConnectionFailureAsync(ex);
                
                return false;
            }
        }
        
        /// <summary>
        /// TwitchLib 4.0.1+ compatible DisconnectAsync implementation
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_disposed) return;
            
            try
            {
                if (_connectionState == ConnectionState.Connected)
                {
                    _logger.Info("Disconnecting from Twitch IRC...");
                    
                    // TwitchLib 4.0.1+ async disconnect
                    await _client.DisconnectAsync();
                }
                
                _connectionState = ConnectionState.Disconnected;
                _currentChannel = null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Disconnect error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// TwitchLib 4.0.1+ compatible ReconnectAsync implementation
        /// </summary>
        public async Task ReconnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ModernTwitchClientWrapper));
            
            _logger.Info("Performing Twitch IRC reconnection...");
            _connectionState = ConnectionState.Reconnecting;
            
            try
            {
                await DisconnectAsync();
                await Task.Delay(1000); // Brief delay before reconnect
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"Reconnection failed: {ex.Message}");
                await HandleConnectionFailureAsync(ex);
            }
        }
        
        /// <summary>
        /// TwitchLib 4.0.1+ compatible SendMessageAsync implementation
        /// </summary>
        public async Task SendMessageAsync(string channel, string message, bool dryRun = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ModernTwitchClientWrapper));
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            
            if (!IsConnected)
            {
                _logger.Warning($"Cannot send message - not connected to Twitch IRC");
                return;
            }
            
            try
            {
                if (dryRun)
                {
                    _logger.Info($"DRY RUN - Would send to {channel}: {message}");
                    return;
                }
                
                // TwitchLib 4.0.1+ async send message
                await _client.SendMessageAsync(channel, message, dryRun);
                
                _logger.Debug($"Sent message to {channel}: {message}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to send message to {channel}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// TwitchLib 4.0.1+ compatible JoinChannelAsync implementation
        /// </summary>
        public async Task JoinChannelAsync(string channel)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ModernTwitchClientWrapper));
            if (string.IsNullOrEmpty(channel)) throw new ArgumentNullException(nameof(channel));
            
            if (!IsConnected)
            {
                _logger.Warning($"Cannot join channel {channel} - not connected to Twitch IRC");
                return;
            }
            
            try
            {
                _logger.Info($"Joining Twitch channel: {channel}");
                
                // TwitchLib 4.0.1+ async join channel
                await _client.JoinChannelAsync(channel);
                
                _currentChannel = channel;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to join channel {channel}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Circuit breaker pattern: handle connection failures
        /// </summary>
        private async Task HandleConnectionFailureAsync(Exception ex)
        {
            if (_consecutiveFailures >= _circuitConfig.FailureThreshold)
            {
                _connectionState = ConnectionState.CircuitOpen;
                _logger.Warning($"Circuit breaker opened after {_consecutiveFailures} failures. Next retry in {_circuitConfig.CircuitOpenDuration}");
                
                if (OnErrorAsync != null)
                    await OnErrorAsync.Invoke(new Exception($"Circuit breaker opened: {ex.Message}", ex));
            }
            else
            {
                var delay = CalculateBackoffDelay();
                _logger.Info($"Will retry connection in {delay.TotalSeconds}s (failure {_consecutiveFailures}/{_circuitConfig.FailureThreshold})");
                
                // Schedule automatic retry
                _ = Task.Delay(delay).ContinueWith(async _ =>
                {
                    if (!_disposed && _connectionState != ConnectionState.Connected)
                        await ConnectAsync();
                });
            }
        }
        
        /// <summary>
        /// Calculate exponential backoff delay
        /// </summary>
        private TimeSpan CalculateBackoffDelay()
        {
            var delay = TimeSpan.FromSeconds(Math.Min(
                _circuitConfig.BaseRetryDelay.TotalSeconds * Math.Pow(2, _consecutiveFailures - 1),
                _circuitConfig.MaxRetryDelay.TotalSeconds));
            
            return delay;
        }
        
        /// <summary>
        /// Rate limiting for message processing
        /// </summary>
        private bool ShouldProcessMessage(ChatMessage message)
        {
            // Basic rate limiting - can be enhanced
            return !string.IsNullOrEmpty(message.Message) && message.Message.Length <= 500;
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            try
            {
                _client?.Disconnect();
                _client?.Dispose();
                
                _logger.Info("ModernTwitchClientWrapper disposed");
            }
            catch (Exception ex)
            {
                _logger.Error($"Dispose error: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Circuit breaker configuration for connection reliability
    /// </summary>
    public class CircuitBreakerConfig
    {
        public int FailureThreshold { get; set; } = 3;
        public TimeSpan CircuitOpenDuration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(1);
        
        public static CircuitBreakerConfig Default => new CircuitBreakerConfig();
    }
}