using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.Native;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Economy.Services
{
    /// <summary>Résultat d'une tentative d'acceptation d'offre commerciale.</summary>
    public sealed class TradeResult
    {
        /// <summary>True si la transaction a été effectuée.</summary>
        public bool Success { get; init; }
        /// <summary>Raison de l'échec (null si succès).</summary>
        public string? FailureReason { get; init; }
        /// <summary>Détails de la transaction (null si échec).</summary>
        public TradeTransaction? Transaction { get; init; }

        /// <summary>Crée un résultat de succès.</summary>
        public static TradeResult Ok(TradeTransaction tx) => new() { Success = true, Transaction = tx };
        /// <summary>Crée un résultat d'échec.</summary>
        public static TradeResult Fail(string reason) => new() { Success = false, FailureReason = reason };
    }

    /// <summary>
    /// Service de commerce entre Mars et les entités terrestres.
    /// <para>Gère les offres actives, leur expiration, et l'exécution des transactions.</para>
    /// <para>Dépendances :</para>
    /// <list type="bullet">
    ///   <item><description><see cref="OrbitalStockService"/> — pour débiter le stock orbital (ventes).</description></item>
    ///   <item><description><see cref="DiplomacyService"/> — pour appliquer les deltas de réputation.</description></item>
    ///   <item><description><see cref="FactionWrapper"/> joueur — pour créditer/débiter la monnaie.</description></item>
    /// </list>
    /// <para>La clé monnaie (<c>moneyResourceKey</c>) est fournie par le mod — le SDK ne définit
    /// aucune ressource monnaie.</para>
    /// <code>
    /// var trade = new TradeService(orbitalService, diplomacy, "resource_terrian_credits");
    /// trade.PostOffer(new TradeOffer { EntityId="corp_helios", ResourceKey="resource_iron", ... });
    /// var result = trade.Accept(offerId, () => FactionWrapper.GetPlayerFaction());
    /// </code>
    /// </summary>
    public sealed class TradeService
    {
        private static readonly LogAspera _log = new LogAspera("TradeService");

        private readonly OrbitalStockService _orbital;
        private readonly DiplomacyService _diplomacy;
        private readonly string _moneyResourceKey;

        private readonly Dictionary<string, TradeOffer> _offers = new();
        private readonly List<PendingDelivery> _pendingDeliveries = new();
        private readonly object _lock = new object();

        // Livraisons différées (achat Terre→Mars)
        private sealed class PendingDelivery
        {
            public TradeTransaction Transaction { get; init; } = null!;
            public double DeliverOnSol { get; init; }
            public string FactionId { get; init; } = string.Empty;
        }

        /// <summary>
        /// Initialise le service.
        /// </summary>
        /// <param name="orbital">Service de stock orbital (pour débits ventes).</param>
        /// <param name="diplomacy">Service de diplomatie (pour réputation).</param>
        /// <param name="moneyResourceKey">Clé YAML de la ressource monnaie du mod.</param>
        public TradeService(OrbitalStockService orbital, DiplomacyService diplomacy, string moneyResourceKey)
        {
            _orbital          = orbital ?? throw new ArgumentNullException(nameof(orbital));
            _diplomacy        = diplomacy ?? throw new ArgumentNullException(nameof(diplomacy));
            _moneyResourceKey = moneyResourceKey ?? throw new ArgumentNullException(nameof(moneyResourceKey));
        }

        // ─────────────────────────── Offres ───────────────────────────

        /// <summary>Retourne la liste des offres actives (non expirées).</summary>
        public IReadOnlyList<TradeOffer> ActiveOffers
        {
            get { lock (_lock) { return _offers.Values.ToList(); } }
        }

        /// <summary>
        /// Publie une offre commerciale. Si une offre avec le même Id existe déjà, elle est écrasée.
        /// </summary>
        public void PostOffer(TradeOffer offer)
        {
            if (offer == null || string.IsNullOrEmpty(offer.Id)) return;
            lock (_lock) { _offers[offer.Id] = offer; }
            _log.Debug($"Offre postée : {offer.Id} ({offer.EntityId} / {offer.Direction} / {offer.ResourceKey})");
        }

        /// <summary>Retire une offre de la liste active (ex : expirée manuellement).</summary>
        public void RetractOffer(string offerId)
        {
            lock (_lock) { _offers.Remove(offerId); }
        }

        // ─────────────────────────── Acceptation ───────────────────────────

        /// <summary>
        /// Accepte une offre et exécute la transaction.
        /// <para>Vente (Mars→Terre) : débite le stock orbital + crédite la monnaie au joueur.</para>
        /// <para>Achat (Terre→Mars) : débite la monnaie du joueur + enregistre une livraison différée.</para>
        /// </summary>
        /// <param name="offerId">Id de l'offre à accepter.</param>
        /// <param name="getPlayerFaction">Callback qui retourne le wrapper faction joueur courant.</param>
        /// <param name="currentSol">Sol courant (pour les délais de livraison).</param>
        /// <param name="deliveryDelaySols">Nombre de sols avant livraison (achat uniquement).</param>
        public TradeResult Accept(string offerId, Func<FactionWrapper?> getPlayerFaction,
            double currentSol = 0, double deliveryDelaySols = 5)
        {
            TradeOffer? offer;
            lock (_lock)
            {
                if (!_offers.TryGetValue(offerId, out offer))
                    return TradeResult.Fail($"Offre inconnue : {offerId}");
                if (currentSol > 0 && currentSol > offer.ExpiresOnSol)
                {
                    _offers.Remove(offerId);
                    return TradeResult.Fail($"Offre expirée : {offerId}");
                }
            }

            var faction = getPlayerFaction?.Invoke();
            if (faction == null) return TradeResult.Fail("Faction joueur introuvable");

            TradeResult result = offer.Direction == TradeDirection.MarsToEarth
                ? ExecuteSell(offer, faction, currentSol)
                : ExecuteBuy(offer, faction, currentSol, deliveryDelaySols);

            if (result.Success)
            {
                lock (_lock) { _offers.Remove(offerId); }
                _diplomacy.ApplyReputationDelta(offer.EntityId, offer.ReputationDelta,
                    "trade_completed", currentSol);

                EnhancedEventBus.Publish(TradeCompletedEvent.Key,
                    new TradeCompletedEvent { Transaction = result.Transaction! });

                _log.Info($"Trade OK : {offer.EntityId} {offer.Direction} {offer.Quantity}×{offer.ResourceKey}");
            }

            return result;
        }

        private TradeResult ExecuteSell(TradeOffer offer, FactionWrapper faction, double sol)
        {
            if (!_orbital.Stock.TryDebit(offer.ResourceKey, offer.Quantity, offer.Id))
                return TradeResult.Fail($"Stock orbital insuffisant pour {offer.ResourceKey}");

            var revenue = offer.Quantity * offer.PricePerUnit;
            faction.AddResource(_moneyResourceKey, revenue);

            return TradeResult.Ok(new TradeTransaction
            {
                Sol               = sol,
                EntityId          = offer.EntityId,
                ResourceKey       = offer.ResourceKey,
                Quantity          = offer.Quantity,
                PricePerUnit      = offer.PricePerUnit,
                Direction         = TradeDirection.MarsToEarth,
                ReputationDeltaApplied = offer.ReputationDelta,
            });
        }

        private TradeResult ExecuteBuy(TradeOffer offer, FactionWrapper faction,
            double sol, double delaySols)
        {
            var cost = offer.Quantity * offer.PricePerUnit;
            if (!faction.TryRemoveResource(_moneyResourceKey, cost))
                return TradeResult.Fail($"Monnaie insuffisante (besoin {cost} {_moneyResourceKey})");

            var tx = new TradeTransaction
            {
                Sol               = sol,
                EntityId          = offer.EntityId,
                ResourceKey       = offer.ResourceKey,
                Quantity          = offer.Quantity,
                PricePerUnit      = offer.PricePerUnit,
                Direction         = TradeDirection.EarthToMars,
                ReputationDeltaApplied = offer.ReputationDelta,
            };

            // Livraison différée
            lock (_lock)
            {
                _pendingDeliveries.Add(new PendingDelivery
                {
                    Transaction   = tx,
                    DeliverOnSol  = sol + delaySols,
                    FactionId     = faction.Name,
                });
            }

            return TradeResult.Ok(tx);
        }

        // ─────────────────────────── Tick ───────────────────────────

        /// <summary>
        /// À appeler à chaque sol (brancher sur <c>MartianDayEventData</c> ou <c>OnDaysPassed</c>).
        /// Traite les expirations d'offres et les livraisons différées.
        /// </summary>
        /// <param name="currentSol">Sol courant.</param>
        /// <param name="getPlayerFaction">Callback faction joueur (pour les livraisons).</param>
        public void Tick(double currentSol, Func<FactionWrapper?> getPlayerFaction)
        {
            List<string> expired;
            List<PendingDelivery> due;

            lock (_lock)
            {
                expired = _offers.Values
                    .Where(o => o.ExpiresOnSol > 0 && currentSol > o.ExpiresOnSol)
                    .Select(o => o.Id)
                    .ToList();

                due = _pendingDeliveries
                    .Where(d => currentSol >= d.DeliverOnSol)
                    .ToList();

                foreach (var id in expired) _offers.Remove(id);
                foreach (var d in due)      _pendingDeliveries.Remove(d);
            }

            if (expired.Count > 0)
                _log.Debug($"Offres expirées : {expired.Count}");

            var faction = getPlayerFaction?.Invoke();
            foreach (var delivery in due)
            {
                faction?.AddResource(delivery.Transaction.ResourceKey, delivery.Transaction.Quantity);
                EnhancedEventBus.Publish(TradeCompletedEvent.Key,
                    new TradeCompletedEvent { Transaction = delivery.Transaction });
                _log.Info($"Livraison : +{delivery.Transaction.Quantity} [{delivery.Transaction.ResourceKey}]");
            }
        }
    }
}
