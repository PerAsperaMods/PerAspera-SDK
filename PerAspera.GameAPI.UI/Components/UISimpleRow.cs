using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Simple row renderer using GUI (no GUILayout). IL2CPP compatible.
    /// Returns the Y position for the next row.
    ///
    /// <example>
    /// float y = 30;
    /// y = UISimpleRow.DrawLabel(panelRect, y, "Label text");
    /// y = UISimpleRow.DrawValue(panelRect, y, "Value", "123.45");
    /// y = UISimpleRow.DrawButton(panelRect, y, "Click me", () => { });
    /// </example>
    /// </summary>
    public static class UISimpleRow
    {
        private const float RowHeight = 22;
        private const float Padding = 8;

        /// <summary>Draw a simple label. Returns next Y position.</summary>
        public static float DrawLabel(Rect panelRect, float y, string text)
        {
            var rect = new Rect(panelRect.x + Padding, y, panelRect.width - Padding * 2, RowHeight);
            GUI.Label(rect, text, UIStyles.Label);
            return y + RowHeight + 2;
        }

        /// <summary>Draw a label + value row (label left, value right). Returns next Y position.</summary>
        public static float DrawRow(Rect panelRect, float y, string label, string value)
        {
            var rect = new Rect(panelRect.x + Padding, y, panelRect.width - Padding * 2, RowHeight);

            // Label (left side)
            var labelRect = new Rect(rect.x, rect.y, rect.width * 0.6f, RowHeight);
            GUI.Label(labelRect, label, UIStyles.Label);

            // Value (right side, cyan)
            var valueRect = new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, RowHeight);
            GUI.Label(valueRect, value, UIStyles.Value);

            return y + RowHeight + 2;
        }

        /// <summary>Draw a button. Returns next Y position.</summary>
        public static float DrawButton(Rect panelRect, float y, string label, System.Action? onClick = null)
        {
            var rect = new Rect(panelRect.x + Padding, y, panelRect.width - Padding * 2, RowHeight);

            if (GUI.Button(rect, label, UIStyles.Button))
            {
                onClick?.Invoke();
            }

            return y + RowHeight + 2;
        }

        /// <summary>Draw a text field. Returns next Y position.</summary>
        public static float DrawTextField(Rect panelRect, float y, string label, ref string value)
        {
            // Label
            var labelRect = new Rect(panelRect.x + Padding, y, panelRect.width * 0.3f - Padding, RowHeight);
            GUI.Label(labelRect, label, UIStyles.Label);

            // Field
            var fieldRect = new Rect(panelRect.x + panelRect.width * 0.3f, y, panelRect.width * 0.7f - Padding * 2, RowHeight);
            value = GUI.TextField(fieldRect, value);

            return y + RowHeight + 2;
        }

        /// <summary>Draw a separator line. Returns next Y position.</summary>
        public static float DrawSeparator(Rect panelRect, float y)
        {
            GUI.backgroundColor = UIColors.PanelBorder;
            GUI.Box(new Rect(panelRect.x + Padding, y + 8, panelRect.width - Padding * 2, 1), "");
            GUI.backgroundColor = Color.white;
            return y + RowHeight + 2;
        }

        /// <summary>Draw a spacer. Returns next Y position.</summary>
        public static float DrawSpacer(float y, float height = 10)
        {
            return y + height;
        }
    }
}
