using System;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Planet class
    /// Provides safe access to planetary properties and climate data
    /// </summary>
    public class Planet : WrapperBase
    {
        private Atmosphere? _atmosphere;
        
        public Planet(object nativePlanet) : base(nativePlanet)
        {
        }
        
        /// <summary>
        /// Get the current planet instance
        /// </summary>
        public static Planet? GetCurrent()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet != null ? new Planet(planet) : null;
        }
        
        // ==================== ATMOSPHERE ====================
        
        /// <summary>
        /// Planet atmosphere (composition, temperature, pressure, effects)
        /// Access via: Planet.Atmosphere.Composition["CO2"].PartialPressure
        /// </summary>
        public Atmosphere Atmosphere
        {
            get
            {
                if (_atmosphere == null && NativeObject != null)
                    _atmosphere = new Atmosphere(NativeObject);
                return _atmosphere ?? throw new InvalidOperationException("Planet not initialized");
            }
        }
        
        // ==================== RESOURCES ====================
        
        /// <summary>
        /// Water stock on the planet (direct access for convenience)
        /// </summary>
        public float WaterStock
        {
            get => SafeInvoke<float?>("GetWaterStock") ?? 0f;
            set => SafeInvokeVoid("SetWaterStock", value);
        }
        
        
        // ==================== RESOURCES ====================
        
        /// <summary>
        /// Get stock of a specific resource
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water", "resource_silicon")</param>
        /// <returns>Current stock amount or 0 if resource not found</returns>
        public float GetResourceStock(string resourceKey)
        {
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
            if (resourceType == null) return 0f;
            
            return SafeInvoke<float?>("GetResourceStock", resourceType) ?? 0f;
        }
        
        /// <summary>
        /// Add resource to planet stock
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water", "resource_silicon")</param>
        /// <param name="amount">Amount to add (can be negative to remove)</param>
        public void AddResource(string resourceKey, float amount)
        {
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
            if (resourceType == null) return;
            
            SafeInvokeVoid("AddResource", resourceType, amount);
        }
        
        // ==================== INFO ====================
        
        public override string ToString()
        {
            if (NativeObject == null || _atmosphere == null)
                return "Planet: Not initialized";
                
            return $"Planet: Temp={Atmosphere.Temperature:F1}K, Pressure={Atmosphere.TotalPressure:F2}kPa " +
                   $"(CO2:{Atmosphere.Composition["CO2"].PartialPressure:F2}, " +
                   $"O2:{Atmosphere.Composition["O2"].PartialPressure:F2}, " +
                   $"N2:{Atmosphere.Composition["N2"].PartialPressure:F2}), " +
                   $"Water:{WaterStock:F1}, Breathable:{Atmosphere.IsBreathable}";
        }
    }
}
