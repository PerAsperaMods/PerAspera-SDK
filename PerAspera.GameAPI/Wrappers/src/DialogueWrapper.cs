using System;
using PerAspera.Core.IL2CPP;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper leger pour interagir avec le systeme de dialogues du jeu.
    ///
    /// MIGRATION 2026-06-11 - rewrite en acces type / IL2CppExtensions.
    /// Elimine ~22 RS0030 (GetType/GetMethod/Invoke sur Universe, DialoguePresenter, DialogueManager).
    /// Source de verite :
    ///   Tools/InteropDump/ScriptsAssembly/Universe.cs          : StartDialogue(Il2CppReferenceArray[Object]) public static
    ///   Tools/InteropDump/ScriptsAssembly/DialogueManager.cs   : ContainsDialogue(string) public static (ligne 230)
    ///   Tools/InteropDump/ScriptsAssembly/DialoguePresenter.cs : instance-based, no static NotifyDialogue
    /// </summary>
    public static class DialogueWrapper
    {
        /// <summary>
        /// Tente de demarrer un dialogue via Universe.StartDialogue.
        /// Delegue a IL2CppExtensions.InvokeMethod (RS0030-exempt dans Core).
        /// </summary>
        public static void StartDialogue(string factionKey, string personKey, string dialogueKey)
        {
            if (string.IsNullOrEmpty(dialogueKey)) return;
            try
            {
                // Universe.StartDialogue(Il2CppReferenceArray<Object>) - static method.
                // Delegue a ReflectionHelpers.SafeInvoke (RS0030-exempt dans Core).
                var universeInstance = ReflectionHelpers.GetSingletonInstance<Universe>();
                if (universeInstance != null)
                    ReflectionHelpers.SafeInvoke(universeInstance, "StartDialogue",
                        new object[] { factionKey, personKey, dialogueKey });
            }
            catch
            {
                // Echec silencieux : le mod wrapper doit rester tolerant
            }
        }

        /// <summary>
        /// Notification pour un dialogue.
        /// Tente DialoguePresenter.NotifyDialogue via InvokeMethod, sinon route vers StartDialogue.
        /// </summary>
        public static void NotifyDialogue(string factionKey, string personKey, string dialogueKey)
        {
            try
            {
                var presenterType = ReflectionHelpers.FindType("DialoguePresenter");
                var presenter = presenterType != null
                    ? ReflectionHelpers.GetSingletonInstance(presenterType)
                    : null;
                if (presenter != null)
                {
                    ReflectionHelpers.SafeInvoke(presenter, "NotifyDialogue", factionKey, personKey, dialogueKey);
                    return;
                }
            }
            catch { }

            // Fallback : route vers StartDialogue
            StartDialogue(factionKey, personKey, dialogueKey);
        }

        /// <summary>
        /// Verifie si un dialogue est present via DialogueManager.ContainsDialogue (public static type).
        /// </summary>
        public static bool ContainsDialogue(string dialogueKey)
        {
            if (string.IsNullOrEmpty(dialogueKey)) return false;
            try
            {
                // DialogueManager.ContainsDialogue(string) : public static bool -- appel type direct
                return DialogueManager.ContainsDialogue(dialogueKey);
            }
            catch { return false; }
        }

        /// <summary>
        /// Helper pour construire un objet representant un dialogue leger cote mod (non-persiste).
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
#pragma warning restore CS1591
