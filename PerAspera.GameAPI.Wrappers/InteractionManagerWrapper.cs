using System;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers.Core;
using PerAspera.GameAPI.Native;

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
    /// InteractionManagerWrapper.DispatchAction(faction.GetHandle(), eventBus, textAction, "CommandsDemo");
    /// </code>
    /// </example>
    public static class InteractionManagerWrapper
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("InteractionManager");
        private static System.Type? _interactionManagerType;
        
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
        public static bool DispatchAction(IHandleable handleable, object? gameEventBus, object textAction, string context)
        {
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
                // Find the DispatchAction method: DispatchAction(IHandleable, GameEventBus, TextAction, String)
                var method = InteractionManagerType.GetMethod("DispatchAction", 
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new System.Type[] { 
                        typeof(IHandleable), 
                        typeof(object), // GameEventBus (we don't have the exact type, use object)
                        typeof(object), // TextAction (we don't have the exact type, use object)
                        typeof(string) 
                    },
                    null);
                
                if (method == null)
                {
                    Log.LogError("❌ DispatchAction method not found with expected signature");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching Action: {textAction?.ToString() ?? "null"} Context: {context}");
                
                // Invoke the static method
                method.Invoke(null, new object[] { 
                    handleable, 
                    gameEventBus, 
                    textAction, 
                    context 
                });
                
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
        public static bool DispatchEvent(IHandleable handleable, object gameEventBus, object gameEvent)
        {
            if (InteractionManagerType == null)
            {
                Log.LogError("❌ InteractionManager type not available");
                return false;
            }
            
            try
            {
                // Find the DispatchAction method: DispatchAction(IHandleable, GameEventBus, GameEvent)
                var method = InteractionManagerType.GetMethod("DispatchAction", 
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new System.Type[] { 
                        typeof(IHandleable), 
                        typeof(object), // GameEventBus
                        typeof(object)  // GameEvent
                    },
                    null);
                
                if (method == null)
                {
                    Log.LogError("❌ DispatchAction(GameEvent) method not found");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching GameEvent");
                
                // Invoke the static method
                method.Invoke(null, new object[] { handleable, gameEventBus, gameEvent });
                
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
        public static bool DispatchActions(IHandleable handleable, object gameEventBus, System.Collections.Generic.List<object> textActions, string context)
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
                
                // Find the DispatchActions method: DispatchActions(IHandleable, GameEventBus, List<TextAction>, String)
                var method = InteractionManagerType.GetMethod("DispatchActions", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (method == null)
                {
                    Log.LogError("❌ DispatchActions method not found");
                    return false;
                }
                
                Log.LogInfo($"🚀 Dispatching {nativeActions.Count} Actions: {context}");
                
                // Invoke the static method
                method.Invoke(null, new object[] { handleable, gameEventBus, nativeActions, context });
                
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