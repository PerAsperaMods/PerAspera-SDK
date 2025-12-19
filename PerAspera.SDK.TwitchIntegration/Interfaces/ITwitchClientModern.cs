using System;
using System.Threading.Tasks;

namespace PerAspera.SDK.TwitchIntegration.Interfaces
{
    /// <summary>
    /// Modern TwitchLib v4.0.1+ compatible interface for Per Aspera SDK integration
    /// 
    /// DESIGN PRINCIPLES:
    /// - Based on actual TwitchLib.Client.Interfaces.ITwitchClient signatures from repository analysis
    /// - All async methods return Task or Task<T> (TwitchLib 4.0+ requirement)
    /// - Minimal interface focused on Per Aspera needs: connect, disconnect, send messages
    /// - Compatible with TwitchLib async event handlers
    /// - Circuit breaker pattern support for reliability
    /// 
    /// DOC REFERENCES:
    /// - TwitchLib Source: F:\ModPeraspera\Internal_doc\repotwitchlib\TwitchLib.Client\TwitchLib.Client\Interfaces\ITwitchClient.cs
    /// - TwitchLib Main: F:\ModPeraspera\Internal_doc\repotwitchlib\TwitchLib\README.md
    /// - Per Aspera Integration: Two-phase initialization pattern
    /// </summary>
    public interface ITwitchClientModern : IDisposable
    {
        /// <summary>
        /// Indicates if the client is currently connected to Twitch IRC
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Current channel name (if connected to a channel)
        /// </summary>
        string? CurrentChannel { get; }
        
        /// <summary>
        /// Connection state for circuit breaker pattern
        /// </summary>
        ConnectionState ConnectionState { get; }
        
        /// <summary>
        /// Connects to Twitch IRC asynchronously
        /// Compatible with TwitchLib 4.0.1+ ConnectAsync signature
        /// </summary>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> ConnectAsync();
        
        /// <summary>
        /// Disconnects from Twitch IRC asynchronously
        /// Compatible with TwitchLib 4.0.1+ DisconnectAsync signature
        /// </summary>
        Task DisconnectAsync();
        
        /// <summary>
        /// Forces reconnection (disconnect + connect)
        /// Compatible with TwitchLib 4.0.1+ ReconnectAsync signature
        /// </summary>
        Task ReconnectAsync();
        
        /// <summary>
        /// Sends message to current channel asynchronously
        /// Compatible with TwitchLib 4.0.1+ SendMessageAsync signature
        /// </summary>
        /// <param name="channel">Channel name</param>
        /// <param name="message">Message content</param>
        /// <param name="dryRun">If true, don't actually send (for testing)</param>
        Task SendMessageAsync(string channel, string message, bool dryRun = false);
        
        /// <summary>
        /// Joins a channel asynchronously
        /// Compatible with TwitchLib 4.0.1+ JoinChannelAsync signature
        /// </summary>
        /// <param name="channel">Channel to join</param>
        Task JoinChannelAsync(string channel);
        
        // Modern TwitchLib 4.0.1+ async events (Task return type, not void)
        event Func<string, Task>? OnConnectedAsync;
        event Func<string, Task>? OnDisconnectedAsync; 
        event Func<string, string, Task>? OnMessageReceivedAsync;
        event Func<Exception, Task>? OnErrorAsync;
        event Func<string, Task>? OnJoinedChannelAsync;
    }
    
    /// <summary>
    /// Connection states for circuit breaker pattern
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Failed,
        CircuitOpen  // Circuit breaker triggered
    }
}