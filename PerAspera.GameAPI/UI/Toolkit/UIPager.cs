using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Generic pagination over the child elements of any uGUI layout-group container
    /// (HorizontalLayoutGroup / VerticalLayoutGroup / GridLayoutGroup). Mods will hit overflow
    /// everywhere — resource bar, building lists, economy/diplomacy panels — so the paging logic
    /// is container-agnostic: it operates on raw child <see cref="Transform"/>s and toggles their
    /// GameObjects so only the current page is shown. The layout group reflows automatically
    /// (inactive children are ignored) and a ContentSizeFitter recenters the container.
    ///
    /// <para><b>Safety:</b> child GameObjects often carry sound/animation triggers
    /// (e.g. PlaySound, TweenEventTrigger). <see cref="Apply"/> only flips an item's active state
    /// when it actually differs, so a stable page never re-fires those components.</para>
    /// </summary>
    public sealed class UIPager
    {
        /// <summary>Max items shown per page.</summary>
        public int PageSize = 6;

        private readonly List<Transform> _all = new();
        private readonly List<Transform> _visible = new();
        private readonly HashSet<Transform> _inPage = new();
        private int _page;

        /// <summary>Current page index (0-based).</summary>
        public int Page => _page;

        /// <summary>Number of children passing the filter.</summary>
        public int VisibleCount => _visible.Count;

        /// <summary>Number of pages (always >= 1).</summary>
        public int PageCount => Mathf.Max(1, Mathf.CeilToInt(_visible.Count / (float)Mathf.Max(1, PageSize)));

        /// <summary>
        /// Transforms to leave completely untouched (never counted, never toggled) — e.g. blank
        /// padding slots a view manages itself. Add to it before <see cref="Rebuild"/>.
        /// </summary>
        public readonly HashSet<Transform> Exclusions = new();

        /// <summary>Number of real (filtered) items shown on the current page (0..PageSize).</summary>
        public int CountOnCurrentPage()
        {
            int start = _page * PageSize;
            return Mathf.Clamp(_visible.Count - start, 0, PageSize);
        }

        /// <summary>Advance one page (clamped).</summary>
        public void Next() { if (_page < PageCount - 1) _page++; }

        /// <summary>Go back one page (clamped).</summary>
        public void Prev() { if (_page > 0) _page--; }

        /// <summary>Jump to a page (clamped).</summary>
        public void GoTo(int page) { _page = Mathf.Clamp(page, 0, PageCount - 1); }

        /// <summary>
        /// Re-read the container's children and recompute the filtered visible list. Call when the
        /// child count changes (items added/removed) or the filter changes. <paramref name="filter"/>
        /// returns true for children that should appear in the paged set (null = all children).
        /// </summary>
        public void Rebuild(Transform container, Func<Transform, bool>? filter)
        {
            _all.Clear();
            _visible.Clear();
            if (container == null) return;

            int n = container.childCount;
            for (int i = 0; i < n; i++)
            {
                var child = container.GetChild(i);
                if (child == null) continue;
                // Skip grafted controls (our own pager UI lives inside the container as an
                // ignore-layout child) so they are never paginated/hidden.
                var le = child.GetComponent<LayoutElement>();
                if (le != null && le.ignoreLayout) continue;
                if (Exclusions.Count > 0 && Exclusions.Contains(child)) continue;
                _all.Add(child);
                if (filter == null || filter(child)) _visible.Add(child);
            }

            int pc = PageCount;
            if (_page >= pc) _page = pc - 1;
            if (_page < 0) _page = 0;
        }

        /// <summary>
        /// Show only the current page: filtered-out children and out-of-page children are
        /// deactivated, the page's children activated. Idempotent.
        /// </summary>
        public void Apply()
        {
            int start = _page * PageSize;
            int end = Math.Min(start + PageSize, _visible.Count);

            _inPage.Clear();
            for (int i = start; i < end; i++) _inPage.Add(_visible[i]);

            foreach (var child in _all)
            {
                bool want = _inPage.Contains(child);
                var go = child.gameObject;
                if (go != null && go.activeSelf != want) go.SetActive(want);
            }
        }

        /// <summary>The visible child at <paramref name="visibleIndex"/> (filtered list), or null.</summary>
        public Transform? VisibleAt(int visibleIndex)
            => (visibleIndex >= 0 && visibleIndex < _visible.Count) ? _visible[visibleIndex] : null;

        /// <summary>Enumerate the children on a given page (filtered list).</summary>
        public IEnumerable<Transform> ItemsOnPage(int page)
        {
            int s = page * PageSize, e = Math.Min(s + PageSize, _visible.Count);
            for (int i = s; i < e; i++) yield return _visible[i];
        }
    }
}
