#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using PerAspera.GameAPI.Wrappers.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Faction class
    /// Provides safe access to faction properties and operations
    /// DOC: Faction.md - Player and AI faction management
    /// </summary>
    public class Faction : WrapperBase
    {
        /// <summary>
        /// Initialize Faction wrapper with native faction object
        /// </summary>
        /// <param name="nativeFaction">Native faction instance from game</param>
        public Faction(object nativeFaction) : base(nativeFaction)
        {
            NativeObject = nativeFaction;
        }
        public InteractionManagerWrapper GetInteractionManage()
        {


            return new InteractionManagerWrapper( NativeObject.GetMemberValue<InteractionManager>("interactionManager"));                
        }
        /// <summary>
        /// Get the Handle for this Faction instance
        /// </summary>
        /// <returns>HandleWrapper for safe access to handle properties</returns>
        /// 
        public IHandleable? GetAsIHandleable()
        {
            try
            {
                // Try to cast the native object to IHandleable
                // This may fail in IL2CPP if the interface isn't properly exposed
                return (IHandleable)GetNativeObject();
            }
            catch (InvalidCastException)
            {
                Log.LogWarning("Cannot cast Faction native object to IHandleable - interface not available in IL2CPP context");
                return null;
            }
        }
        public HandleWrapper? GetHandle()
        {
            try
            {
                // Since Faction implements IHandleable, try to get the Handle property directly
                var handleObj = SafeInvoke<Handle>("get_handle");

                // Fallback: Try multiple possible field names for the handle using same pattern
                string[] possibleNames = {
                    "<Handle>k__BackingField", // Auto-property backing field (confirmed from debugger)
                    "handle",                  // Direct property name
                    "_handle",                 // Private field with underscore
                    "m_handle"                 // Unity-style private field
                };

                foreach (var fieldName in possibleNames)
                {
                    try
                    {
                        var handleObj2 = GetNativeField<Handle>(fieldName,BindingFlags.Instance | BindingFlags.NonPublic);
                        if (handleObj2 != null)
                        {
                            Log.LogInfo($"[GetHandle] Found handle using field '{fieldName}': {handleObj2}");
                            return HandleWrapper.FromNative(handleObj2);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogDebug($"[GetHandle] Field '{fieldName}' not found: {ex.Message}");
                    }
                }

                Log.LogWarning($"[GetHandle] No handle field found on {GetNativeObject()?.GetType().Name}");
                return null;
            }
            catch (Exception ex)
            {
                Log.LogError($"[GetHandle] Error accessing handle: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the raw Handle object for InteractionManager compatibility
        /// </summary>
        /// <returns>Raw handle object for InteractionManager calls</returns>
        public object? GetRawHandle()
        { 
            try
            {
                return GetNativeField<object>("handle");
            }
            catch (Exception ex)
            {
                Log.LogError($"[GetRawHandle] Error accessing handle: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create wrapper from native faction object
        /// </summary>
        public static Faction? FromNative(object? nativeFaction)
        {
            return nativeFaction != null ? new Faction(nativeFaction) : null;
        }

        /// <summary>
        /// Helper method to get native property with multiple naming conventions
        /// </summary>
        private T? GetNativePropertySafe<T>(string propertyName)
        {
            // Try multiple possible field names
            string[] possibleNames = { 
                $"_{propertyName}_k__BackingField", // Auto-property backing field
                propertyName,                        // Direct property name
                $"_{propertyName}",                 // Private field with underscore
                $"m_{propertyName}"                 // Unity-style private field
            };
            
            foreach (var fieldName in possibleNames)
            {
                try
                {
                    return GetNativeProperty<T>(fieldName);
                }
                catch
                {
                    // Try next name variant
                }
            }
            
            Log.LogDebug($"[GetNativePropertySafe] No field found for property '{propertyName}' on {GetNativeObject()?.GetType().Name}");
            return default(T);
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Faction name identifier
        /// Maps to: name field
        /// </summary>
        public string Name
        {
            get => GetNativePropertySafe<string>("name") ?? "Unknown";
        }
        
        /// <summary>
        /// Faction display name for UI
        /// Maps to: displayName field
        /// </summary>
        public string DisplayName
        {
            get => GetNativePropertySafe<string>("displayName") ?? Name;
        }
        
        /// <summary>
        /// Faction type (Player, AI, etc.)
        /// Maps to: factionType field
        /// </summary>
        public object? FactionType
        {
            get => GetNativePropertySafe<object>("factionType");
        }
        
        /// <summary>
        /// Is this the player faction?
        /// Maps to: isPlayerFaction property or comparison with playerFaction
        /// </summary>
        public bool IsPlayerFaction
        {
            get => GetNativePropertySafe<bool?>("isPlayerFaction") ?? false;
        }
        
        // ==================== RESOURCES ====================
        
        /// <summary>
        /// Main faction stockpile for resources
        /// Maps to: mainStockpile field
        /// </summary>
        public object? MainStockpile
        {
            get => GetNativePropertySafe<object>("mainStockpile");
        }
        
        /// <summary>
        /// Get resource stock amount safely
        /// Maps to: mainStockpile resource lookup
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <returns>Current stock amount or 0 if not found</returns>
        public float GetResourceStock(string resourceKey)
        {
            try
            {
                var stockpile = MainStockpile;
                if (stockpile == null) return 0f;
                
                // Try various methods to get resource stock
                var stockAmount = CallNative<float?>("GetResourceStock", resourceKey) ??
                                CallNative<float?>("GetStock", resourceKey) ??
                                CallNative<float?>("GetResourceAmount", resourceKey);
                
                return stockAmount ?? 0f;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get resource stock for {resourceKey}: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Add resource to faction with safe error handling
        /// Maps to: AddResource method or mainStockpile.AddResource
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <param name="amount">Amount to add (can be negative to remove)</param>
        /// <returns>True if operation succeeded</returns>
        public bool AddResource(string resourceKey, float amount)
        {
            try
            {
                // Try direct faction AddResource first
                var result = CallNative<bool?>("AddResource", resourceKey, amount);
                if (result.HasValue) return result.Value;
                
                // Try via stockpile
                var stockpile = MainStockpile;
                if (stockpile != null)
                {
                    var stockpileResult = CallNative<bool?>("AddResource", stockpile, resourceKey, amount);
                    if (stockpileResult.HasValue) return stockpileResult.Value;
                }
                
                Log.LogWarning($"Could not add resource {resourceKey} to faction {Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error adding resource {resourceKey} to faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== RELATIONS ====================
        
        /// <summary>
        /// Get relationship status with another faction
        /// Maps to: relations or diplomacy system
        /// </summary>
        /// <param name="otherFaction">Other faction to check relationship with</param>
        /// <returns>Relationship value (-100 to 100, or null if unknown)</returns>
        public float? GetRelationshipWith(Faction otherFaction)
        {
            if (!otherFaction.IsValidWrapper) return null;
            
            try
            {
                return CallNative<float?>("GetRelationship", otherFaction.GetNativeObject()) ??
                       CallNative<float?>("GetDiplomacyStatus", otherFaction.GetNativeObject()) ??
                       CallNative<float?>("GetStanding", otherFaction.GetNativeObject());
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get relationship between {Name} and {otherFaction.Name}: {ex.Message}");
                return null;
            }
        }
        
        // ==================== BUILDINGS ====================
        
        /// <summary>
        /// Get all buildings owned by this faction
        /// Maps to: buildings collection or planet filtering
        /// </summary>
        /// <returns>List of buildings owned by this faction</returns>
        public List<Building> GetBuildings()
        {
            try
            {
                var buildings = CallNative<object>("get_buildings");
                if (buildings == null)
                {
                    // Use BaseGame architecture (corrected approach)
                    // DOC: BaseGame-Architecture-Corrections.md - Direct planet access
                    try
                    {
                        var planet = Planet.GetCurrent();
                        if (planet != null)
                        {
                            var planetBuildings = CallNative<object>("get_buildings", planet.GetNativeObject());
                            if (planetBuildings is System.Collections.IEnumerable planetEnumerable)
                            {
                                var planetBuildingWrappers = new List<Building>();
                                foreach (var building in planetEnumerable)
                                {
                                    if (building != null)
                                    {
                                        // Filter by faction ownership if possible
                                        var buildingWrapper = new Building(building);
                                        if (buildingWrapper.IsValidWrapper)
                                        {
                                            planetBuildingWrappers.Add(buildingWrapper);
                                        }
                                    }
                                }
                                return planetBuildingWrappers;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogDebug($"Failed to access buildings via BaseGame: {ex.Message}");
                    }
                    
                    return new List<Building>();
                }
                
                // Convert native buildings to wrappers
                var buildingWrappers = new List<Building>();
                var enumerable = buildings as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingWrappers.Add(new Building(building));
                        }
                    }
                }
                
                return buildingWrappers;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get buildings for faction {Name}: {ex.Message}");
                return new List<Building>();
            }
        }
        
        // ==================== AI BEHAVIOR ====================
        
        /// <summary>
        /// AI difficulty level (for AI factions)
        /// Maps to: aiDifficulty or difficultyLevel field
        /// </summary>
        public int AIDifficulty
        {
            get => GetNativeProperty<int?>("aiDifficulty") ?? 
                   GetNativeProperty<int?>("difficultyLevel") ?? 0;
        }
        
        /// <summary>
        /// Is this faction controlled by AI?
        /// Maps to: isAI field or !isPlayerFaction
        /// </summary>
        public bool IsAI
        {
            get => GetNativeProperty<bool?>("isAI") ?? !IsPlayerFaction;
        }
        
        /// <summary>
        /// AI personality type (aggressive, defensive, etc.)
        /// Maps to: aiPersonality or behaviorType field
        /// </summary>
        public string AIPersonality
        {
            get => GetNativeProperty<string>("aiPersonality") ?? 
                   GetNativeProperty<string>("behaviorType") ?? "default";
        }
        
        // ==================== TECHNOLOGY ====================
        
        /// <summary>
        /// Check if faction has researched a specific technology
        /// Maps to: researchedTechnologies or techTree system
        /// </summary>
        /// <param name="technologyKey">Technology key to check</param>
        /// <returns>True if technology is researched</returns>
        public bool HasTechnology(string technologyKey)
        {
            try
            {
                return CallNative<bool?>("HasTechnology", technologyKey) ??
                       CallNative<bool?>("IsTechResearched", technologyKey) ??
                       CallNative<bool?>("HasResearched", technologyKey) ?? false;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to check technology {technologyKey} for faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Research a technology for this faction
        /// Maps to: ResearchTechnology method
        /// </summary>
        /// <param name="technologyKey">Technology key to research</param>
        /// <returns>True if research was initiated successfully</returns>
        public bool ResearchTechnology(string technologyKey)
        {
            try
            {
                var result = CallNative<bool?>("ResearchTechnology", technologyKey);
                if (result.HasValue) return result.Value;
                
                Log.LogWarning($"Could not research technology {technologyKey} for faction {Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error researching technology {technologyKey} for faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get faction color for UI display
        /// Maps to: color or factionColor field
        /// </summary>
        public System.Drawing.Color GetColor()
        {
            try
            {
                var color = GetNativeProperty<object>("color") ?? GetNativeProperty<object>("factionColor");
                if (color != null)
                {
                    // Convert Unity Color to System.Drawing.Color if needed
                    return ExtractColor(color);
                }
                return System.Drawing.Color.Gray; // Default color
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get color for faction {Name}: {ex.Message}");
                return System.Drawing.Color.Gray;
            }
        }
        
        private System.Drawing.Color ExtractColor(object unityColor)
        {
            try
            {
                // Access Unity Color properties directly via reflection
                var r = unityColor.GetFieldValue<float?>("r") ?? 0.5f;
                var g = unityColor.GetFieldValue<float?>("g") ?? 0.5f;
                var b = unityColor.GetFieldValue<float?>("b") ?? 0.5f;
                var a = unityColor.GetFieldValue<float?>("a") ?? 1.0f;
                
                return System.Drawing.Color.FromArgb(
                    (int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
            }
            catch
            {
                return System.Drawing.Color.Gray;
            }
        }
        
        /// <summary>
        /// Get the GameEventBus for this faction to dispatch commands
        /// Accesses protected _gameEventBus field via reflection
        /// </summary>
        /// <returns>GameEventBus instance or null if not accessible</returns>
        public object? GetGameEventBus()
        {
            Log.LogInfo($"[GetGameEventBus] Searching for _gameEventBus field on type: {GetNativeObject()?.GetType().Name}");

            // Use new debugging tools
            DebugGameEventBus();

            // Try property access first (get__gameEventBus)
            var gameEventBus = SafeInvoke<object>("get__gameEventBus");
            Log.LogInfo($"[GetGameEventBus] Property access result: {gameEventBus?.GetType().Name ?? "null"}");

            if (gameEventBus == null)
            {
                // Try direct field access with different names
                gameEventBus = GetNativeField<object>("_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance) ??
                              GetNativeField<object>("gameEventBus", BindingFlags.Public | BindingFlags.Instance) ??
                              GetNativeField<object>("m_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance);
                Log.LogInfo($"[GetGameEventBus] Field access result: {gameEventBus?.GetType().Name ?? "null"}");

                if (gameEventBus == null)
                {
                    // Try with FlattenHierarchy
                    gameEventBus = GetNativeField<object>("_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) ??
                                  GetNativeField<object>("gameEventBus", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    Log.LogInfo($"[GetGameEventBus] Hierarchy field access result: {gameEventBus?.GetType().Name ?? "null"}");
                }
            }

            return gameEventBus;
        }

        #region Command Execution Methods

        /// <summary>
        /// Execute a resource import command for this faction
        /// </summary>
        /// <param name="resourceType">Type of resource to import (e.g., "WATER", "ICE", "CHG")</param>
        /// <param name="amount">Amount of resource to import</param>
        /// <returns>True if command executed successfully</returns>
        public bool ExecuteResourceImportCommand(string resourceType, float amount = 1000f)
        {
            InteractionManagerWrapper a = GetInteractionManage();

            var importAction = PerAspera.GameAPI.Wrappers.ResourceCommandHelper.CreateNativeTextAction(resourceType, amount);



           return  a.DispatchAction(NativeObject,GetGameEventBus(), importAction,"hello" );


        }

        /// <summary>
        /// Execute a custom command for this faction
        /// </summary>
        /// <param name="commandType">Type of command to execute</param>
        /// <param name="parameters">Command parameters as key-value pairs</param>
        /// <returns>True if command executed successfully</returns>
        public bool ExecuteCustomCommand(string commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                Log.LogInfo($"🎯 Executing custom command: {commandType} for faction {Name}");

                var handle = GetHandle();
                if (handle == null)
                {
                    Log.LogError($"❌ Cannot get handle for faction {Name} - custom command execution failed");
                    return false;
                }
                Type fType = NativeObject.GetIl2CppType();
                // Use the SDK command helper
                bool success = PerAspera.GameAPI.Wrappers.ResourceCommandHelper.ExecuteResourceImportCommand(
                    (GameAPI.Native.Faction) NativeObject , commandType, parameters?.ContainsKey("amount") == true ? Convert.ToSingle(parameters["amount"]) : 1000f);

                if (success)
                {
                    Log.LogInfo($"✅ Custom command executed successfully: {commandType}");
                }
                else
                {
                    Log.LogError($"❌ Custom command failed: {commandType}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Custom command execution failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get debug information about command execution capabilities
        /// </summary>
        /// <returns>Debug string with command execution status</returns>
        public string GetCommandDebugInfo()
        {
            try
            {
                var handle = GetHandle();
                var handleStatus = handle != null ? "Available" : "Not Available";

                return $"Faction Command Debug Info:\n" +
                       $"- Name: {Name}\n" +
                       $"- Handle Status: {handleStatus}\n" +
                       $"- Is Player Faction: {IsPlayerFaction}\n" +
                       $"- Is Valid: {IsValidWrapper}\n" +
                       $"- Command Execution: {(handle != null ? "Ready" : "Not Ready")}";
            }
            catch (Exception ex)
            {
                return $"Faction Command Debug Info: Error - {ex.Message}";
            }
        }

        #endregion

        /// <summary>
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"Faction[{Name}] (Valid: {IsValidWrapper}, Player: {IsPlayerFaction}, AI: {IsAI})";
        }
    }
}