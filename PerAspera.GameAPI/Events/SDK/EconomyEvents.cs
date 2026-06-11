using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.Core;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== ECONOMY SDK EVENTS ====================
    // Ces événements sont émis par les services Economy SDK (OrbitalStockService,
    // TradeService, DiplomacyService). Ils ne nécessitent aucun interop IL2CPP.

    /// <summary>
    /// Émis par <c>OrbitalStockService</c> quand une ressource est créditée dans le stock orbital
    /// suite à la complétion d'un projet spatial exporté.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(OrbitalResourceSentEvent.Key, e =>
    /// {
    ///     var ev = (OrbitalResourceSentEvent)e;
    ///     hudPanel.Refresh(ev.ResourceKey, ev.NewOrbitalTotal);
    /// });
    /// </example>
    public class OrbitalResourceSentEvent : SDKEventBase
    {
        public const string Key = "SDK:OrbitalResourceSent";
        public override string EventType => Key;

        /// <summary>Clé YAML de la ressource (<c>ResourceType.name</c>).</summary>
        public string ResourceKey { get; set; } = string.Empty;
        /// <summary>Quantité créditée.</summary>
        public float Quantity { get; set; }
        /// <summary>Nouveau total orbital après crédit.</summary>
        public float NewOrbitalTotal { get; set; }
        /// <summary>Clé du projet source (ex: "mission_arean_survey").</summary>
        public string SourceProjectKey { get; set; } = string.Empty;

        public override string ToString() =>
            $"OrbitalResourceSent: +{Quantity} [{ResourceKey}] total={NewOrbitalTotal} src={SourceProjectKey}";
    }

    /// <summary>
    /// Émis par <c>TradeService</c> quand une transaction commerciale est conclue.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(TradeCompletedEvent.Key, e =>
    /// {
    ///     var ev = (TradeCompletedEvent)e;
    ///     Logger.Info($"Trade: {ev.Transaction.Quantity} {ev.Transaction.ResourceKey} → {ev.Transaction.EntityId}");
    /// });
    /// </example>
    public class TradeCompletedEvent : SDKEventBase
    {
        public const string Key = "SDK:TradeCompleted";
        public override string EventType => Key;

        /// <summary>Détails complets de la transaction.</summary>
        public TradeTransaction Transaction { get; set; } = new TradeTransaction();

        public override string ToString() =>
            $"TradeCompleted: {Transaction.EntityId} {Transaction.Direction} {Transaction.Quantity}×{Transaction.ResourceKey} @{Transaction.PricePerUnit}/u";
    }

    /// <summary>
    /// Émis par <c>DiplomacyService</c> quand la réputation d'une entité terrestre change.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(ReputationChangedEvent.Key, e =>
    /// {
    ///     var ev = (ReputationChangedEvent)e;
    ///     if (ev.NewTier != ev.OldTier) hudPanel.ShowTierChange(ev.EntityId, ev.NewTier);
    /// });
    /// </example>
    public class ReputationChangedEvent : SDKEventBase
    {
        public const string Key = "SDK:ReputationChanged";
        public override string EventType => Key;

        /// <summary>Id de l'entité concernée.</summary>
        public string EntityId { get; set; } = string.Empty;
        /// <summary>Variation appliquée (peut être moindre que demandé si clampée).</summary>
        public float Delta { get; set; }
        /// <summary>Nouvelle valeur (clampée ±100).</summary>
        public float NewValue { get; set; }
        /// <summary>Palier avant la variation.</summary>
        public ReputationTier OldTier { get; set; }
        /// <summary>Palier après la variation.</summary>
        public ReputationTier NewTier { get; set; }
        /// <summary>Raison textuelle (ex: "trade_completed", "twitch_vote").</summary>
        public string Reason { get; set; } = string.Empty;

        public override string ToString() =>
            $"ReputationChanged: [{EntityId}] {Delta:+0.#;-0.#} → {NewValue:F0} ({NewTier}) [{Reason}]";
    }

    /// <summary>
    /// Émis par <c>DiplomacyService</c> quand un seuil de réputation configuré est franchi.
    /// Permet aux mods YAML de déclencher des quêtes ou embargos via le miroir blackboard.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(DiplomaticIncidentEvent.Key, e =>
    /// {
    ///     var ev = (DiplomaticIncidentEvent)e;
    ///     if (ev.IncidentKey == "embargo_helios") FactionWrapper.GetPlayer()?.ApplyImportPenalty();
    /// });
    /// </example>
    public class DiplomaticIncidentEvent : SDKEventBase
    {
        public const string Key = "SDK:DiplomaticIncident";
        public override string EventType => Key;

        /// <summary>Id de l'entité impliquée.</summary>
        public string EntityId { get; set; } = string.Empty;
        /// <summary>Clé de l'incident déclaré par le mod (ex: "embargo_helios").</summary>
        public string IncidentKey { get; set; } = string.Empty;
        /// <summary>Palier ayant déclenché l'incident.</summary>
        public ReputationTier TriggerTier { get; set; }

        public override string ToString() =>
            $"DiplomaticIncident: [{EntityId}] incident={IncidentKey} tier={TriggerTier}";
    }
}
#pragma warning restore CS1591
