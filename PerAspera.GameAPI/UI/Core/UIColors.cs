using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Standardized color palette for Per Aspera UI components.
    /// Matches the visual style of in-game panels (POI Browser, Stock, Demand &amp; Production).
    /// </summary>
    /// <example>
    /// var color = UIColors.Primary;  // Cyan for values
    /// var warning = UIColors.Warning;  // Orange for warnings
    /// </example>
    public static class UIColors
    {
        /// <summary>Primary value color (cyan) - used for resource quantities, main stats</summary>
        public static readonly Color Primary = new Color(0f, 1f, 1f, 1f);  // Cyan

        /// <summary>Secondary/neutral text (white) - used for labels and standard text</summary>
        public static readonly Color Secondary = new Color(1f, 1f, 1f, 1f);  // White

        /// <summary>Warning color (orange) - used for alerts and cautionary information</summary>
        public static readonly Color Warning = new Color(1f, 0.65f, 0f, 1f);  // Orange

        /// <summary>Error/critical color (red) - used for critical alerts and errors</summary>
        public static readonly Color Error = new Color(1f, 0.2f, 0.2f, 1f);  // Red

        /// <summary>Success color (green) - used for completed or positive states</summary>
        public static readonly Color Success = new Color(0.2f, 1f, 0.2f, 1f);  // Green

        /// <summary>Panel background color (dark blue-grey) - matches game panels</summary>
        public static readonly Color PanelBackground = new Color(0.25f, 0.35f, 0.4f, 0.95f);

        /// <summary>Panel border color (light cream/beige)</summary>
        public static readonly Color PanelBorder = new Color(0.85f, 0.82f, 0.75f, 1f);

        /// <summary>Disabled/inactive color (grey)</summary>
        public static readonly Color Disabled = new Color(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// Get color based on status (OK, Warning, Error).
        /// </summary>
        /// <param name="status">Status level: "OK", "WARNING", "ERROR", or custom</param>
        /// <returns>Corresponding color, defaults to Secondary if status not recognized</returns>
        public static Color GetStatusColor(string status)
        {
            return status switch
            {
                "OK" => Success,
                "WARNING" => Warning,
                "ERROR" => Error,
                _ => Secondary
            };
        }

        /// <summary>
        /// Get color based on temperature gradient (cold → hot).
        /// Value range: -150 to 150 (Mars temperature in Celsius).
        /// </summary>
        /// <param name="temperature">Temperature in Celsius</param>
        /// <returns>Color interpolated between blue (cold) → cyan → yellow → red (hot)</returns>
        public static Color GetTemperatureColor(float temperature)
        {
            // Normalize temperature to 0-1 range (-150 to 150)
            float normalized = Mathf.Clamp01((temperature + 150f) / 300f);

            if (normalized < 0.25f)
                return Color.Lerp(new Color(0f, 0f, 1f), new Color(0f, 1f, 1f), normalized * 4f);
            else if (normalized < 0.5f)
                return Color.Lerp(new Color(0f, 1f, 1f), new Color(1f, 1f, 0f), (normalized - 0.25f) * 4f);
            else
                return Color.Lerp(new Color(1f, 1f, 0f), new Color(1f, 0f, 0f), (normalized - 0.5f) * 2f);
        }

        /// <summary>
        /// Get a semi-transparent version of a color.
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
