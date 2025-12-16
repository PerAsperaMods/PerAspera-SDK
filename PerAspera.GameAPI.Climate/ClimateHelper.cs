using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Models;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Stateless helper API for climate operations
    /// Uses dynamic Planet access via KeeperTypeRegistry
    /// Architecture: SDK v2.0 - No instance storage pattern
    /// </summary>
    public static class ClimateHelper
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.Climate");
        
        #region Temperature Operations
        
        /// <summary>Get current average temperature in Kelvin</summary>
        public static float? GetTemperature()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.InvokeMethod<float>("GetAverageTemperature");
        }
        
        /// <summary>
        /// Set average temperature
        /// Triggers native event: NativeTemperatureChanged
        /// </summary>
        /// <param name="kelvin">Temperature in Kelvin (150-350K valid range)</param>
        public static bool SetTemperature(float kelvin)
        {
            if (kelvin < 150f || kelvin > 350f)
            {
                _log.Warning($"Temperature {kelvin}K outside valid range (150-350K)");
                return false;
            }
            
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null)
            {
                _log.Error("Cannot set temperature: Planet instance not available");
                return false;
            }
            
            planet.InvokeMethod<object>("SetAverageTemperature", kelvin);
            _log.Debug($"Temperature set to {kelvin}K");
            return true;
        }
        
        #endregion
        
        #region Atmospheric Pressure Operations
        
        /// <summary>Get total atmospheric pressure (atm)</summary>
        public static float? GetTotalPressure()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.InvokeMethod<float>("GetTotalPressure");
        }
        
        /// <summary>Get CO2 partial pressure (kPa)</summary>
        public static float? GetCO2Pressure()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.GetFieldValue<float>("co2Pressure");
        }
        
        /// <summary>
        /// Set CO2 pressure
        /// Triggers native event: NativeCO2PressureChanged
        /// </summary>
        public static bool SetCO2Pressure(float kpa)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod<object>("SetCO2Pressure", kpa);
            return true;
        }
        
        /// <summary>Get O2 partial pressure (kPa)</summary>
        public static float? GetO2Pressure()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.GetFieldValue<float>("o2Pressure");
        }
        
        /// <summary>
        /// Set O2 pressure
        /// Triggers native event: NativeO2PressureChanged
        /// </summary>
        public static bool SetO2Pressure(float kpa)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod<object>("SetO2Pressure", kpa);
            return true;
        }
        
        /// <summary>Get N2 partial pressure (kPa)</summary>
        public static float? GetN2Pressure()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.GetFieldValue<float>("n2Pressure");
        }
        
        /// <summary>
        /// Set N2 pressure
        /// Triggers native event: NativeN2PressureChanged
        /// </summary>
        public static bool SetN2Pressure(float kpa)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod<object>("SetN2Pressure", kpa);
            return true;
        }
        
        /// <summary>Get GHG (Greenhouse Gas) pressure (kPa)</summary>
        public static float? GetGHGPressure()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.GetFieldValue<float>("ghgPressure");
        }
        
        /// <summary>
        /// Set GHG pressure
        /// Triggers native event: NativeGHGPressureChanged
        /// </summary>
        public static bool SetGHGPressure(float kpa)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod<object>("SetGHGPressure", kpa);
            return true;
        }
        
        #endregion
        
        #region Water Operations
        
        /// <summary>Get current water stock</summary>
        public static float? GetWaterStock()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.GetFieldValue<float>("waterStock");
        }
        
        /// <summary>
        /// Set water stock
        /// Triggers native event: NativeWaterStockChanged
        /// </summary>
        public static bool SetWaterStock(float amount)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod<object>("SetWaterStock", amount);
            return true;
        }
        
        #endregion
        
        #region Composite Operations
        
        /// <summary>
        /// Get complete climate snapshot
        /// Returns immutable DTO with all current climate parameters
        /// </summary>
        public static ClimateSnapshot? GetSnapshot()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return null;
            
            try
            {
                var snapshot = new ClimateSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    Sol = GetCurrentSol() ?? 0,
                    
                    // Temperature
                    Temperature = GetTemperature() ?? 0f,
                    MinTemperature = planet.GetFieldValue<float>("minTemperature"),
                    MaxTemperature = planet.GetFieldValue<float>("maxTemperature"),
                    
                    // Pressure
                    TotalPressure = GetTotalPressure() ?? 0f,
                    CO2Pressure = GetCO2Pressure() ?? 0f,
                    O2Pressure = GetO2Pressure() ?? 0f,
                    N2Pressure = GetN2Pressure() ?? 0f,
                    GHGPressure = GetGHGPressure() ?? 0f,
                    ArgonPressure = planet.GetFieldValue<float>("argonPressure"),
                    
                    // Water
                    WaterStock = GetWaterStock() ?? 0f,
                    WaterVaporPressure = planet.GetFieldValue<float>("waterVaporPressure"),
                    
                    // Effects
                    GreenhouseEffect = planet.GetFieldValue<float>("greenhouseEffect"),
                    Albedo = planet.GetFieldValue<float>("albedo")
                };
                
                return snapshot;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to create climate snapshot: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get atmosphere composition breakdown
        /// </summary>
        public static AtmosphereData? GetAtmosphereData()
        {
            var snapshot = GetSnapshot();
            if (snapshot == null) return null;
            
            return new AtmosphereData
            {
                TotalPressure = snapshot.TotalPressure,
                Sol = snapshot.Sol,
                GasPressures = new Dictionary<string, float>
                {
                    ["CO2"] = snapshot.CO2Pressure,
                    ["O2"] = snapshot.O2Pressure,
                    ["N2"] = snapshot.N2Pressure
                }
            };
        }
        
        /// <summary>
        /// Calculate basic habitability score (0-100%)
        /// Based on temperature, pressure, oxygen
        /// </summary>
        public static float CalculateHabitability()
        {
            var snapshot = GetSnapshot();
            if (snapshot == null) return 0f;
            
            float score = 0f;
            
            // Temperature (0-40 points)
            float tempCelsius = snapshot.TemperatureCelsius;
            if (tempCelsius >= 0f && tempCelsius <= 30f)
                score += 40f;
            else if (tempCelsius >= -10f && tempCelsius <= 40f)
                score += 20f;
            
            // Oxygen (0-30 points)
            if (snapshot.O2Percentage >= 19f && snapshot.O2Percentage <= 23f)
                score += 30f;
            else if (snapshot.O2Percentage >= 15f && snapshot.O2Percentage <= 25f)
                score += 15f;
            
            // Total Pressure (0-20 points)
            if (snapshot.TotalPressure >= 80f && snapshot.TotalPressure <= 120f)
                score += 20f;
            else if (snapshot.TotalPressure >= 60f && snapshot.TotalPressure <= 140f)
                score += 10f;
            
            // Water (0-10 points)
            if (snapshot.WaterStock > 1000f)
                score += 10f;
            else if (snapshot.WaterStock > 100f)
                score += 5f;
            
            return score;
        }
        
        /// <summary>
        /// Get terraforming progress status
        /// </summary>
        public static TerraformingStatus GetTerraformingStatus()
        {
            var snapshot = GetSnapshot();
            if (snapshot == null)
            {
                return new TerraformingStatus
                {
                    TemperaturePhase = "PreTerraforming",
                    PressurePhase = "PreTerraforming",
                    OxygenPhase = "PreTerraforming",
                    Habitability = 0f
                };
            }
            
            // Determine phases
            var tempPhase = snapshot.TemperatureCelsius switch
            {
                >= -10f and <= 30f => "Habitable",
                >= -30f and <= 50f => "Advanced",
                >= -60f and <= 80f => "Intermediate",
                _ => "PreTerraforming"
            };
            
            var pressurePhase = snapshot.TotalPressure switch
            {
                >= 80f and <= 120f => "Habitable",
                >= 50f => "Advanced",
                >= 20f => "Intermediate",
                _ => "PreTerraforming"
            };
            
            var oxygenPhase = snapshot.O2Percentage switch
            {
                >= 19f and <= 23f => "Habitable",
                >= 10f => "Advanced",
                >= 3f => "Intermediate",
                _ => "PreTerraforming"
            };
            
            // Calculate habitability score
            var habitability = CalculateHabitability();
            
            return new TerraformingStatus
            {
                TemperaturePhase = tempPhase,
                PressurePhase = pressurePhase,
                OxygenPhase = oxygenPhase,
                Habitability = habitability,
                TemperatureScore = snapshot.Temperature >= 273f && snapshot.Temperature <= 310f ? 100f : 0f,
                PressureScore = snapshot.TotalPressure >= 80f && snapshot.TotalPressure <= 120f ? 100f : 0f,
                OxygenScore = snapshot.O2Percentage >= 19f && snapshot.O2Percentage <= 23f ? 100f : 0f,
                RecommendedAction = habitability >= 80f ? "Maintain conditions" : "Continue terraforming"
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>Get current Martian sol from Universe</summary>
        private static int? GetCurrentSol()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe?.GetFieldValue<int>("currentSol");
        }
        
        #endregion
    }
}
