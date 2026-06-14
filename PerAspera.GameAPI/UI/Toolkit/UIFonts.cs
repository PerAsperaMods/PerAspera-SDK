using Il2CppInterop.Runtime;
using PerAspera.Core;
using TMPro;
using UnityEngine;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Provides the game's own TextMeshPro font asset so toolkit-built text matches vanilla.
    ///
    /// <para>Per Aspera renders all of its text with TextMeshPro using a custom SDF font
    /// ("Aspera", baked as the <c>Aspera_SDF</c> atlas). A freshly created
    /// <see cref="TextMeshProUGUI"/> would otherwise fall back to TMP's default LiberationSans and
    /// look foreign. <see cref="Game"/> harvests the real asset at runtime — no disk loading, no
    /// asset-bundle juggling — and caches it.</para>
    /// </summary>
    /// <example>
    /// var label = UIBuilder.Text("score", parent, "1.2k");
    /// // label.font is already UIFonts.Game
    /// </example>
    public static class UIFonts
    {
        private static readonly LogAspera _log = new LogAspera("UI.Toolkit.UIFonts");
        private static TMP_FontAsset? _game;

        /// <summary>
        /// The game's primary TMP font asset (cached). Resolves the first loaded
        /// <see cref="TMP_FontAsset"/> whose name contains "Aspera", else the asset used by the
        /// first live <see cref="TMP_Text"/>, else TMP's default. Null only before any TMP text
        /// has been created by the game.
        /// </summary>
        public static TMP_FontAsset? Game
        {
            get
            {
                if (_game != null) return _game;
                _game = Resolve();
                return _game;
            }
        }

        /// <summary>Force a re-resolve on next access (e.g. after a scene/font reload).</summary>
        public static void Invalidate() => _game = null;

        private static TMP_FontAsset? Resolve()
        {
            try
            {
                // 1) Prefer the named "Aspera" SDF asset among everything loaded.
                var assets = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_FontAsset>());
                if (assets != null)
                {
                    TMP_FontAsset? first = null;
                    foreach (var obj in assets)
                    {
                        var fa = obj?.TryCast<TMP_FontAsset>();
                        if (fa == null) continue;
                        first ??= fa;
                        var n = fa.name;
                        if (!string.IsNullOrEmpty(n) && n.IndexOf("Aspera", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _log.Info($"Resolved game font by name: {n}");
                            return fa;
                        }
                    }
                    if (first != null)
                    {
                        _log.Info($"Resolved game font (first loaded asset): {first.name}");
                        return first;
                    }
                }

                // 2) Fallback: borrow the font from any live TMP_Text in the scene.
                var texts = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_Text>());
                if (texts != null)
                {
                    foreach (var obj in texts)
                    {
                        var t = obj?.TryCast<TMP_Text>();
                        var fa = t?.font;
                        if (fa != null)
                        {
                            _log.Info($"Resolved game font from live TMP_Text: {fa.name}");
                            return fa;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _log.Warning($"Font resolve failed: {ex.Message}");
            }

            _log.Warning("No TMP font found yet (game UI not loaded?). Toolkit text will use TMP default.");
            return null;
        }
    }
}
