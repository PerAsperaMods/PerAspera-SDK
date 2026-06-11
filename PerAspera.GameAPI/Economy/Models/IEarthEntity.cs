using System.Collections.Generic;

namespace PerAspera.GameAPI.Economy.Models
{
    /// <summary>
    /// Contrat d'une entité terrestre (pays ou corporation) visible par le SDK.
    /// Les entités concrètes sont déclarées par les mods consommateurs, jamais par le SDK.
    /// </summary>
    /// <example>
    /// public class HeliosCorp : EarthEntity
    /// {
    ///     public HeliosCorp() : base("corp_helios", "Helios Corp", EarthEntityType.Corporation) { }
    ///     public override float GetOfferedPrice(string resourceKey) => resourceKey == "resource_iron" ? 1.5f : 1.0f;
    /// }
    /// </example>
    public interface IEarthEntity
    {
        /// <summary>Identifiant stable (slug), ex: "corp_helios", "country_esa".</summary>
        string Id { get; }
        /// <summary>Nom affiché (localisation à la charge du mod).</summary>
        string DisplayName { get; }
        /// <summary>Type : pays ou corporation.</summary>
        EarthEntityType Type { get; }
        /// <summary>Score de réputation de cette entité.</summary>
        ReputationScore Reputation { get; }
        /// <summary>Clés de ressources YAML demandées par cette entité (ex: "resource_iron").</summary>
        IReadOnlyList<string> DemandedResources { get; }
        /// <summary>Budget fictif en unité monnaie-mod (non géré par le SDK).</summary>
        float Budget { get; set; }
        /// <summary>Prix offert par l'entité pour un achat de cette ressource (unité monnaie/unité).</summary>
        float GetOfferedPrice(string resourceKey);
    }

    /// <summary>
    /// Implémentation de base d'une entité terrestre.
    /// Les mods héritent de cette classe et surchargent <see cref="GetOfferedPrice"/> si nécessaire.
    /// </summary>
    public abstract class EarthEntity : IEarthEntity
    {
        /// <inheritdoc/>
        public string Id { get; }
        /// <inheritdoc/>
        public string DisplayName { get; }
        /// <inheritdoc/>
        public EarthEntityType Type { get; }
        /// <inheritdoc/>
        public ReputationScore Reputation { get; } = new ReputationScore();
        /// <inheritdoc/>
        public IReadOnlyList<string> DemandedResources { get; protected set; } = new List<string>();
        /// <inheritdoc/>
        public float Budget { get; set; }

        /// <summary>Initialise l'entité avec ses données de base.</summary>
        protected EarthEntity(string id, string displayName, EarthEntityType type)
        {
            Id          = id;
            DisplayName = displayName;
            Type        = type;
        }

        /// <inheritdoc/>
        /// <remarks>Par défaut retourne 1.0 (prix neutre). Surcharger pour moduler par ressource.</remarks>
        public virtual float GetOfferedPrice(string resourceKey) => 1.0f;
    }
}
