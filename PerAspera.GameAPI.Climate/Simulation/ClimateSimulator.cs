using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Climate.Simulation.Models;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate.Simulation
{
    /// <summary>
    /// Main climate simulation orchestrator for Mars terraforming
    /// Coordinates temperature, pressure, and atmospheric composition models
    /// </summary>
    public class ClimateSimulator
    {
        private static readonly LogAspera Log = new LogAspera("Climate.ClimateSimulator");
        private readonly ClimateConfig _config;
        private readonly GreenhouseModel _greenhouse;
        private readonly TemperatureModel _temperature;
        private readonly PressureModel _pressure;
        
        public ClimateSimulator(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateRealistic();
            _greenhouse = new GreenhouseModel(_config);
            _temperature = new TemperatureModel(_config);
            _pressure = new PressureModel(_config);
            
            Log.Info($"Climate Simulator initialized with {_config.GetType().Name}");
        }
        
        /// <summary>
        /// Run complete climate simulation step
        /// Updates temperature based on atmospheric composition and solar input
        /// </summary>
        public ClimateSimulationResult SimulateStep(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, float deltaTime = 1.0f)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
            {
                Log.Warning("PerAspera.GameAPI.Wrappers.Atmosphere is null or has no composition");
                return new ClimateSimulationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid PerAspera.GameAPI.Wrappers.Atmosphere data"
                };
            }
            
            try
            {
                // 1. Calculate greenhouse warming from atmospheric gases
                var greenhouseWarming = _greenhouse.CalculateGreenhouseWarming(PerAspera.GameAPI.Wrappers.Atmosphere);
                
                // 2. Calculate equilibrium temperature with greenhouse effect
                var newTemperature = _temperature.CalculateEquilibriumTemperature(
                    PerAspera.GameAPI.Wrappers.Atmosphere, 
                    greenhouseWarming
                );
                
                // 3. Calculate pressure changes (if any dynamic effects)
                var pressureChange = _pressure.CalculatePressureChange(PerAspera.GameAPI.Wrappers.Atmosphere, deltaTime);
                
                // 4. Apply gradual temperature change (thermal inertia)
                var currentTemp = PerAspera.GameAPI.Wrappers.Atmosphere.Temperature;
                var temperatureChange = (newTemperature - currentTemp) * 0.1f * deltaTime;
                var finalTemperature = currentTemp + temperatureChange;
                
                Log.Debug($"Climate step: ΔT={temperatureChange:F2}K, Final T={finalTemperature:F1}K, ΔP={pressureChange:F3}kPa");
                
                return new ClimateSimulationResult
                {
                    Success = true,
                    NewTemperature = finalTemperature,
                    TemperatureChange = temperatureChange,
                    PressureChange = pressureChange,
                    GreenhouseWarming = greenhouseWarming,
                    DeltaTime = deltaTime
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Climate simulation failed: {ex.Message}");
                return new ClimateSimulationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Get current climate status summary
        /// </summary>
        public string GetClimateStatus(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null) return "No PerAspera.GameAPI.Wrappers.Atmosphere data";
            
            var temp = PerAspera.GameAPI.Wrappers.Atmosphere.Temperature;
            var pressure = PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure;
            var greenhouse = _greenhouse.CalculateGreenhouseWarming(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            return $"T:{temp:F1}K ({temp-273:+F1;-F1;0}°C), P:{pressure:F2}kPa, GH:+{greenhouse:F1}K";
        }
    }
    
    /// <summary>
    /// Result of a climate simulation step
    /// </summary>
    public class ClimateSimulationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public float NewTemperature { get; set; }
        public float TemperatureChange { get; set; }
        public float PressureChange { get; set; }
        public float GreenhouseWarming { get; set; }
        public float DeltaTime { get; set; }
        
        public override string ToString()
        {
            if (!Success) return $"Failed: {ErrorMessage}";
            return $"ΔT={TemperatureChange:+F2;-F2;0}K → {NewTemperature:F1}K, GH:+{GreenhouseWarming:F1}K";
        }
    }
}





