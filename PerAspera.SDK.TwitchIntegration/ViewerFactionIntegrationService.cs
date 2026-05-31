// ViewerFactionIntegrationService.cs - Main service for integrating viewer factions with Twitch and the game
using System;
using System.Threading;
using System.Threading.Tasks;
using PerAspera.Core;
using PerAspera.SDK.TwitchIntegration.ViewerFaction;
using PerAspera.SDK.TwitchIntegration.Commands;
// using PerAspera.SDK.TwitchIntegration.Vendor.UnityTwitchChat; // REMOVED: Non-existent namespace

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Main service for integrating viewer factions with Twitch chat and the game
    /// Connects Twitch IRC, processes commands, and manages viewer factions
    /// </summary>
    public class ViewerFactionIntegrationService : IDisposable
    {
        private static readonly LogAspera _logger = new LogAspera("ViewerFactionIntegration");
        
        private readonly ViewerFactionManager _factionManager;
        private readonly ViewerFactionCommands _commandHandler;
        private readonly SimpleTwitchIRCClient? _twitchClient; // CHANGED: Use existing SimpleTwitchIRCClient
        private readonly Timer _cleanupTimer;
        
        private bool _isRunning;
        private bool _disposed;
        
        /// <summary>
        /// Whether the service is currently running
        /// </summary>
        public bool IsRunning => _isRunning && (_twitchClient?.IsConnected ?? false); // CHANGED: Use IsConnected property
        
        /// <summary>
        /// Viewer faction manager
        /// </summary>
        public ViewerFactionManager FactionManager => _factionManager;
        
        public ViewerFactionIntegrationService(TwitchConfiguration? config = null) // CHANGED: Use existing TwitchConfiguration
        {
            _factionManager = new ViewerFactionManager();
            _commandHandler = new ViewerFactionCommands(_factionManager, SendMessage);
            
            // Setup Twitch connection if config is provided
            if (config != null && config.IsValid())
            {
                try
                {
                    // CHANGED: Use existing SimpleTwitchIRCClient instead of non-existent TwitchConnection
                    _twitchClient = new SimpleTwitchIRCClient(
                        config.BotUsername, 
                        config.OAuthToken, 
                        config.ChannelName);
                    
                    // CHANGED: Use existing event system
                    _twitchClient.OnMessageReceived += OnTwitchMessageReceived;
                    _twitchClient.OnConnected += OnTwitchConnected;
                    // REMOVED: Events not available in SimpleTwitchIRCClient
                    // _twitchConnection.OnDisconnected += OnTwitchDisconnected;
                    // _twitchConnection.OnError += OnTwitchError;
                    
                    _logger.Info("Twitch connection initialized");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to initialize Twitch connection: {ex.Message}");
                }
            }
            else
            {
                _logger.Warning("No valid Twitch configuration provided. Running in offline mode.");
            }
            
            // Setup cleanup timer (runs every minute)
            _cleanupTimer = new Timer(CleanupCallback, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            
            _logger.Info("ViewerFactionIntegrationService initialized");
        }
        
        /// <summary>
        /// Start the integration service
        /// </summary>
        public async Task<bool> StartAsync()
        {
            if (_isRunning)
            {
                _logger.Warning("Service is already running");
                return true;
            }
            
            try
            {
                _logger.Info("Starting ViewerFactionIntegrationService...");
                
                if (_twitchClient != null)
                {
                    bool connected = await _twitchClient.ConnectAsync(); // CHANGED: Use ConnectAsync method
                    if (!connected)
                    {
                        _logger.Error("Failed to connect to Twitch IRC");
                        return false;
                    }
                }
                
                _isRunning = true;
                _logger.Info("ViewerFactionIntegrationService started successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to start service: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Stop the integration service
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
                return;
            
            try
            {
                _logger.Info("Stopping ViewerFactionIntegrationService...");
                
                _isRunning = false;
                
                if (_twitchClient != null)
                {
                    _twitchClient.Stop(); // CHANGED: Use Stop method instead of Dispose
                }
                
                _logger.Info("ViewerFactionIntegrationService stopped");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error stopping service: {ex.Message}");
            }
        }
        
        // ==================== TWITCH EVENT HANDLERS ====================
        
        private void OnTwitchMessageReceived(string rawMessage)
        {
            try
            {
                // Parse IRC message
                var parsedMessage = ParseIRCMessage(rawMessage);
                if (parsedMessage.HasValue)
                {
                    var (username, displayName, message) = parsedMessage.Value;
                    _logger.Debug($"Message from {username}: {message}");
                    
                    // Process command
                    _commandHandler.ProcessMessage(username, displayName, message);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing Twitch message: {ex.Message}");
            }
        }
        
        private void OnTwitchConnected()
        {
            _logger.Info("Connected to Twitch IRC");
        }
        
        private void OnTwitchDisconnected()
        {
            _logger.Warning("Disconnected from Twitch IRC");
            _isRunning = false;
        }
        
        private void OnTwitchError(Exception ex)
        {
            _logger.Error($"Twitch connection error: {ex.Message}");
        }
        
        // ==================== MESSAGE PARSING ====================
        
        private (string username, string displayName, string message)? ParseIRCMessage(string rawMessage)
        {
            try
            {
                // IRC message format: :username!username@username.tmi.twitch.tv PRIVMSG #channel :message
                if (!rawMessage.Contains("PRIVMSG"))
                    return null;
                
                var parts = rawMessage.Split(' ');
                if (parts.Length < 4)
                    return null;
                
                // Extract username
                var userPart = parts[0];
                if (!userPart.StartsWith(":"))
                    return null;
                    
                var username = userPart.Substring(1).Split('!')[0];
                
                // Extract message
                var messageIndex = rawMessage.IndexOf(':', 1);
                if (messageIndex < 0)
                    return null;
                    
                var message = rawMessage.Substring(messageIndex + 1).Trim();
                
                // Use username as display name (can be enhanced later)
                return (username, username, message);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to parse IRC message: {ex.Message}");
                return null;
            }
        }
        
        // ==================== MESSAGE SENDING ====================
        
        private void SendMessage(string username, string message)
        {
            if (_twitchClient != null && _twitchClient.IsConnected)
            {
                try
                {
                    // Send as a reply to the user
                    var formattedMessage = $"@{username} {message}";
                    _twitchClient.SendMessageAsync(formattedMessage); // CHANGED: Use SendMessageAsync
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to send message: {ex.Message}");
                }
            }
            else
            {
                // Offline mode - just log
                _logger.Info($"[OFFLINE] Would send to {username}: {message}");
            }
        }
        
        // ==================== CLEANUP ====================
        
        private void CleanupCallback(object? state)
        {
            try
            {
                _factionManager.CleanupExpired();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during cleanup: {ex.Message}");
            }
        }
        
        // ==================== STATISTICS ====================
        
        /// <summary>
        /// Get current statistics
        /// </summary>
        public string GetStatistics()
        {
            return $"Viewers: {_factionManager.TotalViewers}, " +
                   $"Teams: {_factionManager.TotalTeams}, " +
                   $"Active Deals: {_factionManager.TotalActiveDeals}, " +
                   $"Connected: {IsRunning}";
        }
        
        // ==================== DISPOSAL ====================
        
        public void Dispose()
        {
            if (_disposed)
                return;
            
            Stop();
            _cleanupTimer?.Dispose();
            _twitchClient?.Dispose(); // CHANGED: Use _twitchClient
            
            _disposed = true;
            _logger.Info("ViewerFactionIntegrationService disposed");
        }
    }
}
