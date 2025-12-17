using System;
using UnityEngine;
using PerAspera.Core;

namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Wrapper de test pour Unity GUI - teste quelles méthodes sont vraiment disponibles
    /// </summary>
    public static class UnityGuiTester
    {
        private static bool _hasBeenTested = false;
        private static bool _guiEnabled = false;
        private static bool _guiLayoutAvailable = false;
        private static bool _guiStyleAvailable = false;

        /// <summary>
        /// Teste les capacités Unity GUI disponibles dans Per Aspera
        /// </summary>
        public static void TestUnityGuiCapabilities()
        {
            if (_hasBeenTested) return;
            _hasBeenTested = true;

            LogAspera.LogInfo("=== Testing Unity GUI Capabilities ===");

            // Test GUI.enabled
            TestGuiEnabled();
            
            // Test GUILayout methods
            TestGuiLayout();
            
            // Test GUIStyle
            TestGuiStyle();
            
            // Test autres classes Unity
            TestOtherUnityClasses();

            LogAspera.LogInfo("=== Unity GUI Test Complete ===");
        }

        private static void TestGuiEnabled()
        {
            try
            {
                // Test wrapper GUI.enabled - utiliser reflection pour éviter erreur compilation
                var guiType = System.Type.GetType("UnityEngine.GUI");
                if (guiType != null)
                {
                    var enabledProperty = guiType.GetProperty("enabled");
                    if (enabledProperty != null)
                    {
                        bool enabled = (bool)(enabledProperty.GetValue(null) ?? false);
                        LogAspera.LogInfo($"✓ GUI.enabled GET: {enabled}");
                        
                        enabledProperty.SetValue(null, true);
                        LogAspera.LogInfo("✓ GUI.enabled SET: Success");
                        
                        _guiEnabled = true;
                        return;
                    }
                }
                
                LogAspera.LogError("✗ GUI.enabled property not found (stripped)");
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"✗ GUI.enabled failed: {ex.Message}");
            }
        }

        private static void TestGuiLayout()
        {
            try
            {
                // Test si GUILayout.Label existe
                var labelMethod = typeof(GUILayout).GetMethod("Label", new[] { typeof(string) });
                LogAspera.LogInfo($"✓ GUILayout.Label method exists: {labelMethod != null}");

                // Test si BeginVertical existe
                var beginVertical = typeof(GUILayout).GetMethod("BeginVertical", new Type[0]);
                LogAspera.LogInfo($"✓ GUILayout.BeginVertical method exists: {beginVertical != null}");

                // Test si EndVertical existe
                var endVertical = typeof(GUILayout).GetMethod("EndVertical", new Type[0]);
                LogAspera.LogInfo($"✓ GUILayout.EndVertical method exists: {endVertical != null}");

                // Test si Toggle existe
                var toggle = typeof(GUILayout).GetMethod("Toggle", new[] { typeof(bool) });
                LogAspera.LogInfo($"✓ GUILayout.Toggle method exists: {toggle != null}");

                _guiLayoutAvailable = beginVertical != null && endVertical != null;
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"✗ GUILayout reflection failed: {ex.Message}");
            }
        }

        private static void TestGuiStyle()
        {
            try
            {
                // Test si GUIStyle existe
                var guiStyleType = typeof(GUIStyle);
                LogAspera.LogInfo($"✓ GUIStyle type exists: {guiStyleType != null}");

                // Test GUI.skin
                var skinProperty = typeof(GUI).GetProperty("skin");
                LogAspera.LogInfo($"✓ GUI.skin property exists: {skinProperty != null}");

                _guiStyleAvailable = guiStyleType != null && skinProperty != null;
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"✗ GUIStyle test failed: {ex.Message}");
            }
        }

        private static void TestOtherUnityClasses()
        {
            try
            {
                // Test Vector2
                var vector2Type = typeof(Vector2);
                LogAspera.LogInfo($"✓ Vector2 type available: {vector2Type != null}");

                // Test Rect
                var rectType = typeof(Rect);
                LogAspera.LogInfo($"✓ Rect type available: {rectType != null}");

                // Test MonoBehaviour
                var monoBehaviourType = typeof(MonoBehaviour);
                LogAspera.LogInfo($"✓ MonoBehaviour type available: {monoBehaviourType != null}");

                // Test Event
                var eventType = typeof(Event);
                LogAspera.LogInfo($"✓ Event type available: {eventType != null}");

            }
            catch (Exception ex)
            {
                LogAspera.LogError($"✗ Other Unity classes test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne un rapport sur les capacités GUI disponibles
        /// </summary>
        public static string GetCapabilitiesReport()
        {
            if (!_hasBeenTested)
                TestUnityGuiCapabilities();

            return $@"Unity GUI Capabilities Report:
- GUI.enabled: {(_guiEnabled ? "Available" : "Not Available")}
- GUILayout methods: {(_guiLayoutAvailable ? "Available" : "Not Available")}
- GUIStyle: {(_guiStyleAvailable ? "Available" : "Not Available")}";
        }

        /// <summary>
        /// Safe wrapper pour GUI operations - ne fait rien si GUI n'est pas disponible
        /// </summary>
        public static void SafeGuiOperation(Action guiAction, string operationName = "GUI Operation")
        {
            if (!_hasBeenTested)
                TestUnityGuiCapabilities();

            try
            {
                guiAction?.Invoke();
            }
            catch (Exception ex)
            {
                LogAspera.LogWarning($"Safe GUI operation '{operationName}' failed: {ex.Message}");
            }
        }
    }
}