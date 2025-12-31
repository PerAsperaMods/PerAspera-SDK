using System.Collections.Generic;
using PerAspera.GameAPI.Climate.Domain.Cell;
using PerAspera.GameAPI.Wrappers;
using System.Linq;



namespace PerAspera.GameAPI.Climate
{
    public static class CellDefinition
    {
        public const float LatSize = 5f;   // 5° par cellule
        public const float LonSize = 5f;   // 5° par cellule
    }

    public class AtmosphereGrid
    {
        Dictionary<CellCoord, AtmosphereCell> cells;
        HashSet<AtmosphereCell> _activeCells;
        object _nativePlanet; // Référence directe à la planète native

        /// <summary>
        /// Toutes les cellules de la grille (pour compatibilité avec les tests)
        /// </summary>
        public Dictionary<CellCoord, AtmosphereCell> Cells => cells;

        public AtmosphereGrid(object nativePlanet)
        {
            cells = new Dictionary<CellCoord, AtmosphereCell>();
            _activeCells = new HashSet<AtmosphereCell>();
            _nativePlanet = nativePlanet;
        }

        /// <summary>
        /// Constructeur pour les tests (sans planète native)
        /// </summary>
        public AtmosphereGrid()
        {
            cells = new Dictionary<CellCoord, AtmosphereCell>();
            _activeCells = new HashSet<AtmosphereCell>();
            _nativePlanet = null;
        }

        public void Tick(float deltaDays)
        {
            foreach (var cell in _activeCells)
            {
                cell.Composition.Tick(deltaDays);
            }

            // Synchroniser les valeurs cellulaires avec les overrides natifs
            SyncWithNativePlanet();
        }

        /// <summary>
        /// Synchronise les valeurs cellulaires agrégées avec les overrides de la planète native
        /// </summary>
        private void SyncWithNativePlanet()
        {
            if (_activeCells.Count == 0) return;

            // Calculer les valeurs moyennes des cellules actives
            float avgTemperature = _activeCells.Count > 0 ? _activeCells.Select(c => c.Temperature).Average() : 288.15f;
            float totalPressure = _activeCells.Count > 0 ? _activeCells.Select(c => c.TotalPressure).Average() : 101.325f;

            // TODO: Calculer les pressions partielles des gaz depuis les cellules
            // Pour l'instant, utiliser des valeurs par défaut
            float co2Pressure = 0.0004f; // ~400ppm CO2
            float o2Pressure = 0.21f;    // 21% O2
            float n2Pressure = 0.78f;    // 78% N2
            float h2oPressure = 0.01f;   // 1% H2O

            // Appliquer les overrides directement sur la planète native
            Patches.PlanetClimatePatches.SetClimateValue(_nativePlanet, "temperature", avgTemperature);
            Patches.PlanetClimatePatches.SetClimateValue(_nativePlanet, "CO2Pressure", co2Pressure);
            Patches.PlanetClimatePatches.SetClimateValue(_nativePlanet, "O2Pressure", o2Pressure);
            Patches.PlanetClimatePatches.SetClimateValue(_nativePlanet, "N2Pressure", n2Pressure);
            Patches.PlanetClimatePatches.SetClimateValue(_nativePlanet, "waterVaporPressure", h2oPressure);
        }

        /// <summary>
        /// Active le contrôle climatique pour cette planète
        /// </summary>
        public void EnableClimateControl()
        {
            Patches.PlanetClimatePatches.EnableClimateControl(_nativePlanet);
        }

        /// <summary>
        /// Désactive le contrôle climatique pour cette planète
        /// </summary>
        public void DisableClimateControl()
        {
            Patches.PlanetClimatePatches.DisableClimateControl(_nativePlanet);
        }

        public void InitializeGrid()
        {
            int latCells = (int)(180 / CellDefinition.LatSize);
            int lonCells = (int)(360 / CellDefinition.LonSize);

            for (int lat = 0; lat < latCells; lat++)
            {
                for (int lon = 0; lon < lonCells; lon++)
                {
                    var coord = new CellCoord { LatIndex = lat, LonIndex = lon };
                    var cell = new AtmosphereCell(coord);
                    cells[coord] = cell;
                }
            }
        }
        public void ActivateCell(CellCoord coord)
        {
            if (cells.TryGetValue(coord, out var cell) && !_activeCells.Contains(cell))
            {
                cell.Activate();
                _activeCells.Add(cell);
            }
        }

        public void DeactivateCell(CellCoord coord)
        {
            if (cells.TryGetValue(coord, out var cell) && _activeCells.Contains(cell))
            {
                cell.Deactivate();
                _activeCells.Remove(cell);
            }
        }

        /// <summary>
        /// Obtient toutes les cellules atmosphériques actives
        /// Utilisé pour les calculs de graphiques de terraformation
        /// </summary>
        public HashSet<AtmosphereCell> GetActiveCells()
        {
            return new HashSet<AtmosphereCell>(_activeCells);
        }

        /// <summary>
        /// Obtient une cellule spécifique par coordonnées
        /// </summary>
        public AtmosphereCell GetCell(CellCoord coord)
        {
            return cells.TryGetValue(coord, out var cell) ? cell : null;
        }

        /// <summary>
        /// Obtient toutes les cellules dans une région géographique
        /// </summary>
        public IEnumerable<AtmosphereCell> GetCellsInRegion(float minLat, float maxLat, float minLon, float maxLon)
        {
            return _activeCells.Where(cell => 
            {
                float lat = (cell.Coord.LatIndex * CellDefinition.LatSize) - 90f;
                float lon = (cell.Coord.LonIndex * CellDefinition.LonSize) - 180f;
                return lat >= minLat && lat <= maxLat && lon >= minLon && lon <= maxLon;
            });
        }

        /// <summary>
        /// Calcule la température moyenne de toutes les cellules actives
        /// </summary>
        public float GetGlobalAverageTemperature()
        {
            return _activeCells.Any() ? _activeCells.Average(c => c.Temperature) : 288.15f;
        }

        /// <summary>
        /// Calcule la pression moyenne de toutes les cellules actives
        /// </summary>
        public float GetGlobalAveragePressure()
        {
            return _activeCells.Any() ? _activeCells.Average(c => c.TotalPressure) : 101.325f;
        }
    }
}