using System;
using System.Reflection;
using System.Linq;
using BepInEx.Logging;
using Il2CppInterop.Runtime.InteropTypes;
using HarmonyLib;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using PerAspera.Events;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Static wrapper for InteractionManager functionality.
    /// Provides access to command dispatch system and text action processing.
    /// </summary>
    /// <example>
    /// <code>
    /// // Dispatch a resource import command
    /// var faction = Universe.GetCurrent().GetPlayerFaction();
    /// var textAction = TextAction.CreateAddResource("ice", "PlayerFaction", 10.0f);
    /// var eventBus = faction.GetGameEventBus();
    /// InteractionManagerWrapper.DispatchAction(faction.GetNativeObject(), eventBus, textAction, "CommandsDemo");
    /// </code>
    /// </example>
    public  class InteractionManagerWrapper
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("InteractionManager");
        private static System.Type? _interactionManagerType;
        private object   nativeObject;

        public InteractionManagerWrapper(object interactionM)
        {
            nativeObject = interactionM;
        }

        /// <summary>
        /// Gets the native InteractionManager type via reflection
        /// </summary>
        private static System.Type? InteractionManagerType
        {
            get
            {
                if (_interactionManagerType == null)
                {
                    _interactionManagerType = ReflectionHelpers.FindType("InteractionManager");
                    if (_interactionManagerType != null)
                    {
                        Log.LogInfo($"✅ Found InteractionManager type: {_interactionManagerType.FullName}");
                    }
                    else
                    {
                        Log.LogError("❌ Failed to find InteractionManager type");
                    }
                }
                return _interactionManagerType;
            }
        }
        
        /// <summary>
        /// Dispatches a text action command using the native InteractionManager.
        /// This is the main entry point for executing game commands.
        /// </summary>
        /// <param name="handleable">The handleable object (usually a faction handle)</param>
        /// <param name="gameEventBus">The GameEventBus for event processing</param>
        /// <param name="textAction">The TextAction command to execute (native object)</param>
        /// <param name="context">Context string for debugging (e.g., "CommandsDemo")</param>
        /// <returns>True if dispatch was successful, false otherwise</returns>
        public bool DispatchAction(object handleable, object? gameEventBus, object textAction, string context)
        {

           //  (IHandleable)handleable; // Ensure handleable implements IHandleable
            if (InteractionManagerType == null)
            {
                Log.LogError("❌ InteractionManager type not available");
                return false;
            }
            
            if (handleable == null)
            {
                Log.LogError("❌ DispatchAction: handleable is null");
                return false;
            }
            
            // Unwrap wrapper object if it has GetNativeObject — IL2CppExtensions (RS0030-exempt)
            if (AccessTools.Method(handleable.GetType(), "GetNativeObject") != null)
            {
                try
                {
                    handleable = handleable.InvokeMethod<object>("GetNativeObject");
                    Log.LogInfo($"✅ Extracted native object from wrapper: {handleable?.GetType().Name ?? "null"}");
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"⚠️ Failed to get native object from wrapper: {ex.Message}");
                }
            }
            
            if (textAction == null)
            {
                Log.LogError("❌ DispatchAction: textAction is null");
                return false;
            }
            
            if (gameEventBus == null)
            {
                Log.LogWarning("⚠️ DispatchAction: gameEventBus is null, command may not execute properly");
                return false;
            }
            
            try
            {
                // Find the DispatchAction method by parameter count to avoid interface dependencies
                var methods = InteractionManagerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "DispatchAction" && m.GetParameters().Length == 4).ToArray();
                
                if (methods.Length == 0)
                {
                    Log.LogError("❌ No DispatchAction methods found with 4 parameters");
                    return false;
                }
                
                var method = methods[0]; // Take first match
                
                if (method == null)
                {
                    Log.LogError("❌ DispatchAction method not found with expected signature");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching Action: {textAction?.ToString() ?? "null"} Context: {context}");

                // Use typed InteractionManager.DispatchAction — InteropDump ligne 766
                var nativeHandleable = handleable is Il2CppObjectBase hb ? new IHandleable(hb.Pointer) : handleable as IHandleable;
                var nativeBus = gameEventBus as GameEventBus;
                var nativeAction = textAction as TextAction;
                if (nativeHandleable == null || nativeBus == null || nativeAction == null)
                {
                    Log.LogError("❌ DispatchAction: failed to cast arguments to typed IL2CPP types");
                    return false;
                }
                InteractionManager.DispatchAction(nativeHandleable, nativeBus, nativeAction, context);

                Log.LogInfo($"✅ DispatchAction successful: {context}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ DispatchAction failed: {ex.Message}");
                Log.LogDebug($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Dispatches a GameEvent using the native InteractionManager.
        /// Alternative to text action dispatch for direct event processing.
        /// </summary>
        /// <param name="handleable">The handleable object</param>
        /// <param name="gameEventBus">The GameEventBus for event processing</param>
        /// <param name="gameEvent">The GameEvent to dispatch</param>
        /// <returns>True if dispatch was successful, false otherwise</returns>
        public static bool DispatchEvent(object handleable, object gameEventBus, object gameEvent)
        {
            if (InteractionManagerType == null)
            {
                Log.LogError("❌ InteractionManager type not available");
                return false;
            }
            
            try
            {
                // Find DispatchAction method with 3 parameters (handleable, GameEventBus, GameEvent)
                var methods = InteractionManagerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "DispatchAction" && m.GetParameters().Length == 3)
                    .ToArray();
                
                var method = methods.FirstOrDefault();
                if (method == null)
                {
                    Log.LogError("❌ DispatchAction(GameEvent) method with 3 parameters not found");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching GameEvent using method: {method}");

                // Use typed InteractionManager.DispatchAction — InteropDump ligne 780
                var nativeHandleable = handleable is Il2CppObjectBase hb ? new IHandleable(hb.Pointer) : handleable as IHandleable;
                var nativeBus = gameEventBus as GameEventBus;
                if (nativeHandleable == null || nativeBus == null || gameEvent == null)
                {
                    Log.LogError("❌ DispatchEvent: failed to cast arguments to typed IL2CPP types");
                    return false;
                }
                if (gameEvent is GameEvent nativeEvent)
                {
                    InteractionManager.DispatchAction(nativeHandleable, nativeBus, nativeEvent);
                }
                else
                {
                    Log.LogError("❌ DispatchEvent: gameEvent is not a GameEvent struct");
                    return false;
                }

                Log.LogInfo($"✅ DispatchEvent successful");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ DispatchEvent failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Dispatches multiple text actions using the native InteractionManager.
        /// Batch processing for multiple commands.
        /// </summary>
        /// <param name="handleable">The handleable object</param>
        /// <param name="gameEventBus">The GameEventBus for event processing</param>
        /// <param name="textActions">List of TextAction commands to execute (native objects)</param>
        /// <param name="context">Context string for debugging</param>
        /// <returns>True if all dispatches were successful, false otherwise</returns>
        public static bool DispatchActions(object handleable, object gameEventBus, System.Collections.Generic.List<object> textActions, string context)
        {
            if (InteractionManagerType == null)
            {
                Log.LogError("❌ InteractionManager type not available");
                return false;
            }
            
            try
            {
                // Convert wrappers to native objects (already native in this case)
                var nativeActions = new System.Collections.Generic.List<object>(textActions);
                
                // Find the DispatchActions method by parameter count to avoid interface dependencies
                var methods = InteractionManagerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "DispatchActions" && m.GetParameters().Length == 4).ToArray();
                
                if (methods.Length == 0)
                {
                    Log.LogError("❌ No DispatchActions methods found with 4 parameters");
                    return false;
                }
                
                var method = methods[0]; // Take first match
                
                if (method == null)
                {
                    Log.LogError("❌ DispatchActions method not found");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching {nativeActions.Count} Actions: {context}");

                // Build typed Il2CppSystem.Collections.Generic.List<TextAction>
                var il2Actions = new Il2CppSystem.Collections.Generic.List<TextAction>();
                foreach (var a in nativeActions) { if (a is TextAction ta) il2Actions.Add(ta); }
                var nativeHandleable = handleable is Il2CppObjectBase hb ? new IHandleable(hb.Pointer) : handleable as IHandleable;
                var nativeBus = gameEventBus as GameEventBus;
                if (nativeHandleable == null || nativeBus == null)
                {
                    Log.LogError("❌ DispatchActions: failed to cast arguments");
                    return false;
                }
                InteractionManager.DispatchActions(nativeHandleable, nativeBus, il2Actions, context);

                Log.LogInfo($"✅ DispatchActions successful: {context}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ DispatchActions failed: {ex.Message}");
                return false;
            }
        }
    }
}