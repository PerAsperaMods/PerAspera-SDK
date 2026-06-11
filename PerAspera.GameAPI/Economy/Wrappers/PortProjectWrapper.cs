using System.Collections.Generic;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Economy.Wrappers
{
    /// <summary>
    /// Wrapper typé pour <c>PortProject</c> (pipeline orbital d'un SpacePortComponent).
    /// Expose les données du projet en cours : stage courant, ressources collectées, état.
    /// <para>Membres vérifiés dans <c>Tools\InteropDump\ScriptsAssembly\PortProject.cs</c> :
    /// <c>stageKey</c> (string field), <c>stageCompletionFactor</c> (float), <c>done</c> (bool),
    /// <c>developmentResources</c> (Dictionary&lt;ResourceType, CargoQuantity&gt;),
    /// <c>specialProject.type.name</c> (string).</para>
    /// </summary>
    /// <example>
    /// var wrapper = new PortProjectWrapper(portProject);
    /// if (wrapper.Done)
    ///     foreach (var (res, qty) in wrapper.GetDevelopmentResources())
    ///         OrbitalStock.Credit(res, qty, wrapper.ProjectKey);
    /// </example>
    public sealed class PortProjectWrapper : WrapperBase
    {
        /// <summary>Proxy interop typé exposé (accès bas-niveau si besoin).</summary>
        public PortProject? NativePortProject => GetNativeObject() as PortProject;

        /// <summary>Construit un wrapper autour d'un proxy interop typé.</summary>
        public PortProjectWrapper(PortProject? nativePortProject) : base(nativePortProject) { }

        /// <summary>Construit un wrapper depuis un object non typé (compatibilité).</summary>
        public PortProjectWrapper(object? nativePortProject) : base(nativePortProject) { }

        /// <summary>Clé du projet spatial (<c>SpecialProject.type.name</c>). Vide si non disponible.</summary>
        public string ProjectKey => NativePortProject?.specialProject?.type?.name ?? string.Empty;

        /// <summary>Clé du stage courant (<c>PortProject._stageKey</c>). Ex: "gather", "trade", "complete".</summary>
        public string StageKey => NativePortProject?._stageKey ?? string.Empty;

        /// <summary>Progression du stage courant (0–1).</summary>
        public float StageCompletionFactor => NativePortProject?.stageCompletionFactor ?? 0f;

        /// <summary>True si le projet est terminé (<c>PortProject.done</c>).</summary>
        public bool Done => NativePortProject?.done ?? false;

        /// <summary>True si le projet a été retiré (<c>PortProject.removed</c>).</summary>
        public bool Removed => NativePortProject?.removed ?? false;

        /// <summary>True si les ressources requises ont été collectées (<c>PortProject.resourcesMet</c>).</summary>
        public bool ResourcesMet => NativePortProject?.resourcesMet ?? false;

        /// <summary>
        /// Retourne le snapshot des ressources collectées par ce port.
        /// Clés = <c>ResourceType.name</c> (ex: "resource_iron"), valeurs en unités (<c>CargoQuantity.ToFloat()</c>).
        /// Retourne un dictionnaire vide si le projet est null ou sans ressources.
        /// </summary>
        public IReadOnlyDictionary<string, float> GetDevelopmentResources()
        {
            var result = new Dictionary<string, float>();
            var devRes = NativePortProject?.developmentResources;
            if (devRes == null) return result;

            foreach (var kv in devRes)
            {
                var name = kv.Key?.name;
                if (!string.IsNullOrEmpty(name))
                    result[name] = kv.Value.ToFloat();
            }
            return result;
        }

        /// <summary>Factory — retourne null si le projet natif est null.</summary>
        public static PortProjectWrapper? FromNative(PortProject? native)
            => native != null ? new PortProjectWrapper(native) : null;
    }
}
