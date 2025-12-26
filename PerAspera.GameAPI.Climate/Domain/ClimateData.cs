using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Climate.Domain
{
    /// <summary>
    /// Data structure containing all regional climate information
    /// Used for detailed climate analysis and monitoring
    /// </summary>
    public class ClimateRegionData
    {
        public Pole NorthernPole { get; set; }
        public Pole SouthernPole { get; set; }
        public EquatorialRegion EquatorialRegion { get; set; }
        public GlobalClimateAverages GlobalAverages { get; set; }

        public ClimateRegionData(Pole northernPole, Pole southernPole, EquatorialRegion equatorialRegion)
        {
            NorthernPole = northernPole;
            SouthernPole = southernPole;
            EquatorialRegion = equatorialRegion;
            GlobalAverages = new GlobalClimateAverages();
        }
    }

    /// <summary>
    /// Global climate averages calculated from regional data
    /// Provides area-weighted averages instead of simple means
    /// </summary>
    public class GlobalClimateAverages
    {
        // Temperature averages (Kelvin)
        public float SurfaceTemperature { get; set; }
        public float AtmosphericTemperature { get; set; }
        public float IceTemperature { get; set; }

        // Area-weighted properties
        public float AverageAlbedo { get; set; }
        public float TotalIceArea { get; set; } // km²
        public float TotalSurfaceArea { get; set; } // km²

        // Atmospheric properties
        public float AverageHumidity { get; set; }
        public float AverageWindSpeed { get; set; }

        public GlobalClimateAverages()
        {
            // Initialize with default values
            SurfaceTemperature = 273.15f; // 0°C
            AtmosphericTemperature = 268.15f; // -5°C
            IceTemperature = 263.15f; // -10°C
            AverageAlbedo = 0.3f;
            TotalIceArea = 0f;
            TotalSurfaceArea = 0f;
            AverageHumidity = 0.5f;
            AverageWindSpeed = 2.0f;
        }
    }
}