using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using PerAspera.Core.IL2CPP;
using PerAspera.SDK.TwitchIntegration.Core;

namespace PerAspera.SDK.TwitchIntegration.Client
{
    /// <summary>
    /// Modern TwitchLib.Client 4.0.1+ integration manager
    /// Replaces Unity-Twitch-Chat with full TwitchLib capabilities
    /// </summary>
    public class TwitchClientManager : IDisposable
    {
        private static readonly LogAspera Log = LogAspera.Create("TwitchClientManager");
        
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchClientManager> _logger;
        private readonly ITwitchCommandProcessor _commandProcessor;
        private readonly TwitchIntegrationConfig _config;
        
        private bool _isInitialized = false;
        private bool _isConnected = false;
        private string? _currentChannel;
        
        /// <summary>
        /// Modern TwitchLib client initialization with dependency injection
        /// </summary>
        /// <param name="loggerFactory">Logger factory for TwitchLib internal logging</param>
        /// <param name="commandProcessor">Command processor for chat commands</param>
        /// <param name="config">Twitch integration configuration</param>
        public TwitchClientManager(
            ILoggerFactory loggerFactory,
            ITwitchCommandProcessor commandProcessor,
            TwitchIntegrationConfig config)
        {
            _logger = loggerFactory.CreateLogger<TwitchClientManager>();
            _commandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            // TwitchLib 4.0+ modern initialization with logging
            _client = new TwitchClient(loggerFactory: loggerFactory);
            
            // Subscribe to TwitchLib 4.0+ async events
            _client.OnConnected += OnConnectedAsync;
            _client.OnJoinedChannel += OnJoinedChannelAsync;
            _client.OnMessageReceived += OnMessageReceivedAsync;
            _client.OnWhisperReceived += OnWhisperReceivedAsync;
            _client.OnError += OnErrorAsync;
            _client.OnDisconnected += OnDisconnectedAsync;
            _client.OnReconnected += OnReconnectedAsync;
            _client.OnConnectionError += OnConnectionErrorAsync;
            
            Log.Info("TwitchClientManager initialized with TwitchLib 4.0.1+");
        }
        
