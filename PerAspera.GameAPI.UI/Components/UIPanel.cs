using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Simple draggable window panel with header and close button.
    /// Renders using GUI.Box and manual dragging, compatible with IL2CPP.
    ///
    /// <example>
    /// var panel = new UIPanel("My Panel", new Rect(100, 100, 300, 400));
    /// panel.OnGUI(() => {
    ///     GUILayout.Label("Content goes here");
    /// });
    /// </example>
    /// </summary>
    public class UIPanel
    {
        private readonly string _title;
        private Rect _windowRect;
        private bool _isVisible = true;
        private System.Action? _contentCallback;
        private Rect _dragRect;

        /// <summary>Is the panel currently visible?</summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        /// <summary>Current window rectangle (position and size)</summary>
        public Rect WindowRect
        {
            get => _windowRect;
            set => _windowRect = value;
        }

        /// <summary>Panel title</summary>
        public string Title => _title;

        /// <summary>Create a new draggable panel.</summary>
        /// <param name="title">Panel title displayed in header</param>
        /// <param name="initialRect">Initial window position and size</param>
        public UIPanel(string title, Rect initialRect)
        {
            _title = title;
            _windowRect = initialRect;
            _dragRect = new Rect(0, 0, initialRect.width, 24);
        }

        /// <summary>
        /// Render the panel with custom content.
        /// </summary>
        /// <param name="contentCallback">Callback function to render panel content via GUILayout</param>
        public void OnGUI(System.Action? contentCallback = null)
        {
            if (!_isVisible)
                return;

            _contentCallback = contentCallback;

            // Panel background
            GUI.backgroundColor = UIColors.PanelBackground;
            GUI.Box(_windowRect, "");

            // Draw header + content
            GUILayout.BeginArea(_windowRect);
            {
                // Header with title and close button
                GUILayout.BeginHorizontal(GUILayout.Height(24));
                {
                    GUI.contentColor = UIColors.Secondary;
                    GUILayout.Label(_title, UIStyles.Header, GUILayout.ExpandWidth(true));

                    if (GUILayout.Button("✕", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _isVisible = false;
                    }
                }
                GUILayout.EndHorizontal();

                // Separator
                GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));

                // Content area
                GUI.backgroundColor = Color.white;
                GUILayout.BeginVertical();
                {
                    _contentCallback?.Invoke();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();

            // Handle dragging
            HandleDragging();

            // Draw border
            GUI.backgroundColor = UIColors.PanelBorder;
            GUI.Box(_windowRect, "");
            GUI.backgroundColor = Color.white;
        }

        private void HandleDragging()
        {
            var dragArea = new Rect(_windowRect.x, _windowRect.y, _windowRect.width, 24);

            if (GUI.RepeatButton(dragArea, "", new GUIStyle()))
            {
                var delta = Input.mousePosition - new Vector3(_dragRect.x, _dragRect.y);
                _dragRect = new Rect(Input.mousePosition.x, Input.mousePosition.y, 0, 0);
            }
        }

        /// <summary>
        /// Toggle panel visibility.
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
        }

        /// <summary>
        /// Center panel on screen.
        /// </summary>
        public void CenterOnScreen(float width = 300, float height = 400)
        {
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            _windowRect = new Rect(
                (screenWidth - width) / 2,
                (screenHeight - height) / 2,
                width,
                height
            );
        }
    }
}
