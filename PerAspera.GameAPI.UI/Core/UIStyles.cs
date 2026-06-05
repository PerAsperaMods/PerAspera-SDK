using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Standardized GUI styles for Per Aspera UI components.
    /// Lazy-initializes styles on first access to avoid allocations.
    ///
    /// <example>
    /// GUI.Label(rect, "Label Text", UIStyles.Label);
    /// GUI.Label(rect, "100.5", UIStyles.Value);
    /// </example>
    /// </summary>
    public static class UIStyles
    {
        private static GUIStyle? _headerStyle;
        private static GUIStyle? _labelStyle;
        private static GUIStyle? _valueStyle;
        private static GUIStyle? _warningStyle;
        private static GUIStyle? _errorStyle;
        private static GUIStyle? _buttonStyle;

        /// <summary>Header style - bold white text, 14pt, for panel titles</summary>
        public static GUIStyle Header => _headerStyle ??= CreateHeaderStyle();

        /// <summary>Label style - white text, 11pt, for resource names and descriptions</summary>
        public static GUIStyle Label => _labelStyle ??= CreateLabelStyle();

        /// <summary>Value style - cyan text, bold, 11pt, for numeric values</summary>
        public static GUIStyle Value => _valueStyle ??= CreateValueStyle();

        /// <summary>Warning style - orange text, 10pt, for warning messages</summary>
        public static GUIStyle Warning => _warningStyle ??= CreateWarningStyle();

        /// <summary>Error style - red text, bold, 10pt, for error messages</summary>
        public static GUIStyle Error => _errorStyle ??= CreateErrorStyle();

        /// <summary>Button style - standard button with custom colors</summary>
        public static GUIStyle Button => _buttonStyle ??= CreateButtonStyle();

        private static GUIStyle CreateHeaderStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UIColors.Secondary }
            };
            return style;
        }

        private static GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UIColors.Secondary }
            };
            return style;
        }

        private static GUIStyle CreateValueStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = UIColors.Primary }
            };
            return style;
        }

        private static GUIStyle CreateWarningStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UIColors.Warning }
            };
            return style;
        }

        private static GUIStyle CreateErrorStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = UIColors.Error }
            };
            return style;
        }

        private static GUIStyle CreateButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = UIColors.Secondary }
            };
            return style;
        }

        /// <summary>
        /// Force reinitialize all styles (useful for theme changes or testing).
        /// </summary>
        public static void Reset()
        {
            _headerStyle = null;
            _labelStyle = null;
            _valueStyle = null;
            _warningStyle = null;
            _errorStyle = null;
            _buttonStyle = null;
        }
    }
}
