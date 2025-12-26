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
    internal class AtmosphereCell
    {
        public CellCoord Coord;
        public AtmosphericComposition Composition { get; }

        public bool IsActive { get; private set; }
        public float TemperatureBias;
        public float PressureBias;
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
