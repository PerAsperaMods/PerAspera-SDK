using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;

namespace SimpleClimateLogger
{
    /// <summary>
    /// Example mod that demonstrates Per Aspera SDK usage for climate monitoring
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SimpleClimateLoggerPlugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"Loading {MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}");

            // Initialize the ModSDK
            ModSDK.Initialize(this);

            // Subscribe to climate events
            ModSDK.GameEvents.TemperatureChanged += OnTemperatureChanged;
            ModSDK.GameEvents.AtmosphereChanged += OnAtmosphereChanged;
            ModSDK.GameEvents.WaterStockChanged += OnWaterStockChanged;

            Log.LogInfo("Simple Climate Logger initialized successfully!");
        }

        private void OnTemperatureChanged(PerAspera.GameAPI.ClimateEventData eventData)
        {
            Log.LogInfo($"🌡️ Temperature changed: {eventData.PreviousValue}°C → {eventData.CurrentValue}°C (Δ: {eventData.Delta:+0.00;-0.00}°C) [Sol {eventData.MartianSol}]");
        }

        private void OnAtmosphereChanged(PerAspera.GameAPI.ClimateEventData eventData)
        {
            Log.LogInfo($"🌫️ Atmosphere changed: {eventData.PreviousValue} → {eventData.CurrentValue} (Δ: {eventData.Delta:+0.00;-0.00}) [Sol {eventData.MartianSol}]");
        }

        private void OnWaterStockChanged(PerAspera.GameAPI.ClimateEventData eventData)
        {
            Log.LogInfo($"💧 Water stock changed: {eventData.PreviousValue} → {eventData.CurrentValue} (Δ: {eventData.Delta:+0.00;-0.00}) [Sol {eventData.MartianSol}]");
        }
    }
}