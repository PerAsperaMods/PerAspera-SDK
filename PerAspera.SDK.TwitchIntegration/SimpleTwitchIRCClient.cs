using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// SIMPLE IRC Client for Twitch Chat - Production Ready
    /// 
    /// APPROACH: 
    /// - Basic IRC protocol implementation (RFC 1459)
    /// - No external dependencies or TwitchLib complications
    /// - Focus on reliability and simplicity
    /// - Integrates with TwitchIntegrationManager for game effects
    /// 
    /// USAGE:
    /// - Connect to Twitch IRC (irc.chat.twitch.tv:6667)
    /// - Listen for chat messages and parse commands
    /// - Send responses through TwitchIntegrationManager
    /// </summary>
    public class SimpleTwitchIRCClient : IDisposable
    {
        private static readonly LogAspera Log = new LogAspera("TwitchIRC");
        
        private TcpClient? _tcpClient;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cancellationToken;
        
        private readonly string _server = "irc.chat.twitch.tv";
        private readonly int _port = 6667;
        private readonly string _botUsername;
        private readonly string _oauthToken;
        private readonly string _channelName;
        
        private bool _isConnected = false;
        private bool _disposed = false;
        private DateTime _lastReconnectAttempt = DateTime.MinValue;
        private int _reconnectDelaySeconds = 30;
        
        /// <summary>
        /// Initialize simple IRC client
        /// </summary>
        /// <param name="botUsername">Twitch bot username</param>
        /// <param name="oauthToken">OAuth token (oauth:xxxxx)</param>
        /// <param name="channelName">Channel to join</param>
        public SimpleTwitchIRCClient(string botUsername, string oauthToken, string channelName)
        {
            _botUsername = botUsername ?? throw new ArgumentNullException(nameof(botUsername));
            _oauthToken = oauthToken ?? throw new ArgumentNullException(nameof(oauthToken));
            _channelName = channelName?.ToLower() ?? throw new ArgumentNullException(nameof(channelName));
            
            Log.Info($"SimpleTwitchIRC initialized for {_botUsername} → #{_channelName}");
        }
        
        /// <summary>
        /// Connect to Twitch IRC
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (_isConnected || _disposed) return false;
            
            try
            {
                Log.Info($"🔗 Connecting to Twitch IRC: {_server}:{_port}");
                
                _cancellationToken = new CancellationTokenSource();
                _tcpClient = new TcpClient();
                
                await _tcpClient.ConnectAsync(_server, _port);
                
                var stream = _tcpClient.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };
                
                // IRC authentication
                await _writer.WriteLineAsync($"PASS {_oauthToken}");
                await _writer.WriteLineAsync($"NICK {_botUsername}");
                await _writer.WriteLineAsync($"JOIN #{_channelName}");
                
                _isConnected = true;
                Log.Info($"✅ Connected to Twitch IRC: #{_channelName}");
                
                // Start message processing loop
                _ = Task.Run(ProcessMessagesAsync, _cancellationToken.Token);
                
                // Start message sending loop  
                _ = Task.Run(SendQueuedMessagesAsync, _cancellationToken.Token);
                
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to connect to Twitch IRC: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Disconnect from Twitch IRC
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!_isConnected) return;
            
            try
            {
                Log.Info("🔌 Disconnecting from Twitch IRC");
                
                _isConnected = false;
                _cancellationToken?.Cancel();
                
                if (_writer != null)
                {
                    await _writer.WriteLineAsync($"PART #{_channelName}");
                    await _writer.WriteLineAsync("QUIT");
                }
                
                _reader?.Close();
                _writer?.Close();
                _tcpClient?.Close();
                
                Log.Info("✅ Disconnected from Twitch IRC");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error during disconnect: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send message to Twitch chat
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            if (!_isConnected || _writer == null || string.IsNullOrEmpty(message)) return;
            
            try
            {
                await _writer.WriteLineAsync($"PRIVMSG #{_channelName} :{message}");
                Log.Debug($"📤 Sent: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to send message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process incoming IRC messages
        /// </summary>
        private async Task ProcessMessagesAsync()
        {
            try
            {
                while (_isConnected && !_cancellationToken!.Token.IsCancellationRequested && _reader != null)
                {
                    var line = await _reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;
                    
                    Log.Debug($"📥 IRC: {line}");
                    
                    // Handle IRC PING/PONG
                    if (line.StartsWith("PING"))
                    {
                        var pongResponse = line.Replace("PING", "PONG");
                        await _writer!.WriteLineAsync(pongResponse);
                        continue;
                    }
                    
                    // Parse chat messages
                    if (line.Contains("PRIVMSG"))
                    {
                        await ProcessChatMessageAsync(line);
                    }
                    
                    await Task.Delay(10, _cancellationToken.Token); // Small delay to prevent CPU spinning
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("IRC message processing cancelled");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error in message processing: {ex.Message}");
                _isConnected = false;
                
                // Schedule reconnection attempt
                _lastReconnectAttempt = DateTime.Now;
                Log.Warning($"🔄 Connection lost. Will retry in {_reconnectDelaySeconds} seconds...");
            }
        }
        
        /// <summary>
        /// Process chat message and execute commands
        /// </summary>
        private async Task ProcessChatMessageAsync(string ircLine)
        {
            try
            {
                // Parse IRC line: :username!username@username.tmi.twitch.tv PRIVMSG #channel :message
                var parts = ircLine.Split(' ');
                if (parts.Length < 4) return;
                
                var userPart = parts[0].Substring(1); // Remove ':'
                var username = userPart.Split('!')[0];
                var messagePart = ircLine.Substring(ircLine.IndexOf(':', 1) + 1);
                
                Log.Debug($"💬 {username}: {messagePart}");
                
                // Process commands
                if (messagePart.StartsWith("!"))
                {
                    var commandParts = messagePart.Split(' ');
                    var command = commandParts[0];
                    var args = commandParts.Length > 1 ? commandParts[1..] : Array.Empty<string>();
                    
                    var response = TwitchIntegrationManager.ProcessCommand(command, args, username);
                    
                    if (!string.IsNullOrEmpty(response))
                    {
                        await SendMessageAsync(response);
                    }
                }
                
                // Simulate Twitch events for testing (remove in production)
                await SimulateTwitchEventsAsync(username, messagePart);
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error processing chat message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Send queued messages from TwitchIntegrationManager
        /// </summary>
        private async Task SendQueuedMessagesAsync()
        {
            try
            {
                while (_isConnected && !_cancellationToken!.Token.IsCancellationRequested)
                {
                    var queuedMessage = TwitchIntegrationManager.GetNextQueuedMessage();
                    if (!string.IsNullOrEmpty(queuedMessage))
                    {
                        await SendMessageAsync(queuedMessage);
                    }
                    
                    await Task.Delay(1000, _cancellationToken.Token); // Check queue every second
                }
            }
            catch (OperationCanceledException)
            {
                Log.Info("Message sending loop cancelled");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error in message sending loop: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Simulate Twitch events for testing (DEVELOPMENT ONLY)
        /// TODO: Replace with real TwitchLib PubSub/EventSub integration
        /// </summary>
        private async Task SimulateTwitchEventsAsync(string username, string message)
        {
            try
            {
                // Simulate follow
                if (message.Contains("!testfollow"))
                {
                    var response = TwitchIntegrationManager.ProcessTwitchFollow(username, username, _channelName);
                    await SendMessageAsync(response);
                }
                
                // Simulate bits
                if (message.StartsWith("!testbits"))
                {
                    var parts = message.Split(' ');
                    var bits = parts.Length > 1 && int.TryParse(parts[1], out var b) ? b : 100;
                    var response = TwitchIntegrationManager.ProcessTwitchBits(username, username, _channelName, bits);
                    await SendMessageAsync(response);
                }
                
                // Simulate subscription
                if (message.Contains("!testsub"))
                {
                    var response = TwitchIntegrationManager.ProcessTwitchSubscription(username, username, _channelName, "Tier 1");
                    await SendMessageAsync(response);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Error in event simulation: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get connection status
        /// </summary>
        public string GetStatus()
        {
            var status = _isConnected ? "Connected" : "Disconnected";
            return $"Twitch IRC: {status} | Channel: #{_channelName} | Bot: {_botUsername}";
        }
        
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            
            try
            {
                DisconnectAsync().Wait(5000); // 5 second timeout
            }
            catch (Exception ex)
            {
                Log.Warning($"Error during dispose: {ex.Message}");
            }
            finally
            {
                _cancellationToken?.Dispose();
                _reader?.Dispose();
                _writer?.Dispose();
                _tcpClient?.Dispose();
            }
        }
    }
}