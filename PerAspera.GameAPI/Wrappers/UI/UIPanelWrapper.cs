using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using UnityEngine;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Wrappers.UI
{
    /// <summary>
    /// Wrapper for native game UI panels (BuildingScreenPanel, etc).
    /// Provides safe access to game panel sprites, buttons, and properties.
    /// </summary>
    public class UIPanelWrapper : WrapperBase
    {
        private System.Type? _nativeType;

        public UIPanelWrapper(object nativePanel) : base(nativePanel)
        {
            if (nativePanel == null)
                throw new ArgumentNullException(nameof(nativePanel));

            _nativeType = nativePanel.GetType();
        }

        /// <summary>
        /// Typed BuildingScreenPanel proxy when the wrapped panel is one.
        /// </summary>
        public BuildingScreenPanel? AsBuildingScreenPanel
            => GetNativeObject() as BuildingScreenPanel;

        /// <summary>Find BuildingScreenPanel instance in the current scene (typed check).</summary>
        public static UIPanelWrapper? FindBuildingScreenPanel()
        {
            try
            {
                var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.Behaviour>();
                foreach (var behaviour in allObjects)
                    if (behaviour is BuildingScreenPanel)
                        return new UIPanelWrapper(behaviour);
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Get the upgradeNormalBackground sprite from the panel
        /// (typed read of BuildingScreenPanel.upgradeNormalBackground).
        /// </summary>
        public Sprite? GetPanelBackgroundSprite()
            => AsBuildingScreenPanel?.upgradeNormalBackground;

        /// <summary>Get panel by name (e.g., "statusPanel", "upgradePanel").</summary>
        public object? GetPanelByName(string panelName)
        {
            try
            {
                return Utilities.GetMemberValue(GetNativeObject(), new[] { panelName });
            }
            catch { return null; }
        }

        /// <summary>Get text component by name (e.g., "nameText", "statusText").</summary>
        public object? GetTextComponentByName(string textName)
        {
            try
            {
                return Utilities.GetMemberValue(GetNativeObject(), new[] { textName });
            }
            catch { return null; }
        }

        /// <summary>Get a field value by name (generic member access).</summary>
        public T? GetFieldValue<T>(string fieldName) where T : class
        {
            try
            {
                return Utilities.GetMemberValue<T>(GetNativeObject(), new[] { fieldName });
            }
            catch { return null; }
        }

        /// <summary>Check if panel is currently visible/active.</summary>
        public bool IsActive()
        {
            try
            {
                var nativeObj = GetNativeObject() as UnityEngine.Object;
                if (nativeObj is UnityEngine.GameObject go)
                    return go.activeSelf;

                if (nativeObj is UnityEngine.Component comp)
                    return comp.gameObject.activeSelf;

                return false;
            }
            catch { return false; }
        }

        public override string ToString() => $"UIPanelWrapper({_nativeType?.Name ?? "Unknown"})";
    }
}
#pragma warning restore CS1591
