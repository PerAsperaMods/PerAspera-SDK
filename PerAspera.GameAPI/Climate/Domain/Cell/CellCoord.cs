using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Climate.Domain.Cell
{
    public struct CellCoord
    {
        public int LatIndex;
        public int LonIndex;

        public CellCoord(int latIndex, int lonIndex)
        {
            LatIndex = latIndex;
            LonIndex = lonIndex;
        }
    }

}
#pragma warning restore CS1591
