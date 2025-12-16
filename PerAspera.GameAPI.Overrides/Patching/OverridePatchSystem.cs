using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Overrides.Patching
{
    /// <summary>
    /// Automatic patch discovery and application system
    /// Scans assemblies for [AutoOverridePatch] attributes and applies them
    /// </summary>
    public static class OverridePatchSystem
    {
        private static readonly LogAspera Log = new LogAspera("Overrides.PatchSystem");
        private static Harmony? _harmonyInstance;
        private static readonly List<PatchInfo> _appliedPatches = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the patch system with a Harmony instance
        /// </summary>
        public static void Initialize(string harmonyId = "PerAspera.GameAPI.Overrides")
        {
            if (_isInitialized)
            {
                Log.Warning("Patch system already initialized");
                return;
            }

            try
            {
                _harmonyInstance = new Harmony(harmonyId);
                _isInitialized = true;
                Log.Info($"✅ Override patch system initialized (Harmony ID: {harmonyId})");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize patch system: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Discover and apply all patches in the given assembly
        /// </summary>
        public static void DiscoverAndApplyPatches(Assembly assembly)
        {
            if (_harmonyInstance == null)
            {
                Log.Error("Patch system not initialized. Call Initialize() first.");
                return;
            }

            try
            {
                var patchClasses = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<AutoOverridePatchAttribute>() != null)
                    .OrderByDescending(t => t.GetCustomAttribute<AutoOverridePatchAttribute>()!.Priority)
                    .ToList();

                Log.Info($"🔍 Discovered {patchClasses.Count} patch classes in {assembly.GetName().Name}");

                foreach (var patchClass in patchClasses)
                {
                    ApplyPatchClass(patchClass);
                }

                Log.Info($"✅ Applied {_appliedPatches.Count} override patches");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during patch discovery: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply patches from a specific class
        /// </summary>
        private static void ApplyPatchClass(global::System.Type patchClass)
        {
            if (_harmonyInstance == null) return;

            var attr = patchClass.GetCustomAttribute<AutoOverridePatchAttribute>();
            if (attr == null) return;

            try
            {
                // Skip if not enabled by default (can be manually enabled later)
                if (!attr.EnabledByDefault)
                {
                    Log.Debug($"⏭️ Skipping disabled patch: {attr}");
                    return;
                }

                // Apply all Harmony patches in this class
                _harmonyInstance.PatchAll(patchClass);

                var patchInfo = new PatchInfo
                {
                    ClassName = attr.ClassName,
                    MethodName = attr.MethodName,
                    Category = attr.Category,
                    Priority = attr.Priority,
                    PatchType = patchClass
                };

                _appliedPatches.Add(patchInfo);

                Log.Info($"✅ Applied patch: {attr}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to apply patch {patchClass.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply patches manually from a specific type
        /// </summary>
        public static bool ApplyPatch(global::System.Type patchClass)
        {
            if (_harmonyInstance == null)
            {
                Log.Error("Patch system not initialized");
                return false;
            }

            try
            {
                ApplyPatchClass(patchClass);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to apply patch {patchClass.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove all applied patches
        /// </summary>
        public static void RemoveAllPatches()
        {
            if (_harmonyInstance == null) return;

            try
            {
                _harmonyInstance.UnpatchSelf();
                _appliedPatches.Clear();
                Log.Info("🗑️ Removed all override patches");
            }
            catch (Exception ex)
            {
                Log.Error($"Error removing patches: {ex.Message}");
            }
        }

        /// <summary>
        /// Get information about all applied patches
        /// </summary>
        public static IReadOnlyList<PatchInfo> GetAppliedPatches()
        {
            return _appliedPatches.AsReadOnly();
        }

        /// <summary>
        /// Get patches by category
        /// </summary>
        public static IEnumerable<PatchInfo> GetPatchesByCategory(string category)
        {
            return _appliedPatches.Where(p => p.Category == category);
        }

        /// <summary>
        /// Check if patches are applied
        /// </summary>
        public static bool IsPatched => _appliedPatches.Count > 0;

        /// <summary>
        /// Get patch system statistics
        /// </summary>
        public static PatchSystemStatistics GetStatistics()
        {
            return new PatchSystemStatistics
            {
                TotalPatches = _appliedPatches.Count,
                PatchesByCategory = _appliedPatches
                    .GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                IsInitialized = _isInitialized,
                HarmonyId = _harmonyInstance?.Id ?? "Not initialized"
            };
        }
    }

    /// <summary>
    /// Information about an applied patch
    /// </summary>
    public class PatchInfo
    {
        public string ClassName { get; set; } = "";
        public string MethodName { get; set; } = "";
        public string Category { get; set; } = "General";
        public int Priority { get; set; }
        public global::System.Type? PatchType { get; set; }

        public override string ToString()
        {
            return $"{ClassName}.{MethodName} [{Category}] (Priority: {Priority})";
        }
    }

    /// <summary>
    /// Statistics about the patch system
    /// </summary>
    public class PatchSystemStatistics
    {
        public int TotalPatches { get; set; }
        public Dictionary<string, int> PatchesByCategory { get; set; } = new();
        public bool IsInitialized { get; set; }
        public string HarmonyId { get; set; } = "";

        public override string ToString()
        {
            var categories = string.Join(", ", PatchesByCategory.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"Patches: {TotalPatches} applied | Categories: [{categories}] | Harmony: {HarmonyId}";
        }
    }
}
