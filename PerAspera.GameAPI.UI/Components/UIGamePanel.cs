using UnityEngine;
using System.Collections.Generic;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Game-native panel using actual game sprites (BackgroundWindow_2.png).
    /// Renders panels that look 100% identical to in-game panels.
    ///
    /// <example>
    /// var panel = new UIGamePanel("POI Browser", new Rect(50, 50, 500, 650));
    /// panel.OnGUI(y => {
    ///     y = UISimpleRow.DrawLabel(panel.Rect, y, "Content here");
    ///     return y;
    /// });
    /// </example>
    /// </summary>
    public class UIGamePanel
    {
        private static Dictionary<string, Texture2D> _spriteCache = new();

        private readonly string _title;
        private Rect _rect;
        private bool _isVisible = true;
        private Vector2 _dragStart;
        private bool _isDragging = false;
        private Texture2D? _backgroundTexture;
        private string _spriteResourcePath;

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

        /// <summary>Create a new game-style panel.</summary>
        /// <param name="title">Panel title</param>
        /// <param name="initialRect">Initial position and size</param>
        /// <param name="spriteResourcePath">Path to sprite in Resources folder (e.g., "UI/BackgroundWindow_2")</param>
        public UIGamePanel(string title, Rect initialRect, string spriteResourcePath = "BackgroundWindow_2")
        {
            _title = title;
            _rect = initialRect;
            _spriteResourcePath = spriteResourcePath;

            // Load sprite from StreamingAssets
            _backgroundTexture = LoadSprite(spriteResourcePath);
        }

        private static Texture2D? LoadSprite(string resourcePath)
        {
            if (_spriteCache.TryGetValue(resourcePath, out var cached))
                return cached;

            // Try multiple paths
            var paths = new[] {
                // StreamingAssets/Mods/PoiBrowserMod/sprites/BackgroundWindow_2.png
                System.IO.Path.Combine(Application.dataPath, "StreamingAssets", "Mods", "PoiBrowserMod", "sprites", resourcePath + ".png"),
                // Resources/BackgroundWindow_2.png
                System.IO.Path.Combine(Application.dataPath, "Resources", resourcePath + ".png"),
                // Direct file path (normalized)
                resourcePath.EndsWith(".png") ? resourcePath : resourcePath + ".png"
            };

            foreach (var pathRaw in paths)
            {
                try
                {
                    // Normalize path separators for current OS
                    var path = pathRaw.Replace('/', System.IO.Path.DirectorySeparatorChar)
                                      .Replace('\\', System.IO.Path.DirectorySeparatorChar);

                    if (!System.IO.File.Exists(path))
                        continue;

                    var data = System.IO.File.ReadAllBytes(path);
                    var texture = new Texture2D(2, 2);
                    if (texture.LoadImage(data))
                    {
                        _spriteCache[resourcePath] = texture;
                        return texture;
                    }
                }
                catch { }
            }

            return null;
        }

        /// <summary>Render panel with game sprite background.</summary>
        public void OnGUI(System.Func<float, float>? contentDrawer = null)
        {
            if (!_isVisible)
                return;

            // Draw sprite background
            if (_backgroundTexture != null)
            {
                GUI.DrawTexture(_rect, _backgroundTexture, ScaleMode.StretchToFill);
            }
            else
            {
                // Fallback: solid color like game panels
                GUI.backgroundColor = UIColors.PanelBackground;
                GUI.Box(_rect, "");
            }

            // Header area (title + close button)
            var headerRect = new Rect(_rect.x, _rect.y, _rect.width, 24);

            // Title
            var titleRect = new Rect(_rect.x + 12, _rect.y + 4, _rect.width - 40, 16);
            GUI.backgroundColor = Color.clear;
            GUI.contentColor = UIColors.Secondary;
            GUI.Label(titleRect, _title, UIStyles.Header);

            // Close button (cream colored like game UI)
            var closeRect = new Rect(_rect.x + _rect.width - 28, _rect.y + 4, 20, 20);
            GUI.backgroundColor = new Color(0.85f, 0.82f, 0.75f, 0.8f);  // Cream color
            if (GUI.Button(closeRect, "✕", UIStyles.Button))
            {
                _isVisible = false;
            }

            // Content area (with padding to respect sprite borders)
            var contentStartY = _rect.y + 32;
            var contentEndY = _rect.y + _rect.height - 8;

            if (contentDrawer != null && contentStartY < contentEndY)
            {
                GUI.backgroundColor = Color.clear;
                GUI.contentColor = UIColors.Secondary;
                float currentY = contentDrawer(contentStartY);
            }

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

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }
    }
}
