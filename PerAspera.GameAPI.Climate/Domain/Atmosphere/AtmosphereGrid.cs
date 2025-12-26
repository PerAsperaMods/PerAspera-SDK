using Il2CppSystem.Collections.Generic;
using PerAspera.GameAPI.Climate.Domain.Cell;
using PerAspera.GameAPI.Wrappers;



namespace PerAspera.GameAPI.Climate
{
    public static class CellDefinition
    {
        public const float LatSize = 5f;   // 5° par cellule
        public const float LonSize = 5f;   // 5° par cellule
    }

    class AtmosphereGrid
    {
        Dictionary<CellCoord, AtmosphereCell> cells;
        HashSet<AtmosphereCell> _activeCells;
        PlanetWrapper _planet;

        public AtmosphereGrid(PlanetWrapper _P)
        {
            cells = new Dictionary<CellCoord, AtmosphereCell>();
            _activeCells = new HashSet<AtmosphereCell>();
            _planet = _P;
        }

        public void Tick(float deltaDays)
        {
            foreach (var cell in _activeCells)
            {
                cell.Composition.Tick(deltaDays);
            }
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
    }
}