using System;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Centralized helper for accessing Keeper instances from all major game systems
    /// Provides easy access to distributed Keeper architecture:
    /// - Universe.keeper → Factions, planets, resources
    /// - BaseGame.keeper → Core game state
    /// - GameEventBus._keeper → Event system entities  
    /// - Swarm._keeper → Unit swarm management
    /// - Way._keeper → Pathfinding entities
    /// 
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public static class KeeperAccessHelper
    {
        private static ManualLogSource? _logger;
        
        private static ManualLogSource Logger
        {
            get
            {
                if (_logger == null)
                    _logger = BepInEx.Logging.Logger.CreateLogSource("KeeperAccess");
                return _logger;
            }
        }

        // ==================== UNIVERSE KEEPER (Primary for faction commands) ====================
        
        /// <summary>
        /// Get Universe Keeper - manages factions, planets, and resources
        /// 🎯 PRIMARY for faction resource commands
        /// </summary>
        public static KeeperWrapper? GetUniverseKeeper()
        {
            try
            {
                var universe = UniverseWrapper.GetCurrent();
                return universe?.GetKeeper();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GetUniverseKeeper failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Universe KeeperMap - Handle→Object mapping for factions/planets/resources
        /// 🎯 IDEAL for InteractionManager.DispatchAction with faction commands
        /// </summary>
        public static KeeperMapWrapper? GetUniverseKeeperMap()
        {
            return GetUniverseKeeper()?.GetKeeperMap();
        }

        // ==================== BASE GAME KEEPER (Core game state) ====================
        
        /// <summary>
        /// Get BaseGame Keeper - manages core game state
        /// </summary>
        public static KeeperWrapper? GetBaseGameKeeper()
        {
            try
            {
                var baseGame = BaseGameWrapper.GetCurrent();
                return baseGame?.GetKeeper();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GetBaseGameKeeper failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get BaseGame KeeperMap - Handle→Object mapping for core game entities
        /// </summary>
        public static KeeperMapWrapper? GetBaseGameKeeperMap()
        {
            return GetBaseGameKeeper()?.GetKeeperMap();
        }

        // ==================== SPECIALIZED KEEPERS ====================
        
        /// <summary>
        /// Get GameEventBus Keeper - manages event system entities
        /// </summary>
        public static KeeperWrapper? GetEventBusKeeper()
        {
            try
            {
                // Access via BaseGame or direct instantiation if available
                // This would need GameEventBus.GetCurrent() method
                Logger.LogInfo("GameEventBus direct access not yet implemented");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GetEventBusKeeper failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Swarm Keeper - manages unit swarm entities
        /// </summary>
        public static KeeperWrapper? GetSwarmKeeper()
        {
            try
            {
                // This would need Swarm.GetCurrent() method
                Logger.LogInfo("Swarm direct access not yet implemented");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GetSwarmKeeper failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Way Keeper - manages pathfinding entities
        /// </summary>
        public static KeeperWrapper? GetWayKeeper()
        {
            try
            {
                // This would need Way.GetCurrent() method
                Logger.LogInfo("Way direct access not yet implemented");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"GetWayKeeper failed: {ex.Message}");
                return null;
            }
        }

        // ==================== COMMAND SYSTEM HELPERS ====================

        /// <summary>
        /// Get the appropriate KeeperMap for faction resource commands
        /// Returns Universe.keeper.map (best for faction operations)
        /// Fallback to BaseGame.keeper.map if needed
        /// </summary>
        public static KeeperMapWrapper? GetCommandKeeperMap()
        {
            // Try Universe keeper first (handles factions, planets, resources)
            var universeKeeperMap = GetUniverseKeeperMap();
            if (universeKeeperMap != null)
            {
                Logger.LogInfo("Using Universe.keeper.map for commands");
                return universeKeeperMap;
            }

            // Fallback to BaseGame keeper
            var baseGameKeeperMap = GetBaseGameKeeperMap();
            if (baseGameKeeperMap != null)
            {
                Logger.LogInfo("Using BaseGame.keeper.map for commands (fallback)");
                return baseGameKeeperMap;
            }

            Logger.LogWarning("No KeeperMap available for commands");
            return null;
        }

        /// <summary>
        /// Convert any entity Handle to IHandleable using the appropriate KeeperMap
        /// 🎯 Perfect for InteractionManager.DispatchAction
        /// </summary>
        /// <param name="handle">Entity Handle</param>
        /// <returns>IHandleable for DispatchAction, or null if conversion failed</returns>
        public static object? ConvertHandleToIHandleable(object handle)
        {
            if (handle == null) return null;

            try
            {
                // Try Universe KeeperMap first
                var universeKeeperMap = GetUniverseKeeperMap();
                if (universeKeeperMap != null)
                {
                    var result = universeKeeperMap.FindBase(handle);
                    if (result != null)
                    {
                        Logger.LogInfo($"Converted Handle to IHandleable via Universe.keeper.map: {result.GetType().Name}");
                        return result;
                    }
                }

                // Fallback to BaseGame KeeperMap
                var baseGameKeeperMap = GetBaseGameKeeperMap();
                if (baseGameKeeperMap != null)
                {
                    var result = baseGameKeeperMap.FindBase(handle);
                    if (result != null)
                    {
                        Logger.LogInfo($"Converted Handle to IHandleable via BaseGame.keeper.map: {result.GetType().Name}");
                        return result;
                    }
                }

                Logger.LogWarning($"Could not convert Handle {handle.GetType().Name} to IHandleable");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"ConvertHandleToIHandleable failed: {ex.Message}");
                return null;
            }
        }

        // ==================== DIAGNOSTICS ====================

        /// <summary>
        /// Get comprehensive status of all Keeper systems for debugging
        /// </summary>
        public static string GetKeeperSystemStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine("=== KEEPER SYSTEM STATUS ===");

            // Universe Keeper
            var universeKeeper = GetUniverseKeeper();
            status.AppendLine($"Universe.keeper: {(universeKeeper != null ? "✅ Available" : "❌ Null")}");
            if (universeKeeper != null)
            {
                var universeKeeperMap = universeKeeper.GetKeeperMap();
                status.AppendLine($"Universe.keeper.map: {(universeKeeperMap != null ? "✅ Available" : "❌ Null")}");
            }

            // BaseGame Keeper  
            var baseGameKeeper = GetBaseGameKeeper();
            status.AppendLine($"BaseGame.keeper: {(baseGameKeeper != null ? "✅ Available" : "❌ Null")}");
            if (baseGameKeeper != null)
            {
                var baseGameKeeperMap = baseGameKeeper.GetKeeperMap();
                status.AppendLine($"BaseGame.keeper.map: {(baseGameKeeperMap != null ? "✅ Available" : "❌ Null")}");
            }

            // Command system readiness
            var commandKeeperMap = GetCommandKeeperMap();
            status.AppendLine($"Command KeeperMap: {(commandKeeperMap != null ? "✅ Ready for DispatchAction" : "❌ Not available")}");

            return status.ToString();
        }
    }
}
