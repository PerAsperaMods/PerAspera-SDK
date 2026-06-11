using System;

namespace PerAspera.GameAPI.Economy.Models
{
    /// <summary>
    /// Offre de commerce émise par une entité terrestre.
    /// Immuable une fois créée. Publiée dans <c>TradeService.ActiveOffers</c>.
    /// </summary>
    /// <example>
    /// var offer = new TradeOffer
    /// {
    ///     Id             = Guid.NewGuid().ToString(),
    ///     EntityId       = "corp_helios",
    ///     ResourceKey    = "resource_iron",
    ///     Quantity       = 500f,
    ///     PricePerUnit   = 1.5f,
    ///     Direction      = TradeDirection.MarsToEarth,
    ///     ExpiresOnSol   = currentSol + 10,
    ///     ReputationDelta = 5f,
    /// };
    /// tradeService.PostOffer(offer);
    /// </example>
    public sealed class TradeOffer
    {
        /// <summary>Identifiant unique de l'offre.</summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();
        /// <summary>Id de l'entité terrestre qui propose cet échange.</summary>
        public string EntityId { get; init; } = string.Empty;
        /// <summary>Clé YAML de la ressource concernée.</summary>
        public string ResourceKey { get; init; } = string.Empty;
        /// <summary>Quantité (en unités du jeu).</summary>
        public float Quantity { get; init; }
        /// <summary>Prix par unité (en unité monnaie-mod).</summary>
        public float PricePerUnit { get; init; }
        /// <summary>Direction de l'échange.</summary>
        public TradeDirection Direction { get; init; }
        /// <summary>Sol d'expiration de l'offre.</summary>
        public double ExpiresOnSol { get; init; }
        /// <summary>Impact de réputation si l'offre est acceptée.</summary>
        public float ReputationDelta { get; init; }
    }

    /// <summary>
    /// Transaction conclue, résultat de l'acceptation d'une <see cref="TradeOffer"/>.
    /// </summary>
    public sealed class TradeTransaction
    {
        /// <summary>Identifiant unique.</summary>
        public string Id { get; init; } = Guid.NewGuid().ToString();
        /// <summary>Sol martien de la transaction.</summary>
        public double Sol { get; init; }
        /// <summary>Id de l'entité terrestre impliquée.</summary>
        public string EntityId { get; init; } = string.Empty;
        /// <summary>Clé YAML de la ressource.</summary>
        public string ResourceKey { get; init; } = string.Empty;
        /// <summary>Quantité échangée.</summary>
        public float Quantity { get; init; }
        /// <summary>Prix par unité au moment de la transaction.</summary>
        public float PricePerUnit { get; init; }
        /// <summary>Direction de l'échange.</summary>
        public TradeDirection Direction { get; init; }
        /// <summary>Delta de réputation appliqué lors de la transaction.</summary>
        public float ReputationDeltaApplied { get; init; }
    }
}
