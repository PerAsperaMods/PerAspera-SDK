using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.Runtime;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using PerAspera.GameAPI.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for ResourcesPanel providing safe access to resource management UI
    /// Provides access to cached resource types and resource items
    /// </summary>
    public class ResourcesPanelWrapper : WrapperBase
    {
        public ResourcesPanelWrapper(object nativeResourcesPanel) : base(nativeResourcesPanel)
        {
        }

        /// <summary>
        /// Get current ResourcesPanel instance from game's canvas references
        /// </summary>
        public static ResourcesPanelWrapper? GetCurrent()
        {
            try
            {
                var baseGame = BaseGameWrapper.GetCurrent();
                if (baseGame == null)
                {
                    Log.LogWarning("BaseGameWrapper.GetCurrent() returned null");
                    return null;
                }

                // Access canvasRefs directly (it's a public field in BaseGame)
                var canvasRefs = baseGame.canvasRefs;
                if (canvasRefs == null)
                {
                    Log.LogWarning("BaseGame.canvasRefs is null - UI may not be fully initialized yet");
                    return null;
                }

                Log.LogInfo("BaseGame.canvasRefs found, accessing resourcesPanel field");

                // Get resourcesPanel as ResourcesPanelBase (declared type)
                var resourcesPanelBase = canvasRefs.GetFieldValue<object>("resourcesPanel");
                if (resourcesPanelBase == null)
                {
                    Log.LogWarning("canvasRefs.resourcesPanel is null - ResourcesPanel not initialized");
                    return null;
                }

                Log.LogInfo("ResourcesPanel found successfully");
                return new ResourcesPanelWrapper(resourcesPanelBase);
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get current ResourcesPanel: {ex.Message}");
                Log.LogError($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Dictionary mapping ResourceType to ResourceItem UI components
        /// Provides access to UI elements for each resource
        /// </summary>
        public Dictionary<ResourceType, ResourceItem> resourceItems
        {
            get
            {
                try
                {
                    return NativeObject.GetFieldValue<Dictionary<ResourceType, ResourceItem>>("resourceItems")
                           ?? new Dictionary<ResourceType, ResourceItem>();
                }
                catch (Exception ex)
                {
                    Log.LogError($"Failed to get resourceItems: {ex.Message}");
                    return new Dictionary<ResourceType, ResourceItem>();
                }
            }
        }

        /// <summary>
        /// Cached list of all available ResourceType instances
        /// Includes both vanilla and mod-added resources
        /// </summary>
        public List<ResourceType> resourceTypesCached
        {
            get
            {
                try
                {
                    var runtimeType = NativeObject.GetType();
                    Log.LogInfo($"Attempting to get resourceTypesCached from runtime type: {runtimeType.FullName}");

                    // The field is defined on ResourcesPanel, but we might have a ResourcesPanelBase
                    // Try to get the field from the actual ResourcesPanel type
                    var resourcesPanelType = System.Type.GetType("ResourcesPanel, Assembly-CSharp");
                    if (resourcesPanelType == null)
                    {
                        Log.LogError("Could not find ResourcesPanel type");
                        return new List<ResourceType>();
                    }

                    var fieldInfo = resourcesPanelType.GetField("resourceTypesCached", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (fieldInfo == null)
                    {
                        Log.LogError($"resourceTypesCached field not found on ResourcesPanel type");
                        return new List<ResourceType>();
                    }

                    Log.LogInfo($"Found resourceTypesCached field on ResourcesPanel, getting value...");
                    var result = fieldInfo.GetValue(NativeObject) as List<ResourceType>;

                    if (result == null)
                    {
                        Log.LogWarning("resourceTypesCached field returned null");
                        return new List<ResourceType>();
                    }
                    Log.LogInfo($"resourceTypesCached found with {result.Count} items");
                    return result;
                }
                catch (Exception ex)
                {
                    Log.LogError($"Failed to get resourceTypesCached: {ex.Message}");
                    Log.LogError($"Object type: {NativeObject?.GetType()?.FullName}");
                    return new List<ResourceType>();
                }
            }
        }
    }
}
