using System;
using Il2CppInterop.Runtime;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UImage = UnityEngine.UI.Image;
using UButton = UnityEngine.UI.Button;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Fluent wrapper around a uGUI <see cref="GameObject"/> + its <see cref="RectTransform"/>.
    /// Returned by every <see cref="UIBuilder"/> factory so callers can chain layout calls.
    /// </summary>
    public sealed class UINode
    {
        /// <summary>The backing GameObject.</summary>
        public GameObject Go { get; }
        /// <summary>The RectTransform (always present — added on construction if missing).</summary>
        public RectTransform Rect { get; }

        /// <summary>Wrap an existing GameObject, ensuring it carries a RectTransform.</summary>
        public UINode(GameObject go)
        {
            Go = go;
            var rt = go.GetComponent<RectTransform>();
            Rect = rt != null ? rt : go.AddComponent<RectTransform>();
        }

        /// <summary>Set <c>sizeDelta</c> (width/height for an anchored, non-stretched rect).</summary>
        public UINode Size(float w, float h) { Rect.sizeDelta = new Vector2(w, h); return this; }

        /// <summary>Set <c>anchoredPosition</c> relative to the current anchors.</summary>
        public UINode Pos(float x, float y) { Rect.anchoredPosition = new Vector2(x, y); return this; }

        /// <summary>Set anchor min/max independently.</summary>
        public UINode Anchor(Vector2 min, Vector2 max) { Rect.anchorMin = min; Rect.anchorMax = max; return this; }

        /// <summary>Collapse anchors and pivot to a single point (e.g. (0,0)=bottom-left).</summary>
        public UINode AnchorPivot(float x, float y)
        {
            var p = new Vector2(x, y);
            Rect.anchorMin = p; Rect.anchorMax = p; Rect.pivot = p;
            return this;
        }

        /// <summary>Set the pivot.</summary>
        public UINode Pivot(float x, float y) { Rect.pivot = new Vector2(x, y); return this; }

        /// <summary>Stretch to fill the parent with insets (left/top/right/bottom).</summary>
        public UINode Stretch(float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            Rect.anchorMin = Vector2.zero; Rect.anchorMax = Vector2.one;
            Rect.offsetMin = new Vector2(left, bottom);
            Rect.offsetMax = new Vector2(-right, -top);
            return this;
        }

        /// <summary>Toggle the GameObject active state.</summary>
        public UINode Active(bool v) { Go.SetActive(v); return this; }

        /// <summary>Add a horizontal auto-layout group to this node and return it.</summary>
        public HorizontalLayoutGroup HLayout(float spacing = 2f, int padX = 0, int padY = 0)
        {
            var g = Go.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(padX, padX, padY, padY);
            g.childControlWidth = false; g.childControlHeight = false;
            g.childForceExpandWidth = false; g.childForceExpandHeight = false;
            return g;
        }

        /// <summary>Add a vertical auto-layout group to this node and return it.</summary>
        public VerticalLayoutGroup VLayout(float spacing = 2f, int padX = 0, int padY = 0)
        {
            var g = Go.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(padX, padX, padY, padY);
            g.childControlWidth = false; g.childControlHeight = false;
            g.childForceExpandWidth = false; g.childForceExpandHeight = false;
            return g;
        }
    }

    /// <summary>
    /// Builds real Unity UI (uGUI) elements from code — the toolkit's "construction" tier
    /// (the "clone a native prototype" tier lives in <c>UIClone</c>). Everything is parented under
    /// the game's own canvas (via <see cref="Root"/>) so scaling, the GraphicRaycaster and the
    /// scene EventSystem are inherited for free.
    ///
    /// <para><b>Sprites:</b> pass a <see cref="Sprite"/> from <see cref="UISprites"/> (or a typed game
    /// source such as <c>ResourceType.iconName</c>). uGUI renders atlas sprites natively.</para>
    /// <para><b>Text:</b> uses <see cref="UIFonts.Game"/> so labels match vanilla TextMeshPro.</para>
    /// </summary>
    /// <example>
    /// var panel = UIBuilder.Panel("MyBar", UIBuilder.Root, UISprites.Get("IMG_Resources_BG"))
    ///     .AnchorPivot(0, 0).Pos(160, 12).Size(440, 40);
    /// panel.HLayout(spacing: 2);
    /// UIBuilder.Image("icon", panel.Rect, water.iconName).rectTransform.sizeDelta = new Vector2(26, 26);
    /// UIBuilder.Text("qty", panel.Rect, "1.2k", 12);
    /// UIBuilder.Button("next", panel.Rect, UISprites.Get("IMG_ArrowRight"), () => pager.Next());
    /// </example>
    public static class UIBuilder
    {
        private static readonly LogAspera _log = new LogAspera("UI.Toolkit.UIBuilder");

        /// <summary>
        /// The game's root canvas transform (its <c>canvasRect</c>) — default parent for
        /// top-level panels. Null before the game UI is loaded.
        /// </summary>
        public static Transform? Root
        {
            get
            {
                try { return BaseGameWrapper.GetCurrent()?.CanvasRefs?.canvasRect; }
                catch { return null; }
            }
        }

        /// <summary>Create an empty RectTransform node under <paramref name="parent"/>.</summary>
        public static UINode Node(string name, Transform? parent)
        {
            var go = new GameObject(name);
            var node = new UINode(go);
            if (parent != null) node.Rect.SetParent(parent, false);
            return node;
        }

        /// <summary>
        /// Create a panel: a node with a background <see cref="UImage"/>. Pass a sprite for a
        /// game-styled (9-sliced) background, or null for a flat translucent fill.
        /// </summary>
        public static UINode Panel(string name, Transform? parent, Sprite? background = null, Color? tint = null)
        {
            var node = Node(name, parent);
            var img = node.Go.AddComponent<UImage>();
            if (background != null)
            {
                img.sprite = background;
                img.type = UImage.Type.Sliced;
                img.color = tint ?? Color.white;
            }
            else
            {
                img.color = tint ?? new Color(0f, 0f, 0f, 0.6f);
            }
            return node;
        }

        /// <summary>Create an <see cref="UImage"/> showing <paramref name="sprite"/>.</summary>
        public static UImage Image(string name, Transform? parent, Sprite? sprite, Color? tint = null,
            bool preserveAspect = true)
        {
            var node = Node(name, parent);
            var img = node.Go.AddComponent<UImage>();
            if (sprite != null) img.sprite = sprite;
            img.color = tint ?? Color.white;
            img.preserveAspect = preserveAspect;
            return img;
        }

        /// <summary>Create a TextMeshPro label using the game font.</summary>
        public static TextMeshProUGUI Text(string name, Transform? parent, string text,
            float fontSize = 16f, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var node = Node(name, parent);
            var t = node.Go.AddComponent<TextMeshProUGUI>();
            var font = UIFonts.Game;
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = fontSize;
            t.color = color ?? Color.white;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>
        /// Create a clickable <see cref="UButton"/> backed by an <see cref="UImage"/>. The managed
        /// <paramref name="onClick"/> is marshalled to an IL2CPP <see cref="UnityAction"/>.
        /// </summary>
        public static UButton Button(string name, Transform? parent, Sprite? sprite, Action onClick,
            Color? tint = null)
        {
            var img = Image(name, parent, sprite, tint, preserveAspect: false);
            img.raycastTarget = true;
            var btn = img.gameObject.AddComponent<UButton>();
            btn.targetGraphic = img;
            if (onClick != null)
            {
                try { btn.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(onClick)); }
                catch (Exception ex) { _log.Warning($"Button '{name}' onClick wiring failed: {ex.Message}"); }
            }
            return btn;
        }
    }
}
