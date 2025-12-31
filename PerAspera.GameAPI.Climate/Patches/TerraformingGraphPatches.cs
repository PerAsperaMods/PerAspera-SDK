using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Integration;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate.Patches
{
    /// <summary>
    /// Patches Harmony pour intégrer les données cellulaires atmosphériques 
    /// avec le système de graphiques de terraformation de Per Aspera
    /// </summary>
    public static class TerraformingGraphPatches
    {
        private static readonly LogAspera Log = new LogAspera("Climate.TerraformingGraphs");
        private static readonly Dictionary<object, ClimateController> _activeControllers = new();
        
        /// <summary>
        /// Enregistre un ClimateController pour une planète spécifique
        /// Permet aux patches d'accéder aux données cellulaires
        /// </summary>
        public static void RegisterClimateController(object nativePlanet, ClimateController controller)
        {
            _activeControllers[nativePlanet] = controller;
            Log.Info("ClimateController registered for terraforming graph integration");
        }
        
        /// <summary>
        /// Désenregistre un ClimateController
        /// </summary>
        public static void UnregisterClimateController(object nativePlanet)
        {
            if (_activeControllers.Remove(nativePlanet))
            {
                Log.Info("ClimateController unregistered from terraforming graphs");
            }
        }
        
        /// <summary>
        /// Obtient les données d'un graphique de terraformation depuis le système cellulaire
        /// Cette méthode sera appelée par les patches du système de terraformation
        /// </summary>
        public static float GetTerraformingGraphValue(object nativePlanet, string dataKey)
        {
            try
            {
                if (_activeControllers.TryGetValue(nativePlanet, out var controller))
                {
                    // Données spécifiques aux graphiques cellulaires
                    return dataKey switch
                    {
                        // Températures régionales
                        "Temperature_NorthPole" => controller.GetNorthPoleTemperature(),
                        "Temperature_SouthPole" => controller.GetSouthPoleTemperature(),
                        "Temperature_Equator" => controller.GetEquatorTemperature(),
                        
                        // Données cellulaires
                        "ActiveCellsCount" => controller.GetActiveCellsCount(),
                        "Pressure_ActiveCells" => controller.GetTerraformingGraphData("Pressure_ActiveCells"),
                        
                        // Gaz atmosphériques étendus (MoreResources)
                        "CH4 Pressure" => controller.GetTerraformingGraphData("CH4 Pressure"),
                        "Ar Pressure" => controller.GetTerraformingGraphData("Ar Pressure"),
                        "Ne Pressure" => controller.GetTerraformingGraphData("Ne Pressure"),
                        "He Pressure" => controller.GetTerraformingGraphData("He Pressure"),
                        "Kr Pressure" => controller.GetTerraformingGraphData("Kr Pressure"),
                        "Xe Pressure" => controller.GetTerraformingGraphData("Xe Pressure"),
                        
                        // Distribution et variance cellulaire
                        "o2_cellular_variance" => controller.GetTerraformingGraphData("o2_cellular_variance"),
                        "co2_cellular_hotspots" => controller.GetTerraformingGraphData("co2_cellular_hotspots"),
                        
                        // Fallback pour autres données
                        _ => controller.GetTerraformingGraphData(dataKey)
                    };
                }
                
                Log.Debug($"No climate controller found for terraforming graph data: {dataKey}");
                return 0f;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get terraforming graph value for {dataKey}: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Vérifie si des données cellulaires sont disponibles pour une clé
        /// </summary>
        public static bool HasTerraformingGraphData(object nativePlanet, string dataKey)
        {
            if (_activeControllers.TryGetValue(nativePlanet, out var controller))
            {
                return controller.HasTerraformingGraphData(dataKey) ||
                       IsBuiltInCellularData(dataKey);
            }
            return false;
        }
        
        /// <summary>
        /// Vérifie si une clé de données fait partie des données cellulaires intégrées
        /// </summary>
        private static bool IsBuiltInCellularData(string dataKey)
        {
            return dataKey switch
            {
                "Temperature_NorthPole" or "Temperature_SouthPole" or "Temperature_Equator" or
                "ActiveCellsCount" or "Pressure_ActiveCells" or
                "CH4 Pressure" or "Ar Pressure" or "Ne Pressure" or
                "He Pressure" or "Kr Pressure" or "Xe Pressure" or
                "o2_cellular_variance" or "co2_cellular_hotspots" => true,
                _ => false
            };
        }
        
        /// <summary>
        /// Met à jour toutes les données de graphiques pour toutes les planètes contrôlées
        /// Appelé depuis un timer global ou depuis le système de terraformation
        /// </summary>
        public static void UpdateAllTerraformingGraphData()
        {
            try
            {
                foreach (var kvp in _activeControllers)
                {
                    var controller = kvp.Value;
                    if (controller.GraphDataProvider != null)
                    {
                        controller.GraphDataProvider.UpdateGraphData();
                    }
                }
                
                Log.Debug($"Updated terraforming graph data for {_activeControllers.Count} planets");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update terraforming graph data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Obtient des statistiques sur l'état des données de graphiques
        /// Utile pour le debugging et monitoring
        /// </summary>
        public static Dictionary<string, object> GetTerraformingGraphStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["ControlledPlanets"] = _activeControllers.Count,
                ["TotalActiveCells"] = 0,
                ["GraphDataKeys"] = new List<string>()
            };
            
            foreach (var controller in _activeControllers.Values)
            {
                stats["TotalActiveCells"] = (int)stats["TotalActiveCells"] + controller.GetActiveCellsCount();
            }
            
            return stats;
        }
        
        /// <summary>
        /// Méthode d'aide pour les mods qui veulent enregistrer de nouveaux gaz
        /// </summary>
        public static void RegisterAtmosphericGasForAllPlanets(string gasSymbol, string displayName, string unit = "mbar")
        {
            foreach (var controller in _activeControllers.Values)
            {
                controller.RegisterAtmosphericGas(gasSymbol, displayName, unit);
            }
            
            Log.Info($"Registered atmospheric gas '{displayName}' ({gasSymbol}) for all controlled planets");
        }
    }
}