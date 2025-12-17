using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate.Simulation
{
    /// <summary>
    /// Climate simulation engine (stub implementation)
    /// </summary>
    public class ClimateSimulator
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Simulator");
        private readonly ClimateConfig _config;
        
        public ClimateSimulator(ClimateConfig config)
        {
            _config = config;
            Log.Info("ClimateSimulator initialized");
        }
        
        /// <summary>
        /// Simulate one climate step
        /// </summary>
        public void SimulateStep(PerAspera.GameAPI.Wrappers.Atmosphere atmosphere, float deltaTime)
        {
            // Stub: Basic simulation step
            Log.Debug($"Climate simulation step: {deltaTime:F2}s");
        }
        
        /// <summary>
        /// Get current climate status string
        /// </summary>
        public string GetClimateStatus(PerAspera.GameAPI.Wrappers.Atmosphere atmosphere)
        {
            return $"Status: {atmosphere.Temperature:F1}K, {atmosphere.TotalPressure:F2}kPa";
        }
    }
}