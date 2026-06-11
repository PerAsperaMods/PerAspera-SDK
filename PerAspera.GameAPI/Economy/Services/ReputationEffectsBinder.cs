using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Overrides.Models;
using PerAspera.GameAPI.Overrides.Registry;

namespace PerAspera.GameAPI.Economy.Services
{
    /// <summary>
    /// Lie les paliers de réputation à des <see cref="GetterOverride{T}"/> du jeu.
    /// <para>
    /// Quand la réputation d'une entité change de palier, le binder active/désactive
    /// ou met à jour des overrides enregistrés dans <see cref="GetterOverrideRegistry"/>.
    /// </para>
    /// <para>
    /// Override supporté confirmé dans le dump interop :<br/>
    /// <c>Faction.GetSpaceProjectResourceCostDivisor()</c> — divise le coût des ressources
    /// de projets spatiaux. Valeur 1.0 = pas d'effet, 2.0 = moitié prix, 0.5 = double prix.
    /// </para>
    /// <para>
    /// Pour chaque entité mirrorée, un override distinct est enregistré avec la clé
    /// <c>Faction.GetSpaceProjectResourceCostDivisor_{entityId}</c> afin d'éviter les conflits.
    /// </para>
    /// <code>
    /// var binder = new ReputationEffectsBinder();
    /// // Lier l'entité "corp_helios" à l'override de coût d'import
    /// binder.BindEntity("corp_helios", new float[] { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f });
    /// // S'abonner aux changements :
    /// EnhancedEventBus.Subscribe(ReputationChangedEvent.Key, binder.OnReputationChanged);
    /// </code>
    /// </summary>
    public sealed class ReputationEffectsBinder
    {
        private static readonly LogAspera _log = new LogAspera("ReputationEffectsBinder");

        // entityId → override enregistré (un par entité liée)
        private readonly Dictionary<string, GetterOverride<float>> _overrides = new();

        // Multiplicateurs par palier (index = (int)ReputationTier)
        // entityId → float[5]
        private readonly Dictionary<string, float[]> _tierMultipliers = new();

        // ─────────────────────────── Binding ───────────────────────────

        /// <summary>
        /// Lie une entité terrestre à l'override <c>Faction.GetSpaceProjectResourceCostDivisor</c>.
        /// À chaque changement de palier, le diviseur est mis à jour selon <paramref name="tierDivisors"/>.
        /// </summary>
        /// <param name="entityId">Id de l'entité (doit être dans <see cref="EarthEntityRegistry"/>).</param>
        /// <param name="tierDivisors">
        /// Tableau de 5 valeurs (Hostile→Allied). Défaut si null :
        /// 0.5 (Hostile), 0.75 (Wary), 1.0 (Neutral), 1.5 (Friendly), 2.0 (Allied).
        /// </param>
        public void BindEntity(string entityId, float[]? tierDivisors = null)
        {
            if (string.IsNullOrEmpty(entityId)) return;

            var divisors = tierDivisors ?? new float[] { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
            _tierMultipliers[entityId] = divisors;

            var entity = EarthEntityRegistry.Get(entityId);
            var initialTier = entity?.Reputation.Tier ?? ReputationTier.Neutral;
            var initialValue = GetDivisorForTier(divisors, initialTier);

            var overrideKey = $"GetSpaceProjectResourceCostDivisor_{entityId}";
            var getterOverride = new GetterOverride<float>(
                className:   "Faction",
                methodName:  overrideKey,
                defaultValue: 1.0f,
                displayName: $"SpaceProjectCostDivisor [{entityId}]");

            getterOverride.SetValue(initialValue);
            getterOverride.SetEnabled(true);

            GetterOverrideRegistry.RegisterOverride(getterOverride);
            _overrides[entityId] = getterOverride;

            _log.Info($"Binder: {entityId} lié (tier={initialTier}, divisor={initialValue:F2})");
        }

        /// <summary>Retire le binding et l'override pour une entité.</summary>
        public void UnbindEntity(string entityId)
        {
            if (_overrides.Remove(entityId, out var go))
            {
                go.SetEnabled(false);
                GetterOverrideRegistry.UnregisterOverride("Faction", $"GetSpaceProjectResourceCostDivisor_{entityId}");
                _tierMultipliers.Remove(entityId);
                _log.Debug($"Binder: {entityId} délié");
            }
        }

        // ─────────────────────────── Handler events ───────────────────────────

        /// <summary>
        /// Handler à brancher sur <c>EnhancedEventBus.Subscribe(ReputationChangedEvent.Key, ...)</c>.
        /// Met à jour l'override si l'entité est liée et que le palier a changé.
        /// </summary>
        public void OnReputationChanged(object eventData)
        {
            if (eventData is not ReputationChangedEvent ev) return;
            if (ev.OldTier == ev.NewTier) return; // pas de changement de palier
            if (!_overrides.TryGetValue(ev.EntityId, out var go)) return;
            if (!_tierMultipliers.TryGetValue(ev.EntityId, out var divisors)) return;

            var newDivisor = GetDivisorForTier(divisors, ev.NewTier);
            go.SetValue(newDivisor);

            _log.Info($"Binder [{ev.EntityId}]: {ev.OldTier}→{ev.NewTier} divisor={newDivisor:F2}");
        }

        // ─────────────────────────── Helpers ───────────────────────────

        private static float GetDivisorForTier(float[] divisors, ReputationTier tier)
        {
            var idx = (int)tier;
            return (idx >= 0 && idx < divisors.Length) ? divisors[idx] : 1.0f;
        }
    }
}
