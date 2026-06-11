using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Models;
using PerAspera.GameAPI.Events.Native;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Economy.Services
{
    /// <summary>
    /// Service SDK qui maintient le stock orbital à partir des projets spatiaux complétés.
    /// <para>Principe :</para>
    /// <list type="bullet">
    ///   <item>Les mods déclarent quels projets sont des exports orbitaux via <see cref="RegisterExportProject"/>.</item>
    ///   <item>À chaque <see cref="Events.Native.SpecialProjectCompletedNativeEvent"/>, si le projet est enregistré,
    ///   ses ressources collectées sont créditées dans <see cref="Stock"/>.</item>
    ///   <item>À chaque <see cref="Events.Native.PortProjectCancelledNativeEvent"/>, un log d'avertissement
    ///   est émis — les ressources en attente ne sont pas créditées.</item>
    /// </list>
    /// <para>Usage :</para>
    /// <code>
    /// var service = new OrbitalStockService();
    /// service.RegisterExportProject("mission_arean_survey");
    /// // Brancher sur les events natifs (SpaceProjectEventPatchingService doit être actif) :
    /// EnhancedEventBus.Subscribe(SpecialProjectCompletedNativeEvent.Key, service.OnProjectCompleted);
    /// EnhancedEventBus.Subscribe(PortProjectCancelledNativeEvent.Key,    service.OnProjectCancelled);
    /// </code>
    /// </summary>
    public sealed class OrbitalStockService
    {
        private static readonly LogAspera _log = new LogAspera("OrbitalStockService");

        private readonly HashSet<string> _exportProjects = new();

        /// <summary>Stock orbital géré par ce service.</summary>
        public OrbitalStock Stock { get; } = new OrbitalStock();

        // ─────────────────────────── Enregistrement projets ───────────────────────────

        /// <summary>
        /// Déclare qu'un projet spécial YAML est un export orbital.
        /// À sa complétion, ses ressources collectées créditeront <see cref="Stock"/>.
        /// </summary>
        /// <param name="projectKey"><c>SpecialProjectType.name</c> dans le YAML du jeu.</param>
        public void RegisterExportProject(string projectKey)
        {
            if (string.IsNullOrEmpty(projectKey)) return;
            _exportProjects.Add(projectKey);
            _log.Debug($"Export project enregistré : {projectKey}");
        }

        /// <summary>Retire un projet de la liste des exports.</summary>
        public void UnregisterExportProject(string projectKey)
        {
            if (_exportProjects.Remove(projectKey))
                _log.Debug($"Export project retiré : {projectKey}");
        }

        /// <summary>True si le projet est déclaré comme export orbital.</summary>
        public bool IsExportProject(string projectKey)
            => !string.IsNullOrEmpty(projectKey) && _exportProjects.Contains(projectKey);

        // ─────────────────────────── Handlers events natifs ───────────────────────────

        /// <summary>
        /// Handler pour <see cref="SpecialProjectCompletedNativeEvent.Key"/>.
        /// À brancher via <c>EnhancedEventBus.Subscribe</c>.
        /// </summary>
        public void OnProjectCompleted(object eventData)
        {
            if (eventData is not SpecialProjectCompletedNativeEvent ev) return;
            if (!IsExportProject(ev.ProjectKey)) return;

            foreach (var (resourceKey, qty) in ev.GatheredResources)
            {
                if (qty <= 0f) continue;
                Stock.Credit(resourceKey, qty, ev.ProjectKey);

                var newTotal = Stock.Get(resourceKey);
                EnhancedEventBus.Publish(OrbitalResourceSentEvent.Key, new OrbitalResourceSentEvent
                {
                    ResourceKey       = resourceKey,
                    Quantity          = qty,
                    NewOrbitalTotal   = newTotal,
                    SourceProjectKey  = ev.ProjectKey,
                });

                _log.Info($"Orbital credit: +{qty} [{resourceKey}] total={newTotal} (src={ev.ProjectKey})");
            }
        }

        /// <summary>
        /// Handler pour <see cref="PortProjectCancelledNativeEvent.Key"/>.
        /// À brancher via <c>EnhancedEventBus.Subscribe</c>.
        /// </summary>
        public void OnProjectCancelled(object eventData)
        {
            if (eventData is not PortProjectCancelledNativeEvent ev) return;
            if (!IsExportProject(ev.ProjectKey)) return;

            // Aucune ressource à inverser — le jeu les restitue lui-même via RecoverGatheredResources.
            // On logue simplement pour la traçabilité.
            _log.Info($"Orbital: projet annulé, aucun crédit émis ({ev.ProjectKey})");
        }
    }
}
