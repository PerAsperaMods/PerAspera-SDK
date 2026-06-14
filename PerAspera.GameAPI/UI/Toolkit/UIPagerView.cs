using System;
using System.Collections.Generic;
using PerAspera.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Injected MonoBehaviour that drives a <see cref="UIPager"/> for a target layout container and
    /// renders a compact "◀ N/M ▶" control built with <see cref="UIBuilder"/>. Generic and reusable
    /// for any overflowing uGUI bar/list (resource bar, building lists, future economy/diplomacy).
    ///
    /// <para><b>Grafted = responsive.</b> The control is parented INTO the native container itself as
    /// an <c>ignoreLayout</c> child anchored to an edge (default: right-center). uGUI then keeps it
    /// glued to that edge automatically as the bar resizes — no per-frame world-space repositioning,
    /// no fighting the container's odd rect. <see cref="UIPager"/> skips ignore-layout children, so the
    /// control is never paginated.</para>
    ///
    /// <para>Set <see cref="Target"/> + <see cref="Pager"/> (and optionally <see cref="Filter"/>) right
    /// after <c>AddComponent</c>; parent this GameObject to <see cref="Target"/> with a
    /// <see cref="LayoutElement"/> (<c>ignoreLayout = true</c>). The owning plugin must call
    /// <c>ClassInjector.RegisterTypeInIl2Cpp&lt;UIPagerView&gt;()</c> once before use.</para>
    /// </summary>
    public class UIPagerView : MonoBehaviour
    {
        /// <summary>IL2CPP injection constructor.</summary>
        public UIPagerView(IntPtr ptr) : base(ptr) { }

        // ── configuration (set by creator before Start) ─────────────────────────
        /// <summary>Container whose children are paginated (also this control's parent).</summary>
        public Transform Target = null!;
        /// <summary>Pagination logic instance.</summary>
        public UIPager Pager = null!;
        /// <summary>Optional child filter (null = paginate all non-ignoreLayout children).</summary>
        public Func<Transform, bool>? Filter;
        /// <summary>Anchor point on the parent (default right-center).</summary>
        public Vector2 EdgeAnchor = new Vector2(1f, 0.5f);
        /// <summary>Pivot of the control (default left-center, so it sits just outside the edge).</summary>
        public Vector2 EdgePivot = new Vector2(0f, 0.5f);
        /// <summary>Offset from the anchor (canvas units).</summary>
        public Vector2 EdgeOffset = new Vector2(8f, 0f);
        /// <summary>Background sprite name for the buttons (from <see cref="UISprites"/>).</summary>
        public string ButtonSpriteName = "AdditionalBuildingButton_Normal";
        /// <summary>Pad each page with blank slots so the container always shows <c>Pager.PageSize</c> children.</summary>
        public bool PadToPageSize;
        /// <summary>Factory for a blank padding slot: (container, index) → child GameObject (null = stop).</summary>
        public Func<Transform, int, GameObject?>? PlaceholderFactory;

        private static readonly LogAspera _log = new LogAspera("UI.Toolkit.UIPagerView");

        private RectTransform _rt = null!;
        private TextMeshProUGUI? _label;
        private GameObject? _prevGo, _nextGo, _labelGo;
        private readonly List<GameObject> _pads = new();
        private int _lastChildCount = -1;
        private bool _built;

        private void Start()
        {
            try
            {
                _rt = GetComponent<RectTransform>();
                if (_rt == null) _rt = gameObject.AddComponent<RectTransform>();

                // Glue to a container edge — uGUI keeps it there as the bar resizes (responsive).
                _rt.anchorMin = _rt.anchorMax = EdgeAnchor;
                _rt.pivot = EdgePivot;
                _rt.sizeDelta = new Vector2(190f, 60f);
                _rt.anchoredPosition = EdgeOffset;

                BuildControl();
                _built = true;
                _log.Info($"grafted onto {Target?.name} (anchor={EdgeAnchor}, offset={EdgeOffset})");
            }
            catch (Exception ex)
            {
                _log.Error($"Start failed: {ex.Message}");
            }
        }

        private void BuildControl()
        {
            var bg = UISprites.Get(ButtonSpriteName);

            var prev = UIBuilder.Button("Prev", transform, bg, () => Pager?.Prev());
            _prevGo = prev.gameObject;
            Place(prev.GetComponent<RectTransform>(), -64f, 0f, 58f, 56f);
            Glyph(prev.transform, "<");

            _label = UIBuilder.Text("Page", transform, "1/1", 28f);
            _labelGo = _label.gameObject;
            Place(_label.rectTransform, 0f, 0f, 66f, 56f);

            var next = UIBuilder.Button("Next", transform, bg, () => Pager?.Next());
            _nextGo = next.gameObject;
            Place(next.GetComponent<RectTransform>(), 64f, 0f, 58f, 56f);
            Glyph(next.transform, ">");
        }

        private void LateUpdate()
        {
            if (!_built || Target == null || Pager == null) return;
            try
            {
                int cc = Target.childCount;
                if (cc != _lastChildCount)
                {
                    _lastChildCount = cc;
                    Pager.Rebuild(Target, Filter);
                    foreach (var p in _pads) if (p != null) p.transform.SetAsLastSibling();
                }

                Pager.Apply();
                UpdatePadding();

                bool multi = Pager.PageCount > 1;
                if (_prevGo != null && _prevGo.activeSelf != multi) _prevGo.SetActive(multi);
                if (_nextGo != null && _nextGo.activeSelf != multi) _nextGo.SetActive(multi);
                if (_labelGo != null && _labelGo.activeSelf != multi) _labelGo.SetActive(multi);

                if (multi && _label != null) _label.text = $"{Pager.Page + 1}/{Pager.PageCount}";
            }
            catch (Exception ex)
            {
                _log.Warning($"LateUpdate: {ex.Message}");
            }
        }

        private void UpdatePadding()
        {
            if (!PadToPageSize || PlaceholderFactory == null) return;
            int need = Mathf.Clamp(Pager.PageSize - Pager.CountOnCurrentPage(), 0, Pager.PageSize);

            // Lazily create blank slots up to what we need, kept at the end of the container.
            while (_pads.Count < need)
            {
                var go = PlaceholderFactory(Target, _pads.Count);
                if (go == null) break;
                go.SetActive(false);
                _pads.Add(go);
                Pager.Exclusions.Add(go.transform);
                go.transform.SetAsLastSibling();
            }

            for (int i = 0; i < _pads.Count; i++)
            {
                bool want = i < need;
                if (_pads[i] != null && _pads[i].activeSelf != want) _pads[i].SetActive(want);
            }
        }

        private static void Place(RectTransform? rt, float x, float y, float w, float h)
        {
            if (rt == null) return;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void Glyph(Transform parent, string glyph)
        {
            var t = UIBuilder.Text("Glyph", parent, glyph, 34f);
            new UINode(t.gameObject).Stretch();
        }
    }
}
