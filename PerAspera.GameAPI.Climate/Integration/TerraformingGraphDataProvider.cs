using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Climate.Domain.Cell;

namespace PerAspera.GameAPI.Climate.Integration
{
    /// <summary>
    /// Intégration entre le système cellulaire atmosphérique et les graphiques de terraformation
    /// Fournit les données pour les nouveaux graphiques régionaux et gaz atmosphériques
    /// </summary>
    public class TerraformingGraphDataProvider
    {
        private static readonly LogAspera Log = new LogAspera("Climate.GraphData");
        
        private readonly AtmosphereGrid _atmosphereGrid;
        private readonly Dictionary<string, float> _graphData;
        
        public TerraformingGraphDataProvider(AtmosphereGrid atmosphereGrid)
        {
            _atmosphereGrid = atmosphereGrid;
            _graphData = new Dictionary<string, float>();
            
            Log.Info("TerraformingGraphDataProvider initialized for cellular atmosphere integration");
        }
        
        /// <summary>
        /// Met à jour toutes les données de graphique depuis les cellules atmosphériques
        /// Appelé depuis le système de terraformation pour alimenter les graphiques
        /// </summary>
        public void UpdateGraphData()
        {
            try
            {
                // Données de température régionale
                UpdateRegionalTemperatures();
                
                // Données de pression cellulaire
                UpdateCellularPressure();
                
                // Comptage des cellules actives
                UpdateActiveCellsCount();
                
                // Nouveaux gaz atmosphériques (MoreResources)
                UpdateModdedAtmosphericGases();
                
                // Distribution et variance cellulaire
                UpdateCellularDistribution();
                
                Log.Debug($"Graph data updated: {_graphData.Count} data points");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update graph data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Calcule les températures moyennes par région (pôles, équateur)
        /// Correspond aux nouveaux graphiques temp_north_pole_cellular, etc.
        /// </summary>
        private void UpdateRegionalTemperatures()
        {
            var activeCells = _atmosphereGrid.GetActiveCells();
            if (activeCells.Count == 0) return;
            
            // Classifier les cellules par région
            var northPoleCells = activeCells.Where(c => GetLatitude(c.Coord) > 75f).ToList();
            var southPoleCells = activeCells.Where(c => GetLatitude(c.Coord) < -75f).ToList();
            var equatorCells = activeCells.Where(c => Math.Abs(GetLatitude(c.Coord)) < 15f).ToList();
            
            // Calculer températures moyennes
            if (northPoleCells.Any())
                _graphData["Temperature_NorthPole"] = northPoleCells.Average(c => c.Temperature);
            
            if (southPoleCells.Any())
                _graphData["Temperature_SouthPole"] = southPoleCells.Average(c => c.Temperature);
            
            if (equatorCells.Any())
                _graphData["Temperature_Equator"] = equatorCells.Average(c => c.Temperature);
        }
        
        /// <summary>
        /// Calcule la pression moyenne des cellules actives
        /// Correspond au graphique pressure_active_cells
        /// </summary>
        private void UpdateCellularPressure()
        {
            var activeCells = _atmosphereGrid.GetActiveCells();
            if (activeCells.Any())
            {
                _graphData["Pressure_ActiveCells"] = activeCells.Average(c => c.TotalPressure);
            }
        }
        
        /// <summary>
        /// Compte le nombre de cellules atmosphériques actives
        /// Correspond au graphique active_cells_count
        /// </summary>
        private void UpdateActiveCellsCount()
        {
            _graphData["ActiveCellsCount"] = _atmosphereGrid.GetActiveCells().Count;
        }
        
        /// <summary>
        /// Met à jour les données des gaz atmosphériques ajoutés par les mods
        /// Support pour methane_atmospheric, argon_atmospheric, etc.
        /// </summary>
        private void UpdateModdedAtmosphericGases()
        {
            var activeCells = _atmosphereGrid.GetActiveCells();
            if (!activeCells.Any()) return;
            
            // Calculer pressions moyennes pour les nouveaux gaz
            _graphData["CH4 Pressure"] = CalculateAverageGasPressure(activeCells, "CH4");
            _graphData["Ar Pressure"] = CalculateAverageGasPressure(activeCells, "Ar");
            _graphData["Ne Pressure"] = CalculateAverageGasPressure(activeCells, "Ne");
            
            // Gaz spéciaux qui peuvent être ajoutés par MoreResources
            _graphData["He Pressure"] = CalculateAverageGasPressure(activeCells, "He");
            _graphData["Kr Pressure"] = CalculateAverageGasPressure(activeCells, "Kr");
            _graphData["Xe Pressure"] = CalculateAverageGasPressure(activeCells, "Xe");
        }
        
        /// <summary>
        /// Calcule la distribution et variance des gaz entre cellules
        /// Correspond aux graphiques o2_cellular_distribution, co2_cellular_hotspots
        /// </summary>
        private void UpdateCellularDistribution()
        {
            var activeCells = _atmosphereGrid.GetActiveCells();
            if (activeCells.Count < 2) return;
            
            // Variance O2 entre cellules
            var o2Pressures = activeCells.Select(c => GetGasPressure(c, "O2")).ToList();
            if (o2Pressures.Any())
            {
                var variance = (o2Pressures.Max() - o2Pressures.Min()) / o2Pressures.Average() * 100f;
                _graphData["o2_cellular_variance"] = variance;
            }
            
            // Hotspots CO2 (cellules avec CO2 > moyenne + 2σ)
            var co2Pressures = activeCells.Select(c => GetGasPressure(c, "CO2")).ToList();
            if (co2Pressures.Any())
            {
                var mean = co2Pressures.Average();
                var stdDev = (float)Math.Sqrt(co2Pressures.Average(p => Math.Pow(p - mean, 2)));
                var threshold = mean + 2 * stdDev;
                
                var hotspots = co2Pressures.Count(p => p > threshold);
                _graphData["co2_cellular_hotspots"] = hotspots;
            }
        }
        
        /// <summary>
        /// Obtient la donnée de graphique pour une clé spécifique
        /// Utilisé par le système de terraformation pour alimenter les graphiques
        /// </summary>
        public float GetGraphData(string dataKey)
        {
            return _graphData.TryGetValue(dataKey, out var value) ? value : 0f;
        }
        
        /// <summary>
        /// Vérifie si des données sont disponibles pour une clé
        /// </summary>
        public bool HasGraphData(string dataKey)
        {
            return _graphData.ContainsKey(dataKey);
        }
        
        // Méthodes utilitaires
        private float GetLatitude(CellCoord coord) => (coord.LatIndex * CellDefinition.LatSize) - 90f;
        private float GetLongitude(CellCoord coord) => (coord.LonIndex * CellDefinition.LonSize) - 180f;
        
        private float CalculateAverageGasPressure(IEnumerable<AtmosphereCell> cells, string gasSymbol)
        {
            var pressures = cells.Select(c => GetGasPressure(c, gasSymbol)).Where(p => p > 0f);
            return pressures.Any() ? pressures.Average() : 0f;
        }
        
        private float GetGasPressure(AtmosphereCell cell, string gasSymbol)
        {
            var gas = cell.Composition[gasSymbol];
            return gas?.PartialPressure ?? 0f;
        }
        
        /// <summary>
        /// Enregistre un nouveau type de gaz atmosphérique pour tracking
        /// Utilisé par les mods comme MoreResources pour ajouter des gaz
        /// </summary>
        public void RegisterAtmosphericGas(string gasSymbol, string displayName, string unit = "mbar")
        {
            // TODO: Intégrer avec le registre des types de ressources atmosphériques
            Log.Info($"Registered new atmospheric gas for graph tracking: {gasSymbol} ({displayName})");
        }
        
        /// <summary>
        /// Crée automatiquement des entrées YAML pour de nouveaux gaz
        /// Génère la configuration de graphique pour les mods
        /// </summary>
        public Dictionary<string, object> GenerateYAMLForGas(string gasSymbol, string displayName, 
            float minValue = 0f, float maxValue = 100f, string color = "FFFFFF")
        {
            return new Dictionary<string, object>
            {
                ["categoryType"] = "!terraforming_plan_category category_gases",
                ["dataKey"] = $"{gasSymbol} Pressure",
                ["criterionKey"] = $"pressure_{gasSymbol.ToLower()}",
                ["min"] = minValue,
                ["max"] = maxValue,
                ["position"] = 50 + gasSymbol.GetHashCode() % 100, // Auto position
                ["positionInCategory"] = 50,
                ["general"] = false,
                ["color"] = color,
                ["lineColor"] = GetDarkerColor(color),
                ["iconName"] = $"Terraforming Screen Icons/ICO_TerraPlan_{gasSymbol.ToLower()}",
                ["unit"] = "mbar",
                ["valueStringOffset"] = 0,
                ["derived"] = false,
                ["usewarnings"] = false,
                ["warningValue"] = 0,
                ["dangerValue"] = 0
            };
        }
        
        private string GetDarkerColor(string hexColor)
        {
            // Simplistic color darkening - could be more sophisticated
            return hexColor.Length == 6 ? hexColor.Substring(0, 4) + "00" : "000000";
        }
    }
}