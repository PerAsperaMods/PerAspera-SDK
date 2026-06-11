using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Economy.Models;

namespace PerAspera.GameAPI.Economy
{
    /// <summary>
    /// Registre statique des entités terrestres déclarées par les mods.
    /// Le SDK lui-même ne livre aucune entité — c'est une responsabilité des mods consommateurs.
    /// <para>
    /// Cycle de vie typique :
    /// <code>
    /// // Dans le plugin BepInEx du mod :
    /// EarthEntityRegistry.Register(new HeliosCorp());
    /// EarthEntityRegistry.Register(new EsaCountry());
    /// // ...
    /// EarthEntityRegistry.OnRegistered += entity => LogAspera.Info($"Entity: {entity.Id}");
    /// </code>
    /// </para>
    /// </summary>
    public static class EarthEntityRegistry
    {
        private static readonly Dictionary<string, IEarthEntity> _entities = new();
        private static readonly object _lock = new object();

        /// <summary>Déclenché après chaque enregistrement réussi.</summary>
        public static event Action<IEarthEntity>? OnRegistered;

        /// <summary>Déclenché après chaque suppression réussie.</summary>
        public static event Action<string>? OnUnregistered;

        /// <summary>
        /// Enregistre une entité. Lance <see cref="ArgumentException"/> si l'Id est déjà pris.
        /// </summary>
        /// <param name="entity">Entité à enregistrer. Son <c>Id</c> doit être unique.</param>
        /// <exception cref="ArgumentNullException">entity ou entity.Id est null.</exception>
        /// <exception cref="ArgumentException">Un entité avec ce même Id existe déjà.</exception>
        public static void Register(IEarthEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entity.Id)) throw new ArgumentException("Entity.Id ne peut pas être vide.", nameof(entity));

            lock (_lock)
            {
                if (_entities.ContainsKey(entity.Id))
                    throw new ArgumentException($"Une entité avec l'Id '{entity.Id}' est déjà enregistrée.", nameof(entity));
                _entities[entity.Id] = entity;
            }

            OnRegistered?.Invoke(entity);
        }

        /// <summary>
        /// Supprime une entité par son Id. Retourne false si l'Id est inconnu.
        /// </summary>
        public static bool Unregister(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return false;

            bool removed;
            lock (_lock) { removed = _entities.Remove(entityId); }

            if (removed) OnUnregistered?.Invoke(entityId);
            return removed;
        }

        /// <summary>
        /// Retourne l'entité correspondant à l'Id, ou null si absente.
        /// </summary>
        public static IEarthEntity? Get(string entityId)
        {
            if (string.IsNullOrEmpty(entityId)) return null;
            lock (_lock) { return _entities.TryGetValue(entityId, out var e) ? e : null; }
        }

        /// <summary>
        /// Retourne une copie en lecture seule de toutes les entités enregistrées.
        /// </summary>
        public static IReadOnlyCollection<IEarthEntity> All
        {
            get
            {
                lock (_lock) { return new List<IEarthEntity>(_entities.Values); }
            }
        }

        /// <summary>
        /// Vide le registre (utile pour les tests ou le rechargement de save).
        /// </summary>
        public static void Clear()
        {
            lock (_lock) { _entities.Clear(); }
        }
    }
}
