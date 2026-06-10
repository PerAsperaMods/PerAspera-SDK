using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core.IL2CPP;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// High-level convenience extensions for common Per Aspera operations
    /// Simplifies access to game singletons, resources, and common patterns
    /// </summary>
    public static class PerAsperaExtensions
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("PerAsperaExtensions");

        // ==================== SINGLETON ACCESS ====================

        /// <summary>
        /// Get BaseGame.Instance safely
        /// </summary>
        public static object? GetBaseGame()
        {
            try
            {
                var baseGameType = ReflectionHelpers.FindType("BaseGame");
                if (baseGameType == null) return null;

                var instanceProperty = baseGameType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                return instanceProperty?.GetValue(null);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to get BaseGame.Instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get current Universe from BaseGame
        /// </summary>
        public static object? GetCurrentUniverse()
        {
            var baseGame = GetBaseGame();
            return baseGame?.GetMemberValue<object>("universe");
        }

        /// <summary>
        /// Get current Planet from Universe
        /// </summary>
        public static object? GetCurrentPlanet()
        {
            var universe = GetCurrentUniverse();
            return universe?.GetMemberValue<object>("currentPlanet");
        }

        /// <summary>
        /// Get current Faction from Universe
        /// </summary>
        public static object? GetCurrentFaction()
        {
            var universe = GetCurrentUniverse();
            return universe?.GetMemberValue<object>("currentFaction");
        }

        /// <summary>
        /// Get faction's stockpile
        /// </summary>
        public static object? GetFactionStockpile(object faction)
        {
            return faction?.GetMemberValue<object>("stockpile");
        }

        // ==================== RESOURCE OPERATIONS ====================

        /// <summary>
        /// Parse resource type string to enum value
        /// </summary>
        public static object? ParseResourceType(string resourceString)
        {
            try
            {
                var resourceTypeEnum = ReflectionHelpers.FindType("ResourceType");
                if (resourceTypeEnum == null || !resourceTypeEnum.IsEnum)
                    return null;

                // Common resource mappings
                var normalized = resourceString.ToUpperInvariant();
                switch (normalized)
                {
                    case "WATER":
                    case "H2O":
                        return Enum.Parse(resourceTypeEnum, "WATER");
                    case "ICE":
                        return Enum.Parse(resourceTypeEnum, "ICE");
                    case "CHG":
                    case "METAL":
                    case "IRON":
                    case "PARTS":
                        return Enum.Parse(resourceTypeEnum, "CHG");
                    case "NITROGEN":
                        return Enum.Parse(resourceTypeEnum, "NITROGEN");
                    case "OXYGEN":
                        return Enum.Parse(resourceTypeEnum, "OXYGEN");
                    default:
                        // Try direct parsing
                        return Enum.Parse(resourceTypeEnum, normalized);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create a CargoQuantity instance
        /// </summary>
        public static object? CreateCargoQuantity(float amount)
        {
            try
            {
                var cargoQuantityType = ReflectionHelpers.FindType("CargoQuantity");
                if (cargoQuantityType == null) return null;

                return Activator.CreateInstance(cargoQuantityType, amount);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to create CargoQuantity: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Add resources to faction's stockpile
        /// </summary>
        public static bool AddResourcesToFaction(object faction, string resourceType, float amount)
        {
            try
            {
                if (faction == null) return false;

                var resourceEnumValue = ParseResourceType(resourceType);
                if (resourceEnumValue == null) return false;

                var cargoQuantity = CreateCargoQuantity(amount);
                if (cargoQuantity == null) return false;

                var stockpile = GetFactionStockpile(faction);
                if (stockpile == null) return false;

                // Try to call XAddIdleCargo
                return stockpile.InvokeMethod("XAddIdleCargo", resourceEnumValue, cargoQuantity, null);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to add resources: {ex.Message}");
                return false;
            }
        }

        // ==================== CONSOLE OPERATIONS ====================

        /// <summary>
        /// Get Console instance
        /// </summary>
        public static object? GetConsole()
        {
            try
            {
                var consoleType = ReflectionHelpers.FindType("Console");
                if (consoleType == null) return null;

                var instanceProperty = consoleType.GetProperty("instance",
                    BindingFlags.Public | BindingFlags.Static);
                return instanceProperty?.GetValue(null);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to get Console.instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Execute console command
        /// </summary>
        public static bool ExecuteConsoleCommand(string command)
        {
            var console = GetConsole();
            return console?.InvokeMethod("ExecuteCommandString", command) ?? false;
        }

        // ==================== HARMONY PATCH HELPERS ====================

        /// <summary>
        /// Create a Harmony patch target method resolver
        /// </summary>
        public static MethodBase CreatePatchTarget(string typeName, string methodName, System.Type[] parameterTypes = null)
        {
            var type = ReflectionHelpers.FindType(typeName);
            if (type == null) return null;

            if (parameterTypes != null)
            {
                return type.GetMethod(methodName, parameterTypes);
            }
            else
            {
                return type.GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            }
        }

        /// <summary>
        /// Get InteractionManager.Instance safely
        /// </summary>
        public static object? GetInteractionManager()
        {
            try
            {
                var interactionManagerType = ReflectionHelpers.FindType("InteractionManager");
                if (interactionManagerType == null) return null;

                var instanceProperty = interactionManagerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                return instanceProperty?.GetValue(null);
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to get InteractionManager.Instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes a console command with the given arguments
        /// </summary>
        public static void ExecuteConsoleCommand(string command, params object[] args)
        {
            var interactionManager = GetInteractionManager();
            if (interactionManager == null) return;

            // Use SafeInvoke to execute the command
            ReflectionHelpers.SafeInvoke(interactionManager, "ExecuteCommand", command, args);
        }
    }

    /// <summary>
    /// Extension methods for common game object operations
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Get all buildings from a planet
        /// </summary>
        public static IList<object> GetBuildings(this object planet)
        {
            return planet?.GetMemberValue<IList<object>>("buildings") ?? new List<object>();
        }

        /// <summary>
        /// Get faction from a building
        /// </summary>
        public static object GetFaction(this object building)
        {
            return building?.GetMemberValue<object>("faction");
        }

        /// <summary>
        /// Get building type from a building
        /// </summary>
        public static object GetBuildingType(this object building)
        {
            return building?.GetMemberValue<object>("buildingType");
        }
    }
}