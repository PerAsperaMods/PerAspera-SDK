#nullable enable
using System;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Unity GUI Loader - charge les vraies DLLs Unity depuis unity-libs
    /// DÉCOUVERTE: Les unity-libs contiennent les vraies implémentations Unity !
    /// </summary>
    public static class UnityGuiRealLoader
    {
        private static Assembly? _unityGuiAssembly;
        private static System.Type? _guiType;
        private static System.Type? _guiLayoutType;
        private static PropertyInfo? _guiEnabledProperty;
        private static bool _isInitialized = false;

        static UnityGuiRealLoader()
        {
            InitializeRealUnityGui();
        }

        private static void InitializeRealUnityGui()
        {
            try
            {
                // Charger les VRAIES DLLs Unity depuis unity-libs (pas interop!)
                string realUnityPath = @"F:\SteamLibrary\steamapps\common\Per Aspera\BepInEx\unity-libs\UnityEngine.IMGUIModule.dll";
                
                if (System.IO.File.Exists(realUnityPath))
                {
                    _unityGuiAssembly = Assembly.LoadFrom(realUnityPath);
                    _guiType = _unityGuiAssembly.GetType("UnityEngine.GUI");
                    _guiLayoutType = _unityGuiAssembly.GetType("UnityEngine.GUILayout");
                    
                    if (_guiType != null)
                    {
                        _guiEnabledProperty = _guiType.GetProperty("enabled");
                    }
                    
                    _isInitialized = true;
                    LogAspera.LogInfo($"🎮 Real Unity GUI loaded! GUI: {_guiType != null}, GUILayout: {_guiLayoutType != null}, GUI.enabled: {_guiEnabledProperty != null}");
                }
                else
                {
                    LogAspera.LogWarning($"❌ Real Unity libs not found at: {realUnityPath}");
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"❌ Real Unity GUI loader failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test si le loader est prêt
        /// </summary>
        public static bool IsReady => _isInitialized && _guiType != null && _guiLayoutType != null;

        /// <summary>
        /// GET GUI.enabled depuis les vraies DLLs Unity
        /// </summary>
        public static bool GetGuiEnabled()
        {
            try
            {
                if (IsReady && _guiEnabledProperty != null)
                {
                    return (bool)(_guiEnabledProperty.GetValue(null) ?? true);
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"GetGuiEnabled failed: {ex.Message}");
            }
            return true;
        }

        /// <summary>
        /// SET GUI.enabled depuis les vraies DLLs Unity
        /// </summary>
        public static void SetGuiEnabled(bool enabled)
        {
            try
            {
                if (IsReady && _guiEnabledProperty != null)
                {
                    _guiEnabledProperty.SetValue(null, enabled);
                    LogAspera.LogDebug($"✅ GUI.enabled set to {enabled}");
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"SetGuiEnabled failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Appelle GUILayout.BeginVertical depuis les vraies DLLs Unity
        /// </summary>
        public static bool CallBeginVertical()
        {
            try
            {
                if (IsReady && _guiLayoutType != null)
                {
                    var method = _guiLayoutType.GetMethod("BeginVertical", System.Type.EmptyTypes);
                    if (method != null)
                    {
                        method.Invoke(null, null);
                        LogAspera.LogDebug("✅ GUILayout.BeginVertical called");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"CallBeginVertical failed: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Appelle GUILayout.EndVertical depuis les vraies DLLs Unity
        /// </summary>
        public static void CallEndVertical()
        {
            try
            {
                if (IsReady && _guiLayoutType != null)
                {
                    var method = _guiLayoutType.GetMethod("EndVertical", System.Type.EmptyTypes);
                    if (method != null)
                    {
                        method.Invoke(null, null);
                        LogAspera.LogDebug("✅ GUILayout.EndVertical called");
                    }
                }
            }
            catch (Exception ex)
            {
                LogAspera.LogError($"CallEndVertical failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Wrapper simple utilisant le vrai loader Unity
    /// </summary>
    public static class RealUnityGuiWrapper
    {
        private static bool _savedGuiEnabled = true;

        /// <summary>
        /// Wrapper pour GUILayout.BeginVertical avec les vraies DLLs
        /// </summary>
        public static bool SafeBeginVertical()
        {
            return UnityGuiRealLoader.CallBeginVertical();
        }

        /// <summary>
        /// Wrapper pour GUILayout.EndVertical avec les vraies DLLs
        /// </summary>
        public static void SafeEndVertical()
        {
            UnityGuiRealLoader.CallEndVertical();
        }

        /// <summary>
        /// Sauvegarde et définit GUI.enabled avec les vraies DLLs
        /// </summary>
        public static void SafeSetGuiEnabled(bool enabled)
        {
            _savedGuiEnabled = UnityGuiRealLoader.GetGuiEnabled();
            UnityGuiRealLoader.SetGuiEnabled(enabled);
        }

        /// <summary>
        /// Restaure GUI.enabled avec les vraies DLLs
        /// </summary>
        public static void SafeRestoreGuiEnabled()
        {
            UnityGuiRealLoader.SetGuiEnabled(_savedGuiEnabled);
        }

        /// <summary>
        /// Statut du loader pour debug
        /// </summary>
        public static bool IsReady => UnityGuiRealLoader.IsReady;
    }
}