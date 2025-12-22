using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI;

#nullable enable

// Aliases pour éviter les conflits de noms
using NativeBaseGame = PerAspera.GameAPI.Native.BaseGame;
using NativeUniverse = PerAspera.GameAPI.Native.Universe;
using NativePlanet = PerAspera.GameAPI.Native.Planet;
using NativeFaction = PerAspera.GameAPI.Native.Faction;

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// Native Instance Manager - Central registry for live game object instances
    /// Provides access to BaseGame, Universe, Planet, and Faction singletons
    /// Auto-updates on game state changes (new game, load game, etc.)
    ///
    /// 📋 Critical Gap Resolution: F:\ModPeraspera\Internal_doc\SDK\newfeature\TODO\CRITICAL-GAPS-FROM-LOGS-ANALYSIS.md
    /// 🎯 Agent: @per-aspera-sdk-coordinator
    /// 🔄 Integration: GameTypeInitializer, Harmony patches for auto-registration
    /// </summary>
    public static class InstanceManager
    {
        private static readonly LogAspera Log = new LogAspera("Native.InstanceManager");

        // Registry des instances natives
        private static readonly Dictionary<string, object> _instances = new();
        private static bool _isInitialized = false;

        // Événements pour notifier les changements d'instances
        public static event Action<string, object?>? OnInstanceChanged;

        /// <summary>
        /// Initialise le manager d'instances
        /// Doit être appelé au démarrage du SDK
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                Log.Warning("InstanceManager already initialized");
                return;
            }

            _isInitialized = true;
            Log.Info("Native InstanceManager initialized - ready for instance registration");

            // Auto-enregistrement des instances existantes si disponibles
            TryAutoRegisterExistingInstances();
        }

        /// <summary>
        /// Enregistre une instance native dans le registry
        /// </summary>
        /// <param name="key">Clé d'identification (ex: "BaseGame", "CurrentPlanet")</param>
        /// <param name="instance">Instance native IL2CPP</param>
        public static void RegisterInstance(string key, object instance)
        {
            if (!_isInitialized)
            {
                Log.Warning($"Cannot register instance '{key}': InstanceManager not initialized");
                return;
            }

            if (instance == null)
            {
                Log.Warning($"Cannot register null instance for key '{key}'");
                return;
            }

            var oldInstance = _instances.ContainsKey(key) ? _instances[key] : null;
            _instances[key] = instance;

            Log.Info($"Registered native instance: {key} = {instance.GetType().Name}");

            // Notifier les listeners du changement
            OnInstanceChanged?.Invoke(key, instance);

            // Validation de cohérence
            ValidateInstanceRegistration(key, instance);
        }

        /// <summary>
        /// Récupère une instance native depuis le registry
        /// </summary>
        /// <typeparam name="T">Type attendu de l'instance</typeparam>
        /// <param name="key">Clé d'identification</param>
        /// <returns>Instance native ou null si non trouvée</returns>
        public static T? GetInstance<T>(string key) where T : class
        {
            if (!_instances.TryGetValue(key, out var instance))
            {
                Log.Debug($"Instance not found: {key}");
                return null;
            }

            if (instance is T typedInstance)
            {
                return typedInstance;
            }

            Log.Warning($"Instance type mismatch for '{key}': expected {typeof(T).Name}, got {instance.GetType().Name}");
            return null;
        }

        /// <summary>
        /// Vérifie si une instance est enregistrée
        /// </summary>
        public static bool HasInstance(string key)
        {
            return _instances.ContainsKey(key);
        }

        /// <summary>
        /// Supprime une instance du registry
        /// </summary>
        public static void UnregisterInstance(string key)
        {
            if (_instances.Remove(key))
            {
                Log.Info($"Unregistered instance: {key}");
                OnInstanceChanged?.Invoke(key, null);
            }
        }

        /// <summary>
        /// Obtient le statut du registry pour le debugging
        /// </summary>
        public static string GetStatus()
        {
            if (!_isInitialized)
                return "InstanceManager: NOT_INITIALIZED";

            var registeredCount = _instances.Count;
            var keys = string.Join(", ", _instances.Keys);

            return $"InstanceManager: INITIALIZED | {registeredCount} instances registered | Keys: [{keys}]";
        }

        /// <summary>
        /// Liste toutes les instances enregistrées (pour debugging)
        /// </summary>
        public static IEnumerable<KeyValuePair<string, object>> GetAllInstances()
        {
            return _instances;
        }

        // ========== MÉTHODES SPÉCIALISÉES POUR LES TYPES PRINCIPAUX ==========

        /// <summary>
        /// Enregistre l'instance BaseGame (singleton principal)
        /// </summary>
        public static void RegisterBaseGame(object baseGameInstance)
        {
            RegisterInstance("BaseGame", baseGameInstance);
        }

        /// <summary>
        /// Récupère l'instance BaseGame
        /// </summary>
        public static object? GetBaseGame()
        {
            return GetInstance<object>("BaseGame");
        }

        /// <summary>
        /// Enregistre l'instance Universe actuelle
        /// </summary>
        public static void RegisterUniverse(object universeInstance)
        {
            RegisterInstance("Universe", universeInstance);
        }

        /// <summary>
        /// Récupère l'instance Universe
        /// </summary>
        public static object? GetUniverse()
        {
            return GetInstance<object>("Universe");
        }

        /// <summary>
        /// Enregistre la planète actuelle
        /// </summary>
        public static void RegisterCurrentPlanet(object planetInstance)
        {
            RegisterInstance("CurrentPlanet", planetInstance);
        }

        /// <summary>
        /// Récupère la planète actuelle
        /// </summary>
        public static object? GetCurrentPlanet()
        {
            return GetInstance<object>("CurrentPlanet");
        }

        /// <summary>
        /// Enregistre une faction active
        /// </summary>
        /// <param name="factionId">ID unique de la faction</param>
        /// <param name="factionInstance">Instance native de la faction</param>
        public static void RegisterFaction(string factionId, object factionInstance)
        {
            RegisterInstance($"Faction.{factionId}", factionInstance);
        }

        /// <summary>
        /// Récupère une faction par son ID
        /// </summary>
        public static object? GetFaction(string factionId)
        {
            return GetInstance<object>($"Faction.{factionId}");
        }

        /// <summary>
        /// Liste toutes les factions enregistrées
        /// </summary>
        public static IEnumerable<object> GetAllFactions()
        {
            var factions = new List<object>();
            foreach (var kvp in _instances)
            {
                if (kvp.Key.StartsWith("Faction."))
                {
                    factions.Add(kvp.Value);
                }
            }
            return factions;
        }

        // ========== AUTO-REGISTRATION LOGIC ==========

        /// <summary>
        /// Tente d'auto-enregistrer les instances existantes au démarrage
        /// Utilise la réflexion pour trouver les singletons du jeu
        /// </summary>
        private static void TryAutoRegisterExistingInstances()
        {
            try
            {
                Log.Info("Attempting auto-registration of existing game instances...");

                // Essayer de trouver BaseGame.Instance
                var baseGameInstance = TryFindBaseGameInstance();
                if (baseGameInstance != null)
                {
                    RegisterBaseGame(baseGameInstance);
                    Log.Info("Auto-registered BaseGame instance");

                    // Si BaseGame trouvé, essayer Universe et Planet
                    var universeInstance = TryFindUniverseInstance(baseGameInstance);
                    if (universeInstance != null)
                    {
                        RegisterUniverse(universeInstance);
                        Log.Info("Auto-registered Universe instance");

                        var planetInstance = TryFindCurrentPlanetInstance(universeInstance);
                        if (planetInstance != null)
                        {
                            RegisterCurrentPlanet(planetInstance);
                            Log.Info("Auto-registered CurrentPlanet instance");
                        }
                    }
                }
                else
                {
                    Log.Warning("Could not auto-register BaseGame - game may not be fully loaded yet");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Auto-registration failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Recherche l'instance BaseGame via réflexion
        /// </summary>
        private static object? TryFindBaseGameInstance()
        {
            try
            {
                // Utilise la réflexion directe pour trouver BaseGame
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetName().Name?.Contains("Assembly-CSharp") == true ||
                        assembly.GetName().Name?.Contains("PerAspera") == true)
                    {
                        var baseGameType = assembly.GetType("BaseGame");
                        if (baseGameType != null)
                        {
                            // Recherche la propriété statique Instance
                            var instanceProperty = baseGameType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if (instanceProperty != null)
                            {
                                var instance = instanceProperty.GetValue(null);
                                if (instance != null)
                                {
                                    Log.Info($"Found BaseGame.Instance via reflection");
                                    return instance;
                                }
                            }
                        }
                    }
                }

                Log.Warning("BaseGame instance not found via reflection");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error finding BaseGame instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Recherche l'instance Universe depuis BaseGame
        /// </summary>
        private static object? TryFindUniverseInstance(object baseGameInstance)
        {
            try
            {
                var universeProperty = baseGameInstance.GetType().GetProperty("universe");
                if (universeProperty != null)
                {
                    var universe = universeProperty.GetValue(baseGameInstance);
                    if (universe != null)
                    {
                        Log.Info("Found Universe via BaseGame.universe");
                        return universe;
                    }
                }

                Log.Warning("Universe property not found on BaseGame");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error finding Universe instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Recherche la planète actuelle depuis Universe
        /// </summary>
        private static object? TryFindCurrentPlanetInstance(object universeInstance)
        {
            try
            {
                var planetProperty = universeInstance.GetType().GetProperty("currentPlanet");
                if (planetProperty != null)
                {
                    var planet = planetProperty.GetValue(universeInstance);
                    if (planet != null)
                    {
                        Log.Info("Found currentPlanet via Universe.currentPlanet");
                        return planet;
                    }
                }

                Log.Warning("currentPlanet property not found on Universe");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error finding currentPlanet instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Valide la cohérence d'une instance enregistrée
        /// </summary>
        private static void ValidateInstanceRegistration(string key, object instance)
        {
            // Validation basique - peut être étendue
            if (instance == null)
            {
                Log.Warning($"Validation failed: null instance for key '{key}'");
                return;
            }

            // Pour les types critiques, vérifier qu'ils ont les propriétés attendues
            switch (key)
            {
                case "BaseGame":
                    ValidateBaseGameInstance(instance);
                    break;
                case "Universe":
                    ValidateUniverseInstance(instance);
                    break;
                case "CurrentPlanet":
                    ValidatePlanetInstance(instance);
                    break;
            }
        }

        private static void ValidateBaseGameInstance(object instance)
        {
            // Vérifier que BaseGame a les propriétés attendues
            var type = instance.GetType();
            var hasUniverse = type.GetProperty("universe") != null;
            if (!hasUniverse)
            {
                Log.Warning("BaseGame instance validation: missing 'universe' property");
            }
        }

        private static void ValidateUniverseInstance(object instance)
        {
            var type = instance.GetType();
            var hasCurrentPlanet = type.GetProperty("currentPlanet") != null;
            if (!hasCurrentPlanet)
            {
                Log.Warning("Universe instance validation: missing 'currentPlanet' property");
            }
        }

        private static void ValidatePlanetInstance(object instance)
        {
            var type = instance.GetType();
            var hasAtmosphere = type.GetProperty("atmosphere") != null;
            if (!hasAtmosphere)
            {
                Log.Warning("Planet instance validation: missing 'atmosphere' property");
            }
        }
    }
}