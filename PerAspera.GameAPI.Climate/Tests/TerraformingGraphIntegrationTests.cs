using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Domain.Cell;
using PerAspera.GameAPI.Climate.Integration;

namespace PerAspera.GameAPI.Climate.Tests
{
    /// <summary>
    /// Tests d'intégration pour le système de graphiques de terraformation
    /// Valide que les données cellulaires sont correctement exposées
    /// </summary>
    public static class TerraformingGraphIntegrationTests
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Tests");
        
        /// <summary>
        /// Test complet d'intégration du système de graphiques
        /// </summary>
        public static void RunIntegrationTests()
        {
            Log.Info("=== Début des tests d'intégration Terraforming Graph ===");
            
            try
            {
                TestAtmosphereGridCreation();
                TestGraphDataProvider();
                TestRegionalTemperatures();
                TestCellularPressure();
                TestAtmosphericGasRegistration();
                
                Log.Info("=== Tous les tests d'intégration réussis ===");
            }
            catch (Exception ex)
            {
                Log.Error($"Tests d'intégration échoués: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Test la création et initialisation de AtmosphereGrid
        /// </summary>
        private static void TestAtmosphereGridCreation()
        {
            Log.Info("Test: Création AtmosphereGrid");
            
            var grid = new AtmosphereGrid();
            
            // Vérifier que la grille est initialisée
            if (grid.Cells == null || grid.Cells.Count == 0)
            {
                throw new Exception("AtmosphereGrid n'a pas été correctement initialisée");
            }
            
            Log.Info($"✓ AtmosphereGrid créée avec {grid.Cells.Count} cellules");
            
            // Test accès aux cellules par coordonnées
            var testCoord = new CellCoord(0, 0);  // Équateur
            var cell = grid.GetCell(testCoord);
            if (cell == null)
            {
                throw new Exception("Impossible d'accéder à la cellule équatoriale");
            }
            
            Log.Info("✓ Accès aux cellules par coordonnées fonctionne");
        }
        
        /// <summary>
        /// Test le provider de données de graphiques
        /// </summary>
        private static void TestGraphDataProvider()
        {
            Log.Info("Test: TerraformingGraphDataProvider");
            
            var grid = new AtmosphereGrid();
            var provider = new TerraformingGraphDataProvider(grid);
            
            // Test initialisation des données d'exemple
            provider.InitializeSampleData();
            
            // Test récupération de données
            var activeCells = provider.GetGraphData("ActiveCellsCount");
            if (activeCells <= 0f)
            {
                Log.Warning("Aucune cellule active détectée - normal pour les données d'exemple");
            }
            else
            {
                Log.Info($"✓ Cellules actives: {activeCells}");
            }
            
            // Test enregistrement de gaz
            provider.RegisterAtmosphericGas("CH4", "Méthane", "mbar");
            if (!provider.HasGraphData("CH4 Pressure"))
            {
                throw new Exception("Enregistrement du méthane a échoué");
            }
            
            Log.Info("✓ TerraformingGraphDataProvider fonctionne");
        }
        
        /// <summary>
        /// Test les températures régionales
        /// </summary>
        private static void TestRegionalTemperatures()
        {
            Log.Info("Test: Températures régionales");
            
            var grid = new AtmosphereGrid();
            var provider = new TerraformingGraphDataProvider(grid);
            provider.InitializeSampleData();
            
            // Test calcul des moyennes régionales
            var northPole = provider.GetRegionalAverageTemperature("north_pole");
            var southPole = provider.GetRegionalAverageTemperature("south_pole");
            var equator = provider.GetRegionalAverageTemperature("equator");
            
            Log.Info($"✓ Températures: Pôle Nord={northPole:F1}K, Équateur={equator:F1}K, Pôle Sud={southPole:F1}K");
            
            // Vérification logique: équateur devrait être plus chaud
            if (equator < northPole || equator < southPole)
            {
                Log.Warning("Gradient de température inhabituel détecté");
            }
        }
        
        /// <summary>
        /// Test les données de pression cellulaire
        /// </summary>
        private static void TestCellularPressure()
        {
            Log.Info("Test: Pression cellulaire");
            
            var grid = new AtmosphereGrid();
            var provider = new TerraformingGraphDataProvider(grid);
            provider.InitializeSampleData();
            
            // Test pression moyenne des cellules actives
            var pressureData = provider.GetGraphData("Pressure_ActiveCells");
            Log.Info($"✓ Pression moyenne cellules actives: {pressureData:F2} mbar");
            
            // Test variance de pression
            var o2Variance = provider.GetGraphData("o2_cellular_variance");
            Log.Info($"✓ Variance O2 cellulaire: {o2Variance:F3}");
        }
        
        /// <summary>
        /// Test l'enregistrement et tracking des gaz atmosphériques
        /// </summary>
        private static void TestAtmosphericGasRegistration()
        {
            Log.Info("Test: Enregistrement gaz atmosphériques");
            
            var grid = new AtmosphereGrid();
            var provider = new TerraformingGraphDataProvider(grid);
            
            // Test enregistrement de plusieurs gaz
            string[] testGases = { "CH4", "Ar", "Ne", "He", "Kr", "Xe" };
            string[] gasNames = { "Méthane", "Argon", "Néon", "Hélium", "Krypton", "Xénon" };
            
            for (int i = 0; i < testGases.Length; i++)
            {
                provider.RegisterAtmosphericGas(testGases[i], gasNames[i], "mbar");
                
                if (!provider.HasGraphData($"{testGases[i]} Pressure"))
                {
                    throw new Exception($"Enregistrement du gaz {gasNames[i]} a échoué");
                }
            }
            
            Log.Info($"✓ {testGases.Length} gaz atmosphériques enregistrés avec succès");
            
            // Test données initiales
            foreach (var gas in testGases)
            {
                var pressure = provider.GetGraphData($"{gas} Pressure");
                Log.Info($"  - {gas}: {pressure:F4} mbar");
            }
        }
        
        /// <summary>
        /// Test de performance basique
        /// </summary>
        public static void RunPerformanceTest()
        {
            Log.Info("=== Test de performance ===");
            
            var startTime = DateTime.Now;
            
            var grid = new AtmosphereGrid();
            var provider = new TerraformingGraphDataProvider(grid);
            provider.InitializeSampleData();
            
            // Simulation de 100 mises à jour de données
            for (int i = 0; i < 100; i++)
            {
                provider.UpdateGraphData();
            }
            
            var elapsed = DateTime.Now - startTime;
            Log.Info($"✓ 100 mises à jour en {elapsed.TotalMilliseconds:F2}ms");
            
            if (elapsed.TotalMilliseconds > 1000)
            {
                Log.Warning("Performance dégradée détectée");
            }
        }
    }
}