using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.Data;
using PerAspera.GameAPI.Events.Native;

namespace PerAspera.GameAPI.Economy.Services
{
    /// <summary>
    /// Configuration du simulateur de marché terrestre.
    /// </summary>
    public sealed class MarketSimulatorConfig
    {
        /// <summary>Volatilité des prix (0 = stable, 1 = très volatile). Défaut 0.15.</summary>
        public float PriceVolatility { get; set; } = 0.15f;

        /// <summary>Nombre d'offres générées par sol (par entité). Défaut 0–2.</summary>
        public int MinOffersPerSolPerEntity { get; set; } = 0;
        /// <summary>Maximum d'offres générées par sol par entité. Défaut 2.</summary>
        public int MaxOffersPerSolPerEntity { get; set; } = 2;

        /// <summary>Durée de vie d'une offre en sols. Défaut 10.</summary>
        public double OfferLifetimeSols { get; set; } = 10.0;

        /// <summary>Multiplicateur de prix selon le palier de réputation (Hostile → Allied).</summary>
        public float[] TierPriceMultipliers { get; set; } =
        {
            0.5f,   // Hostile  — prix très défavorables
            0.75f,  // Wary
            1.0f,   // Neutral
            1.25f,  // Friendly
            1.5f,   // Allied   — prix très favorables
        };

        /// <summary>
        /// Seed pour la génération reproductible des offres (derive from initial sol to be save-stable).
        /// Si 0, utilise le sol courant comme seed.
        /// </summary>
        public int Seed { get; set; } = 0;
    }

    /// <summary>
    /// Simule le marché terrestre : génère des offres de commerce à chaque sol
    /// en fonction des entités enregistrées, de leur réputation et d'une marche aléatoire des prix.
    /// <para>
    /// Brancher sur <c>EnhancedEventBus.Subscribe("MartianDayPassed", simulator.OnSolTick)</c>.
    /// </para>
    /// <para>
    /// Reproductibilité : la seed est dérivée du sol pour garantir les mêmes offres
    /// après un save/load au même sol.
    /// </para>
    /// <code>
    /// var sim = new EarthMarketSimulator(tradeService, new MarketSimulatorConfig { Seed = 42 });
    /// EnhancedEventBus.Subscribe("MartianDayPassed", sim.OnSolTick);
    /// </code>
    /// </summary>
    public sealed class EarthMarketSimulator
    {
        private static readonly LogAspera _log = new LogAspera("EarthMarketSimulator");

        private readonly TradeService _trade;
        private readonly MarketSimulatorConfig _cfg;

        // Marche aléatoire des prix : resourceKey → prix courant
        private readonly Dictionary<string, float> _currentPrices = new();

        /// <summary>Config courante (lecture seule).</summary>
        public MarketSimulatorConfig Config => _cfg;

        /// <summary>
        /// Initialise le simulateur.
        /// </summary>
        /// <param name="tradeService">Service de commerce auquel les offres générées seront postées.</param>
        /// <param name="config">Configuration (optionnel — utilise les valeurs par défaut si null).</param>
        public EarthMarketSimulator(TradeService tradeService, MarketSimulatorConfig? config = null)
        {
            _trade = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
            _cfg   = config ?? new MarketSimulatorConfig();
        }

        // ─────────────────────────── Handler sol ───────────────────────────

        /// <summary>
        /// Handler à brancher sur <c>EnhancedEventBus.Subscribe("MartianDayPassed", ...)</c>.
        /// Génère les offres pour ce sol et purge les offres expirées (via TradeService.Tick).
        /// </summary>
        public void OnSolTick(object eventData)
        {
            double sol = 0;
            if (eventData is MartianDayEventData day)
                sol = day.CurrentSol;

            try
            {
                GenerateOffersForSol(sol);
            }
            catch (Exception ex)
            {
                _log.Warning($"EarthMarketSimulator.OnSolTick erreur (sol={sol}) : {ex.Message}");
            }
        }

        // ─────────────────────────── Génération d'offres ───────────────────────────

        private void GenerateOffersForSol(double sol)
        {
            var entities = EarthEntityRegistry.All;
            if (entities.Count == 0) return;

            // Seed déterministe : sol courant → reproductible après save/load
            var seed = _cfg.Seed != 0 ? _cfg.Seed ^ (int)sol : (int)sol;
            var rng  = new Random(seed);

            foreach (var entity in entities)
            {
                int count = rng.Next(_cfg.MinOffersPerSolPerEntity, _cfg.MaxOffersPerSolPerEntity + 1);
                if (count == 0) continue;

                var demanded = entity.DemandedResources;
                if (demanded == null || demanded.Count == 0) continue;

                for (int i = 0; i < count; i++)
                {
                    // Ressource aléatoire dans la liste demandée
                    var resourceKey = demanded[rng.Next(demanded.Count)];

                    // Prix : marche aléatoire + bonus palier réputation
                    var price = ComputePrice(entity, resourceKey, rng);

                    // Quantité : 100–1000 unités
                    float qty = rng.Next(100, 1001);

                    // Direction : principalement Mars→Terre (export) mais parfois Terre→Mars (import)
                    var direction = rng.NextDouble() < 0.7
                        ? TradeDirection.MarsToEarth
                        : TradeDirection.EarthToMars;

                    // Delta réputation modest positif à chaque trade conclu
                    float repDelta = direction == TradeDirection.MarsToEarth ? 2f : 1f;

                    var offer = new TradeOffer
                    {
                        Id             = $"{entity.Id}_{resourceKey}_{sol:F0}_{i}",
                        EntityId       = entity.Id,
                        ResourceKey    = resourceKey,
                        Quantity       = qty,
                        PricePerUnit   = price,
                        Direction      = direction,
                        ExpiresOnSol   = sol + _cfg.OfferLifetimeSols,
                        ReputationDelta = repDelta,
                    };

                    _trade.PostOffer(offer);
                }
            }

            _log.Debug($"Marché sol {sol:F0} : offres générées pour {entities.Count} entités");
        }

        // ─────────────────────────── Prix ───────────────────────────

        private float ComputePrice(IEarthEntity entity, string resourceKey, Random rng)
        {
            // Prix de base de l'entité
            if (!_currentPrices.TryGetValue($"{entity.Id}_{resourceKey}", out var basePrice))
                basePrice = entity.GetOfferedPrice(resourceKey);

            // Marche aléatoire : ±volatilité
            var change    = (float)(rng.NextDouble() * 2.0 - 1.0) * _cfg.PriceVolatility;
            var newPrice  = MathF.Max(0.01f, basePrice * (1f + change));
            _currentPrices[$"{entity.Id}_{resourceKey}"] = newPrice;

            // Multiplicateur selon le palier de réputation
            var tier  = (int)entity.Reputation.Tier;
            var mults = _cfg.TierPriceMultipliers;
            var mult  = (tier >= 0 && tier < mults.Length) ? mults[tier] : 1f;

            return MathF.Round(newPrice * mult, 2);
        }
    }
}
