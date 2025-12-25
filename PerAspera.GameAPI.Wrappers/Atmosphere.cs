using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.Core;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Represents a single atmospheric gas component
    /// 
    /// 🚨 SDK EXCLUSIVE - NOT IN VANILLA GAME CLASS
    /// 💫 Enhanced Feature: Complete atmospheric composition access
    /// 📋 Enhanced Documentation: F:\ModPeraspera\SDK-Enhanced-Classes\Planet-Enhanced.md#atmosphere-api
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 📊 Climate Module: F:\ModPeraspera\SDK\PerAspera.GameAPI.Climate\ClimateSnapshot.cs
    /// </summary>
    public class AtmosphericGas
    {
        private readonly object _nativePlanet;
        private readonly string _getterMethod;
        private readonly string? _setterMethod;
        
        public string Name { get; }
        public string Symbol { get; }

        ResourceTypeWrapper resourceType = null;

        internal AtmosphericGas(object nativePlanet, string name, string symbol, string getterMethod, string? setterMethod = null)
        {
            _nativePlanet = nativePlanet;
            Name = name;
            Symbol = symbol;
            _getterMethod = getterMethod;
            _setterMethod = setterMethod;
        }
        
        /// <summary>
        /// Partial pressure of this gas (kPa)
        /// </summary>
        public float PartialPressure
        {
            get => _nativePlanet.InvokeMethod<float>(_getterMethod);
            set
            {
                if (_setterMethod == null)
                    throw new InvalidOperationException($"{Name} pressure is read-only");
                _nativePlanet.InvokeMethod(_setterMethod, value);
            }
        }
        
        /// <summary>
        /// Percentage in total atmosphere (0-100%)
        /// </summary>
        public float Percentage { get; internal set; }
        
        /// <summary>
        /// Check if this gas can be modified
        /// </summary>
        public bool IsReadOnly => _setterMethod == null;
        
        public override string ToString() => $"{Symbol}: {PartialPressure:F2}kPa ({Percentage:F1}%)";
    }
    
    /// <summary>
    /// Represents a terraforming effect on climate
    /// </summary>
    public class TerraformingEffect
    {
        private readonly object _nativePlanet;
        private readonly string _getterMethod;
        
        public string Name { get; }
        public string Description { get; }
        
        internal TerraformingEffect(object nativePlanet, string name, string description, string getterMethod)
        {
            _nativePlanet = nativePlanet;
            Name = name;
            Description = description;
            _getterMethod = getterMethod;
        }
        
        /// <summary>
        /// Temperature effect in Kelvin
        /// </summary>
        public float TemperatureEffect => _nativePlanet.InvokeMethod<float>(_getterMethod);
        
        public override string ToString() => $"{Name}: {TemperatureEffect:+0.00;-0.00;0}K";
    }
    
    /// <summary>
    /// Collection of atmospheric gases with dynamic composition and cargo integration
    /// Provides access to atmospheric gases as Cargo objects for building interactions
    /// </summary>
    public class AtmosphericComposition
    {
        private readonly Dictionary<string, AtmosphericGas> _gases;
        private readonly Dictionary<string, Cargo> _gasCargos;
        private readonly Func<float> _getTotalPressure;
        private readonly object _nativePlanet;

        internal AtmosphericComposition(Dictionary<string, AtmosphericGas> gases, Func<float> getTotalPressure, object nativePlanet)
        {
            _gases = gases;
            _gasCargos = new Dictionary<string, Cargo>();
            _getTotalPressure = getTotalPressure;
            _nativePlanet = nativePlanet;

            // Initialize cargo representations for atmospheric gases
            InitializeGasCargos();
        }

        /// <summary>
        /// Initialize Cargo objects for each atmospheric gas to enable building interactions
        /// </summary>
        private void InitializeGasCargos()
        {
            foreach (var gasEntry in _gases)
            {
                var gas = gasEntry.Value;
                // Create a cargo representation for this gas
                // Note: This is a conceptual representation - actual cargo creation would need building context
                _gasCargos[gasEntry.Key] = CreateAtmosphericCargo(gas);
            }
        }

        /// <summary>
        /// Create a conceptual Cargo object representing atmospheric gas
        /// This allows atmospheric gases to interact with building mechanics
        /// </summary>
        private Cargo CreateAtmosphericCargo(AtmosphericGas gas)
        {
            // Find the corresponding ResourceType for this gas
            var resourceType = FindAtmosphericResourceType(gas.Symbol);
            if (resourceType == null) return null;

            // Create cargo with current gas pressure as quantity
            // Note: In a real implementation, this would be managed by the planet's atmosphere system
            var cargo = new Cargo();
            cargo._resource = resourceType;
            cargo._quantity = CargoQuantity.FromUnitFloat(gas.PartialPressure);

            return cargo;
        }

        /// <summary>
        /// Find ResourceType corresponding to atmospheric gas symbol
        /// </summary>
        private ResourceType FindAtmosphericResourceType(string symbol)
        {
            // Map gas symbols to resource type constants
            var resourceKey = symbol switch
            {
                "CO2" => "resource_carbon_dioxide_release",
                "O2" => "resource_oxygen_release",
                "N2" => "resource_nitrogen_release",
                "GHG" => "resource_ghg_release",
                "H2O" => "resource_water", // Water vapor maps to water resource
                _ => null
            };

            if (resourceKey != null)
            {
                return (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
            }

            return null;
        }

        /// <summary>
        /// Get gas by symbol (CO2, O2, N2, H2O)
        /// </summary>
        public AtmosphericGas? this[string symbol] => _gases.TryGetValue(symbol, out var gas) ? gas : null;

        /// <summary>
        /// Get cargo representation of atmospheric gas for building interactions
        /// </summary>
        public Cargo? GetGasCargo(string symbol) => _gasCargos.TryGetValue(symbol, out var cargo) ? cargo : null;

        /// <summary>
        /// All atmospheric gases
        /// </summary>
        public IEnumerable<AtmosphericGas> AllGases => _gases.Values;

        /// <summary>
        /// All atmospheric gas cargos for building interactions
        /// </summary>
        public IEnumerable<Cargo> AllGasCargos => _gasCargos.Values.Where(c => c != null);

        /// <summary>
        /// Update percentages based on total pressure and sync cargo quantities
        /// </summary>
        internal void UpdatePercentages()
        {
            var total = _getTotalPressure();
            if (total <= 0) return;

            foreach (var gas in _gases.Values)
            {
                gas.Percentage = (gas.PartialPressure / total) * 100f;
            }

            // Sync cargo quantities with current gas pressures
            SyncCargoQuantities();
        }

        /// <summary>
        /// Sync cargo quantities with current atmospheric gas pressures
        /// This enables buildings to interact with atmospheric gases through cargo system
        /// </summary>
        private void SyncCargoQuantities()
        {
            foreach (var gasEntry in _gases)
            {
                var cargo = _gasCargos[gasEntry.Key];
                if (cargo != null)
                {
                    // Update cargo quantity to match current gas pressure
                    cargo._quantity = CargoQuantity.FromUnitFloat(gasEntry.Value.PartialPressure);
                }
            }
        }

        /// <summary>
        /// Transfer gas between buildings via cargo system
        /// </summary>
        public bool TransferGasToBuilding(string gasSymbol, Building targetBuilding, float amount)
        {
            var cargo = GetGasCargo(gasSymbol);
            if (cargo == null || targetBuilding == null) return false;

            try
            {
                // Check if building can accept this cargo
                if (!Atmosphere.CanAcceptCargo(targetBuilding, cargo))
                {
                    LogAspera.LogWarning($"Building cannot accept {gasSymbol} cargo");
                    return false;
                }

                // Check if we have enough gas in atmosphere
                var gas = this[gasSymbol];
                if (gas == null || gas.PartialPressure < amount)
                {
                    LogAspera.LogWarning($"Insufficient {gasSymbol} in atmosphere ({gas?.PartialPressure ?? 0} < {amount})");
                    return false;
                }

                // Create a cargo object with the transfer amount
                var transferCargo = new Cargo();
                transferCargo._resource = cargo.resource;
                transferCargo._quantity = CargoQuantity.FromUnitFloat(amount);

                // Transfer cargo to building
                if (Atmosphere.AcceptCargo(targetBuilding, transferCargo))
                {
                    // Reduce atmospheric gas pressure
                    gas.PartialPressure -= amount;
                    LogAspera.LogInfo($"Transferred {amount} {gasSymbol} from atmosphere to building");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Failed to transfer {gasSymbol} to building: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Extract gas from building back to atmosphere
        /// </summary>
        public bool ExtractGasFromBuilding(string gasSymbol, Building sourceBuilding, float amount)
        {
            if (sourceBuilding == null) return false;

            try
            {
                // Find cargo in building
                var resourceKey = GetResourceKeyForGas(gasSymbol);
                if (resourceKey == null) return false;

                var resourceType = (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
                var cargo = Atmosphere.FindCargoByResource(sourceBuilding, resourceType);

                if (cargo == null || cargo.quantity.ToFloat() < amount)
                {
                    LogAspera.LogWarning($"Building doesn't have enough {gasSymbol} to extract ({cargo?.quantity.ToFloat() ?? 0} < {amount})");
                    return false;
                }

                // Remove cargo from building
                if (Atmosphere.RemoveCargo(sourceBuilding, cargo, amount))
                {
                    // Increase atmospheric gas pressure
                    var gas = this[gasSymbol];
                    if (gas != null)
                    {
                        gas.PartialPressure += amount;
                        LogAspera.LogInfo($"Extracted {amount} {gasSymbol} from building to atmosphere");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Failed to extract {gasSymbol} from building: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Get resource key for gas symbol
        /// </summary>
        private string GetResourceKeyForGas(string gasSymbol)
        {
            return gasSymbol switch
            {
                "CO2" => "resource_carbon_dioxide_release",
                "O2" => "resource_oxygen_release",
                "N2" => "resource_nitrogen_release",
                "GHG" => "resource_ghg_release",
                "H2O" => "resource_water",
                _ => null
            };
        }

        /// <summary>
        /// Get quantity of atmospheric gas by resource key
        /// </summary>
        public float GetGasQuantity(string resourceKey)
        {
            var resourceType = (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
            if (resourceType == null || !_gasCargos.TryGetValue(GetGasSymbolForResource(resourceKey), out var cargo)) return 0f;
            return cargo?._quantity.ToFloat() ?? 0f;
        }

        /// <summary>
        /// Get gas symbol for resource key
        /// </summary>
        private string GetGasSymbolForResource(string resourceKey)
        {
            return resourceKey switch
            {
                "resource_carbon_dioxide_release" => "CO2",
                "resource_oxygen_release" => "O2",
                "resource_nitrogen_release" => "N2",
                "resource_ghg_release" => "GHG",
                "resource_water" => "H2O",
                _ => null
            };
        }

        public override string ToString()
        {
            UpdatePercentages();
            return string.Join(", ", _gases.Values.Select(g => $"{g.Symbol}:{g.Percentage:F1}%"));
        }
    }

    /// <summary>
    /// Wrapper for Planet atmosphere properties
    /// Provides type-safe access to atmospheric composition and climate data
    /// </summary>
    public class Atmosphere
    {
        private readonly object _nativePlanet;
        private readonly AtmosphericComposition _composition;
        private readonly Dictionary<string, TerraformingEffect> _effects;

        public Atmosphere(object nativePlanet)
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
    }
}
