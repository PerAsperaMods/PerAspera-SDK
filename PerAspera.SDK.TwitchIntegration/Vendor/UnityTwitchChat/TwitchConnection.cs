// Unity-Twitch-Chat Integration - Ported for Per Aspera
// Original: https://github.com/lexonegit/Unity-Twitch-Chat
// License: MIT (compatible with Per Aspera project)

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration.Vendor.UnityTwitchChat
{
    /// <summary>
    /// Per Aspera adapted version of Unity-Twitch-Chat TwitchConnection
    /// Provides IRC connectivity for Twitch chat integration
    /// </summary>
    public partial class TwitchConnection : IDisposable
    {
        private TcpClient? _tcpClient;
        private string _oauth = string.Empty;
        private string _nick = string.Empty;
        private string _channel = string.Empty;
        
        private int _readBufferSize = 1024;
        private int _readInterval = 16;
        private int _writeInterval = 16;
        
        private readonly ConcurrentQueue<string> _alertQueue = new();
        private readonly ConcurrentQueue<string> _writeQueue = new();
        
        private bool _isConnected;
        private bool _disposed;
        
        // Events for Per Aspera integration
        public event Action<string>? OnMessageReceived;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<Exception>? OnError;
        
        public bool IsConnected => _isConnected && _tcpClient?.Connected == true;
        
        public TwitchConnection(TwitchConnectionConfig config)
        {
            try
            {
                _tcpClient = new TcpClient(config.Address, config.Port);
                _oauth = config.OAuth;
                _nick = config.Username;
                _channel = config.Channel;
                _readBufferSize = config.ReadBufferSize;
                _readInterval = config.ReadInterval;
                _writeInterval = config.WriteInterval;
                
                LogAspera.Info($"TwitchConnection created for {_nick}@{_channel}");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create TwitchConnection: {ex.Message}");
                _tcpClient = null;
                OnError?.Invoke(ex);
            }
        }
        
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_tcpClient?.Connected != true)
                {
                    LogAspera.Warning("TcpClient not connected, attempting reconnection...");
                    return false;
                }
                
                // Start read/write threads
                StartReadThread();
                StartWriteThread();
                
                // Send IRC authentication
                await SendIRCAuthenticationAsync();
                
                _isConnected = true;
                OnConnected?.Invoke();
                LogAspera.Info($"Successfully connected to Twitch IRC for {_channel}");
                
                return true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Connection failed: {ex.Message}");
                OnError?.Invoke(ex);
                return false;
            }
        }
        
        public void SendMessage(string message)
        {
            if (!IsConnected)
            {
                LogAspera.Warning("Cannot send message - not connected");
                return;
            }
            
            var ircMessage = $"PRIVMSG #{_channel} :{message}";
            _writeQueue.Enqueue(ircMessage);
            LogAspera.Debug($"Queued message: {message}");
        }
        
        private async Task SendIRCAuthenticationAsync()
        {
            var authCommands = new[]
            {
                $"PASS {_oauth}",
                $"NICK {_nick}",
                $"JOIN #{_channel}",
                "CAP REQ :twitch.tv/commands"
            };
            
            foreach (var command in authCommands)
            {
                _writeQueue.Enqueue(command);
                await Task.Delay(100); // Small delay between auth commands
            }
        }
        
        public void Disconnect()
        {
            try
            {
                _isConnected = false;
                _tcpClient?.Close();
                OnDisconnected?.Invoke();
                LogAspera.Info("Disconnected from Twitch IRC");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error during disconnect: {ex.Message}");
                OnError?.Invoke(ex);
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            Disconnect();
            _tcpClient?.Dispose();
            _disposed = true;
            
            LogAspera.Debug("TwitchConnection disposed");
        }
    }
    
    /// <summary>
    /// Configuration for TwitchConnection
    /// </summary>
    public class TwitchConnectionConfig
    {
        public string Address { get; set; } = "irc.chat.twitch.tv";
        public int Port { get; set; } = 6667;
        public string OAuth { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int ReadBufferSize { get; set; } = 1024;
        public int ReadInterval { get; set; } = 16;
        public int WriteInterval { get; set; } = 16;
    }
}