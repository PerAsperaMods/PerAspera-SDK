using PerAspera.GameAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Climate.Domain.Cell
{
    enum CellType
    {
        Normal,
        Pole,
        Equator
    }
    public class AtmosphereCell
    {
        public CellCoord Coord;
        public AtmosphericComposition Composition { get; }

        public bool IsActive { get; private set; }
        public float TemperatureBias;
        public float PressureBias;

        public AtmosphereCell(CellCoord coord)
        {
            Coord = coord;
            // TODO: Initialize with proper gas dictionary and pressure function
            Composition = new AtmosphericComposition(new Dictionary<string, AtmosphericGas>(), () => 101.325f, null);
        }

        // TODO: Implement cellular temperature and pressure properties
        public float Temperature => 288.15f; // Default 15°C in Kelvin
        public float TotalPressure => 101.325f; // Default 1 atm in kPa

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

    }
}
