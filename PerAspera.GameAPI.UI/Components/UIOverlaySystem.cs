using System;
using System.Collections.Generic;
using UnityEngine;
using PerAspera.Core;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// System to add overlay elements to native game panels.
    /// Allows mods to inject UI elements into existing game panels.
    ///
    /// <example>
    /// var overlay = new UIOverlaySystem("POI Browser", new Rect(50, 50, 500, 650));
    /// overlay.OnGUI(y => {
    ///     y = UISimpleRow.DrawLabel(overlay.PanelRect, y, "Custom content");
    ///     return y;
    /// });
    /// </example>
    /// </summary>
    public class UIOverlaySystem
    {
        private readonly string _title;
        private Rect _panelRect;
        private bool _isVisible = true;
        private Vector2 _dragStart;
        private bool _isDragging = false;
        private System.Func<float, float>? _contentDrawer;
        private Sprite? _backgroundSprite;

        /// <summary>Panel rectangle (position and size).</summary>
        public Rect PanelRect
        {
            get => _panelRect;
            set => _panelRect = value;
        }

        /// <summary>Is panel visible.</summary>
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        /// <summary>Create a new overlay panel.</summary>
        public UIOverlaySystem(string title, Rect initialRect, Sprite? backgroundSprite = null)
        {
            _title = title;
            _panelRect = initialRect;
            _backgroundSprite = backgroundSprite;
        }

        /// <summary>Set custom background sprite from game.</summary>
        public void SetBackgroundSprite(Sprite sprite)
        {
            _backgroundSprite = sprite;
        }

        /// <summary>Register content drawer callback (receives Y position, returns next Y).</summary>
        public void SetContentDrawer(System.Func<float, float>? contentDrawer)
        {
            if (contentDrawer != null)
            {
                _contentDrawer = contentDrawer;
            }
        }

        /// <summary>Render the overlay panel.</summary>
        public void OnGUI(System.Func<float, float>? contentDrawer = null)
        {
            if (!_isVisible)
                return;

            var drawer = contentDrawer ?? _contentDrawer;

            // Draw background
            if (_backgroundSprite != null && _backgroundSprite.texture != null)
            {
                GUI.DrawTexture(_panelRect, _backgroundSprite.texture, ScaleMode.StretchToFill);
            }
            else
            {
                // Fallback: solid color
                GUI.backgroundColor = new Color(0.2f, 0.25f, 0.3f, 0.9f);
                GUI.Box(_panelRect, "");
                GUI.backgroundColor = Color.white;
            }

            // Header
            var headerRect = new Rect(_panelRect.x, _panelRect.y, _panelRect.width, 24);

            // Title
            var titleRect = new Rect(_panelRect.x + 12, _panelRect.y + 4, _panelRect.width - 40, 16);
            GUI.backgroundColor = Color.clear;
            GUI.contentColor = new Color(0.8f, 1f, 1f);  // Cyan
            GUI.Label(titleRect, _title);

            // Close button
            var closeRect = new Rect(_panelRect.x + _panelRect.width - 28, _panelRect.y + 4, 20, 20);
            GUI.backgroundColor = new Color(0.85f, 0.82f, 0.75f, 0.8f);
            if (GUI.Button(closeRect, "✕"))
            {
                _isVisible = false;
            }

            // Content area
            var contentStartY = _panelRect.y + 32;
            var contentEndY = _panelRect.y + _panelRect.height - 8;

            if (drawer != null && contentStartY < contentEndY)
            {
                GUI.backgroundColor = Color.clear;
                GUI.contentColor = Color.white;
                float currentY = drawer(contentStartY);
            }

            // Separator
            GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            GUI.Box(new Rect(_panelRect.x, _panelRect.y + 26, _panelRect.width, 1), "");

            // Reset colors
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            // Handle dragging
            HandleDragging(headerRect);
        }

        private void HandleDragging(Rect headerRect)
        {
            if (Input.GetMouseButtonDown(0) && headerRect.Contains(Input.mousePosition))
            {
                _isDragging = true;
                _dragStart = (Vector2)Input.mousePosition - _panelRect.position;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                _panelRect.position = (Vector2)Input.mousePosition - _dragStart;
            }
        }

        /// <summary>Toggle panel visibility.</summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
        }

        /// <summary>Reset to initial position.</summary>
        public void ResetPosition(Rect initialRect)
        {
            _panelRect = initialRect;
        }
    }
}
