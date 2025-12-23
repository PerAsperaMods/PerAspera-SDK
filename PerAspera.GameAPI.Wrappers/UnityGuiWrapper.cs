using System;
using UnityEngine;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers.Unity.GUI;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper Unity GUI basique - utilise RealUnityGuiWrapper pour accès aux vraies DLLs Unity
    /// Architecture: Interface publique → RealUnityGuiWrapper → chargement dynamique DLL
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
        /// Wrapper sécurisé pour GUILayout.Toggle - implémentation complète
        /// </summary>
        public static bool SafeToggle(bool value, string label, float width = 0)
        {
            return RealUnityGuiWrapper.SafeToggle(value, label);
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.Button
        /// </summary>
        public static bool SafeButton(string text)
        {
            return RealUnityGuiWrapper.SafeButton(text);
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.Label
        /// </summary>
        public static void SafeLabel(string text)
        {
            RealUnityGuiWrapper.SafeLabel(text);
        }

        /// <summary>
        /// Wrapper sécurisé pour GUILayout.HorizontalSlider - TODO: implémenter dans RealUnityGuiWrapper
        /// </summary>
        public static float SafeHorizontalSlider(float value, float min, float max)
        {
            LogAspera.LogInfo($"SafeHorizontalSlider not yet implemented: {value} ({min}-{max})");
            return value; // TODO: ajouter SafeHorizontalSlider dans RealUnityGuiWrapper
        }

        /// <summary>
        /// Statut du système Unity GUI
        /// </summary>
        public static bool IsReady => RealUnityGuiWrapper.IsReady;
    }
}