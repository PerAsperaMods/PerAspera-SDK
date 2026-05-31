using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using System.Linq;
using System.Linq;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Resource-based climate system that calculates atmospheric pressures from released resources
    /// Migrated from ClimateRework mod for better SDK integration
    /// </summary>
    public class ResourceBasedClimate
    {
        private static readonly LogAspera Log = new LogAspera("Climate.ResourceBased");

        // Mappings des ressources atmosphériques vers leurs clés de pression
        private static readonly Dictionary<string, string> AtmosphericResourceKeys = new()
        {
            { "co2Pressure", "resource_carbon_dioxide" },
            { "o2Pressure", "resource_oxygen" },
            { "n2Pressure", "resource_nitrogen" },
            { "h2oPressure", "resource_water_vapor_release" },
            { "ch4Pressure", "resource_methane" },
            { "so2Pressure", "resource_sulfur_dioxide_release" },
            { "n2oPressure", "resource_nitrous_oxide_release" },
            { "arPressure", "resource_argon_release" },
            { "h2Pressure", "resource_hydrogen" }
        };

        // Cache des ressources pour performance
        private readonly Dictionary<string, object> _atmosphericResources = new();
        private bool _initialized = false;

        /// <summary>
        /// Initialize resource-based climate system
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            Log.Info("🌡️ Initializing Resource-Based Climate System");

            // S'abonner aux événements du jeu
            EnhancedEventBus.Subscribe<GameFullyLoadedEvent>(OnGameFullyLoaded);
            
            _initialized = true;
            Log.Info("✅ Resource-Based Climate System initialized");
        }

        private void OnGameFullyLoaded(GameFullyLoadedEvent evt)
        {
            try
            {
                Log.Info("🌍 Game fully loaded - initializing resource-based climate...");

                // Initialiser le cache des ressources atmosphériques
                InitializeAtmosphericResources();

                Log.Info("✅ Resource-based climate system ready");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Resource-based climate initialization failed: {ex.Message}");
            }
        }

        private void InitializeAtmosphericResources()
        {
            Log.Info("🔍 Initializing atmospheric resource cache...");

            foreach (var (pressureField, resourceKey) in AtmosphericResourceKeys)
            {
                try
                {
                    // Utiliser le SDK pour obtenir la ressource
                    var resourceType = ResourceTypeWrapper.GetByKey(resourceKey);
                    if (resourceType != null)
                    {
                        _atmosphericResources[pressureField] = resourceType;
                        Log.Info($"✅ Mapped {pressureField} -> {resourceKey}");
                    }
                    else
                    {
                        Log.Warning($"⚠️ Resource not found: {resourceKey} for {pressureField}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"❌ Failed to map {pressureField} -> {resourceKey}: {ex.Message}");
                }
            }

            Log.Info($"📊 Atmospheric resources cache: {_atmosphericResources.Count} resources loaded");
        }

        /// <summary>
        /// Calculate atmospheric pressure for a specific gas based on released resources
        /// </summary>
        /// <param name="pressureField">Pressure field name (e.g., "co2Pressure")</param>
        /// <returns>Calculated pressure in kPa</returns>
        public float GetAtmosphericPressure(string pressureField)
        {
            if (_atmosphericResources.TryGetValue(pressureField, out var resource))
            {
                // TODO: Calculer la pression basée sur la quantité de ressource relâchée
                // Pour l'instant, utiliser les valeurs par défaut du SDK Climate
                return GetDefaultPressure(pressureField);
            }

            Log.Warning($"⚠️ No resource mapping for pressure field: {pressureField}");
            return 0f;
        }

        /// <summary>
        /// Set atmospheric pressure for a specific gas
        /// </summary>
        /// <param name="pressureField">Pressure field name</param>
        /// <param name="pressure">New pressure value in kPa</param>
        public void SetAtmosphericPressure(string pressureField, float pressure)
        {
            if (_atmosphericResources.ContainsKey(pressureField))
            {
                Log.Info($"🌡️ Setting {pressureField} pressure to {pressure:F2} kPa");
                // TODO: Implémenter la logique de modification de pression
                // via les mécanismes du SDK Climate existant
            }
            else
            {
                Log.Warning($"⚠️ Cannot set pressure for unmapped field: {pressureField}");
            }
        }

        /// <summary>
        /// Get default pressure values for atmospheric gases
        /// </summary>
        private float GetDefaultPressure(string pressureField)
        {
            return pressureField switch
            {
                "co2Pressure" => 0.636f,  // Mars baseline CO2 pressure
                "o2Pressure" => 0.001f,   // Trace oxygen
                "n2Pressure" => 0.027f,   // Small nitrogen component
                "h2oPressure" => 0.0003f, // Water vapor traces
                "arPressure" => 0.016f,   // Argon component
                _ => 0.0001f              // Trace gases
            };
        }

        /// <summary>
        /// Get all available atmospheric resource mappings
        /// </summary>
        public IReadOnlyDictionary<string, string> GetResourceMappings()
        {
            return AtmosphericResourceKeys;
        }

        /// <summary>
        /// Check if the resource-based climate system is active
        /// </summary>
        public bool IsInitialized => _initialized;
    }
}