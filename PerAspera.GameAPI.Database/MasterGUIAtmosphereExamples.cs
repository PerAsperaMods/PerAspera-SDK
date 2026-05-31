using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using PerAspera.GameAPI.Database;

namespace PerAspera.Examples
{
    /// <summary>
    /// MasterGUI Atmospheric Composition Display Examples
    /// Shows how to properly display real atmospheric gases vs derived resources
    /// </summary>
    public static class MasterGUIAtmosphereExamples
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("MasterGUIAtmosphere");
        /// <summary>
        /// Example 1: Get and display atmosphere composition for MasterGUI
        /// Shows the correct way to display atmospheric gases (not derived resources)
        /// </summary>
        public static void DisplayAtmosphereComposition()
        {
            Logger.LogInfo("=== MasterGUI: Atmosphere Composition Display ===");

            var db = ModDatabase.Instance;
            var atmosphere = db.GetAtmosphereComposition();

            Logger.LogInfo($"Atmosphere contains {atmosphere.Count} gases:");
            Logger.LogInfo("");

            foreach (var gas in atmosphere.Values.OrderByDescending(g => g.Priority))
            {
                var status = gas.IsNative ? "🌍" : gas.IsModAdded ? "🆕" : "🤖";
                Logger.LogInfo($"{status} {gas.DisplayName} (Priority: {gas.Priority}) - {gas.GetSourceDescription()}");
            }

            Logger.LogInfo("");
            Logger.LogInfo("✅ These are REAL atmospheric gases - not building I/O resources!");
        }

        /// <summary>
        /// Example 2: Compare atmospheric vs derived resources
        /// Demonstrates the difference between displayable atmosphere and building calculations
        /// </summary>
        public static void CompareAtmosphericVsDerived()
        {
            Logger.LogInfo("=== Comparing Atmospheric vs Derived Resources ===");

            var db = ModDatabase.Instance;

            // Get real atmospheric gases (for display)
            var atmospheric = db.GetAtmosphericResources();
            Logger.LogInfo($"🌀 REAL ATMOSPHERIC GASES (display in atmosphere): {atmospheric.Count}");
            foreach (var kvp in atmospheric.OrderBy(k => k.Key))
            {
                Logger.LogInfo($"   - {kvp.Key}");
            }

            Logger.LogInfo("");

            // Get derived resources (building I/O, not for atmosphere display)
            var derived = db.GetDerivedResources();
            var derivedSample = derived.Where(d => d.Key.Contains("_Up") ||
                                                   d.Key.Contains("_Down") ||
                                                   d.Key.Contains("_capture") ||
                                                   d.Key.Contains("_production"))
                                      .Take(10) // Show first 10
                                      .ToList();

            Logger.LogInfo($"🏭 DERIVED RESOURCES (building I/O, NOT for atmosphere): {derived.Count} total");
            Logger.LogInfo("   Sample of derived resources:");
            foreach (var kvp in derivedSample)
            {
                Logger.LogInfo($"   - {kvp.Key} (used by buildings)");
            }

            Logger.LogInfo("");
            Logger.LogInfo("💡 MasterGUI should ONLY display atmospheric resources, not derived ones!");
        }

        /// <summary>
        /// Example 3: Initialize atmosphere display with proper resource keys
        /// Shows how MasterGUI should initialize its atmosphere monitoring
        /// </summary>
        public static void InitializeAtmosphereDisplay()
        {
            Logger.LogInfo("=== MasterGUI Atmosphere Display Initialization ===");

            var db = ModDatabase.Instance;

            // Get ordered atmospheric resources for consistent display
            var orderedAtmospheric = db.GetAtmosphericResourcesOrdered();

            Logger.LogInfo("Initializing atmosphere display with resources:");
            Logger.LogInfo("(Ordered by priority for consistent UI layout)");
            Logger.LogInfo("");

            var displayOrder = 1;
            foreach (var kvp in orderedAtmospheric)
            {
                var resourceName = kvp.Key;
                var resourceData = kvp.Value as Dictionary<object, object>;

                // Extract display information
                var displayName = resourceData?.TryGetValue("displayName", out var dn) == true
                    ? dn?.ToString() ?? resourceName
                    : resourceName;

                var materialType = resourceData?.TryGetValue("materialType", out var mt) == true
                    ? mt?.ToString() ?? "Unknown"
                    : "Unknown";

                Logger.LogInfo($"{displayOrder:00}. {displayName} ({resourceName}) - {materialType}");
                displayOrder++;
            }

            Logger.LogInfo("");
            Logger.LogInfo("✅ MasterGUI should initialize monitoring for these resources only!");
            Logger.LogInfo("🚫 Do NOT monitor derived resources like 'resource_O2_Up' or 'resource_oxygen_capture'");
        }

