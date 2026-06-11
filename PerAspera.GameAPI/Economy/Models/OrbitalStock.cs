using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Economy.Models
{
    /// <summary>
    /// Registre du stock orbital : ressources collectées par les projets spatiaux
    /// en attente d'être achetées par des entités terrestres.
    /// <para>
    /// Ce stock est géré côté SDK (pas de stock natif "orbital" dans Per Aspera).
    /// Alimenté par <c>OrbitalStockService</c> sur <see cref="Events.Native.SpecialProjectCompletedNativeEvent"/>.
    /// </para>
    /// </summary>
    /// <example>
    /// // Crédit après export
    /// orbitalStock.Credit("resource_iron", 500f, "mission_arean_survey");
    ///
    /// // Débit pour livraison
    /// if (!orbitalStock.TryDebit("resource_iron", 100f, "trade_offer_abc"))
    ///     return TradeResult.Fail("Stock orbital insuffisant");
    ///
    /// // Abonnement aux changements
    /// orbitalStock.OnChanged += (key, delta, total) =>
    ///     LogAspera.Info($"Orbital [{key}] Δ{delta:+0.#;-0.#} → {total}");
    /// </example>
    public sealed class OrbitalStock
    {
        private readonly Dictionary<string, float> _stock = new();
        private readonly object _lock = new object();

        /// <summary>
        /// Déclenché à chaque modification : (resourceKey, delta, newTotal).
        /// </summary>
        public event Action<string, float, float>? OnChanged;

        /// <summary>Retourne le stock courant pour une ressource (0 si absente).</summary>
        public float Get(string resourceKey)
        {
            lock (_lock)
            {
                return _stock.TryGetValue(resourceKey, out var v) ? v : 0f;
            }
        }

        /// <summary>Retourne un snapshot (copie) du stock complet.</summary>
        public IReadOnlyDictionary<string, float> Snapshot()
        {
            lock (_lock) { return new Dictionary<string, float>(_stock); }
        }

        /// <summary>
        /// Crédite une ressource dans le stock orbital.
        /// </summary>
        /// <param name="resourceKey">Clé YAML de la ressource.</param>
        /// <param name="amount">Quantité à ajouter (doit être &gt; 0).</param>
        /// <param name="source">Source du crédit (ex: nom de projet, pour traçabilité).</param>
        public void Credit(string resourceKey, float amount, string source)
        {
            if (amount <= 0f || string.IsNullOrEmpty(resourceKey)) return;

            float newTotal;
            lock (_lock)
            {
                _stock.TryGetValue(resourceKey, out var current);
                newTotal = current + amount;
                _stock[resourceKey] = newTotal;
            }
            OnChanged?.Invoke(resourceKey, amount, newTotal);
        }

        /// <summary>
        /// Tente de débiter une ressource. Retourne false si le stock est insuffisant
        /// (atomique : rien n'est modifié en cas d'échec).
        /// </summary>
        /// <param name="resourceKey">Clé YAML de la ressource.</param>
        /// <param name="amount">Quantité à retirer (doit être &gt; 0).</param>
        /// <param name="reason">Raison du débit (pour traçabilité).</param>
        /// <returns>True si le débit a réussi.</returns>
        public bool TryDebit(string resourceKey, float amount, string reason)
        {
            if (amount <= 0f || string.IsNullOrEmpty(resourceKey)) return false;

            float newTotal;
            lock (_lock)
            {
                _stock.TryGetValue(resourceKey, out var current);
                if (current < amount) return false;
                newTotal = current - amount;
                _stock[resourceKey] = newTotal;
            }
            OnChanged?.Invoke(resourceKey, -amount, newTotal);
            return true;
        }
    }
}
