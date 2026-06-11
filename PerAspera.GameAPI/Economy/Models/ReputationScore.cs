using System;
using System.Collections.Generic;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Economy.Models
{
    /// <summary>
    /// Enregistre un delta de réputation avec sa raison et son sol.
    /// </summary>
    public sealed class ReputationDelta
    {
        /// <summary>Variation appliquée (positive = amélioration, négative = dégradation).</summary>
        public float Delta { get; }
        /// <summary>Raison textuelle du changement (ex: "trade_completed", "twitch_vote").</summary>
        public string Reason { get; }
        /// <summary>Sol martien au moment de la variation.</summary>
        public double Sol { get; }

        public ReputationDelta(float delta, string reason, double sol)
        {
            Delta  = delta;
            Reason = reason ?? string.Empty;
            Sol    = sol;
        }
    }

    /// <summary>
    /// Score de réputation d'une entité terrestre, clampé entre -100 et +100.
    /// Thread-safe pour la lecture (snapshot + event). Non thread-safe pour <see cref="Apply"/>.
    /// </summary>
    /// <example>
    /// rep.Apply(+10f, "trade_completed");
    /// rep.OnChanged += d => LogAspera.Info($"Rep: {d.Delta:+0.#;-0.#} ({d.Reason})");
    /// </example>
    public sealed class ReputationScore
    {
        /// <summary>Seuils de palier (modifiables globalement avant l'init des entités).</summary>
        public static float[] Thresholds { get; set; } = { -50f, -20f, 20f, 50f };

        private float _value;
        private readonly List<ReputationDelta> _history = new();

        /// <summary>Valeur courante, clampée dans [-100, +100].</summary>
        public float Value => _value;

        /// <summary>Palier calculé depuis <see cref="Value"/> et <see cref="Thresholds"/>.</summary>
        public ReputationTier Tier => ComputeTier(_value);

        /// <summary>Historique des variations (lecture seule).</summary>
        public IReadOnlyList<ReputationDelta> History => _history;

        /// <summary>Déclenché à chaque variation.</summary>
        public event Action<ReputationDelta>? OnChanged;

        /// <summary>Construit un score à la valeur initiale donnée (défaut 0 = Neutral).</summary>
        public ReputationScore(float initialValue = 0f)
        {
            _value = Clamp(initialValue);
        }

        /// <summary>
        /// Applique un delta et notifie les abonnés.
        /// </summary>
        /// <param name="delta">Variation (+/-). Sera clampé si la valeur résultante dépasse ±100.</param>
        /// <param name="reason">Raison textuelle (ex: "trade_completed").</param>
        /// <param name="sol">Sol courant (0 si non fourni).</param>
        public void Apply(float delta, string reason, double sol = 0)
        {
            var before = _value;
            _value = Clamp(_value + delta);
            var actualDelta = _value - before;

            var record = new ReputationDelta(actualDelta, reason, sol);
            _history.Add(record);
            OnChanged?.Invoke(record);
        }

        private static float Clamp(float v) => MathF.Max(-100f, MathF.Min(100f, v));

        private static ReputationTier ComputeTier(float v)
        {
            var t = Thresholds;
            if (v < t[0]) return ReputationTier.Hostile;
            if (v < t[1]) return ReputationTier.Wary;
            if (v < t[2]) return ReputationTier.Neutral;
            if (v < t[3]) return ReputationTier.Friendly;
            return ReputationTier.Allied;
        }
    }
}
#pragma warning restore CS1591
