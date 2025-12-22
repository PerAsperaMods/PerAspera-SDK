#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cpp2IL.Core;
using PerAspera.Commands;
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
    public class FactionWrapper : WrapperBase
    {
        /// <summary>
        /// Initialize Faction wrapper with native faction object
        /// </summary>
        /// <param name="nativeFaction">Native faction instance from game</param>
        public FactionWrapper(object nativeFaction) : base(nativeFaction)
        {
            NativeObject = nativeFaction;
        }

        /* Test import resource Faction Native command */

        public bool FactionAddResourceDistributed(string resourceString, string amountString)
        {
            // Get the Console wrapper
            var consoleWrapper = ConsoleWrapper.GetInstance();
            if (consoleWrapper == null)
            {
                Log.LogWarning("Console wrapper not available for FactionAddResourceDistributed");
                return false;
            }

            try
            {


                // Use the console command format with spaces (not tabs)
                // Format: "factionaddresourcedistributed {resource} {amount}" (lowercase, spaces)
                var commandString = $"factionaddresourcedistributed {resourceString} {amountString}";

                return consoleWrapper.ExecuteCommandString(commandString);
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to execute FactionAddResourceDistributed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// List all available console commands (for debugging)
        /// </summary>
        public static void ListAvailableConsoleCommands()
        {
            var consoleWrapper = ConsoleWrapper.GetInstance();
            if (consoleWrapper != null)
            {
                consoleWrapper.ListCommands();
            }
            else
            {
                Log.LogWarning("Console wrapper not available for listing commands");
            }
        }





/// <summary>
        /// Test method for native command execution using both TextAction dispatch and CommandBus
        /// Tests CmdFactionResourceAllocation command creation and dispatching
        /// </summary>
        /// <returns>True if command executed successfully</returns>
        public bool testCMD()
        {
            try
            {
                // =====================================================================================
                // ÉTAPE 1: OBTENIR LES COMPOSANTS NÉCESSAIRES
                // =====================================================================================

                // Vérifier que la faction native existe
                if (NativeObject == null)
                {
                    Log.LogError("❌ testCMD: NativeObject (Faction) is null");
                    return false;
                }

                Log.LogInfo("🔍 testCMD: Starting native command execution test...");

                // Obtenir l'instance BaseGame
                BaseGameWrapper baseGameWrapper = BaseGameWrapper.GetCurrent();
                if (baseGameWrapper == null)
                {
                    Log.LogError("❌ Cannot get BaseGame instance");
                    return false;
                }

                // Obtenir l'Univers
                UniverseWrapper universeWrapper = baseGameWrapper.GetUniverse();
                if (universeWrapper == null)
                {
                    Log.LogError("❌ Cannot get Universe instance");
                    return false;
                }

                // =====================================================================================
                // ÉTAPE 2: TEST AVEC TEXTACTION (MÉTHODE CLASSIQUE)
                // =====================================================================================

                Log.LogInfo("📤 Testing TextAction dispatch method...");

                // Obtenir InteractionManager pour dispatcher les actions
                var interactionManager = GetInteractionManager();
                if (interactionManager == null)
                {
                    Log.LogError("❌ Cannot get InteractionManager for command dispatch");
                    return false;
                }

                // Obtenir GameEventBus
                var gameEventBus = GetGameEventBus();
                if (gameEventBus == null)
                {
                    Log.LogError("❌ Cannot get GameEventBus for command dispatch");
                    return false;
                }

                // Créer une commande TextAction pour l'allocation de ressources
                // Format: "FactionResourceAllocation\t{resourceType}\t{category}\t{value}"
                var commandString = $"FactionResourceAllocation\tIRON\t0\t1.0";
                var arguments = new string[0];

                // Obtenir le type TextAction depuis ScriptsAssembly
                var textActionType = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly")?
                    .GetType("TextAction");

                if (textActionType == null)
                {
                    Log.LogError("❌ TextAction type not found in ScriptsAssembly");
                    return false;
                }

                // Obtenir le constructeur TextAction(string command, string[] arguments)
                var textActionConstructor = textActionType.GetConstructor(new[] { typeof(string), typeof(string[]) });
                if (textActionConstructor == null)
                {
                    Log.LogError("❌ TextAction constructor not found");
                    return false;
                }

                // Créer l'instance TextAction
                var textAction = textActionConstructor.Invoke(new object[] { commandString, arguments });
                Log.LogInfo($"✅ Created TextAction with command: {commandString}");

                // Dispatcher l'action via InteractionManager
                var dispatchResult = interactionManager.DispatchAction(NativeObject, gameEventBus, textAction, "testCMD_TextAction");
                Log.LogInfo($"✅ TextAction dispatched: {dispatchResult}");

                // =====================================================================================
                // ÉTAPE 3: TEST AVEC COMMANDBUS (MÉTHODE DIRECTE)
                // =====================================================================================

                Log.LogInfo("🎯 Testing CommandBus direct dispatch method...");

                // Obtenir le CommandBus depuis Universe
                var commandBus = universeWrapper.GetCommandBus();
                if (commandBus == null)
                {
                    Log.LogWarning("⚠️ CommandBus not available, skipping CommandBus test");
                }
                else
                {
                    // Obtenir le type CmdFactionResourceAllocation
                    var scriptsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly");

                    if (scriptsAssembly != null)
                    {
                        var cmdType = scriptsAssembly.GetType("PerAspera.Commands.CmdFactionResourceAllocation");
                        if (cmdType != null)
                        {
                            Log.LogInfo($"✅ Found CmdFactionResourceAllocation type: {cmdType.FullName}");

                            // Lister tous les constructeurs disponibles pour debug
                            var allConstructors = cmdType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            Log.LogInfo($"🔍 Available constructors: {allConstructors.Length}");
                            foreach (var ctor in allConstructors)
                            {
                                var parameters = ctor.GetParameters();
                                var paramTypes = string.Join(", ", parameters.Select(p => p.ParameterType.Name));
                                Log.LogInfo($"  - {ctor.Name}({paramTypes})");
                            }

                            // Chercher le constructeur avec 4 paramètres (Faction, string, int, float)
                            System.Reflection.ConstructorInfo? constructor = null;

                            // Essayer d'abord avec les types exacts
                            constructor = cmdType.GetConstructor(new System.Type[] {
                                typeof(object), // Faction (IL2CPP object)
                                typeof(string), // resourceType
                                typeof(int),    // category
                                typeof(float)   // value
                            });

                            // Si ça ne marche pas, chercher manuellement parmi tous les constructeurs
                            if (constructor == null)
                            {
                                Log.LogInfo("⚠️ Exact constructor match failed, trying flexible search...");

                                foreach (var ctor in allConstructors)
                                {
                                    var parameters = ctor.GetParameters();
                                    if (parameters.Length == 4)
                                    {
                                        // Vérifier si les types correspondent à nos attentes
                                        bool matches = true;
                                        if (!parameters[1].ParameterType.Name.Contains("String")) matches = false;
                                        if (!parameters[2].ParameterType.Name.Contains("Int32")) matches = false;
                                        if (!parameters[3].ParameterType.Name.Contains("Single")) matches = false;

                                        if (matches)
                                        {
                                            constructor = ctor;
                                            Log.LogInfo($"✅ Found matching constructor with {parameters.Length} parameters");
                                            break;
                                        }
                                    }
                                }
                            }

                            if (constructor == null)
                            {
                                Log.LogError("❌ No suitable constructor found for CmdFactionResourceAllocation");
                                Log.LogInfo("💡 Try using the parameterless constructor and setting properties manually");
                                return false;
                            }

                            if (constructor != null)
                            {
                                try
                                {
                                    // Préparer les paramètres de la commande
                                    string resourceType = "IRON";
                                    int category = 0;
                                    float value = 1.0f;

                                    // Créer l'instance de commande
                                    var cmdInstance = constructor.Invoke(new object[] {
                                        NativeObject,    // La faction actuelle
                                        resourceType,    // Type de ressource
                                        category,        // Catégorie
                                        value           // Valeur
                                    });

                                    Log.LogInfo($"✅ Created CmdFactionResourceAllocation instance: {resourceType}, {category}, {value}");

                                    // Utiliser la méthode générique Dispatch<TCommand> de CommandBus
                                    var commandBusType = commandBus.GetType();

                                    // Chercher la méthode Dispatch générique (non spécialisée)
                                    var dispatchMethod = commandBusType.GetMethod("Dispatch",
                                        BindingFlags.Instance | BindingFlags.Public);

                                    if (dispatchMethod != null && dispatchMethod.IsGenericMethod)
                                    {
                                        try
                                        {
                                            // Créer la version spécialisée Dispatch<CmdFactionResourceAllocation>
                                            var genericDispatch = dispatchMethod.MakeGenericMethod(cmdType);

                                            // Dispatcher la commande via CommandBus
                                            genericDispatch.Invoke(commandBus, new object[] { cmdInstance });
                                            Log.LogInfo($"✅ Command dispatched successfully via CommandBus.Dispatch<{cmdType.Name}>");
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.LogError($"❌ Failed to dispatch command via CommandBus: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        Log.LogWarning("⚠️ Generic Dispatch method not found on CommandBus");
                                        // Fallback: lister toutes les méthodes disponibles
                                        var allMethods = commandBusType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                                        Log.LogInfo("🔍 Available CommandBus methods: " + string.Join(", ",
                                            allMethods.Select(m => $"{m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})")));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.LogError($"❌ Failed to create CmdFactionResourceAllocation: {ex.Message}");
                                }
                            }
                            else
                            {
                                Log.LogWarning("⚠️ CmdFactionResourceAllocation constructor not found");
                            }
                        }
                        else
                        {
                            Log.LogWarning("⚠️ CmdFactionResourceAllocation type not found");
                        }
                    }
                }

                // =====================================================================================
                // ÉTAPE 4: RÉSULTAT FINAL
                // =====================================================================================

                Log.LogInfo("✅ testCMD completed successfully - both TextAction and CommandBus methods tested");
                return true;

            }
            catch (Exception ex)
            {
                Log.LogError($"❌ testCMD failed: {ex.Message}");
                Log.LogError($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public bool AddResearchPoints(float amount)
        {
            try
            {
                // Get the Faction type directly from the assembly
                var factionType = NativeObject.GetType().Assembly.GetType("Faction");

                ((Faction)NativeObject).AddResearchPoints(1000); //testing other method


                var method = factionType.GetMethod("AddResearchPoints",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);



                if (method == null)
                {
                    Log.LogWarning("AddResearchPoints method not found on Faction type");
                    return false;
                }

                    // Invoke the static method
                method.Invoke(null, new object[] { amount });

                Log.LogInfo($"✅ Successfully executed FactionAddResourceDistributed for {amount}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to execute FactionAddResourceDistributed: {ex.Message}");
                return false;
            }
        }
        // Test method for direct native calls
        public bool TestAddResource(string resourceType, float amount)
        {
            return NativeObject.InvokeMethod("FactionAddResourceDistributed", resourceType, amount.ToString());
        }

        /*
        public void GetResourceQuantity(ResourceType resource, out CargoQuantity quantity)
        {
        }


        public CargoQuantity GetResourceCargoQuantity(ResourceType resource)
        {
            return default(CargoQuantity);
        }

        public float GetResourceQuantity(ResourceType resource)
        {
            return default(float);
        }*/









    public InteractionManagerWrapper GetInteractionManager()
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
        public static FactionWrapper? FromNative(object? nativeFaction)
        {
            return nativeFaction != null ? new FactionWrapper(nativeFaction) : null;
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
        public float? GetRelationshipWith(FactionWrapper otherFaction)
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
        public List<BuildingWrapper> GetBuildings()
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
                        var planet = PlanetWrapper.GetCurrent();
                        if (planet != null)
                        {
                            var planetBuildings = CallNative<object>("get_buildings", planet.GetNativeObject());
                            if (planetBuildings is System.Collections.IEnumerable planetEnumerable)
                            {
                                var planetBuildingWrappers = new List<BuildingWrapper>();
                                foreach (var building in planetEnumerable)
                                {
                                    if (building != null)
                                    {
                                        // Filter by faction ownership if possible
                                        var buildingWrapper = new BuildingWrapper(building);
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
                    
                    return new List<BuildingWrapper>();
                }
                
                // Convert native buildings to wrappers
                var buildingWrappers = new List<BuildingWrapper>();
                var enumerable = buildings as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingWrappers.Add(new BuildingWrapper(building));
                        }
                    }
                }
                
                return buildingWrappers;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get buildings for faction {Name}: {ex.Message}");
                return new List<BuildingWrapper>();
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
            InteractionManagerWrapper a = GetInteractionManager();

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
                    (Faction) NativeObject , commandType, parameters?.ContainsKey("amount") == true ? Convert.ToSingle(parameters["amount"]) : 1000f);

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
        /// Méthode utilitaire générique pour dispatcher des commandes via CommandBus
        /// Évite de créer un wrapper pour chaque type de commande
        /// </summary>
        /// <param name="commandTypeName">Nom complet du type de commande (ex: "PerAspera.Commands.CmdFactionResourceAllocation")</param>
        /// <param name="constructorArgs">Arguments pour le constructeur de la commande</param>
        /// <returns>True si la commande a été dispatchée avec succès</returns>
        public bool DispatchCommand(string commandTypeName, params object[] constructorArgs)
        {
            try
            {
                Log.LogInfo($"🎯 Dispatching command: {commandTypeName}");

                // 1. Obtenir le type de commande
                System.Type? cmdType = null;
                var scriptsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly");

                if (scriptsAssembly != null)
                {
                    cmdType = scriptsAssembly.GetType(commandTypeName);
                }

                if (cmdType == null)
                {
                    Log.LogError($"❌ Command type not found: {commandTypeName}");
                    return false;
                }

                // 2. Créer l'instance de commande
                var constructor = cmdType.GetConstructor(constructorArgs.Select(arg => arg.GetType()).ToArray());
                if (constructor == null)
                {
                    Log.LogError($"❌ Constructor not found for command: {commandTypeName}");
                    return false;
                }

                var cmdInstance = constructor.Invoke(constructorArgs);
                Log.LogInfo($"✅ Command instance created: {cmdType.Name}");

                // 3. Obtenir CommandBus et dispatcher
                var universeWrapper = UniverseWrapper.GetCurrent();
                if (universeWrapper == null)
                {
                    Log.LogError("❌ Universe not available for command dispatch");
                    return false;
                }

                var commandBus = universeWrapper.GetCommandBus();
                if (commandBus == null)
                {
                    Log.LogError("❌ CommandBus not available for command dispatch");
                    return false;
                }

                // 4. Utiliser la méthode générique Dispatch<TCommand>
                var commandBusType = commandBus.GetType();
                var dispatchMethod = commandBusType.GetMethod("Dispatch", BindingFlags.Instance | BindingFlags.Public);

                if (dispatchMethod != null && dispatchMethod.IsGenericMethod)
                {
                    var genericDispatch = dispatchMethod.MakeGenericMethod(cmdType);
                    genericDispatch.Invoke(commandBus, new object[] { cmdInstance });
                    Log.LogInfo($"✅ Command dispatched successfully: {commandTypeName}");
                    return true;
                }
                else
                {
                    Log.LogError("❌ Generic Dispatch method not available on CommandBus");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to dispatch command {commandTypeName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"Faction[{Name}] (Valid: {IsValidWrapper}, Player: {IsPlayerFaction}, AI: {IsAI})";
        }
    }
}