        /// <summary>
        /// Example 4: Handle mod-added atmospheric resources
        /// Shows how to handle resources added by mods like MoreResources
        /// </summary>
        public static void HandleModAtmosphericResources()
        {
            Logger.LogInfo("=== Handling Mod-Added Atmospheric Resources ===");

            var db = ModDatabase.Instance;
            var atmosphere = db.GetAtmosphereComposition();

            var nativeGases = atmosphere.Values.Where(g => g.IsNative).ToList();
            var modGases = atmosphere.Values.Where(g => g.IsModAdded).ToList();
            var autoDetectedGases = atmosphere.Values.Where(g => !g.IsNative && !g.IsModAdded).ToList();

            Logger.LogInfo($"Native atmospheric gases: {nativeGases.Count}");
            foreach (var gas in nativeGases)
            {
                Logger.LogInfo($"  🌍 {gas.DisplayName} (Priority: {gas.Priority})");
            }

            Logger.LogInfo("");
            Logger.LogInfo($"Mod-added atmospheric gases: {modGases.Count}");
            foreach (var gas in modGases)
            {
                Logger.LogInfo($"  🆕 {gas.DisplayName} (Priority: {gas.Priority}) - from MoreResources or similar mods");
            }

            Logger.LogInfo("");
            Logger.LogInfo($"Auto-detected atmospheric gases: {autoDetectedGases.Count}");
            foreach (var gas in autoDetectedGases)
            {
                Logger.LogInfo($"  🤖 {gas.DisplayName} (Priority: {gas.Priority}) - auto-detected");
            }

            Logger.LogInfo("");
            Logger.LogInfo("💡 MasterGUI should display ALL atmospheric resources regardless of source");
            Logger.LogInfo("   But prioritize native > mod-added > auto-detected for UI layout");
        }

        /// <summary>
        /// Example 5: Atmosphere monitoring setup for MasterGUI
        /// Shows the complete setup for atmosphere composition display
        /// </summary>
        public static void SetupAtmosphereMonitoring()
        {
            Logger.LogInfo("=== MasterGUI Atmosphere Monitoring Setup ===");

            var db = ModDatabase.Instance;

            // 1. Get the complete atmosphere composition
            var atmosphereComposition = db.GetAtmosphereComposition();

            // 2. Setup monitoring for each atmospheric gas
            Logger.LogInfo("Setting up atmosphere monitoring:");
            Logger.LogInfo("");

            var monitoringSetup = new List<string>();
            foreach (var gas in atmosphereComposition.Values.OrderBy(g => g.Priority))
            {
                var monitorType = gas.IsNative ? "Primary" :
                                gas.IsModAdded ? "Secondary" : "Tertiary";

                Logger.LogInfo($"{monitorType} Monitor: {gas.DisplayName}");
                Logger.LogInfo($"  Resource Key: {gas.Name}");
                Logger.LogInfo($"  Display Priority: {gas.Priority}");
                Logger.LogInfo($"  Source: {gas.GetSourceDescription()}");
                Logger.LogInfo("");

                monitoringSetup.Add(gas.Name);
            }

            // 3. Summary for MasterGUI implementation
            Logger.LogInfo("=== MasterGUI Implementation Summary ===");
            Logger.LogInfo($"Monitor these {monitoringSetup.Count} atmospheric resources:");
            for (int i = 0; i < monitoringSetup.Count; i++)
            {
                Logger.LogInfo($"  {i + 1:00}. {monitoringSetup[i]}");
            }

            Logger.LogInfo("");
            Logger.LogInfo("🚫 EXCLUDE these patterns from atmosphere display:");
            Logger.LogInfo("  - Resources ending with _Up, _Down");
            Logger.LogInfo("  - Resources containing _capture, _production, _respiration");
            Logger.LogInfo("  - Building input/output calculation resources");
            Logger.LogInfo("");
            Logger.LogInfo("✅ MasterGUI atmosphere display is now properly configured!");
        }

        /// <summary>
        /// Run all MasterGUI atmosphere examples
        /// </summary>
        public static void RunAllMasterGUIExamples()
        {
            try
            {
                Logger.LogInfo("🚀 Starting MasterGUI Atmosphere Examples...");
                Logger.LogInfo("These examples show proper atmosphere composition display");
                Logger.LogInfo("vs building I/O resources that should NOT be displayed.");
                Logger.LogInfo("");

                DisplayAtmosphereComposition();
                Logger.LogInfo("");

                CompareAtmosphericVsDerived();
                Logger.LogInfo("");

                InitializeAtmosphereDisplay();
                Logger.LogInfo("");

                HandleModAtmosphericResources();
                Logger.LogInfo("");

                SetupAtmosphereMonitoring();
                Logger.LogInfo("");

                Logger.LogInfo("✅ All MasterGUI atmosphere examples completed!");
                Logger.LogInfo("💡 Key takeaway: Display REAL atmospheric gases, not building calculations!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"❌ Error running MasterGUI atmosphere examples: {ex.Message}");
            }
        }
    }
}
