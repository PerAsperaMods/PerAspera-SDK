using System;
using System.Reflection;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper léger pour interagir avec le système de dialogues du jeu.
    /// Utilise la réflexion pour éviter une dépendance forte sur les assemblies IL2CPP.
    /// </summary>
    public static class DialogueWrapper
    {
        /// <summary>
        /// Tente de démarrer un dialogue en appelant une méthode statique native probable.
        /// Signature attendue (exemples) : Universe.StartDialogue(string factionKey, string personKey, string dialogueKey)
        /// </summary>
        public static void StartDialogue(string factionKey, string personKey, string dialogueKey)
        {
            if (string.IsNullOrEmpty(dialogueKey)) return;

            var universeType = Type.GetType("Universe") ?? Type.GetType("PerAspera.Universe, Assembly-CSharp");
            if (universeType == null) return;

            // Cherche StartDialogue public static
            var method = universeType.GetMethod("StartDialogue", BindingFlags.Public | BindingFlags.Static);
            if (method == null) method = universeType.GetMethod("StartDialogue", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) return;

            try
            {
                method.Invoke(null, new object[] { factionKey, personKey, dialogueKey });
            }
            catch (TargetParameterCountException)
            {
                // Tentative d'appel avec signature alternative
                try { method.Invoke(null, new object[] { dialogueKey }); } catch { }
            }
            catch
            {
                // Echec silencieux : le mod wrapper doit rester tolérant
            }
        }

        /// <summary>
        /// Notification pour un dialogue (ex: NotifyMandatoryDialogue via commandes du moteur)
        /// Si une méthode dédiée existe, tente de l'appeler.
        /// </summary>
        public static void NotifyDialogue(string factionKey, string personKey, string dialogueKey)
        {
            var presenterType = Type.GetType("DialoguePresenter") ?? Type.GetType("PerAspera.DialoguePresenter, Assembly-CSharp");
            if (presenterType != null)
            {
                var notify = presenterType.GetMethod("NotifyDialogue", BindingFlags.Public | BindingFlags.Static) ?? presenterType.GetMethod("NotifyDialogue", BindingFlags.Instance | BindingFlags.Public);
                if (notify != null)
                {
                    try
                    {
                        if (notify.IsStatic) notify.Invoke(null, new object[] { factionKey, personKey, dialogueKey });
                        else
                        {
                            // crée une instance si nécessaire (tolérance maximale)
                            var inst = Activator.CreateInstance(presenterType);
                            notify.Invoke(inst, new object[] { factionKey, personKey, dialogueKey });
                        }
                        return;
                    }
                    catch { }
                }
            }

            // Fallback : route vers StartDialogue
            StartDialogue(factionKey, personKey, dialogueKey);
        }

        /// <summary>
        /// Vérifie de manière heuristique si un dialogue est présent via DialogueManager.ContainsDialogue
        /// </summary>
        public static bool ContainsDialogue(string dialogueKey)
        {
            if (string.IsNullOrEmpty(dialogueKey)) return false;
            var managerType = Type.GetType("DialogueManager") ?? Type.GetType("PerAspera.DialogueManager, Assembly-CSharp");
            if (managerType == null) return false;

            var method = managerType.GetMethod("ContainsDialogue", BindingFlags.Public | BindingFlags.Static) ?? managerType.GetMethod("HasDialogue", BindingFlags.Public | BindingFlags.Static);
            if (method == null) return false;
            try
            {
                var res = method.Invoke(null, new object[] { dialogueKey });
                return res is bool b && b;
            }
            catch { return false; }
        }

        /// <summary>
        /// Helper pour construire un objet représentant un dialogue léger côté mod (non-persisté).
        /// Utile pour tests et génération dynamique de dialogues avant conversion en YAML/TextAsset.
        /// </summary>
        public static DialogueDescriptor BuildDialogue(string id, string title, params DialogueLine[] lines)
        {
            return new DialogueDescriptor
            {
                Id = id,
                Title = title,
                Lines = lines ?? Array.Empty<DialogueLine>()
            };
        }
    }

    public class DialogueDescriptor
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DialogueLine[] Lines { get; set; }
    }

    public class DialogueLine
    {
        public string SpeakerKey { get; set; }
        public string TextKey { get; set; }
        public string AudioKey { get; set; }
        public string[] Options { get; set; }
    }
}