        /// <summary>
        /// Initialize and connect to Twitch IRC using modern async patterns
        /// </summary>
        /// <param name="username">Bot username</param>
        /// <param name="oauthToken">OAuth token (oauth:xxxxx format)</param>
        /// <param name="channel">Channel to join</param>
        public async Task InitializeAsync(string username, string oauthToken, string channel)
        {
            if (_isInitialized)
            {
                Log.Warning("TwitchClientManager already initialized");
                return;
            }
            
            try
            {
                _currentChannel = channel;
                
                // Create TwitchLib credentials
                var credentials = new ConnectionCredentials(username, oauthToken);
                
                // TwitchLib 4.0+ initialization pattern
                _client.Initialize(credentials);
                
                // Modern async connection
                await _client.ConnectAsync();
                
                _isInitialized = true;
                Log.Info($"TwitchLib client initialized for user: {username}, channel: {channel}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize TwitchLib client: {ex.Message}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Send message to current channel using TwitchLib async API
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (!_isConnected || string.IsNullOrEmpty(_currentChannel))
            {
                Log.Warning($"Cannot send message - not connected to channel: {message}");
                return;
            }
            
            try
            {
                // TwitchLib 4.0+ async message sending
                await _client.SendMessageAsync(_currentChannel, message);
                Log.Debug($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send message '{message}': {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Send whisper using TwitchLib async API
        /// </summary>
        public async Task SendWhisperAsync(string username, string message)
        {
            if (!_isConnected)
            {
                Log.Warning($"Cannot send whisper - not connected: {username}: {message}");
                return;
            }
            
            try
            {
                // TwitchLib 4.0+ async whisper sending
                await _client.SendWhisperAsync(username, message);
                Log.Debug($"Sent whisper to {username}: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send whisper to '{username}': {ex.Message}", ex);
            }
        }
        
        #region TwitchLib 4.0+ Async Event Handlers
        
        /// <summary>
        /// Handle TwitchLib connection event (4.0+ async pattern)
        /// </summary>
        private async Task OnConnectedAsync(object? sender, OnConnectedEventArgs e)
        {
            try
            {
                _logger.LogInformation($"Connected to Twitch IRC: {e.AutoJoinChannel}");
                Log.Info($"Connected to Twitch IRC server: {e.BotUsername}");
                
                // Auto-join configured channel
                if (!string.IsNullOrEmpty(_currentChannel))
                {
                    await _client.JoinChannelAsync(_currentChannel);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnConnectedAsync: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handle channel join event (4.0+ async pattern)
        /// </summary>
        private async Task OnJoinedChannelAsync(object? sender, OnJoinedChannelArgs e)
        {
            try
            {
                _isConnected = true;
                _currentChannel = e.Channel;
                
                _logger.LogInformation($"Joined Twitch channel: {e.Channel}");
                Log.Info($"Successfully joined channel: {e.Channel}");
                
                // Send initial connection message if configured
                if (_config.SendConnectMessage && !string.IsNullOrEmpty(_config.ConnectMessage))
                {
                    await Task.Delay(1000); // Brief delay to avoid spam detection
                    await SendMessageAsync(_config.ConnectMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnJoinedChannelAsync: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handle chat message received (4.0+ async pattern with Per Aspera command processing)
        /// </summary>
        private async Task OnMessageReceivedAsync(object? sender, OnMessageReceivedArgs e)
        {
            try
            {
                var chatMessage = e.ChatMessage;
                
                _logger.LogDebug($"[{chatMessage.Channel}] {chatMessage.Username}: {chatMessage.Message}");
                
                // Process potential commands through Per Aspera command system
                if (chatMessage.Message.StartsWith(_config.CommandPrefix))
                {
                    var commandResult = await _commandProcessor.ProcessCommandAsync(chatMessage);
                    
                    if (commandResult.ShouldRespond && !string.IsNullOrEmpty(commandResult.Response))
                    {
                        await SendMessageAsync(commandResult.Response);
                    }
                }
                
                // Log high-level chat activity for analytics
                Log.Debug($"Chat: {chatMessage.Username}#{chatMessage.Channel}: {chatMessage.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing chat message: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handle whisper received (4.0+ async pattern)
        /// </summary>
        private async Task OnWhisperReceivedAsync(object? sender, OnWhisperReceivedArgs e)
        {
            try
            {
                var whisperMessage = e.WhisperMessage;
                
                _logger.LogInformation($"Whisper from {whisperMessage.Username}: {whisperMessage.Message}");
                
                // Process whisper commands (typically admin commands)
                if (whisperMessage.Message.StartsWith(_config.CommandPrefix))
                {
                    var commandResult = await _commandProcessor.ProcessWhisperCommandAsync(whisperMessage);
                    
                    if (commandResult.ShouldRespond && !string.IsNullOrEmpty(commandResult.Response))
                    {
                        await SendWhisperAsync(whisperMessage.Username, commandResult.Response);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing whisper: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Handle TwitchLib errors (4.0+ async pattern)
        /// </summary>
        private async Task OnErrorAsync(object? sender, OnErrorEventArgs e)
        {
            _logger.LogError($"TwitchLib error: {e.Exception?.Message}");
            Log.Error($"TwitchLib client error: {e.Exception?.Message}", e.Exception);
            
            // Auto-recovery logic could go here
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Handle disconnection (4.0+ async pattern)
        /// </summary>
        private async Task OnDisconnectedAsync(object? sender, OnDisconnectedEventArgs e)
        {
            _isConnected = false;
            
            _logger.LogWarning("Disconnected from Twitch IRC");
            Log.Warning("Disconnected from Twitch IRC - will attempt to reconnect");
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Handle reconnection (4.0+ async pattern)
        /// </summary>
        private async Task OnReconnectedAsync(object? sender, OnReconnectedEventArgs e)
        {
            _logger.LogInformation("Reconnected to Twitch IRC");
            Log.Info("Successfully reconnected to Twitch IRC");
            
            // Rejoin channel after reconnection
            if (!string.IsNullOrEmpty(_currentChannel))
            {
                await _client.JoinChannelAsync(_currentChannel);
            }
        }
        
        /// <summary>
        /// Handle connection errors (4.0+ async pattern)
        /// </summary>
        private async Task OnConnectionErrorAsync(object? sender, OnConnectionErrorArgs e)
        {
            _logger.LogError($"TwitchLib connection error: {e.Error?.Message}");
            Log.Error($"TwitchLib connection error: {e.Error?.Message}", e.Error);
            
            await Task.CompletedTask;
        }
        
        #endregion
        
        /// <summary>
        /// Get current connection status
        /// </summary>
        public bool IsConnected => _isConnected && _client?.IsConnected == true;
        
        /// <summary>
        /// Get current channel
        /// </summary>
        public string? CurrentChannel => _currentChannel;
        
        /// <summary>
        /// Graceful shutdown of TwitchLib client
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_client?.IsConnected == true)
                {
                    if (!string.IsNullOrEmpty(_currentChannel))
                    {
                        await _client.LeaveChannelAsync(_currentChannel);
                    }
                    
                    await _client.DisconnectAsync();
                }
                
                _isConnected = false;
                Log.Info("TwitchLib client disconnected gracefully");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during TwitchLib disconnect: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Synchronous cleanup for disposal
                _client?.Disconnect();
                _client?.Dispose();
                
                Log.Info("TwitchClientManager disposed");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during TwitchClientManager disposal: {ex.Message}", ex);
            }
        }
    }
}