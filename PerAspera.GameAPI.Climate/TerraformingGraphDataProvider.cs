using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.GameAPI.Climate.Domain.Cell;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Fournisseur de données pour les graphiques de terraformation
    /// Encapsule l'accès aux données atmosphériques pour les visualisations
    /// </summary>
    public class TerraformingGraphDataProvider
    {
        private readonly AtmosphereGrid _grid;
        private readonly Dictionary<string, object> _graphData;
        private readonly Dictionary<string, AtmosphericGas> _registeredGases;

        public TerraformingGraphDataProvider(AtmosphereGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _graphData = new Dictionary<string, object>();
            _registeredGases = new Dictionary<string, AtmosphericGas>();
        }

        /// <summary>
        /// Initialise des données d'exemple pour les tests
        /// </summary>
        public void InitializeSampleData()
        {
            // Initialiser la grille si nécessaire
            if (_grid.Cells.Count == 0)
            {
                _grid.InitializeGrid();
            }

            // Activer quelques cellules d'exemple
            _grid.ActivateCell(new CellCoord(18, 36)); // Équateur
            _grid.ActivateCell(new CellCoord(0, 36));  // Pôle nord
            _grid.ActivateCell(new CellCoord(35, 36)); // Pôle sud

            // Données de base pour les graphiques
            _graphData["ActiveCellsCount"] = _grid.GetActiveCells().Count;
            _graphData["GlobalTemperature"] = _grid.GetGlobalAverageTemperature();
            _graphData["GlobalPressure"] = _grid.GetGlobalAveragePressure();

            // Gaz atmosphériques par défaut
            RegisterAtmosphericGas("CO2", "Dioxyde de carbone", "ppm");
            RegisterAtmosphericGas("O2", "Oxygène", "%");
            RegisterAtmosphericGas("N2", "Azote", "%");
            RegisterAtmosphericGas("H2O", "Vapeur d'eau", "%");
        }

        /// <summary>
        /// Enregistre un gaz atmosphérique pour les graphiques
        /// </summary>
        public void RegisterAtmosphericGas(string id, string name, string unit)
        {
            var gas = new AtmosphericGas(id, name, unit);
            _registeredGases[id] = gas;
            _graphData[$"{id} Pressure"] = 0.0f; // Valeur par défaut
        }

        /// <summary>
        /// Vérifie si des données de graphique existent pour une clé
        /// </summary>
        public bool HasGraphData(string key)
        {
            return _graphData.ContainsKey(key);
        }

        /// <summary>
        /// Récupère des données de graphique
        /// </summary>
        public float GetGraphData(string key)
        {
            if (_graphData.TryGetValue(key, out var value))
            {
                return Convert.ToSingle(value);
            }

            // Valeurs spéciales calculées
            switch (key)
            {
                case "ActiveCellsCount":
                    return _grid.GetActiveCells().Count;
                case "GlobalTemperature":
                    return _grid.GetGlobalAverageTemperature();
                case "GlobalPressure":
                    return _grid.GetGlobalAveragePressure();
                case "Temperature_NorthPole":
                    return GetRegionalAverageTemperature("north_pole");
                case "Temperature_SouthPole":
                    return GetRegionalAverageTemperature("south_pole");
                case "Temperature_Equator":
                    return GetRegionalAverageTemperature("equator");
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Met à jour les données de graphique
        /// </summary>
        public void UpdateGraphData(string key, float value)
        {
            _graphData[key] = value;
        }

        /// <summary>
        /// Met à jour les données de graphique avec un objet
        /// </summary>
        public void UpdateGraphData(string key, object value)
        {
            _graphData[key] = value;
        }

        /// <summary>
        /// Met à jour toutes les données de graphique depuis la grille
        /// </summary>
        public void UpdateGraphData()
        {
            // Mettre à jour les données calculées depuis la grille
            _graphData["ActiveCellsCount"] = _grid.GetActiveCells().Count;
            _graphData["GlobalTemperature"] = _grid.GetGlobalAverageTemperature();
            _graphData["GlobalPressure"] = _grid.GetGlobalAveragePressure();

            // Mettre à jour les températures régionales
            _graphData["Temperature_NorthPole"] = GetRegionalAverageTemperature("north_pole");
            _graphData["Temperature_SouthPole"] = GetRegionalAverageTemperature("south_pole");
            _graphData["Temperature_Equator"] = GetRegionalAverageTemperature("equator");
        }

        /// <summary>
        /// Calcule la température moyenne d'une région
        /// </summary>
        public float GetRegionalAverageTemperature(string region)
        {
            var cells = GetCellsForRegion(region);
            if (!cells.Any()) return 288.15f; // Température par défaut

            return cells.Average(c => c.Temperature);
        }

        /// <summary>
        /// Obtient les cellules pour une région donnée
        /// </summary>
        private IEnumerable<AtmosphereCell> GetCellsForRegion(string region)
        {
            switch (region.ToLower())
            {
                case "north_pole":
                    return _grid.GetCellsInRegion(75, 90, -180, 180);
                case "south_pole":
                    return _grid.GetCellsInRegion(-90, -75, -180, 180);
                case "equator":
                    return _grid.GetCellsInRegion(-5, 5, -180, 180);
                default:
                    return _grid.GetActiveCells();
            }
        }

        /// <summary>
        /// Classe interne pour représenter un gaz atmosphérique
        /// </summary>
        private class AtmosphericGas
        {
            public string Id { get; }
            public string Name { get; }
            public string Unit { get; }

            public AtmosphericGas(string id, string name, string unit)
            {
                Id = id;
                Name = name;
                Unit = unit;
            }
        }
    }
}