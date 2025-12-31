using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Climate.Simulation;
using PerAspera.GameAPI.Climate.Patches;
using PerAspera.GameAPI.Climate.Integration;
using PerAspera.GameAPI.Wrappers;
using System.Linq;

// Aliases pour éviter le conflit Unity.Atmosphere vs PerAspera.GameAPI.Climate.Atmosphere
using PlanetWrapped = PerAspera.GameAPI.Wrappers.PlanetWrapper;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Master controller for climate simulation integration with Per Aspera
    /// Manages bidirectional synchronization between simulation and game state
    /// Provides the requested bidirectional control over atmospheric values.
    /// 
    /// 📋 Enhanced Documentation: F:\ModPeraspera\SDK-Enhanced-Classes\Planet-Enhanced.md#atmosphere-api
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator (Climate expertise)
    /// 🌡️ User Guide: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/Climate.md
    /// 🔄 Integration: F:\ModPeraspera\SDK\PerAspera.GameAPI.Wrappers\PlanetaryAtmosphere.cs
    /// </summary>
    public class ClimateController
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Controller");
        
        private readonly ClimateSimulator _simulator;
        private readonly ClimateConfig _config;
        private readonly ResourceBasedClimate _resourceClimate;
        private TerraformingEffectsController? _terraformingController;
        private TerraformingGraphDataProvider? _graphDataProvider;
        private AtmosphereGrid? _atmosphereGrid;
        
        private PlanetWrapped? _planet;
        private bool _isActive = false;
        private bool _resourceBasedMode = false;
        private DateTime _lastUpdate = DateTime.Now;
        
        public ClimateController(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateGameBalanced();
            _simulator = new ClimateSimulator(_config);
            _resourceClimate = new ResourceBasedClimate();
            
            Log.Info("ClimateController initialized with bidirectional Harmony control + resource-based support");
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
            
            // Initialise le contrôleur d'effets de terraformation
            _terraformingController = new TerraformingEffectsController(planet);
            _terraformingController.EnableControl();
            
            // Initialise la grille atmosphérique cellulaire
            _atmosphereGrid = new AtmosphereGrid(planet.GetNativeObject());
            _atmosphereGrid.InitializeGrid();
            _atmosphereGrid.EnableClimateControl();
            
            // Initialise le fournisseur de données pour les graphiques
            _graphDataProvider = new TerraformingGraphDataProvider(_atmosphereGrid);
            _graphDataProvider.InitializeSampleData(); // Données d'exemple pour les tests
            
            // Enregistre les gaz atmosphériques étendus pour MoreResources
            RegisterExtendedAtmosphericGases();
            
            // Enregistre ce contrôleur pour les patches de graphiques
            TerraformingGraphPatches.RegisterClimateController(planet.GetNativeObject(), this);
            
            Log.Info("Climate control activated with cellular atmosphere grid + terraforming graphs");
        }
        
        /// <summary>
        /// Enregistre automatiquement les gaz atmosphériques étendus supportés par MoreResources
        /// Permet l'affichage dans les graphiques de terraformation
        /// </summary>
        private void RegisterExtendedAtmosphericGases()
        {
            if (_graphDataProvider == null) return;
            
            // Gaz nobles et autres gaz rares du mod MoreResources
            var extendedGases = new[]
            {
                ("CH4", "Méthane"),
                ("Ar", "Argon"),
                ("Ne", "Néon"),
                ("He", "Hélium"),
                ("Kr", "Krypton"),
                ("Xe", "Xénon"),
                ("H2S", "Sulfure d'hydrogène"),
                ("SO2", "Dioxyde de soufre"),
                ("NH3", "Ammoniac")
            };
            
            foreach (var (symbol, name) in extendedGases)
            {
                _graphDataProvider.RegisterAtmosphericGas(symbol, name, "mbar");
            }
            
            Log.Info($"Registered {extendedGases.Length} extended atmospheric gases for graphing");
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
                TerraformingGraphPatches.UnregisterClimateController(_planet.GetNativeObject());
            }
            
            _atmosphereGrid?.DisableClimateControl();
            _atmosphereGrid = null;
            _graphDataProvider = null;
            _terraformingController?.DisableControl();
            _terraformingController = null;
            
            _isActive = false;
            _planet = null;
            Log.Info("Climate control deactivated - cellular atmosphere disabled");
        }
        
        /// <summary>
        /// Accès au contrôleur d'effets de terraformation
        /// Permet d'ajouter des effets personnalisés (heatwaves, cold snaps, etc.)
        /// </summary>
        public TerraformingEffectsController? TerraformingEffects => _terraformingController;
        
        /// <summary>
        /// Accès au fournisseur de données pour les graphiques de terraformation
        /// Permet d'obtenir les données cellulaires pour alimenter les graphiques
        /// </summary>
        public TerraformingGraphDataProvider? GraphDataProvider => _graphDataProvider;
        
        /// <summary>
        /// Accès à la grille atmosphérique cellulaire
        /// Permet de manipuler directement les cellules atmosphériques
        /// </summary>
        public AtmosphereGrid? AtmosphereGrid => _atmosphereGrid;
        
        /// <summary>
        /// Update climate simulation and synchronize with game if active
        /// Called from mod's update loop
        /// </summary>
        public void UpdateClimate(float deltaTime)
        {
            if (!_isActive || _planet == null) return;
            
            try
            {
                // Update cellular atmosphere simulation
                _atmosphereGrid?.Tick(deltaTime / 86400f); // Convert seconds to days
                
                // Update graph data from cellular atmosphere
                _graphDataProvider?.UpdateGraphData();
                
                // Run legacy climate simulation (TODO: integrate with cellular)
                object atmosphere = null; // Placeholder until cellular integration is complete
                _simulator.SimulateStep(atmosphere, deltaTime);

                // Calculate metrics for monitoring
                var status = _simulator.GetClimateStatus(atmosphere);
                var activeCells = _atmosphereGrid?.GetActiveCells().Count ?? 0;

                Log.Debug($"Climate update: {status} | Active cells: {activeCells}");
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
                // TODO: Update for cellular atmosphere architecture
                // Example: Get current values from cellular simulation and boost O2 production
                // var atmosphere = _planet.Atmosphere;
                float currentCO2 = 0.6f; // Mars baseline - TODO: Get from cellular atmosphere
                float currentO2 = 0.01f; // TODO: Get from cellular atmosphere

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
            
            // TODO: Update for cellular atmosphere architecture
            object atmosphere = null; // Will be replaced with cellular atmosphere reference
            var status = _simulator.GetClimateStatus(atmosphere);
            
            return $"Climate Control: ACTIVE (Harmony) | {status} | {_terraformingController?.GetStatus() ?? "Terraforming: INACTIVE"}";
        }
        
        /// <summary>
        /// Get regional climate data for detailed analysis
        /// Returns polar regions and equatorial region with area-specific calculations
        /// </summary>
        public ClimateRegionData GetRegionalClimateData()
        {
            return _simulator.GetRegionalData();
        }
        
        /// <summary>
        /// Get specific polar region data by hemisphere
        /// </summary>
        /// <param name="isNorthern">true for northern pole, false for southern pole</param>
        public Pole GetPolarRegionData(bool isNorthern)
        {
            var regionalData = _simulator.GetRegionalData();
            return isNorthern ? regionalData.NorthPole : regionalData.SouthPole;
        }
        
        /// <summary>
        /// Get equatorial region climate data
        /// </summary>
        public EquatorialRegion GetEquatorialRegionData()
        {
            var regionalData = _simulator.GetRegionalData();
            return regionalData.EquatorialRegion;
        }
        
        /// <summary>
        /// Get global climate averages calculated from regional data
        /// </summary>
        public GlobalClimateAverages GetGlobalClimateAverages()
        {
            var regionalData = _simulator.GetRegionalData();
            return regionalData.GlobalAverages;
        }
        
        /// <summary>
        /// Get detailed climate simulation status with regional breakdowns
        /// </summary>
        public string GetDetailedClimateStatus()
        {
            if (!_isActive || _planet == null)
                return "Climate Control: INACTIVE";
            
            var regionalData = _simulator.GetRegionalData();
            // TODO: Update for cellular atmosphere architecture
            object atmosphere = null; // Will be replaced with cellular atmosphere reference

            return $"Climate Control: ACTIVE (Cellular Simulation)\n" +
                   $"Global: {regionalData.GlobalAverages.SurfaceTemperature:F1}K ({regionalData.GlobalAverages.SurfaceTemperature - 273.15f:F1}°C)\n" +
                   $"North Pole: {regionalData.NorthPole.SurfaceTemperature:F1}K (Ice: {regionalData.NorthPole.IceCapArea:F0}km²)\n" +
                   $"South Pole: {regionalData.SouthPole.SurfaceTemperature:F1}K (Ice: {regionalData.SouthPole.IceCapArea:F0}km²)\n" +
                   $"Equatorial: {regionalData.EquatorialRegion.SurfaceTemperature:F1}K (Humidity: {regionalData.EquatorialRegion.RelativeHumidity:P1})\n" +
                   $"Atmosphere: Cellular (pending implementation)";
        }
        
        /// <summary>
        /// Méthode de convenance pour ajouter rapidement un effet de terraformation
        /// Idéal pour les événements Twitch ou autres triggers externes
        /// </summary>
        /// <param name="effectName">Nom de l'effet</param>
        /// <param name="temperatureChange">Changement de température en Kelvin</param>
        /// <param name="source">Source de l'effet</param>
        public void AddTerraformingEffect(string effectName, float temperatureChange, string source = "External")
        {
            if (_terraformingController == null)
            {
                Log.Warning("Cannot add terraforming effect: terraforming controller not initialized");
                return;
            }
            
            _terraformingController.AddTempEffect(effectName, temperatureChange, source);
        }

        /// <summary>
        /// Enable resource-based climate mode
        /// Uses resource quantities to calculate atmospheric pressures
        /// </summary>
        public void EnableResourceBasedMode()
        {
            _resourceBasedMode = true;
            _resourceClimate.Initialize();
            Log.Info("🔧 Resource-based climate mode enabled");
        }

        /// <summary>
        /// Disable resource-based climate mode
        /// Returns to simulation-only mode
        /// </summary>
        public void DisableResourceBasedMode()
        {
            _resourceBasedMode = false;
            Log.Info("🔧 Resource-based climate mode disabled");
        }

        /// <summary>
        /// Get atmospheric pressure for a specific gas (resource-based or simulation)
        /// </summary>
        /// <param name="pressureField">Pressure field name</param>
        /// <returns>Pressure in kPa</returns>
        public float GetAtmosphericPressure(string pressureField)
        {
            if (_resourceBasedMode && _resourceClimate.IsInitialized)
            {
                return _resourceClimate.GetAtmosphericPressure(pressureField);
            }

            // TODO: Update for cellular atmosphere architecture
            // Fallback to simulation values - will be replaced with cellular access
            // var atmosphere = _planet?.Atmosphere;
            // if (atmosphere != null)
            // {
            //     var gases = atmosphere.Composition.AllGases;
            //     return pressureField switch
            //     {
            //         "co2Pressure" => gases.FirstOrDefault(g => g.Symbol == "CO2")?.PartialPressure ?? 0f,
            //         "o2Pressure" => gases.FirstOrDefault(g => g.Symbol == "O2")?.PartialPressure ?? 0f,
            //         "n2Pressure" => gases.FirstOrDefault(g => g.Symbol == "N2")?.PartialPressure ?? 0f,
            //         "h2oPressure" => gases.FirstOrDefault(g => g.Symbol == "H2O")?.PartialPressure ?? 0f,
            //         _ => 0f
            //     };
            // }

            return 0f;
        }

        /// <summary>
        /// Access to resource-based climate system
        /// </summary>
        public ResourceBasedClimate ResourceClimate => _resourceClimate;
        
        /// <summary>
        /// Obtient une donnée de graphique de terraformation par clé
        /// Utilisé par le système de terraformation pour alimenter les nouveaux graphiques cellulaires
        /// </summary>
        public float GetTerraformingGraphData(string dataKey)
        {
            return _graphDataProvider?.GetGraphData(dataKey) ?? 0f;
        }
        
        /// <summary>
        /// Vérifie si une donnée de graphique est disponible
        /// </summary>
        public bool HasTerraformingGraphData(string dataKey)
        {
            return _graphDataProvider?.HasGraphData(dataKey) ?? false;
        }
        
        /// <summary>
        /// Enregistre un nouveau gaz atmosphérique pour le tracking des graphiques
        /// Utilisé par les mods comme MoreResources
        /// </summary>
        public void RegisterAtmosphericGas(string gasSymbol, string displayName, string unit = "mbar")
        {
            _graphDataProvider?.RegisterAtmosphericGas(gasSymbol, displayName, unit);
        }
        
        /// <summary>
        /// Active une cellule atmosphérique à des coordonnées spécifiques
        /// Permet de contrôler quelles régions sont simulées
        /// </summary>
        public void ActivateAtmosphereCell(int latIndex, int lonIndex)
        {
            var coord = new Domain.Cell.CellCoord { LatIndex = latIndex, LonIndex = lonIndex };
            _atmosphereGrid?.ActivateCell(coord);
        }
        
        /// <summary>
        /// Désactive une cellule atmosphérique
        /// </summary>
        public void DeactivateAtmosphereCell(int latIndex, int lonIndex)
        {
            var coord = new Domain.Cell.CellCoord { LatIndex = latIndex, LonIndex = lonIndex };
            _atmosphereGrid?.DeactivateCell(coord);
        }
        
        /// <summary>
        /// Obtient le nombre de cellules atmosphériques actives
        /// </summary>
        public int GetActiveCellsCount()
        {
            return _atmosphereGrid?.GetActiveCells().Count ?? 0;
        }
        
        /// <summary>
        /// Obtient la température moyenne des cellules polaires nord
        /// </summary>
        public float GetNorthPoleTemperature()
        {
            if (_graphDataProvider?.HasGraphData("Temperature_NorthPole") == true)
                return _graphDataProvider.GetGraphData("Temperature_NorthPole");
            return 200f; // Fallback valeur par défaut Mars
        }
        
        /// <summary>
        /// Obtient la température moyenne des cellules polaires sud
        /// </summary>
        public float GetSouthPoleTemperature()
        {
            if (_graphDataProvider?.HasGraphData("Temperature_SouthPole") == true)
                return _graphDataProvider.GetGraphData("Temperature_SouthPole");
            return 195f; // Fallback valeur par défaut Mars
        }
        
        /// <summary>
        /// Obtient la température moyenne des cellules équatoriales
        /// </summary>
        public float GetEquatorTemperature()
        {
            if (_graphDataProvider?.HasGraphData("Temperature_Equator") == true)
                return _graphDataProvider.GetGraphData("Temperature_Equator");
            return 250f; // Fallback valeur par défaut Mars
        }
    }
}