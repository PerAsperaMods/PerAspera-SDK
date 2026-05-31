using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.Core;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Climate.Domain.Atmosphere
{
    /// <summary>
    /// Wrapper for Planet atmosphere properties
    /// Provides type-safe access to atmospheric composition and climate data
    /// </summary>
    public class PlanetaryAtmosphere
    {
        private readonly object _nativePlanet;
        private readonly AtmosphericComposition _composition;
        private readonly Dictionary<string, TerraformingEffect> _effects;

        public PlanetaryAtmosphere(object nativePlanet)
        {
            _nativePlanet = nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet));

            // 🔄 DYNAMIC: Initialize gas composition from ResourceType system
            var gases = new Dictionary<string, AtmosphericGas>();

            // Core atmospheric gases (always present)
            InitializeCoreGases(gases);

            // 🎯 MODABLE: Discover additional atmospheric ResourceType from mods
            DiscoverModdedAtmosphericResources(gases);

            _composition = new AtmosphericComposition(gases, () => TotalPressure, _nativePlanet);

            // Initialize terraforming effects (also potentially modable)
            _effects = InitializeTerraformingEffects();
        }

        /// <summary>
        /// Initialize core atmospheric gases that are always present
        /// </summary>
        private void InitializeCoreGases(Dictionary<string, AtmosphericGas> gases)
        {
            // Base game atmospheric ResourceType
            gases["CO2"] = new AtmosphericGas(_nativePlanet, "Carbon Dioxide", "CO2", "GetCO2Pressure", null);
            gases["O2"] = new AtmosphericGas(_nativePlanet, "Oxygen", "O2", "GetO2Pressure", null);
            gases["N2"] = new AtmosphericGas(_nativePlanet, "Nitrogen", "N2", "GetN2Pressure", null);
            gases["GHG"] = new AtmosphericGas(_nativePlanet, "Greenhouse Gas", "GHG", "GetGHGPressure", null);
            // H2O removed - no public GetH2OPressure method exists
        }

        /// <summary>
        /// 🎯 MODABLE: Discover atmospheric resources from ResourceType system
        /// Dynamically adds mod-added atmospheric gases to the composition
        /// </summary>
        private void DiscoverModdedAtmosphericResources(Dictionary<string, AtmosphericGas> gases)
        {
            try
            {
                // Get all ResourceType instances from the static collection
                var allResourceTypes = ResourceType.StaticValues;
                if (allResourceTypes == null) return;

                // Known atmospheric resource IDs from base game
                var knownAtmosphericIds = new HashSet<string>
                {
                    "resource_oxygen_release",
                    "resource_carbon_dioxide_release",
                    "resource_oxygen_capture",
                    "resource_carbon_dioxide_capture",
                    "resource_oxygen_respiration",
                    "resource_nitrogen_release",
                    "resource_ghg_release",
                    "resource_O2_Up",
                    "resource_CO2_Down"
                };

                foreach (var resourceType in allResourceTypes)
                {
                    if (resourceType == null || string.IsNullOrEmpty(resourceType.key)) continue;

                    // Check if this is a known atmospheric resource
                    if (knownAtmosphericIds.Contains(resourceType.key))
                    {
                        // Map resource IDs to gas symbols and create dynamic gas entries
                        var gasInfo = MapResourceToGas(resourceType.key);
                        if (gasInfo != null && !gases.ContainsKey(gasInfo.Value.symbol))
                        {
                            // Create dynamic atmospheric gas for mod-added resources
                            gases[gasInfo.Value.symbol] = new AtmosphericGas(
                                _nativePlanet,
                                gasInfo.Value.name,
                                gasInfo.Value.symbol,
                                gasInfo.Value.getterMethod,
                                gasInfo.Value.setterMethod
                            );
                        }
                    }

                    // TODO: Future enhancement - check for custom atmospheric resources
                    // This could involve checking resource properties or categories
                    // that indicate atmospheric behavior
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail - core gases still work
                LogAspera.LogWarning($"Failed to discover modded atmospheric resources: {ex.Message}");
            }
        }

        /// <summary>
        /// Maps known atmospheric resource IDs to gas information
        /// </summary>
        private (string name, string symbol, string getterMethod, string? setterMethod)? MapResourceToGas(string resourceId)
        {
            return resourceId switch
            {
                "resource_oxygen_release" => ("Oxygen Release", "O2_REL", "GetO2Pressure", null),
                "resource_carbon_dioxide_release" => ("CO2 Release", "CO2_REL", "GetCO2Pressure", null),
                "resource_oxygen_capture" => ("Oxygen Capture", "O2_CAP", "GetO2Pressure", null),
                "resource_carbon_dioxide_capture" => ("CO2 Capture", "CO2_CAP", "GetCO2Pressure", null),
                "resource_oxygen_respiration" => ("Oxygen Respiration", "O2_RESP", "GetO2Pressure", null),
                "resource_nitrogen_release" => ("Nitrogen Release", "N2_REL", "GetN2Pressure", null),
                "resource_ghg_release" => ("GHG Release", "GHG_REL", "GetGHGPressure", null),
                "resource_O2_Up" => ("Oxygen Increase", "O2_UP", "GetO2Pressure", null),
                "resource_CO2_Down" => ("CO2 Decrease", "CO2_DOWN", "GetCO2Pressure", null),
                _ => null
            };
        }

        /// <summary>
        /// Initialize terraforming effects (potentially modable)
        /// </summary>
        private Dictionary<string, TerraformingEffect> InitializeTerraformingEffects()
        {
            return new Dictionary<string, TerraformingEffect>
            {
                ["PolarNuke"] = new TerraformingEffect(_nativePlanet, "Polar Nuclear", "Temperature effect from polar ice cap nukes", "get_polarTemperatureNukeEffect"),
                ["PolarDust"] = new TerraformingEffect(_nativePlanet, "Polar Dust", "Temperature effect from polar dust reduction", "get_polarTemperatureDustEffect"),
                ["Comet"] = new TerraformingEffect(_nativePlanet, "Comet Impact", "Temperature effect from comet impacts", "get_temperatureCometEffect"),
                ["Deimos"] = new TerraformingEffect(_nativePlanet, "Deimos Crash", "Temperature effect from Deimos moon crash", "get_temperatureDeimosEffect")
                // TODO: Support for mod-added terraforming effects
            };
        }

        // ==================== COMPOSITION ====================

        /// <summary>
        /// Atmospheric gas composition (CO2, O2, N2, H2O)
        /// Access via: Atmosphere.Composition["CO2"].PartialPressure
        /// </summary>
        public AtmosphericComposition Composition => _composition;

        /// <summary>
        /// Get cargo representation of atmospheric gas for building interactions
        /// </summary>
        public Cargo? GetGasCargo(string gasSymbol) => _composition.GetGasCargo(gasSymbol);

        /// <summary>
        /// All atmospheric gas cargos for building interactions
        /// </summary>
        public IEnumerable<Cargo> GasCargos => _composition.AllGasCargos;

        // ==================== PRESSURE ====================

        /// <summary>
        /// Total atmospheric pressure (sum of all partial pressures, kPa)
        /// Property: GetTotalPressure (READ-ONLY - calculated by game)
        /// </summary>
        public float TotalPressure => _nativePlanet.InvokeMethod<float>("GetTotalPressure");

        // ==================== TEMPERATURE ====================

        /// <summary>
        /// Current average surface temperature (Kelvin)
        /// Property: GetAverageTemperature (READ-ONLY - calculated by game)
        /// </summary>
        public float Temperature => _nativePlanet.InvokeMethod<float>("GetAverageTemperature");

        /// <summary>
        /// Minimum temperature recorded (Kelvin)
        /// </summary>
        public float MinTemperature => _nativePlanet.InvokeMethod<float>("get_minTemperature");

        /// <summary>
        /// Maximum temperature recorded (Kelvin)
        /// </summary>
        public float MaxTemperature => _nativePlanet.InvokeMethod<float>("get_maxTemperature");

        /// <summary>
        /// Temperature in Celsius (convenience property)
        /// </summary>
        public float TemperatureCelsius => Temperature - 273.15f;

        // ==================== CLIMATE EFFECTS ====================

        /// <summary>
        /// Greenhouse effect contribution (Kelvin)
        /// Property: greenhouseEffect (READ-ONLY - calculated by game)
        /// </summary>
        public float GreenhouseEffect => _nativePlanet.InvokeMethod<float>("get_greenhouseEffect");

        /// <summary>
        /// Planetary albedo (reflectivity, 0-1)
        /// Property: albedo (READ-ONLY - calculated by game)
        /// </summary>
        public float Albedo => _nativePlanet.InvokeMethod<float>("get_albedo");

        /// <summary>
        /// Terraforming effects on climate (polar nukes, dust, comets, Deimos)
        /// Access via: Atmosphere.Effects["PolarNuke"].TemperatureEffect
        /// </summary>
        public IReadOnlyDictionary<string, TerraformingEffect> Effects => _effects;

        // ==================== WATER ====================

        /// <summary>
        /// Available water stock (kilotons)
        /// Property: GetWaterStock (READ-ONLY - use IncreaseWater/DecreaseWater pour modifications)
        /// </summary>
        public float WaterStock => _nativePlanet.InvokeMethod<float>("GetWaterStock");

        /// <summary>
        /// Permafrost deposits (kilotons)
        /// Property: permafrostDeposits (READ-ONLY)
        /// </summary>
        public float PermafrostDeposits => _nativePlanet.InvokeMethod<float>("get_permafrostDeposits");

        // ==================== HABITABILITY ====================

        /// <summary>
        /// Check if atmosphere is breathable (O2 > 15%, pressure > 50kPa)
        /// </summary>
        public bool IsBreathable
        {
            get
            {
                _composition.UpdatePercentages();
                var o2 = _composition["O2"];
                return o2 != null && o2.Percentage >= 15f && TotalPressure >= 50f;
            }
        }

        /// <summary>
        /// Check if temperature is habitable (273-310K = 0-37°C)
        /// </summary>
        public bool IsTemperatureHabitable => Temperature >= 273f && Temperature <= 310f;

        /// <summary>
        /// Check if pressure is habitable (50-150kPa)
        /// </summary>
        public bool IsPressureHabitable => TotalPressure >= 50f && TotalPressure <= 150f;

        /// <summary>
        /// Overall habitability score (0-100%)
        /// </summary>
        public float HabitabilityScore
        {
            get
            {
                _composition.UpdatePercentages();
                var o2 = _composition["O2"];
                float o2Percentage = o2?.Percentage ?? 0f;

                float score = 0f;

                // Temperature contribution (33%)
                if (IsTemperatureHabitable) score += 33f;
                else if (Temperature >= 250f && Temperature <= 330f) score += 16f;

                // Pressure contribution (33%)
                if (IsPressureHabitable) score += 33f;
                else if (TotalPressure >= 20f) score += 16f;

                // Oxygen contribution (34%)
                if (o2Percentage >= 15f) score += 34f;
                else if (o2Percentage >= 5f) score += 17f;

                return score;
            }
        }

        public float TemperatureCelcius => Temperature+273.15f;


        // ==================== MODABLE RESOURCE MANAGEMENT ====================

        /// <summary>
        /// 🎯 MODABLE: Modify atmospheric gas by ResourceType ID
        /// Supports both core gases (CO2, O2) and mod-added gases
        /// </summary>
        /// <param name="gasId">Gas identifier (CO2, O2, N2, or mod ResourceType ID)</param>
        /// <param name="amount">Amount to add (positive) or remove (negative)</param>
        /// <returns>Actual amount changed</returns>
        public float ModifyGas(string gasId, float amount)
        {
            try
            {
                // Route to appropriate Planet method based on gas type
                switch (gasId.ToUpper())
                {
                    case "CO2":
                        return amount > 0 
                            ? _nativePlanet.InvokeMethod<float>("IncreaseCO2", Math.Abs(amount))
                            : _nativePlanet.InvokeMethod<float>("DecreaseCO2", Math.Abs(amount));
                    
                    case "O2":
                        return amount > 0
                            ? _nativePlanet.InvokeMethod<float>("IncreaseO2", Math.Abs(amount))
                            : _nativePlanet.InvokeMethod<float>("DecreaseO2", Math.Abs(amount));
                    
                    case "N2":
                        if (amount > 0)
                            _nativePlanet.InvokeMethod("IncreaseN2", Math.Abs(amount));
                        else
                            _nativePlanet.InvokeMethod("DecreaseN2", Math.Abs(amount));
                        return Math.Abs(amount); // N2 methods return void
                    
                    case "GHG":
                        if (amount > 0)
                            _nativePlanet.InvokeMethod("IncreaseGHG", Math.Abs(amount));
                        else
                            _nativePlanet.InvokeMethod("DecreaseGHG", Math.Abs(amount));
                        return Math.Abs(amount); // GHG methods return void
                    
                    default:
                        // TODO: Handle mod-added atmospheric ResourceType
                        throw new NotSupportedException($"Atmospheric gas '{gasId}' modification not yet implemented");
                }
            }
            catch (Exception)
            {
                return 0f; // Failed to modify
            }
        }
        
        /// <summary>
        /// 🎯 MODABLE: Convert one gas to another (for atmospheric chemistry mods)
        /// </summary>
        /// <param name="fromGas">Source gas to convert</param>
        /// <param name="toGas">Target gas to produce</param>
        /// <param name="amount">Amount to convert</param>
        /// <returns>Actual amount converted</returns>
        public float ConvertGas(string fromGas, string toGas, float amount)
        {
            // Core conversions (hardcoded in Planet)
            if (fromGas == "CO2" && toGas == "O2")
                return _nativePlanet.InvokeMethod<float>("ConvertCO2IntoO2", amount);
            
            if (fromGas == "O2" && toGas == "CO2")
                return _nativePlanet.InvokeMethod<float>("ConvertO2IntoCO2", amount);
            
            // TODO: Support mod-added gas conversions
            throw new NotSupportedException($"Conversion {fromGas} -> {toGas} not yet implemented");
        }
        
        /// <summary>
        /// Transfer atmospheric gas to a building via cargo system
        /// </summary>
        public bool TransferGasToBuilding(string gasSymbol, Building targetBuilding, float amount)
        {
            return _composition.TransferGasToBuilding(gasSymbol, targetBuilding, amount);
        }

        /// <summary>
        /// Extract gas from building back to atmosphere via cargo system
        /// </summary>
        public bool ExtractGasFromBuilding(string gasSymbol, Building sourceBuilding, float amount)
        {
            return _composition.ExtractGasFromBuilding(gasSymbol, sourceBuilding, amount);
        }

        /// <summary>
        /// Check if a building can accept a specific cargo type
        /// </summary>
        public static bool CanAcceptCargo(Building building, Cargo cargo)
        {
            if (building == null || cargo == null) return false;

            try
            {
                // Check if building has capacity for this resource type
                var resourceType = cargo.resource;
                if (resourceType == null) return false;

                // Check current capacity vs incoming capacity
                var currentIncoming = building.droneAccounting.capacityIncoming;
                var currentDocked = building.droneAccounting.capacityDocked;

                // Buildings can accept cargo if they have available capacity
                // This is a simplified check - actual logic would depend on building type
                return currentIncoming.ToFloat() > 0 || currentDocked.ToFloat() > 0;
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error checking if building can accept cargo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Make a building accept cargo (add to building's cargo storage)
        /// </summary>
        public static bool AcceptCargo(Building building, Cargo cargo)
        {
            if (building == null || cargo == null) return false;

            try
            {
                // In the actual game, cargo acceptance would involve:
                // 1. Checking building capacity
                // 2. Adding cargo to building's storage
                // 3. Updating building state

                // For now, we'll simulate this by calling the building's OnCargoDelivered method
                // This is the closest thing to cargo acceptance in the native Building class
                building.OnCargoDelivered(cargo);

                LogAspera.LogInfo($"Building {building.buildingType?.name} accepted cargo: {cargo._resource?.name} x{cargo._quantity.ToFloat()}");
                return true;
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error accepting cargo in building: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove cargo from a building (extract from building's storage)
        /// </summary>
        public static bool RemoveCargo(Building building, Cargo cargo, float amount)
        {
            if (building == null || cargo == null || amount <= 0) return false;

            try
            {
                // In the actual game, cargo removal would involve:
                // 1. Finding the cargo in building's storage
                // 2. Reducing the quantity
                // 3. Updating building state

                // For now, we'll simulate this by reducing cargo quantity
                // This is a simplified implementation
                if (cargo._quantity.ToFloat() >= amount)
                {
                    cargo._quantity = CargoQuantity.FromUnitFloat(cargo._quantity.ToFloat() - amount);
                    LogAspera.LogInfo($"Removed {amount} {cargo._resource?.name} from building {building.buildingType?.name}");
                    return true;
                }
                else
                {
                    LogAspera.LogWarning($"Building doesn't have enough {cargo._resource?.name} to remove {amount}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error removing cargo from building: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Find cargo in a building by resource type
        /// </summary>
        public static Cargo FindCargoByResource(Building building, ResourceType resourceType)
        {
            if (building == null || resourceType == null) return null;

            try
            {
                // Buildings don't have a direct cargo storage in the native class
                // This would need to be implemented based on how the game actually stores cargo
                // For now, return null as buildings don't directly store cargo in this simplified model
                return null;
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error finding cargo in building: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get quantity of atmospheric gas by resource key
        /// </summary>
        public float GetGasQuantity(string resourceKey)
        {
            return _composition.GetGasQuantity(resourceKey);
        }

        public override string ToString()
        {
            _composition.UpdatePercentages();
            return $"Atmosphere [Temp:{TemperatureCelsius:F1}°C, Pressure:{TotalPressure:F2}kPa, {_composition}, Habitable:{HabitabilityScore:F0}%]";
        }

        public void ModifyTemperature(float suggestedTemperatureBoost, float effectDuration, string v)
        {
            throw new NotImplementedException();
        }

        public void ModifyPressure(float pressureChange, float duration, string source)
        {
            throw new NotImplementedException();
        }

        public void ModifyGas(string v, float oxygenChange, float duration, string source)
        {
            throw new NotImplementedException();
        }

        //------------------------------------------------------
        // CARGO INTERACTION METHODS (STATIC UTILITIES)
        //------------------------------------------------------

        /// <summary>
        /// Check if a building can accept a specific cargo
        /// </summary>
        /// <param name="building">Target building object</param>
        /// <param name="cargo">Cargo to check</param>
        /// <returns>True if building can accept the cargo</returns>
        public static bool CanAcceptCargo(object building, Cargo cargo)
        {
            if (building == null || cargo == null) return false;

            try
            {
                // Use reflection to check if building has cargo acceptance methods
                var buildingType = building.GetType();
                var canAcceptMethod = buildingType.GetMethod("CanAcceptCargo", new[] { typeof(Cargo) });
                if (canAcceptMethod != null)
                {
                    return (bool)canAcceptMethod.Invoke(building, new object[] { cargo });
                }

                // Fallback: assume building can accept cargo if it has storage
                var storageProp = buildingType.GetProperty("cargoStorage");
                return storageProp != null;
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error checking cargo acceptance: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Make a building accept a cargo
        /// </summary>
        /// <param name="building">Target building object</param>
        /// <param name="cargo">Cargo to accept</param>
        /// <returns>True if cargo was accepted successfully</returns>
        public static bool AcceptCargo(object building, Cargo cargo)
        {
            if (building == null || cargo == null) return false;

            try
            {
                // Use reflection to call AcceptCargo method
                var buildingType = building.GetType();
                var acceptMethod = buildingType.GetMethod("AcceptCargo", new[] { typeof(Cargo) });
                if (acceptMethod != null)
                {
                    return (bool)acceptMethod.Invoke(building, new object[] { cargo });
                }

                // Fallback: try to add to cargo storage directly
                var storageProp = buildingType.GetProperty("cargoStorage");
                if (storageProp != null)
                {
                    var storage = storageProp.GetValue(building);
                    if (storage != null)
                    {
                        var addMethod = storage.GetType().GetMethod("Add");
                        if (addMethod != null)
                        {
                            addMethod.Invoke(storage, new object[] { cargo });
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Error accepting cargo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Find cargo by resource type
        /// </summary>
        /// <param name="resource">Resource type to search for</param>
        /// <returns>Cargo object if found, null otherwise</returns>
        public static Cargo? FindCargoByResource(object resource)
        {
            // This is a placeholder - would need to search through available cargo
            // For now, return null as this needs more context about where to search
            LogAspera.LogWarning("FindCargoByResource not implemented - needs cargo storage context");
            return null;
        }

        /// <summary>
        /// Remove a cargo from storage
        /// </summary>
        /// <param name="cargo">Cargo to remove</param>
        /// <returns>True if cargo was removed successfully</returns>
        public static bool RemoveCargo(Cargo cargo)
        {
            if (cargo == null) return false;

            // This is a placeholder - would need to know which storage to remove from
            // For now, return false as this needs more context
            LogAspera.LogWarning("RemoveCargo not implemented - needs storage context");
            return false;
        }
    }
}
