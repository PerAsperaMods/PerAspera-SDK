using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Climate.Simulation;
using PerAspera.GameAPI.Climate.Analysis;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Master controller for climate simulation integration with Per Aspera
    /// Manages bidirectional synchronization between simulation and game state
    /// </summary>
    public class ClimateController
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Controller");
        
        private readonly ClimateSimulator _simulator;
        private readonly HabitabilityAnalyzer _habitability;
        private readonly TerraformingAnalyzer _terraforming;
        private readonly ClimateConfig _config;
        
        private PerAspera.GameAPI.Wrappers.Planet? _planet;
        private bool _isActive = false;
        private DateTime _lastUpdate = DateTime.Now;
        
        public ClimateController(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateGameBalanced();
            _simulator = new ClimateSimulator(_config);
            _habitability = new HabitabilityAnalyzer(_config);
            _terraforming = new TerraformingAnalyzer(_config);
            
            Log.Info("ClimateController initialized with enhanced terraforming simulation");
        }
        
        /// <summary>
        /// Enable climate simulation control over PerAspera.GameAPI.Wrappers.Planet PerAspera.GameAPI.Wrappers.Atmosphere
        /// </summary>
        public void EnableClimateControl(PerAspera.GameAPI.Wrappers.Planet PerAspera.GameAPI.Wrappers.Planet)
        {
            _planet = PerAspera.GameAPI.Wrappers.Planet ?? throw new ArgumentNullException(nameof(PerAspera.GameAPI.Wrappers.Planet));
            _isActive = true;
            _lastUpdate = DateTime.Now;
            
            Log.Info("Climate control activated for PerAspera.GameAPI.Wrappers.Planet");
        }
        
        /// <summary>
        /// Disable climate simulation (game returns to normal behavior)
        /// </summary>
        public void DisableClimateControl()
        {
            _isActive = false;
            _planet = null;
            
            Log.Info("Climate control deactivated - game takes over");
        }
        
        /// <summary>
        /// Update climate simulation and apply results to game
        /// Call this periodically (e.g., every game day/hour)
        /// </summary>
        public ClimateUpdateResult UpdateClimate(float deltaTimeSols = 1.0f)
        {
            if (!_isActive || _planet == null)
            {
                return new ClimateUpdateResult
                {
                    Success = false,
                    Message = "Climate control not active"
                };
            }
            
            try
            {
                var PerAspera.GameAPI.Wrappers.Atmosphere = _planet.PerAspera.GameAPI.Wrappers.Atmosphere;
                
                // 1. Run physics simulation
                var simResult = _simulator.SimulateStep(PerAspera.GameAPI.Wrappers.Atmosphere, deltaTimeSols);
                
                if (!simResult.Success)
                {
                    return new ClimateUpdateResult
                    {
                        Success = false,
                        Message = $"Simulation failed: {simResult.ErrorMessage}"
                    };
                }
                
                // 2. Apply temperature changes to game
                ApplyTemperatureToGame(PerAspera.GameAPI.Wrappers.Atmosphere, simResult.NewTemperature);
                
                // 3. Calculate analysis scores
                var habitabilityScore = _habitability.CalculateHabitabilityScore(PerAspera.GameAPI.Wrappers.Atmosphere);
                var terraformingProgress = _terraforming.CalculateTerraformingProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
                
                // 4. Log status for debugging
                var status = _simulator.GetClimateStatus(PerAspera.GameAPI.Wrappers.Atmosphere);
                Log.Debug($"Climate update: {status} | Habitability: {habitabilityScore:F1}% | Progress: {terraformingProgress:F1}%");
                
                return new ClimateUpdateResult
                {
                    Success = true,
                    Message = "Climate simulation updated successfully",
                    TemperatureChange = simResult.TemperatureChange,
                    NewTemperature = simResult.NewTemperature,
                    HabitabilityScore = habitabilityScore,
                    TerraformingProgress = terraformingProgress,
                    GreenhouseWarming = simResult.GreenhouseWarming
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Climate update failed: {ex.Message}");
                return new ClimateUpdateResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Override atmospheric gas pressure in the game
        /// Allows direct control of O2, CO2, N2, H2O levels
        /// </summary>
        public bool SetGasPressure(string gasType, float pressureKPa)
        {
            if (!_isActive || _planet == null)
            {
                Log.Warning("Cannot set gas pressure: climate control not active");
                return false;
            }
            
            try
            {
                var gas = _planet.PerAspera.GameAPI.Wrappers.Atmosphere.Composition[gasType];
                if (gas == null)
                {
                    Log.Warning($"Unknown gas type: {gasType}");
                    return false;
                }
                
                if (gas.IsReadOnly)
                {
                    Log.Warning($"Gas {gasType} is read-only");
                    return false;
                }
                
                gas.PartialPressure = pressureKPa;
                Log.Debug($"Set {gasType} pressure to {pressureKPa:F2} kPa");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set {gasType} pressure: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get current status summary for UI display
        /// </summary>
        public string GetStatusSummary()
        {
            if (!_isActive || _planet == null)
                return "Climate Control: DISABLED";
                
            try
            {
                var PerAspera.GameAPI.Wrappers.Atmosphere = _planet.PerAspera.GameAPI.Wrappers.Atmosphere;
                var habitability = _habitability.GetHabitabilityAssessment(PerAspera.GameAPI.Wrappers.Atmosphere);
                var terraforming = _terraforming.GetTerraformingStatusSummary(PerAspera.GameAPI.Wrappers.Atmosphere);
                
                return $"🌡️ {habitability}\\n🌍 {terraforming}";
            }
            catch (Exception ex)
            {
                return $"Climate Control: ERROR - {ex.Message}";
            }
        }
        
        /// <summary>
        /// Apply calculated temperature to game PerAspera.GameAPI.Wrappers.Planet
        /// </summary>
        private void ApplyTemperatureToGame(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, float newTemperature)
        {
            try
            {
                // Set new temperature with validation bounds
                var clampedTemp = Math.Max(150f, Math.Min(400f, newTemperature));
                PerAspera.GameAPI.Wrappers.Atmosphere.Temperature = clampedTemp;
                
                if (Math.Abs(clampedTemp - newTemperature) > 0.1f)
                {
                    Log.Warning($"Temperature clamped from {newTemperature:F1}K to {clampedTemp:F1}K");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to apply temperature: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Terraforming boost: rapidly increase greenhouse gases
        /// </summary>
        public bool BoostTerraforming(float co2BoostKPa = 5.0f, float h2oBoostKPa = 1.0f)
        {
            if (!_isActive)
                return false;
                
            var success = true;
            success &= SetGasPressure("CO2", (_planet?.PerAspera.GameAPI.Wrappers.Atmosphere.Composition["CO2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f) + co2BoostKPa);
            success &= SetGasPressure("H2O", (_planet?.PerAspera.GameAPI.Wrappers.Atmosphere.Composition["H2O"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f) + h2oBoostKPa);
            
            if (success)
            {
                Log.Info($"Terraforming boost applied: +{co2BoostKPa}kPa CO2, +{h2oBoostKPa}kPa H2O");
            }
            
            return success;
        }
    }
    
    /// <summary>
    /// Result of a climate update operation
    /// </summary>
    public class ClimateUpdateResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public float TemperatureChange { get; set; }
        public float NewTemperature { get; set; }
        public float HabitabilityScore { get; set; }
        public float TerraformingProgress { get; set; }
        public float GreenhouseWarming { get; set; }
        
        public override string ToString()
        {
            if (!Success) return $"❌ {Message}";
            return $"✅ ΔT={TemperatureChange:+F1;-F1;0}K → {NewTemperature:F0}K | Habitability:{HabitabilityScore:F0}% | Progress:{TerraformingProgress:F0}%";
        }
    }
}




