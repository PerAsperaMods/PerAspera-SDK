using AsmResolver.PE.DotNet.Metadata.Tables;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Universe class
    /// Provides safe access to time, game state and universe-level properties
    /// 
    /// 📚 Vanilla Reference: F:\ModPeraspera\CleanedScriptAssemblyClass\Universe.md (290 fields, 143 methods)
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🌐 User Wiki: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/
    /// 🕰️ Time System: Game state, pause, time management functionality
    /// </summary>
    public class Universe : WrapperBase
    {
        public Universe(object nativeUniverse) : base(nativeUniverse)
        {
        }

        /// <summary>
        /// Get the current universe instance
        /// </summary>
        public static Universe? GetCurrent()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe != null ? new Universe(universe) : null;
        }
        public Faction GetPlayerFaction()
        {
            return NativeObject.InvokeMethod<object>("GetPlayerFaction") is { } nativeFaction
                ? new GameAPI.Wrappers.Faction(nativeFaction)
                : null;
        }


        // ==================== TIME PROPERTIES ====================

        /// <summary>
        /// Get current Martian sol (days passed)
        /// </summary>
        public int CurrentSol => SafeInvoke<int?>("GetDaysPassed") ?? 0;
        
        /// <summary>
        /// Get current game speed multiplier
        /// </summary>
        public float GameSpeed
        {
            get => SafeInvoke<float?>("GetGameSpeed") ?? 1.0f;
            set => SafeInvokeVoid("SetGameSpeed", value);
        }
        
        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsPaused => SafeInvoke<bool>("IsPaused");
        
        // ==================== GAME STATE ====================
        
        /// <summary>
        /// Get the planet wrapper instance
        /// </summary>
        public Planet? GetPlanet()
        {
            var nativePlanet = SafeInvoke<object>("GetPlanet");
            return nativePlanet != null ? new Planet(nativePlanet) : null;
        }
        
        /// <summary>
        /// Get the current planet instance (alias for GetPlanet for convenience)
        /// </summary>
        public Planet? CurrentPlanet => GetPlanet();
        
        /// <summary>
        /// Get the base game wrapper instance
        /// </summary>
        public BaseGame? GetBaseGame()
        {
            var nativeBaseGame = SafeInvoke<object>("GetBaseGame");
            return nativeBaseGame != null ? new BaseGame(nativeBaseGame) : null;
        }
        

        
        /// <summary>
        /// Get all factions in the universe
        /// Maps to: factions field or GetFactions() method
        /// </summary>
        public List<Faction> GetFactions()
        {
            try
            {
                var nativeFactions = SafeInvoke<object>("get_factions") ?? 
                                   SafeInvoke<object>("GetFactions");
                
                if (nativeFactions == null) return new List<Faction>();
                
                var factionWrappers = new List<Faction>();
                var enumerable = nativeFactions as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var faction in enumerable)
                    {
                        if (faction != null)
                        {
                            factionWrappers.Add(new Faction(faction));
                        }
                    }
                }
                
                return factionWrappers;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get factions: {ex.Message}");
                return new List<Faction>();
            }
        }
        
        // ==================== ACTIONS ====================
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void Pause()
        {
            SafeInvokeVoid("SetPaused", true);
        }
        
        /// <summary>
        /// Unpause the game
        /// </summary>
        public void Unpause()
        {
            SafeInvokeVoid("SetPaused", false);
        }
        
        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SafeInvokeVoid("SetPaused", !IsPaused);
        }
        
        // ==================== BLACKBOARD SYSTEM ====================
        
        /// <summary>
        /// Get the main blackboard instance
        /// Field: blackboardMain (public Blackboard)
        /// </summary>
        public BlackBoard? GetMainBlackBoard()
        {
            var nativeBlackboard = SafeGetField<object>("blackboardMain");
            return nativeBlackboard != null ? new BlackBoard(nativeBlackboard) : null;
        }
        
        /// <summary>
        /// Get a specific blackboard by name from the blackboards dictionary
        /// Field: blackboards (private Dictionary&lt;string, Blackboard&gt;)
        /// </summary>
        public BlackBoard? GetBlackBoard(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Warning("GetBlackBoard called with null or empty name");
                return null;
            }
            
            var blackboardsDict = SafeGetField<object>("blackboards");
            if (blackboardsDict == null)
            {
                Log.Warning("blackboards dictionary is null in Universe");
                return null;
            }
            
            try
            {
                // Access Dictionary<string, Blackboard> using IL2CPP interop
                var nativeBlackboard = blackboardsDict.InvokeMethod<object>("get_Item", name);
                return nativeBlackboard != null ? new BlackBoard(nativeBlackboard) : null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get blackboard '{name}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all blackboard names from the blackboards dictionary
        /// Field: blackboards (private Dictionary&lt;string, Blackboard&gt;)
        /// </summary>
        public IList<string>? GetBlackBoardNames()
        {
            var blackboardsDict = SafeGetField<object>("blackboards");
            if (blackboardsDict == null) return null;
            
            try
            {
                var keys = blackboardsDict.InvokeMethod<object>("get_Keys");
                return keys?.ConvertIl2CppList<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get blackboard names: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if a blackboard with the given name exists
        /// Field: blackboards (private Dictionary&lt;string, Blackboard&gt;)
        /// </summary>
        public bool HasBlackBoard(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            var blackboardsDict = SafeGetField<object>("blackboards");
            if (blackboardsDict == null) return false;
            
            try
            {
                return blackboardsDict.InvokeMethod<bool>("ContainsKey", name);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check if blackboard '{name}' exists: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Add a new blackboard to the universe
        /// Method: AddBlackboard(Blackboard blackboard)
        /// Note: Based on decompiled Universe.md method
        /// </summary>
        public void AddBlackBoard(BlackBoard blackboard)
        {
            if (blackboard?.GetNativeObject() == null)
            {
                Log.Warning("Cannot add null blackboard to Universe");
                return;
            }
            
            SafeInvokeVoid("AddBlackboard", blackboard);
        }
        
        /// <summary>
        /// Get count of blackboards in the universe
        /// Field: blackboards (private Dictionary&lt;string, Blackboard&gt;)
        /// </summary>
        public int GetBlackBoardCount()
        {
            var blackboardsDict = SafeGetField<object>("blackboards");
            if (blackboardsDict == null) return 0;
            
            try
            {
                return blackboardsDict.InvokeMethod<int>("get_Count");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get blackboard count: {ex.Message}");
                return 0;
            }
        }

        // ==================== INFO ====================

        public override string ToString()
        {
            var blackboardCount = GetBlackBoardCount();
            var mainBlackboardName = GetMainBlackBoard()?.Name ?? "None";
            return $"Universe: Sol {CurrentSol}, Speed={GameSpeed}x, Paused={IsPaused}, Blackboards={blackboardCount}, MainBB={mainBlackboardName}";
        }
    }
}
