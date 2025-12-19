using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using PerAspera.Core.IL2CPP;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// BepInX plugin entry point for Twitch Integration
    /// Starts immediately and uses two-phase initialization system
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("PerAspera.GameAPI.Events", BepInDependency.DependencyFlags.HardDependency)]
    public class TwitchIntegrationPlugin : BasePlugin
    {
        public const string PluginGuid = "PerAspera.SDK.TwitchIntegration";
        public const string PluginName = "Per Aspera Twitch Integration";
        public const string PluginVersion = "1.0.0";
        
        private static readonly LogAspera Log = LogAspera.Create("TwitchIntegrationPlugin");
        
        /// <summary>
        /// Plugin load - starts immediately when BepInX loads
        /// The actual initialization happens via SDK events
        /// </summary>
        public override void Load()
        {
            try
            {
                Log.Info($"Loading {PluginName} v{PluginVersion}");
                
                // Apply any necessary Harmony patches
                ApplyHarmonyPatches();
                
                // The TwitchIntegrationManager will automatically initialize
                // via static constructor and event subscriptions
                
                // Force static initialization
                var status = TwitchIntegrationManager.GetInitializationStatus();
                Log.Info("TwitchIntegrationManager static initialization triggered");
                
                Log.Info($"✅ {PluginName} loaded successfully - waiting for game initialization events");
            }
            catch (System.Exception ex)
            {
                Log.Error($"❌ Failed to load {PluginName}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Apply any required Harmony patches for Twitch integration
        /// </summary>
        private void ApplyHarmonyPatches()
        {
            try
            {
                var harmony = new Harmony(PluginGuid);
                
                // Apply any Twitch-specific patches if needed
                // For now, we rely on the SDK Events system for initialization
                
                Log.Info("Harmony patches applied successfully");
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to apply Harmony patches: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Plugin unload cleanup
        /// </summary>
        public override bool Unload()
        {
            try
            {
                Log.Info($"Unloading {PluginName}");
                
                // Cleanup TwitchIntegrationManager
                TwitchIntegrationManager.Cleanup();
                
                Log.Info($"✅ {PluginName} unloaded successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"❌ Error during {PluginName} unload: {ex.Message}");
                return false;
            }
        }
    }
}