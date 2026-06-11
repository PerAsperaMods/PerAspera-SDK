using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// A single row displaying resource information (label + quantity + optional bar).
    /// Standardized layout matching in-game resource displays.
    ///
    /// <example>
    /// UIResourceRow.DrawRow("Oxygen", 150.5f);
    /// UIResourceRow.DrawRow("Water", 75.2f, 100f);  // With max for bar
    /// </example>
    /// </summary>
    public static class UIResourceRow
    {
        /// <summary>
        /// Draw a simple resource row with label and quantity.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="quantity">Current quantity</param>
        /// <param name="displayFormat">Format string for quantity (e.g., "F1")</param>
        public static void DrawRow(string resourceName, float quantity, string displayFormat = "F1")
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(resourceName, UIStyles.Label, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    quantity.ToString(displayFormat),
                    UIStyles.Value,
                    GUILayout.Width(80)
                );
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a resource row with a progress bar.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="current">Current quantity</param>
        /// <param name="maximum">Maximum capacity</param>
        /// <param name="displayFormat">Format string for quantity</param>
        public static void DrawRowWithBar(
            string resourceName,
            float current,
            float maximum,
            string displayFormat = "F1")
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(resourceName, UIStyles.Label, GUILayout.Width(120));

                // Progress bar
                DrawProgressBar(current, maximum, GUILayout.ExpandWidth(true), GUILayout.Height(16));

                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    current.ToString(displayFormat),
                    UIStyles.Value,
                    GUILayout.Width(80)
                );
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a resource row with status indicator.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="quantity">Current quantity</param>
        /// <param name="status">Status ("OK", "WARNING", "ERROR")</param>
        /// <param name="displayFormat">Format string for quantity</param>
        public static void DrawRowWithStatus(
            string resourceName,
            float quantity,
            string status,
            string displayFormat = "F1")
        {
            var statusColor = UIColors.GetStatusColor(status);

            GUILayout.BeginHorizontal();
            {
                // Status indicator (colored square)
                GUI.backgroundColor = statusColor;
                GUILayout.Box("", GUILayout.Width(12), GUILayout.Height(12));
                GUI.backgroundColor = Color.white;

                GUILayout.Label(resourceName, UIStyles.Label, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    quantity.ToString(displayFormat),
                    UIStyles.Value,
                    GUILayout.Width(80)
                );
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw a simple progress bar (used internally).
        /// </summary>
        private static void DrawProgressBar(float current, float maximum, params GUILayoutOption[] options)
        {
            var rect = GUILayoutUtility.GetRect(100, 16, options);
            var normalizedValue = maximum > 0 ? current / maximum : 0;

            // Background
            GUI.backgroundColor = UIColors.PanelBackground;
            GUI.Box(rect, "");

            // Fill
            var fillRect = new Rect(rect.x, rect.y, rect.width * normalizedValue, rect.height);
            var barColor = normalizedValue > 0.5f ? UIColors.Success : UIColors.Warning;
            GUI.backgroundColor = barColor;
            GUI.Box(fillRect, "");

            // Reset background color for subsequent elements
            GUI.backgroundColor = Color.white;

            // Border
            GUI.backgroundColor = UIColors.PanelBorder;
            GUI.Box(rect, "", new GUIStyle(GUI.skin.box) { normal = { background = null } });
            GUI.backgroundColor = Color.white;
        }
    }
}
