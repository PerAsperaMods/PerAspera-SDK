using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Events;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// DEPRECATED: Use TwitchIntegrationManager instead
    /// This class is kept for compatibility but delegates to TwitchIntegrationManager
    /// </summary>
    [Obsolete("Use TwitchIntegrationManager instead. This class will be removed in future versions.")]
    public class TwitchCommandProcessor
    {
        private static readonly LogAspera Log = LogAspera.Create("TwitchCommandProcessor");
        
        /// <summary>
        /// Process a Twitch command (delegates to TwitchIntegrationManager)
        /// </summary>
        public static string ProcessCommand(string command, string[] args)
        {
            Log.Warning("TwitchCommandProcessor.ProcessCommand is deprecated. Use TwitchIntegrationManager.ProcessCommand instead.");
            return TwitchIntegrationManager.ProcessCommand(command, args, "unknown_user");
        }
        
        /// <summary>
        /// Check if processor is ready (delegates to TwitchIntegrationManager)
        /// </summary>
        public static bool IsReady => TwitchIntegrationManager.IsFullPhaseReady;
        
        /// <summary>
        /// Get initialization status (delegates to TwitchIntegrationManager)
        /// </summary>
        public static string GetInitializationStatus()
        {
            return TwitchIntegrationManager.GetInitializationStatus();
        }
        
        /// <summary>
        /// Cleanup method (delegates to TwitchIntegrationManager)
        /// </summary>
        public static void Cleanup()
        {
            TwitchIntegrationManager.Cleanup();
        }
    }
}