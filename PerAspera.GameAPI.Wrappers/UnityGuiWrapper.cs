using System;
using UnityEngine;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper Unity GUI basique - utilise RealUnityGuiLoader pour accès aux vraies DLLs
    /// </summary>
    public static class UnityGuiWrapper
    {
        private static bool _oldGuiEnabled = true;

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.BeginVertical
        /// </summary>
        public static bool SafeBeginVertical()
        {
            return RealUnityGuiWrapper.SafeBeginVertical();
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.EndVertical
        /// </summary>
        public static void SafeEndVertical()
        {
            RealUnityGuiWrapper.SafeEndVertical();
        }

        /// <summary>
        /// Wrapper sécurisé pour définir GUI.enabled
        /// </summary>
        public static void SafeSetGuiEnabled(bool enabled)
        {
            RealUnityGuiWrapper.SafeSetGuiEnabled(enabled);
        }

        /// <summary>
        /// Wrapper sécurisé pour restaurer GUI.enabled
        /// </summary>
        public static void SafeRestoreGuiEnabled()
        {
            RealUnityGuiWrapper.SafeRestoreGuiEnabled();
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.Toggle - placeholder
        /// </summary>
        public static bool SafeToggle(bool value, string label, float width = 0)
        {
            LogAspera.LogInfo($"SafeToggle placeholder: {value} - {label}");
            return value; // TODO: implémenter via RealUnityGuiLoader
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.HorizontalSlider - placeholder 
        /// </summary>
        public static float SafeHorizontalSlider(float value, float min, float max)
        {
            LogAspera.LogInfo($"SafeHorizontalSlider placeholder: {value} ({min}-{max})");
            return value; // TODO: implémenter via RealUnityGuiLoader
        }

        /// <summary>
        /// Statut du système Unity GUI
        /// </summary>
        public static bool IsReady => RealUnityGuiWrapper.IsReady;
    }
}