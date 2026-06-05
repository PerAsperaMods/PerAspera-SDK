using UnityEngine;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Ultra-simple GUI-only panel (no GUILayout). IL2CPP compatible.
    /// Draws a window-like panel with title, close button, and custom content area.
    ///
    /// <example>
    /// var panel = new UISimplePanel("My Panel", new Rect(100, 100, 300, 400));
    ///
    /// void OnGUI() {
    ///     panel.OnGUI(y => {
    ///         GUI.Label(new Rect(10, y, 280, 20), "Content here");
    ///         return y + 25;
    ///     });
    /// }
    /// </example>
    /// </summary>
    public class UISimplePanel
    {
        private readonly string _title;
        private Rect _rect;
        private bool _isVisible = true;
        private Vector2 _dragStart;
        private bool _isDragging = false;

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public Rect Rect
        {
            get => _rect;
            set => _rect = value;
        }

        /// <summary>Create a new simple panel.</summary>
        public UISimplePanel(string title, Rect initialRect)
        {
            _title = title;
            _rect = initialRect;
        }

        /// <summary>
        /// Render panel. ContentDrawer receives Y position and returns next Y position.
        /// </summary>
        public void OnGUI(System.Func<float, float>? contentDrawer = null)
        {
            if (!_isVisible)
                return;

            // Panel background
            GUI.backgroundColor = UIColors.PanelBackground;
            GUI.Box(_rect, "");

            // Header area (title + close button)
            var headerRect = new Rect(_rect.x, _rect.y, _rect.width, 24);
            GUI.backgroundColor = UIColors.PanelBackground;
            GUI.Box(headerRect, "");

            // Title
            var titleRect = new Rect(_rect.x + 5, _rect.y + 2, _rect.width - 30, 20);
            GUI.backgroundColor = Color.clear;
            GUI.contentColor = UIColors.Secondary;
            GUI.Label(titleRect, _title, UIStyles.Header);

            // Close button
            var closeRect = new Rect(_rect.x + _rect.width - 25, _rect.y + 2, 20, 20);
            GUI.backgroundColor = UIColors.Warning;
            if (GUI.Button(closeRect, "✕"))
            {
                _isVisible = false;
            }

            // Content area
            var contentStartY = _rect.y + 30;
            var contentEndY = _rect.y + _rect.height - 5;

            if (contentDrawer != null && contentStartY < contentEndY)
            {
                GUI.backgroundColor = Color.white;
                GUI.contentColor = UIColors.Secondary;
                float currentY = contentDrawer(contentStartY);
            }

            // Separator line
            GUI.backgroundColor = UIColors.PanelBorder;
            GUI.Box(new Rect(_rect.x, _rect.y + 26, _rect.width, 2), "");

            // Border
            GUI.backgroundColor = UIColors.PanelBorder;
            DrawPanelBorder(_rect, 2);

            // Reset colors
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            // Handle dragging
            HandleDragging();
        }

        private void HandleDragging()
        {
            var headerRect = new Rect(_rect.x, _rect.y, _rect.width, 24);

            if (Input.GetMouseButtonDown(0) && headerRect.Contains(Input.mousePosition))
            {
                _isDragging = true;
                _dragStart = (Vector2)Input.mousePosition - _rect.position;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                _rect.position = (Vector2)Input.mousePosition - _dragStart;
            }
        }

        private void DrawPanelBorder(Rect rect, int thickness)
        {
            // Top
            GUI.Box(new Rect(rect.x, rect.y, rect.width, thickness), "");
            // Bottom
            GUI.Box(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), "");
            // Left
            GUI.Box(new Rect(rect.x, rect.y, thickness, rect.height), "");
            // Right
            GUI.Box(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), "");
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }
    }
}
