using System;
using System.Collections.Generic;
using UnityEngine;

namespace PerAspera.GameAPI.UI.Toolkit
{
    /// <summary>
    /// Reusable clone pool over a native uGUI prototype — the toolkit's "clone a native element"
    /// tier (highest visual fidelity: the clone IS a native widget). Generalises the in-game-proven
    /// <c>MultiOutputPanelPool</c> pattern (2026-06-12).
    ///
    /// <para><b>Rules baked in (learned the hard way):</b></para>
    /// <list type="bullet">
    /// <item><b>Never Destroy a clone</b> — Unity defers Destroy and a vanilla update loop may still
    /// hold the reference that frame → crash. Clones are pooled and toggled with SetActive.</item>
    /// <item><b>Reconfigure on reuse</b> — pass a configurator to <see cref="Sync"/>; it runs for each
    /// active clone every call so labels/sprites stay current.</item>
    /// <item><b>No reflection</b> — only typed interop members.</item>
    /// </list>
    /// </summary>
    /// <typeparam name="T">Component type of the prototype (e.g. a native row/card, or a ResourceItem).</typeparam>
    /// <example>
    /// // Clone a native row prototype N times into a container:
    /// var pool = new UIClonePool&lt;ResourceDetail&gt;(prototype, myContainer, "EconomyRow");
    /// pool.Sync(resources.Count, (row, i) =&gt; {
    ///     row.imageIcon.sprite = resources[i].iconName;
    ///     row.textQuantity.text = amounts[i].ToString();
    /// });
    /// </example>
    public sealed class UIClonePool<T> where T : Component
    {
        private readonly T _prototype;
        private readonly Transform _container;
        private readonly string _cloneName;
        private readonly List<T> _pool = new();

        /// <summary>Create a pool that instantiates clones of <paramref name="prototype"/> under <paramref name="container"/>.</summary>
        /// <param name="prototype">A live native widget to clone (kept untouched).</param>
        /// <param name="container">Parent transform for the clones (usually a layout container).</param>
        /// <param name="cloneName">Name tag applied to every clone (debug/identification).</param>
        public UIClonePool(T prototype, Transform container, string cloneName = "UIClone")
        {
            _prototype = prototype;
            _container = container;
            _cloneName = cloneName;
        }

        /// <summary>All clones ever created (active + pooled-inactive).</summary>
        public IReadOnlyList<T> Items => _pool;

        /// <summary>Number of clones currently active.</summary>
        public int ActiveCount
        {
            get
            {
                int n = 0;
                foreach (var c in _pool) if (IsAlive(c) && c.gameObject.activeSelf) n++;
                return n;
            }
        }

        /// <summary>
        /// Bring the active clone set to exactly <paramref name="count"/>: reuse pooled clones,
        /// instantiate the missing ones, deactivate the surplus. <paramref name="configure"/> runs for
        /// each active clone (index 0..count-1) so it can set sprites/text. Returns active count.
        /// </summary>
        public int Sync(int count, Action<T, int>? configure)
        {
            if (_prototype == null || _container == null) return 0;
            Cleanup();

            if (count < 0) count = 0;
            for (int i = 0; i < count; i++)
            {
                T clone = (i < _pool.Count) ? _pool[i] : CreateNew();
                try { configure?.Invoke(clone, i); }
                catch (Exception) { /* a single bad row must not break the rest */ }
                if (!clone.gameObject.activeSelf) clone.gameObject.SetActive(true);
            }

            for (int i = count; i < _pool.Count; i++)
                if (IsAlive(_pool[i]) && _pool[i].gameObject.activeSelf)
                    _pool[i].gameObject.SetActive(false);

            return count;
        }

        /// <summary>Rent the next reusable clone (creating one if needed) and activate it.</summary>
        public T Rent()
        {
            Cleanup();
            foreach (var c in _pool)
                if (IsAlive(c) && !c.gameObject.activeSelf) { c.gameObject.SetActive(true); return c; }
            var fresh = CreateNew();
            fresh.gameObject.SetActive(true);
            return fresh;
        }

        /// <summary>Deactivate every clone (data left intact for reuse).</summary>
        public void HideAll()
        {
            foreach (var c in _pool)
                if (IsAlive(c) && c.gameObject.activeSelf) c.gameObject.SetActive(false);
        }

        private T CreateNew()
        {
            var clone = UnityEngine.Object.Instantiate(_prototype, _container, false);
            clone.name = _cloneName;
            _pool.Add(clone);
            return clone;
        }

        private void Cleanup() => _pool.RemoveAll(c => !IsAlive(c));

        private static bool IsAlive(T c)
        {
            try { return c != null && c.gameObject != null; }
            catch { return false; }
        }
    }
}
