using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using System;

namespace ExampleMod
{
    /// <summary>
    /// Example mod using the new PerAspera ModSDK
    /// This demonstrates how simple and clean mod development becomes with the SDK
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ExampleModPlugin : BasePlugin
    {
        public override void Load()
        {
            // ?? Initialize SDK - handles all the complexity for you
            ModSDK.Initialize("ExampleMod", PluginInfo.PLUGIN_VERSION);

            // ?? Subscribe to game events using clean, typed constants
            ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
            ModSDK.Events.Subscribe(GameEvents.TemperatureChanged, OnTemperatureChanged);
            ModSDK.Events.Subscribe(GameEvents.WaterStockChanged, OnWaterChanged);

            // ?? Clean logging with mod name automatically included
            ModSDK.Log.Info("Example mod loaded successfully!");
            ModSDK.Log.Info($"SDK Version: {ModSDK.Version.GetFullVersion()}");
        }

        public override bool Unload()
        {
            // ?? SDK handles cleanup automatically
            ModSDK.Shutdown();
            return true;
        }

        /// <summary>
        /// Handle new Martian day events
        /// </summary>
        private void OnMartianDay(object eventData)
        {
            try
            {
                // ?? Get game state using simple SDK APIs
                var sol = ModSDK.Universe.GetCurrentSol();
                var planet = ModSDK.Universe.GetPlanet();
                
                ModSDK.Log.Info($"New Martian day! Sol: {sol}");
                
                // Example: Do something every 10 sols
                if (sol % 10 == 0)
                {
                    ModSDK.Log.Info($"Milestone reached: Sol {sol}!");
                    
                    // ?? Publish custom mod event for other mods to listen to
                    ModSDK.Events.Publish("ExampleMod_Milestone", new { Sol = sol, Planet = planet });
                }
            }
            catch (Exception ex)
            {
                ModSDK.Log.Error($"Error in Martian day handler: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle temperature changes
        /// </summary>
        private void OnTemperatureChanged(object eventData)
        {
            try
            {
                // Example temperature monitoring
                ModSDK.Log.Info("Temperature changed - monitoring planetary climate");
            }
            catch (Exception ex)
            {
                ModSDK.Log.Error($"Error in temperature handler: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle water stock changes
        /// </summary>
        private void OnWaterChanged(object eventData)
        {
            try
            {
                ModSDK.Log.Debug("Water stock changed - monitoring planetary hydration");
                
                // Example: Alert if water is running low
                // (In real mod, you'd get the actual water data from the event)
            }
            catch (Exception ex)
            {
                ModSDK.Log.Error($"Error in water handler: {ex.Message}");
            }
        }
    }
}