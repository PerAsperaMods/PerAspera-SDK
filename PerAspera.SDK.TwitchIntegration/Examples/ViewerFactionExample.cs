// Example.cs - Demonstrates how to use the Twitch Faction Viewer system
using System;
using System.Threading.Tasks;
using PerAspera.SDK.TwitchIntegration;
using PerAspera.SDK.TwitchIntegration.ViewerFaction;
using PerAspera.SDK.TwitchIntegration.Vendor.UnityTwitchChat;
using PerAspera.SDK.TwitchIntegration.Commands;

namespace PerAspera.SDK.TwitchIntegration.Examples
{
    /// <summary>
    /// Example usage of the Twitch Faction Viewer system
    /// </summary>
    public class ViewerFactionExample
    {
        /// <summary>
        /// Example 1: Basic setup with Twitch connection
        /// </summary>
        public static async Task ConnectedModeExample()
        {
            Console.WriteLine("=== Connected Mode Example ===\n");
            
            // Configure Twitch connection
            var config = new TwitchConnectionConfig
            {
                OAuth = "oauth:your_oauth_token_here",
                Username = "your_bot_username",
                Channel = "your_channel_name"
            };
            
            // Create and start the service
            using var service = new ViewerFactionIntegrationService(config);
            
            Console.WriteLine("Starting Twitch Faction Viewer service...");
            bool started = await service.StartAsync();
            
            if (started)
            {
                Console.WriteLine("Service started successfully!");
                Console.WriteLine($"Status: {service.GetStatistics()}");
                
                // Keep running
                Console.WriteLine("\nPress any key to stop...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Failed to start service");
            }
            
            service.Stop();
            Console.WriteLine("Service stopped");
        }
        
        /// <summary>
        /// Example 2: Offline mode for testing without Twitch
        /// </summary>
        public static void OfflineModeExample()
        {
            Console.WriteLine("=== Offline Mode Example ===\n");
            
            // Create service without Twitch connection (offline mode)
            using var service = new ViewerFactionIntegrationService(null);
            var manager = service.FactionManager;
            
            // Simulate viewer interactions
            Console.WriteLine("Creating test viewers...");
            var alice = manager.GetOrCreateViewer("alice", "Alice");
            var bob = manager.GetOrCreateViewer("bob", "Bob");
            var charlie = manager.GetOrCreateViewer("charlie", "Charlie");
            
            Console.WriteLine($"Created {manager.TotalViewers} viewers");
            
            // Test team formation
            Console.WriteLine("\nTesting team formation...");
            var invitation = manager.SendTeamInvitation(alice, bob);
            Console.WriteLine($"Alice invited Bob to team: {invitation != null}");
            
            bool joined = manager.AcceptTeamInvitation(bob, alice);
            Console.WriteLine($"Bob joined Alice's team: {joined}");
            
            // Check team
            var teams = manager.GetAllTeams();
            Console.WriteLine($"\nActive teams: {teams.Count}");
            foreach (var team in teams)
            {
                Console.WriteLine($"  {team}");
            }
            
            // Test deals
            Console.WriteLine("\nTesting deals...");
            var deal = manager.ProposeDeal(
                charlie, alice,
                "Trade 50 metal for 25 water",
                "resource_metal", 50f,
                "resource_water", 25f
            );
            Console.WriteLine($"Charlie proposed deal to Alice: {deal != null}");
            
            if (deal != null)
            {
                Console.WriteLine($"  Deal: {deal}");
            }
            
            // Show statistics
            Console.WriteLine($"\n{service.GetStatistics()}");
        }
        
