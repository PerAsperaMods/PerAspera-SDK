using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.Native;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Economy.Services
{
    /// <summary>
    /// Enregistrement d'un déclencheur d'incident diplomatique.
    /// </summary>
    internal sealed class IncidentTrigger
    {
        public string IncidentKey { get; init; } = string.Empty;
        public string EntityId    { get; init; } = string.Empty;
        /// <summary>Palier qui déclenche l'incident.</summary>
        public ReputationTier Tier { get; init; }
        /// <summary>True = déclencher à l'entrée dans le palier, False = à la sortie.</summary>
        public bool OnEnter       { get; init; } = true;
        /// <summary>True si déjà déclenché pour ce palier (anti-spam).</summary>
        public bool Fired         { get; set; }
    }

    /// <summary>
    /// Service de gestion des relations diplomatiques avec les entités terrestres.
    /// <para>Fonctions :</para>
    /// <list type="bullet">
    ///   <item><description>Appliquer des deltas de réputation + émettre <see cref="ReputationChangedEvent"/>.</description></item>
    ///   <item><description>Déclencher des incidents (<see cref="DiplomaticIncidentEvent"/>) sur changement de palier.</description></item>
    ///   <item><description>Miroir du score de réputation dans le blackboard principal du jeu
    ///   (<c>main.rep_{entityId}</c>) pour que les règles YAML natives déclenchent quêtes/embargos.</description></item>
    /// </list>
    /// <code>
    /// var diplomacy = new DiplomacyService();
    /// diplomacy.ApplyReputationDelta("corp_helios", +10f, "trade_completed", currentSol);
    /// diplomacy.RegisterIncidentTrigger("embargo_helios", "corp_helios", ReputationTier.Hostile);
    /// diplomacy.MirrorToBlackboard("corp_helios");  // main.rep_corp_helios = valeur
    /// </code>
    /// </summary>
    public sealed class DiplomacyService
    {
        private static readonly LogAspera _log = new LogAspera("DiplomacyService");

        private readonly List<IncidentTrigger> _triggers = new();
        // Cache des paliers précédents pour la détection des transitions
        private readonly Dictionary<string, ReputationTier> _previousTiers = new();
        // Clés entityId qui doivent être mirrorées dans le blackboard
        private readonly Dictionary<string, string> _blackboardMirrors = new(); // entityId → prefix

        // ─────────────────────────── Réputation ───────────────────────────

        /// <summary>
        /// Applique un delta de réputation à une entité enregistrée.
        /// Émet <see cref="ReputationChangedEvent"/> et, si le palier change, vérifie les incidents.
        /// </summary>
        /// <param name="entityId">Id de l'entité terrestre.</param>
        /// <param name="delta">Variation (+/-).</param>
        /// <param name="reason">Raison textuelle (ex: "trade_completed", "twitch_vote").</param>
        /// <param name="sol">Sol courant (0 si non fourni).</param>
        public void ApplyReputationDelta(string entityId, float delta, string reason, double sol = 0)
        {
            if (string.IsNullOrEmpty(entityId)) return;

            var entity = EarthEntityRegistry.Get(entityId);
            if (entity == null)
            {
                _log.Warning($"ApplyReputationDelta : entité '{entityId}' inconnue dans le registre");
                return;
            }

            var oldTier = entity.Reputation.Tier;
            entity.Reputation.Apply(delta, reason, sol);
            var newTier = entity.Reputation.Tier;

            EnhancedEventBus.Publish(ReputationChangedEvent.Key, new ReputationChangedEvent
            {
                EntityId  = entityId,
                Delta     = delta,
                NewValue  = entity.Reputation.Value,
                OldTier   = oldTier,
                NewTier   = newTier,
                Reason    = reason,
            });

            if (newTier != oldTier)
            {
                _log.Info($"Rep [{entityId}] : {oldTier} → {newTier} ({entity.Reputation.Value:F0})");
                CheckIncidentTriggers(entityId, oldTier, newTier);
            }

            UpdateBlackboardMirror(entityId, entity.Reputation.Value);
        }

        // ─────────────────────────── Incidents ───────────────────────────

        /// <summary>
        /// Enregistre un déclencheur d'incident diplomatique sur changement de palier.
        /// </summary>
        /// <param name="incidentKey">Clé de l'incident (ex: "embargo_helios").</param>
        /// <param name="entityId">Entité concernée.</param>
        /// <param name="tier">Palier qui déclenche l'incident.</param>
        /// <param name="onEnter">True = entrer dans le palier, False = sortir.</param>
        public void RegisterIncidentTrigger(string incidentKey, string entityId,
            ReputationTier tier, bool onEnter = true)
        {
            _triggers.Add(new IncidentTrigger
            {
                IncidentKey = incidentKey,
                EntityId    = entityId,
                Tier        = tier,
                OnEnter     = onEnter,
                Fired       = false,
            });
        }

        private void CheckIncidentTriggers(string entityId, ReputationTier oldTier, ReputationTier newTier)
        {
            foreach (var trigger in _triggers)
            {
                if (trigger.EntityId != entityId) continue;
                if (trigger.Fired) continue;

                bool shouldFire = trigger.OnEnter
                    ? newTier == trigger.Tier
                    : oldTier == trigger.Tier && newTier != trigger.Tier;

                if (!shouldFire) continue;

                trigger.Fired = true;
                EnhancedEventBus.Publish(DiplomaticIncidentEvent.Key, new DiplomaticIncidentEvent
                {
                    EntityId    = entityId,
                    IncidentKey = trigger.IncidentKey,
                    TriggerTier = trigger.Tier,
                });
                _log.Info($"Incident déclenché : [{entityId}] {trigger.IncidentKey} (tier={trigger.Tier})");
            }
        }

        // ─────────────────────────── Miroir Blackboard ───────────────────────────

        /// <summary>
        /// Active le miroir de réputation d'une entité dans le blackboard principal du jeu.
        /// La variable <c>main.{blackboardKeyPrefix}{entityId}</c> = valeur float ±100
        /// est mise à jour à chaque changement de réputation.
        /// <para>Cela permet aux Criterion YAML natifs de déclencher des quêtes/embargos :
        /// <c>main.rep_corp_helios &lt; -50 → mission_embargo_helios</c>.</para>
        /// </summary>
        /// <param name="entityId">Id de l'entité à mirrorer.</param>
        /// <param name="blackboardKeyPrefix">Préfixe de la clé blackboard (défaut "rep_").</param>
        public void MirrorToBlackboard(string entityId, string blackboardKeyPrefix = "rep_")
        {
            if (string.IsNullOrEmpty(entityId)) return;
            _blackboardMirrors[entityId] = blackboardKeyPrefix;
            _log.Debug($"Miroir blackboard activé : {entityId} → main.{blackboardKeyPrefix}{entityId}");
        }

        /// <summary>Désactive le miroir pour une entité.</summary>
        public void UnmirrorFromBlackboard(string entityId)
        {
            _blackboardMirrors.Remove(entityId);
        }

        private void UpdateBlackboardMirror(string entityId, float value)
        {
            if (!_blackboardMirrors.TryGetValue(entityId, out var prefix)) return;

            try
            {
                var universe = UniverseWrapper.GetCurrent();
                var bb = universe?.GetMainBlackBoard();
                if (bb == null)
                {
                    _log.Warning($"Miroir blackboard impossible : blackboardMain non disponible (sol trop tôt ?)");
                    return;
                }

                var key = $"{prefix}{entityId}";
                // Blackboard.SetValue(string, float) confirmé dans InteropDump\ScriptsAssembly\Blackboard.cs:503
                bb.NativeBlackboard?.SetValue(key, value);
            }
            catch (Exception ex)
            {
                _log.Warning($"Miroir blackboard erreur [{entityId}] : {ex.Message}");
            }
        }
    }
}
