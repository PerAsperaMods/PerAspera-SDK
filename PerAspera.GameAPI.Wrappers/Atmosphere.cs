using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Represents a single atmospheric gas component
    /// </summary>
    public class AtmosphericGas
    {
        private readonly object _nativePlanet;
        private readonly string _getterMethod;
        private readonly string? _setterMethod;
        
        public string Name { get; }
        public string Symbol { get; }
        
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
    /// Collection of atmospheric gases with dynamic composition
    /// </summary>
    public class AtmosphericComposition
    {
        private readonly Dictionary<string, AtmosphericGas> _gases;
        private readonly Func<float> _getTotalPressure;
        
        internal AtmosphericComposition(Dictionary<string, AtmosphericGas> gases, Func<float> getTotalPressure)
        {
            _gases = gases;
            _getTotalPressure = getTotalPressure;
        }
        
        /// <summary>
        /// Get gas by symbol (CO2, O2, N2, H2O)
        /// </summary>
        public AtmosphericGas? this[string symbol] => _gases.TryGetValue(symbol, out var gas) ? gas : null;
        
        /// <summary>
        /// All atmospheric gases
        /// </summary>
        public IEnumerable<AtmosphericGas> AllGases => _gases.Values;
        
        /// <summary>
        /// Update percentages based on total pressure
        /// </summary>
        internal void UpdatePercentages()
        {
            var total = _getTotalPressure();
            if (total <= 0) return;
            
            foreach (var gas in _gases.Values)
            {
                gas.Percentage = (gas.PartialPressure / total) * 100f;
            }
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
        
        internal Atmosphere(object nativePlanet)
        {
            _nativePlanet = nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet));
            
            // Initialize gas composition
            var gases = new Dictionary<string, AtmosphericGas>
            {
                ["CO2"] = new AtmosphericGas(_nativePlanet, "Carbon Dioxide", "CO2", "get_CO2Pressure", "set_CO2Pressure"),
                ["O2"] = new AtmosphericGas(_nativePlanet, "Oxygen", "O2", "get_O2Pressure", "set_O2Pressure"),
                ["N2"] = new AtmosphericGas(_nativePlanet, "Nitrogen", "N2", "get_N2Pressure", "set_N2Pressure"),
                ["H2O"] = new AtmosphericGas(_nativePlanet, "Water Vapor", "H2O", "get_waterVaporPressure", "set_waterVaporPressure")
            };
            
            _composition = new AtmosphericComposition(gases, () => TotalPressure);
            
            // Initialize terraforming effects
            _effects = new Dictionary<string, TerraformingEffect>
            {
                ["PolarNuke"] = new TerraformingEffect(_nativePlanet, "Polar Nuclear", "Temperature effect from polar ice cap nukes", "get_polarNukeEffect"),
                ["PolarDust"] = new TerraformingEffect(_nativePlanet, "Polar Dust", "Temperature effect from polar dust reduction", "get_polarDustEffect"),
                ["Comet"] = new TerraformingEffect(_nativePlanet, "Comet Impact", "Temperature effect from comet impacts", "get_cometEffect"),
                ["Deimos"] = new TerraformingEffect(_nativePlanet, "Deimos Crash", "Temperature effect from Deimos moon crash", "get_deimosEffect")
            };
        }
        
        // ==================== COMPOSITION ====================
        
        /// <summary>
        /// Atmospheric gas composition (CO2, O2, N2, H2O)
        /// Access via: Atmosphere.Composition["CO2"].PartialPressure
        /// </summary>
        public AtmosphericComposition Composition => _composition;
        
        // ==================== PRESSURE ====================
        
        /// <summary>
        /// Total atmospheric pressure (sum of all partial pressures, kPa)
        /// Property: totalPressure (READ-ONLY - calculated by game)
        /// </summary>
        public float TotalPressure => _nativePlanet.InvokeMethod<float>("get_totalPressure");
        
        // ==================== TEMPERATURE ====================
        
        /// <summary>
        /// Current surface temperature (Kelvin)
        /// Property: temperature
        /// </summary>
        public float Temperature
        {
            get => _nativePlanet.InvokeMethod<float>("get_temperature");
            set => _nativePlanet.InvokeMethod("set_temperature", value);
        }
        
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
        /// Property: waterStock
        /// </summary>
        public float WaterStock
        {
            get => _nativePlanet.InvokeMethod<float>("get_waterStock");
            set => _nativePlanet.InvokeMethod("set_waterStock", value);
        }
        
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
        
        public override string ToString()
        {
            _composition.UpdatePercentages();
            return $"Atmosphere [Temp:{TemperatureCelsius:F1}°C, Pressure:{TotalPressure:F2}kPa, {_composition}, Habitable:{HabitabilityScore:F0}%]";
        }
    }
}