        /// <summary>
        /// Example 3: Command processing simulation
        /// </summary>
        public static void CommandSimulationExample()
        {
            Console.WriteLine("=== Command Simulation Example ===\n");
            
            var manager = new ViewerFactionManager();
            
            // Create command handler with console output
            var commands = new ViewerFactionCommands(
                manager,
                (username, message) => Console.WriteLine($"[@{username}] {message}")
            );
            
            // Simulate chat commands
            Console.WriteLine("Simulating Twitch chat commands:\n");
            
            Console.WriteLine("[alice] !join");
            commands.ProcessMessage("alice", "Alice", "!join");
            Console.WriteLine();
            
            Console.WriteLine("[bob] !join");
            commands.ProcessMessage("bob", "Bob", "!join");
            Console.WriteLine();
            
            Console.WriteLine("[alice] !team bob");
            commands.ProcessMessage("alice", "Alice", "!team bob");
            Console.WriteLine();
            
            Console.WriteLine("[bob] !accept alice");
            commands.ProcessMessage("bob", "Bob", "!accept alice");
            Console.WriteLine();
            
            Console.WriteLine("[alice] !status");
            commands.ProcessMessage("alice", "Alice", "!status");
            Console.WriteLine();
            
            Console.WriteLine("[charlie] !join");
            commands.ProcessMessage("charlie", "Charlie", "!join");
            Console.WriteLine();
            
            Console.WriteLine("[charlie] !deal alice 100 metal for 50 water");
            commands.ProcessMessage("charlie", "Charlie", "!deal alice 100 metal for 50 water");
            Console.WriteLine();
            
            Console.WriteLine("[alice] !status");
            commands.ProcessMessage("alice", "Alice", "!status");
            Console.WriteLine();
            
            Console.WriteLine("[bob] !factions");
            commands.ProcessMessage("bob", "Bob", "!factions");
            Console.WriteLine();
            
            Console.WriteLine("[alice] !alliances");
            commands.ProcessMessage("alice", "Alice", "!alliances");
            Console.WriteLine();
        }
        
        /// <summary>
        /// Example 4: Advanced faction management
        /// </summary>
        public static void AdvancedManagementExample()
        {
            Console.WriteLine("=== Advanced Management Example ===\n");
            
            var manager = new ViewerFactionManager();
            
            // Customize settings
            Console.WriteLine("Configuring manager...");
            manager.MaxTeamSize = 10;
            manager.MaxDealsPerViewer = 5;
            manager.DefaultDealDuration = TimeSpan.FromMinutes(10);
            
            // Custom starting resources
            manager.StartingResources = new System.Collections.Generic.Dictionary<string, float>
            {
                { "resource_metal", 200f },
                { "resource_silicon", 150f },
                { "resource_water", 100f },
                { "resource_food", 50f }
            };
            
            Console.WriteLine($"Max team size: {manager.MaxTeamSize}");
            Console.WriteLine($"Max deals per viewer: {manager.MaxDealsPerViewer}");
            Console.WriteLine($"Deal duration: {manager.DefaultDealDuration.TotalMinutes} minutes");
            
            // Create multiple factions
            Console.WriteLine("\nCreating 5 factions...");
            for (int i = 1; i <= 5; i++)
            {
                var viewer = manager.GetOrCreateViewer($"player{i}", $"Player {i}");
                viewer.Points = i * 100; // Assign some points
                Console.WriteLine($"  Created: {viewer}");
            }
            
            // Form teams
            Console.WriteLine("\nForming teams...");
            var p1 = manager.GetViewer("player1")!;
            var p2 = manager.GetViewer("player2")!;
            var p3 = manager.GetViewer("player3")!;
            
            manager.SendTeamInvitation(p1, p2);
            manager.AcceptTeamInvitation(p2, p1);
            Console.WriteLine($"  Team 1: Player1 + Player2");
            
            manager.SendTeamInvitation(p1, p3);
            manager.AcceptTeamInvitation(p3, p1);
            Console.WriteLine($"  Team 1: Player1 + Player2 + Player3");
            
            // Show leaderboard
            Console.WriteLine("\nLeaderboard:");
            var leaderboard = manager.GetLeaderboard(5);
            for (int i = 0; i < leaderboard.Count; i++)
            {
                var viewer = leaderboard[i];
                var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"{i + 1}." };
                Console.WriteLine($"  {medal} {viewer}");
            }
            
            // Statistics
            Console.WriteLine($"\nTotal Viewers: {manager.TotalViewers}");
            Console.WriteLine($"Total Teams: {manager.TotalTeams}");
            Console.WriteLine($"Active Deals: {manager.TotalActiveDeals}");
        }
        
        /// <summary>
        /// Main entry point - runs all examples
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Twitch Faction Viewer System - Examples\n");
            Console.WriteLine("========================================\n");
            
            try
            {
                // Example 2: Offline mode (no Twitch connection needed)
                OfflineModeExample();
                Console.WriteLine("\n");
                
                // Example 3: Command simulation
                CommandSimulationExample();
                Console.WriteLine("\n");
                
                // Example 4: Advanced management
                AdvancedManagementExample();
                Console.WriteLine("\n");
                
                // Example 1: Connected mode (commented out - requires Twitch credentials)
                // await ConnectedModeExample();
                
                Console.WriteLine("\nAll examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
