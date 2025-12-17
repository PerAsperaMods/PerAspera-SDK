using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Climate.Simulation;
using PerAspera.GameAPI.Climate.Patches;
using PerAspera.GameAPI.Wrappers;

// Aliases pour éviter le conflit Unity.Atmosphere vs PerAspera.GameAPI.Wrappers.Atmosphere
using Atmo = PerAspera.GameAPI.Wrappers.Atmosphere;
using PlanetWrapped = PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Master controller for climate simulation integration with Per Aspera
    /// Manages bidirectional synchronization between simulation and game state
    /// Provides the requested bidirectional control over atmospheric values.
    /// </summary>
    public class ClimateController
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Controller");
        
        private readonly ClimateSimulator _simulator;
        private readonly ClimateConfig _config;
        
        private PlanetWrapped? _planet;
        private bool _isActive = false;
        private DateTime _lastUpdate = DateTime.Now;
        
        public ClimateController(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateGameBalanced();
            _simulator = new ClimateSimulator(_config);
            
            Log.Info("ClimateController initialized with bidirectional Harmony control");
        }
        
        /// <summary>
        /// Enable climate control for the specified planet
        /// Activates Harmony patches for bidirectional control
        /// </summary>
        public void EnableClimateControl(PlanetWrapped planet)
        {
            _planet = planet ?? throw new ArgumentNullException(nameof(planet));
            _isActive = true;
            
            // Active les patches Harmony pour ce planet
            PlanetClimatePatches.EnableClimateControl(planet.GetNativeObject());
            
            Log.Info("Climate control activated with Harmony patches - full bidirectional control enabled");
        }
        
        /// <summary>
        /// Disable climate control - game takes over atmospheric management
        /// Deactivates Harmony patches
        /// </summary>
        public void DisableClimateControl()
        {
            if (_planet != null)
            {
                PlanetClimatePatches.DisableClimateControl(_planet.GetNativeObject());
            }
            
            _isActive = false;
            _planet = null;
            Log.Info("Climate control deactivated - game takes over, Harmony patches disabled");
        }
        
        /// <summary>
        /// Update climate simulation and synchronize with game if active
        /// Called from mod's update loop
        /// </summary>
        public void UpdateClimate(float deltaTime)
        {
            if (!_isActive || _planet == null) return;
            
            try
            {
                var atmosphere = _planet.Atmosphere;
                
                // Run climate simulation step
                _simulator.SimulateStep(atmosphere, deltaTime);
                
                // Calculate metrics for monitoring
                var status = _simulator.GetClimateStatus(atmosphere);
                
                Log.Debug($"Climate update: {status} | Bidirectional control active");
            }
            catch (Exception ex)
            {
                Log.Error($"Climate update failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set gas pressure directly (bidirectional control feature)
        /// Uses Harmony patches to override game getters
        /// </summary>
        public void SetGasPressure(string gasType, float pressure)
        {
            try
            {
                if (!_isActive || _planet == null)
                {
                    Log.Warning("Cannot set gas pressure: climate control not active");
                    return;
                }
                
                // Met à jour via les patches Harmony - le jeu verra cette valeur
                PlanetClimatePatches.SetClimateValue(_planet.GetNativeObject(), $"{gasType}Pressure", pressure);
                
                Log.Debug($"Set gas pressure via Harmony: {gasType}={pressure:F2}kPa");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set gas pressure: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set temperature directly (bidirectional control feature)
        /// Uses Harmony patches to override game getters
        /// </summary>
        public void SetTemperature(float temperatureKelvin)
        {
            try
            {
                if (!_isActive || _planet == null)
                {
                    Log.Warning("Cannot set temperature: climate control not active");
                    return;
                }
                
                // Met à jour via les patches Harmony - le jeu verra cette valeur 
                PlanetClimatePatches.SetClimateValue(_planet.GetNativeObject(), "temperature", temperatureKelvin);
                
                Log.Debug($"Set temperature via Harmony: {temperatureKelvin:F1}K ({temperatureKelvin - 273.15f:F1}°C)");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set temperature: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Boost terraforming progress by temporarily accelerating atmospheric changes
        /// Uses Harmony-controlled values for immediate game impact
        /// </summary>
        public void BoostTerraforming(float boostFactor = 2.0f, int durationMinutes = 30)
        {
            if (_planet == null) return;
            
            try
            {
                // Example: Get current values from our simulation and boost O2 production
                var atmosphere = _planet.Atmosphere;
                float currentCO2 = atmosphere.Composition["CO2"]?.PartialPressure ?? 0.6f; // Mars baseline
                float currentO2 = atmosphere.Composition["O2"]?.PartialPressure ?? 0.01f;
                
                float co2ToConvert = currentCO2 * 0.02f * boostFactor; // Convert 2% * boost factor
                
                // Use Harmony patches for immediate effect
                SetGasPressure("CO2", currentCO2 - co2ToConvert);
                SetGasPressure("O2", currentO2 + co2ToConvert * 0.7f); // 70% efficiency
                
                Log.Info($"Terraforming boosted via Harmony: rate={boostFactor:F2}x for {durationMinutes}m");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to boost terraforming: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get current climate status for monitoring/debugging
        /// </summary>
        public string GetStatus()
        {
            if (!_isActive || _planet == null) 
                return "Climate Control: INACTIVE";
            
            var atmosphere = _planet.Atmosphere;
            var status = _simulator.GetClimateStatus(atmosphere);
            
            return $"Climate Control: ACTIVE (Harmony) | {status}";
        }
    }
}