using Il2CppInterop.Runtime.Injection;
using System;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
// TODO: Remove circular dependency - move this to Wrappers when HandleHelper is implemented
// using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Helper pour Unity GUI avec IL2CPP
    /// Simplifie la création d'interfaces utilisateur dans Per Aspera
    /// </summary>
    public static class UnityGuiHelper 
    {
        /// <summary>
        /// Enregistre un type MonoBehaviour pour IL2CPP
        /// </summary>
        /// <typeparam name="T">Type MonoBehaviour à enregistrer</typeparam>
        /// <returns>True si réussi</returns>
        public static bool RegisterMonoBehaviour<T>() where T : MonoBehaviour
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<T>();
                return true;
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to register MonoBehaviour {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Crée un GameObject persistant avec MonoBehaviour
        /// </summary>
        /// <typeparam name="T">Type MonoBehaviour à attacher</typeparam>
        /// <param name="name">Nom du GameObject</param>
        /// <returns>Le composant T attaché, null si échec</returns>
        public static T? CreatePersistentGameObject<T>(string name) where T : MonoBehaviour
        {
            try
            {
                // Enregistrer le type d'abord
                if (!RegisterMonoBehaviour<T>())
                {
                    return null;
                }

                // Créer GameObject persistant
                var gameObject = new GameObject(name);
                GameObject.DontDestroyOnLoad(gameObject);
                
                // Ajouter le composant
                return gameObject.AddComponent<T>();
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to create persistent GameObject {name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Dessine une catégorie pliable
        /// </summary>
        /// <param name="categoryName">Nom de la catégorie</param>
        /// <param name="icon">Icône (optionnel)</param>
        /// <param name="isExpanded">État actuel (pliée/dépliée)</param>
        /// <returns>Nouvel état après clic</returns>
        public static bool DrawCollapsibleCategory(string categoryName, string icon = "", bool isExpanded = false)
        {
            var arrow = isExpanded ? "▼" : "▶";
            var label = string.IsNullOrEmpty(icon) ? $"{arrow} {categoryName}" : $"{arrow} {icon} {categoryName}";
            
            return GUILayout.Button(label);
        }

        /// <summary>
        /// Dessine une section avec bordure
        /// TEMPORAIREMENT DÉSACTIVÉ - GUI APIs strippées dans Per Aspera
        /// </summary>
        /// <param name="content">Action pour dessiner le contenu</param>
        /// <param name="style">Style optionnel (par défaut GUI.skin.box)</param>
        public static void DrawBoxedSection(System.Action content, object? style = null)
        {
            // TODO: GUI APIs strippées dans IL2CPP - implementation différée
            content?.Invoke();
            /*
            UnityGuiTester.SafeGuiOperation(() => {
                // var boxStyle = style ?? GUI.skin.box;
                GUILayout.BeginVertical();
                content?.Invoke();
                GUILayout.EndVertical();
            }, "DrawBoxedSection");
            */
        }

        /// <summary>
        /// Dessine un contrôle avec label et valeur sur la même ligne
        /// </summary>
        /// <param name="label">Label à afficher</param>
        /// <param name="content">Action pour dessiner le contrôle</param>
        /// <param name="labelWidth">Largeur du label (0 = auto)</param>
        public static void DrawLabeledControl(string label, System.Action content, float labelWidth = 120f)
        {
            GUILayout.BeginHorizontal();
            try
            {
                if (labelWidth > 0)
                {
                    GUILayout.Label(label, GUILayout.Width(labelWidth));
                }
                else
                {
                    GUILayout.Label(label);
                }
                
                content?.Invoke();
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Dessine un slider avec checkbox d'activation
        /// </summary>
        /// <param name="displayName">Nom à afficher</param>
        /// <param name="isEnabled">État actuel (activé/désactivé)</param>
        /// <param name="currentValue">Valeur actuelle</param>
        /// <param name="minValue">Valeur minimum</param>
        /// <param name="maxValue">Valeur maximum</param>
        /// <param name="unit">Unité à afficher</param>
        /// <param name="onEnabledChanged">Callback quand l'état change</param>
        /// <param name="onValueChanged">Callback quand la valeur change</param>
        /// <param name="onResetClicked">Callback pour reset</param>
        public static void DrawToggleSlider(
            string displayName,
            bool isEnabled,
            float currentValue,
            float minValue,
            float maxValue,
            string unit = "",
            System.Action<bool>? onEnabledChanged = null,
            System.Action<float>? onValueChanged = null,
            System.Action? onResetClicked = null)
        {
            // TODO: GUI APIs strippées dans IL2CPP - implementation différée
            // Placeholder implementation - sera implémenté via PerAspera.SDK.UI
        }

        /// <summary>
        /// Dessine une fenêtre draggable
        /// </summary>
        /// <param name="windowRect">Rectangle de la fenêtre</param>
        /// <param name="title">Titre de la fenêtre</param>
        /// <param name="content">Action pour dessiner le contenu</param>
        /// <param name="isDragging">État de drag actuel</param>
        /// <param name="dragOffset">Offset du drag</param>
        /// <returns>Nouveau rectangle et états de drag</returns>
        public static (Rect newRect, bool newIsDragging, Vector2 newDragOffset) DrawDraggableWindow(
            Rect windowRect,
            string title,
            System.Action content,
            bool isDragging,
            Vector2 dragOffset)
        {
            var newIsDragging = isDragging;
            var newDragOffset = dragOffset;

            // Gérer le dragging
            var currentEvent = Event.current;
            var titleBarRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 20);

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (titleBarRect.Contains(currentEvent.mousePosition))
                    {
                        newIsDragging = true;
                        newDragOffset = currentEvent.mousePosition - new Vector2(windowRect.x, windowRect.y);
                        currentEvent.Use();
                    }
                    break;

                case EventType.MouseUp:
                    newIsDragging = false;
                    break;

                case EventType.MouseDrag:
                    if (newIsDragging)
                    {
                        var newPosition = currentEvent.mousePosition - newDragOffset;
                        windowRect.x = Mathf.Clamp(newPosition.x, 0, Screen.width - windowRect.width);
                        windowRect.y = Mathf.Clamp(newPosition.y, 0, Screen.height - windowRect.height);
                        currentEvent.Use();
                    }
                    break;
            }

            // Dessiner la fenêtre
            GUILayout.BeginArea(windowRect, new GUIContent(title), GUI.skin.window);
            try
            {
                content?.Invoke();
            }
            finally
            {
                GUILayout.EndArea();
            }

            return (windowRect, newIsDragging, newDragOffset);
        }
    }
}