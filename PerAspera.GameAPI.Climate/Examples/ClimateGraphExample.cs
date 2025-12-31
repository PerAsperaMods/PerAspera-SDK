using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate;
using PerAspera.GameAPI.Climate.Tests;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate.Examples
{
    /// <summary>
    /// Exemple d'utilisation complète du système Climate avec graphiques de terraformation
    /// Démontre l'intégration entre système cellulaire, graphiques et mod YAML
    /// </summary>
    public static class ClimateGraphExample
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Example");
        private static ClimateController? _controller;
        
        /// <summary>
        /// Exemple d'initialisation complète du système
        /// À appeler depuis un plugin BepInX après initialisation du jeu
        /// </summary>
        public static void InitializeClimateSystem()
        {
            Log.Info("=== Initialisation du système Climate + Graphiques ===");
            
            try
            {
                // 1. Obtenir la planète actuelle
                var planet = PlanetWrapper.GetCurrent();
                if (planet == null)
                {
                    Log.Error("Aucune planète active trouvée");
                    return;
                }
                
                // 2. Créer et configurer le contrôleur climatique
                _controller = new ClimateController();
                _controller.EnableClimateControl(planet);
                
                // 3. Vérifier que les graphiques sont bien connectés
                if (_controller.GraphDataProvider == null)
                {
                    throw new Exception("GraphDataProvider non initialisé");
                }
                
                // 4. Exécuter les tests d'intégration
                Log.Info("Exécution des tests d'intégration...");
                TerraformingGraphIntegrationTests.RunIntegrationTests();
                
                // 5. Démonstration des capacités
                DemonstrateGraphCapabilities();
                
                Log.Info("✓ Système Climate + Graphiques initialisé avec succès");
                
                // 6. Démarrer la surveillance périodique
                StartPeriodicMonitoring();
                
            }
            catch (Exception ex)
            {
                Log.Error($"Échec de l'initialisation: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Démontre les capacités du système de graphiques
        /// </summary>
        private static void DemonstrateGraphCapabilities()
        {
            if (_controller?.GraphDataProvider == null) return;
            
            Log.Info("=== Démonstration des capacités graphiques ===");
            
            var provider = _controller.GraphDataProvider;
            
            // Températures régionales
            Log.Info("Températures régionales (système cellulaire 5°):");
            Log.Info($"  • Pôle Nord: {_controller.GetNorthPoleTemperature():F1} K");
            Log.Info($"  • Équateur: {_controller.GetEquatorTemperature():F1} K");
            Log.Info($"  • Pôle Sud: {_controller.GetSouthPoleTemperature():F1} K");
            
            // Données cellulaires
            Log.Info("Données cellulaires:");
            Log.Info($"  • Cellules actives: {_controller.GetActiveCellsCount()}");
            Log.Info($"  • Pression moyenne: {_controller.GetTerraformingGraphData("Pressure_ActiveCells"):F2} mbar");
            
            // Gaz atmosphériques étendus (MoreResources)
            Log.Info("Gaz atmosphériques étendus:");
            string[] gases = { "CH4", "Ar", "Ne", "He", "Kr", "Xe" };
            foreach (var gas in gases)
            {
                var pressure = _controller.GetTerraformingGraphData($"{gas} Pressure");
                Log.Info($"  • {gas}: {pressure:F4} mbar");
            }
            
            // Données de variance et distribution
            Log.Info("Analyse de distribution cellulaire:");
            Log.Info($"  • Variance O2: {_controller.GetTerraformingGraphData("o2_cellular_variance"):F3}");
            Log.Info($"  • Points chauds CO2: {_controller.GetTerraformingGraphData("co2_cellular_hotspots"):F0}");
        }
        
        /// <summary>
        /// Démarre une surveillance périodique du système
        /// </summary>
        private static void StartPeriodicMonitoring()
        {
            Log.Info("Démarrage de la surveillance périodique (toutes les 60s)");
            
            // Dans un vrai plugin, utiliser un Timer ou intégration avec le game loop
            // Ici on simule juste l'affichage du statut
            LogSystemStatus();
        }
        
        /// <summary>
        /// Affiche le statut actuel du système
        /// </summary>
        public static void LogSystemStatus()
        {
            if (_controller == null) return;
            
            Log.Info("=== Statut système Climate + Graphiques ===");
            
            // Statut général
            Log.Info($"Contrôleur actif: {(_controller.AtmosphereGrid != null ? "✓" : "✗")}");
            Log.Info($"Graphiques actifs: {(_controller.GraphDataProvider != null ? "✓" : "✗")}");
            
            if (_controller.AtmosphereGrid != null && _controller.GraphDataProvider != null)
            {
                var activeCells = _controller.GetActiveCellsCount();
                var avgTemp = _controller.GetEquatorTemperature();
                var avgPressure = _controller.GetTerraformingGraphData("Pressure_ActiveCells");
                
                Log.Info($"Performance: {activeCells} cellules, T={avgTemp:F1}K, P={avgPressure:F2}mbar");
            }
            
            // Statistiques des graphiques
            var stats = Patches.TerraformingGraphPatches.GetTerraformingGraphStats();
            Log.Info($"Statistiques: {stats["ControlledPlanets"]} planètes, {stats["TotalActiveCells"]} cellules");
        }
        
        /// <summary>
        /// Exemple de mise à jour périodique
        /// À appeler depuis le Update() de votre plugin
        /// </summary>
        public static void UpdateClimateSystem(float deltaTime)
        {
            try
            {
                _controller?.UpdateClimate(deltaTime);
                
                // Mise à jour globale des données de graphiques
                Patches.TerraformingGraphPatches.UpdateAllTerraformingGraphData();
            }
            catch (Exception ex)
            {
                Log.Error($"Erreur mise à jour Climate: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test de performance du système complet
        /// </summary>
        public static void RunPerformanceTest()
        {
            Log.Info("=== Test de performance système complet ===");
            
            try
            {
                // Test intégration
                TerraformingGraphIntegrationTests.RunPerformanceTest();
                
                // Test mises à jour en boucle
                var startTime = DateTime.Now;
                
                for (int i = 0; i < 1000; i++)
                {
                    UpdateClimateSystem(1.0f); // 1 seconde simulée
                }
                
                var elapsed = DateTime.Now - startTime;
                Log.Info($"✓ 1000 mises à jour système complet en {elapsed.TotalMilliseconds:F2}ms");
                
                if (elapsed.TotalMilliseconds > 5000)
                {
                    Log.Warning("Performance système dégradée détectée");
                }
                else
                {
                    Log.Info("✓ Performance système acceptable");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Test de performance échoué: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Nettoyage des ressources
        /// À appeler lors de l'arrêt du plugin
        /// </summary>
        public static void Shutdown()
        {
            Log.Info("Arrêt du système Climate + Graphiques");
            
            _controller?.DisableClimateControl();
            _controller = null;
            
            Log.Info("✓ Système arrêté proprement");
        }
        
        /// <summary>
        /// Exemple d'ajout de gaz atmosphérique personnalisé
        /// </summary>
        public static void RegisterCustomAtmosphericGas(string symbol, string name, string unit = "mbar")
        {
            if (_controller?.GraphDataProvider == null)
            {
                Log.Warning("Système non initialisé - impossible d'enregistrer le gaz");
                return;
            }
            
            _controller.RegisterAtmosphericGas(symbol, name, unit);
            Log.Info($"✓ Gaz personnalisé enregistré: {name} ({symbol})");
        }
    }
}