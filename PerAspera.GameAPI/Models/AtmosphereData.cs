using System.Collections.Generic;

namespace PerAspera.GameAPI.Models
{
    /// <summary>
    /// Detailed atmospheric composition data
    /// Provides gas-by-gas breakdown with ratios and analysis
    /// </summary>
    public class AtmosphereData
    {
        /// <summary>All gases with their partial pressures (kPa)</summary>
        public Dictionary<string, float> GasPressures { get; set; } = new();
        
        /// <summary>Total atmospheric pressure (atm)</summary>
        public float TotalPressure { get; set; }
        
        /// <summary>Martian sol when data was captured</summary>
        public int Sol { get; set; }
        
        /// <summary>Get pressure ratio for a specific gas (0-1)</summary>
        public float GetRatio(string gasSymbol)
        {
            if (!GasPressures.TryGetValue(gasSymbol, out var pressure))
                return 0f;
            
            return TotalPressure > 0 ? pressure / TotalPressure : 0f;
        }
        
        /// <summary>Get percentage for a specific gas (0-100)</summary>
        public float GetPercentage(string gasSymbol)
        {
            return GetRatio(gasSymbol) * 100f;
        }
        
        /// <summary>Is gas present in atmosphere?</summary>
        public bool HasGas(string gasSymbol)
        {
            return GasPressures.ContainsKey(gasSymbol) && GasPressures[gasSymbol] > 0f;
        }
        
        /// <summary>Get all greenhouse gases and their contribution</summary>
        public Dictionary<string, float> GetGreenhouseGases()
        {
            var greenhouse = new Dictionary<string, float>();
            
            // Primary greenhouse gases in Per Aspera
            if (HasGas("CO2")) greenhouse["CO2"] = GasPressures["CO2"];
            if (HasGas("GHG")) greenhouse["GHG"] = GasPressures["GHG"];
            if (HasGas("H2O")) greenhouse["H2O"] = GasPressures["H2O"];
            
            return greenhouse;
        }
        
        /// <summary>Calculate effective greenhouse warming potential</summary>
        public float CalculateGreenhouseEffect()
        {
            float effect = 0f;
            
            // CO2 - baseline greenhouse gas
            if (HasGas("CO2"))
                effect += GasPressures["CO2"] * 1.0f;
            
            // GHG - super-greenhouse (100x more potent)
            if (HasGas("GHG"))
                effect += GasPressures["GHG"] * 100f;
            
            // H2O - water vapor (2x more potent than CO2)
            if (HasGas("H2O"))
                effect += GasPressures["H2O"] * 2.0f;
            
            return effect;
        }
        
        public override string ToString()
        {
            return $"Atmosphere[Sol {Sol}]: {TotalPressure:F3}atm total, {GasPressures.Count} gases tracked";
        }
    }
}
