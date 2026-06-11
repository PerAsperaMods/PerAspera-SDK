#nullable enable
using System;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for ResourcesPanel providing safe access to resource management UI.
    ///
    /// MIGRATION 2026-06-10 — interop typé : GameCanvasReferences.resourcesPanel,
    /// ResourcesPanel.resourceItems et resourceTypesCached sont tous exposés typés.
    /// Le downcast ResourcesPanelBase → ResourcesPanel passe par TryCast (IL2CPP).
    /// </summary>
    public class ResourcesPanelWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native panel (compat). Prefer the typed overload.</summary>
        public ResourcesPanelWrapper(object nativeResourcesPanel) : base(nativeResourcesPanel) { }

        /// <summary>Wraps a typed interop ResourcesPanelBase proxy.</summary>
        public ResourcesPanelWrapper(ResourcesPanelBase nativeResourcesPanel) : base(nativeResourcesPanel) { }

        /// <summary>Typed base proxy (null when the wrapper is invalid).</summary>
        public ResourcesPanelBase? NativeResourcesPanelBase => GetNativeObject() as ResourcesPanelBase;

        /// <summary>
        /// Typed ResourcesPanel proxy (null si l'instance n'est pas vue comme un
        /// ResourcesPanel concret côté managé).
        /// </summary>
        public ResourcesPanel? AsResourcesPanel => GetNativeObject() as ResourcesPanel;

        /// <summary>
        /// Get current ResourcesPanel instance from game's canvas references (typed chain).
        /// </summary>
        /// <example>var panel = ResourcesPanelWrapper.GetCurrent();</example>
        public static ResourcesPanelWrapper? GetCurrent()
        {
            var canvasRefs = BaseGameWrapper.GetCurrent()?.CanvasRefs;
            var panel = canvasRefs?.resourcesPanel;
            if (panel == null)
            {
                Log.LogWarning("canvasRefs.resourcesPanel is null - UI may not be fully initialized yet");
                return null;
            }
            return new ResourcesPanelWrapper(panel);
        }

        /// <summary>
        /// Dictionary mapping ResourceType to ResourceItem UI components (typed read).
        /// </summary>
        public Il2CppSystem.Collections.Generic.Dictionary<ResourceType, ResourceItem>? resourceItems
            => AsResourcesPanel?.resourceItems;

        /// <summary>
        /// Cached list of all available ResourceType instances (typed read).
        /// Includes both vanilla and mod-added resources.
        /// </summary>
        public Il2CppSystem.Collections.Generic.List<ResourceType>? resourceTypesCached
            => AsResourcesPanel?.resourceTypesCached;
    }
}
