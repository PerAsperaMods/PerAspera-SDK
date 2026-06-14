using System.Collections.Generic;
using Il2CppInterop.Runtime;
using PerAspera.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Runtime sprite directory ("annuaire") for the uGUI UI toolkit.
    ///
    /// <para>The game ships ~1900 named sprites baked into GPU atlases. The decompiled
    /// <c>PerAsperaData\Sprite\</c> folder is the documented list of those names (a catalog, NOT a
    /// load source: the PNGs are already resident in the running atlases). This class harvests the
    /// live <see cref="Sprite"/> objects from the loaded UI so a mod can reference any of them by
    /// name and hand them straight to a uGUI <see cref="Image"/>.</para>
    ///
    /// <para><b>Why uGUI, not IMGUI:</b> a uGUI <see cref="Image"/> renders an atlas sprite
    /// natively (the game does it every frame). IMGUI's <c>GUI.DrawTexture*</c> cannot — the
    /// atlases are compressed, non-readable GPU textures. So sprites obtained here are meant to be
    /// assigned to <c>image.sprite</c>, never drawn with the legacy <c>HUDSpriteCache.Draw</c>.</para>
    /// </summary>
    /// <example>
    /// UISprites.Refresh();                       // once, after the game UI is loaded
    /// var bg = UISprites.Get("IMG_Resources_BG");
    /// myImage.sprite = bg;                        // renders identical to vanilla
    /// </example>
    public static class UISprites
    {
        private static readonly LogAspera _log = new LogAspera("UI.Toolkit.UISprites");
        private static readonly Dictionary<string, Sprite> _byName = new();

        /// <summary>Number of distinct named sprites currently indexed.</summary>
        public static int Count => _byName.Count;

        /// <summary>All indexed sprite names (for tooling / dumps).</summary>
        public static IReadOnlyCollection<string> Names => _byName.Keys;

        /// <summary>
        /// Rebuild the index by scanning every loaded <see cref="Image"/> (including inactive
        /// objects and in-memory prefabs via <c>Resources.FindObjectsOfTypeAll</c>). Safe to call
        /// repeatedly — later panels add their sprites to the directory. Call once the game UI
        /// exists (e.g. after <c>GameFullyLoadedEvent</c>); calling earlier just yields fewer entries.
        /// </summary>
        /// <returns>The number of distinct sprites indexed after the scan.</returns>
        public static int Refresh()
        {
            try
            {
                var images = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Image>());
                if (images != null)
                {
                    foreach (var obj in images)
                    {
                        var img = obj?.TryCast<Image>();
                        Add(img?.sprite);
                        Add(img?.overrideSprite);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _log.Warning($"Refresh scan failed: {ex.Message}");
            }
            return _byName.Count;
        }

        /// <summary>Index a single sprite by its <c>sprite.name</c> (no-op if null/blank/dup).</summary>
        public static void Add(Sprite? sprite)
        {
            if (sprite == null) return;
            string name = sprite.name;
            if (string.IsNullOrEmpty(name)) return;
            _byName[name] = sprite;
        }

        /// <summary>Index every sprite found under <paramref name="root"/> (any child Image).</summary>
        public static void AddFrom(GameObject? root)
        {
            if (root == null) return;
            var images = root.GetComponentsInChildren(Il2CppType.Of<Image>(), true);
            if (images == null) return;
            foreach (var obj in images)
            {
                var img = obj?.TryCast<Image>();
                Add(img?.sprite);
                Add(img?.overrideSprite);
            }
        }

        /// <summary>The sprite registered under <paramref name="name"/>, or null.</summary>
        public static Sprite? Get(string name)
            => _byName.TryGetValue(name, out var sp) ? sp : null;

        /// <summary>True if a sprite named <paramref name="name"/> is indexed.</summary>
        public static bool Has(string name) => _byName.ContainsKey(name);

        /// <summary>Try-get variant for hot paths.</summary>
        public static bool TryGet(string name, out Sprite? sprite)
            => _byName.TryGetValue(name, out sprite);

        /// <summary>Dump every indexed sprite (name + pixel size) to the log — debugging aid.</summary>
        public static void Dump(string prefix = "[UISprites]")
        {
            _log.Info($"{prefix} {_byName.Count} sprites:");
            foreach (var kv in _byName)
            {
                var r = kv.Value.rect;
                _log.Info($"{prefix}   {kv.Key}  {(int)r.width}x{(int)r.height}");
            }
        }
    }
}
