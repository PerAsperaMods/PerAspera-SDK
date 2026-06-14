using System;
using System.Collections.Generic;
using PerAspera.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Cache de sprites de jeu pour le dessin IMGUI (HUD mods, EconomyHUD, ResourceBar...).
    ///
    /// Collecte les sprites depuis des Image components Unity deja charges en memoire,
    /// puis les dessine via SpriteUtils.GetUvRect + GUI.DrawTextureWithTexCoords
    /// -- aucune extraction CPU (GetPixels), compatible atlas non-readable.
    ///
    /// Usage :
    ///   var cache = new HUDSpriteCache();
    ///   cache.ScanGameObject(someGO);
    ///   cache.ScanAll();
    ///   cache.Draw("IMG_Resources_BG", rect);
    ///   cache.DumpToLog(log);
    /// </summary>
    public class HUDSpriteCache
    {
        private readonly Dictionary<string, Sprite> _sprites = new();

        public int Count => _sprites.Count;

        // Collecte

        public void ScanGameObject(GameObject? root, bool includeInactive = true)
        {
            if (root == null) return;
            try
            {
                var images = root.GetComponentsInChildren<Image>(includeInactive);
                foreach (var img in images)
                    TryAdd(img?.sprite);
            }
            catch { }
        }

        public void ScanImage(Image? img) => TryAdd(img?.sprite);
        public void ScanSprite(Sprite? sp)  => TryAdd(sp);

        /// <summary>Scanne TOUS les Image actifs dans la scene -- appel unique.</summary>
        public void ScanAll()
        {
            try
            {
                foreach (var img in UnityEngine.Object.FindObjectsOfType<Image>())
                    TryAdd(img?.sprite);
            }
            catch { }
        }

        private void TryAdd(Sprite? sp)
        {
            if (sp == null || string.IsNullOrEmpty(sp.name)) return;
            _sprites.TryAdd(sp.name, sp);
        }

        // Dessin

        /// <summary>
        /// Dessine un sprite depuis le cache.
        /// Utilise SpriteUtils.GetUvRect pour les coords atlas -- aucune extraction CPU.
        /// Retourne false si le sprite n'est pas en cache.
        /// </summary>
        public bool Draw(string name, Rect rect, bool alphaBlend = true)
        {
            if (!_sprites.TryGetValue(name, out var sp) || sp?.texture == null)
                return false;
            try
            {
                var uv = SpriteUtils.GetUvRect(sp);
                GUI.DrawTextureWithTexCoords(rect, sp.texture, uv, alphaBlend);
                return true;
            }
            catch { return false; }
        }

        public Sprite? Get(string name) => _sprites.GetValueOrDefault(name);
        public bool    Has(string name) => _sprites.ContainsKey(name);

        public bool DrawTinted(string name, Rect rect, Color tint)
        {
            var old = GUI.color;
            GUI.color = tint;
            bool result = Draw(name, rect);
            GUI.color = old;
            return result;
        }

        // Debug

        public void DumpToLog(LogAspera log, string prefix = "[HUDSpriteCache]")
        {
            log.Info($"{prefix} {Count} sprites en cache :");
            foreach (var kv in _sprites)
            {
                var sp = kv.Value;
                string dims = sp?.texture != null
                    ? $"{(int)sp.textureRect.width}x{(int)sp.textureRect.height} atlas={sp.texture.width}x{sp.texture.height}"
                    : "no texture";
                log.Info($"{prefix}   [{kv.Key}] {dims}");
            }
        }
    }
}
