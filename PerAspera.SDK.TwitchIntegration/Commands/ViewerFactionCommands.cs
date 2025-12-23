// ViewerFactionCommands.cs - Twitch chat commands for viewer faction system
using System;
using System.Linq;
using System.Text;
using PerAspera.Core;
using PerAspera.SDK.TwitchIntegration.ViewerFaction;

namespace PerAspera.SDK.TwitchIntegration.Commands
{
    /// <summary>
    /// Handles Twitch chat commands for the viewer faction system
    /// </summary>
    public class ViewerFactionCommands
    {
        private static readonly LogAspera _logger = new LogAspera("ViewerFactionCommands");
        
        private readonly ViewerFactionManager _factionManager;
        private readonly Action<string, string> _sendMessage;
        
        /// <summary>
        /// Command prefix (default: !)
        /// </summary>
        public string CommandPrefix { get; set; } = "!";
        
        public ViewerFactionCommands(ViewerFactionManager factionManager, Action<string, string> sendMessage)
        {
            _factionManager = factionManager ?? throw new ArgumentNullException(nameof(factionManager));
            _sendMessage = sendMessage ?? throw new ArgumentNullException(nameof(sendMessage));
        }
        
        /// <summary>
        /// Process a chat message and execute commands
        /// </summary>
        public void ProcessMessage(string username, string displayName, string message)
        {
            if (string.IsNullOrWhiteSpace(message) || !message.StartsWith(CommandPrefix))
                return;
            
            var parts = message.Substring(CommandPrefix.Length).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return;
            
            var command = parts[0].ToLowerInvariant();
            var args = parts.Skip(1).ToArray();
            
            try
            {
                switch (command)
                {
                    case "join":
                        HandleJoin(username, displayName);
                        break;
                    
                    case "team":
                        HandleTeam(username, displayName, args);
                        break;
                    
                    case "deal":
                        HandleDeal(username, displayName, args);
                        break;
                    
                    case "accept":
                        HandleAccept(username, displayName, args);
                        break;
                    
                    case "decline":
                    case "reject":
                        HandleDecline(username, displayName, args);
                        break;
                    
                    case "status":
                        HandleStatus(username, displayName);
                        break;
                    
                    case "alliances":
                    case "teams":
                        HandleAlliances(username);
                        break;
                    
                    case "factions":
                    case "viewers":
                        HandleFactions(username);
                        break;
                    
                    case "leaderboard":
                    case "top":
                        HandleLeaderboard(username);
                        break;
                    
                    case "leave":
                        HandleLeave(username, displayName);
                        break;
                    
                    case "help":
                        HandleHelp(username);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing command '{command}' from {username}: {ex.Message}");
                Reply(username, $"Error executing command: {ex.Message}");
            }
        }
        
        // ==================== COMMAND HANDLERS ====================
        
        private void HandleJoin(string username, string displayName)
        {
            var viewer = _factionManager.GetOrCreateViewer(username, displayName);
            
            if (viewer.CreatedAt < DateTime.UtcNow.AddSeconds(-5))
            {
                Reply(username, $"Welcome back, {displayName}! Your faction has been reactivated.");
            }
            else
            {
                Reply(username, $"Welcome, {displayName}! You've joined as a faction leader. Use !help for commands.");
            }
            
            _logger.Info($"Viewer {username} joined as faction");
        }
        
        private void HandleTeam(string username, string displayName, string[] args)
        {
            if (args.Length == 0)
            {
                Reply(username, "Usage: !team <username> - Invite a viewer to your team");
                return;
            }
            
            var viewer = _factionManager.GetOrCreateViewer(username, displayName);
            var targetUsername = args[0].TrimStart('@');
            
            if (targetUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                Reply(username, "You cannot team up with yourself!");
                return;
            }
            
            var targetViewer = _factionManager.GetViewer(targetUsername);
            if (targetViewer == null)
            {
                Reply(username, $"Viewer {targetUsername} hasn't joined yet. They need to use !join first.");
                return;
            }
            
            var invitation = _factionManager.SendTeamInvitation(viewer, targetViewer);
            if (invitation != null)
            {
                Reply(username, $"Team invitation sent to {targetViewer.DisplayName}!");
                Reply(targetUsername, $"{displayName} invited you to join their team! Use !accept {username} to join.");
            }
            else
            {
                Reply(username, $"Could not send team invitation to {targetViewer.DisplayName}. They might already be in a team.");
            }
        }
        
        private void HandleDeal(string username, string displayName, string[] args)
        {
            if (args.Length < 2)
            {
                Reply(username, "Usage: !deal <username> <terms> - Propose a deal to another viewer");
                return;
            }
            
            var viewer = _factionManager.GetOrCreateViewer(username, displayName);
            var targetUsername = args[0].TrimStart('@');
            var terms = string.Join(" ", args.Skip(1));
            
            if (targetUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                Reply(username, "You cannot make a deal with yourself!");
                return;
            }
            
            var targetViewer = _factionManager.GetViewer(targetUsername);
            if (targetViewer == null)
            {
                Reply(username, $"Viewer {targetUsername} hasn't joined yet. They need to use !join first.");
                return;
            }
            
            var deal = _factionManager.ProposeDeal(viewer, targetViewer, terms);
            if (deal != null)
            {
                Reply(username, $"Deal proposed to {targetViewer.DisplayName}: {terms}");
                Reply(targetUsername, $"{displayName} proposed a deal: {terms}. Use !accept {username} to accept or !decline {username} to reject.");
            }
            else
            {
                Reply(username, $"Could not propose deal. You might have too many active deals.");
            }
        }
        
        private void HandleAccept(string username, string displayName, string[] args)
        {
            if (args.Length == 0)
            {
                Reply(username, "Usage: !accept <username> - Accept a team invitation or deal");
                return;
            }
            
            var viewer = _factionManager.GetOrCreateViewer(username, displayName);
            var fromUsername = args[0].TrimStart('@');
            
            var fromViewer = _factionManager.GetViewer(fromUsername);
            if (fromViewer == null)
            {
                Reply(username, $"Viewer {fromUsername} not found.");
                return;
            }
            
            var invitation = viewer.GetInvitationFrom(fromUsername);
            if (invitation == null || !invitation.IsPending)
            {
                Reply(username, $"No pending invitation from {fromViewer.DisplayName}.");
                return;
            }
            
            if (invitation.Type == InvitationType.Team)
            {
                bool accepted = _factionManager.AcceptTeamInvitation(viewer, fromViewer);
                if (accepted)
                {
                    Reply(username, $"You joined {fromViewer.DisplayName}'s team!");
                    Reply(fromUsername, $"{displayName} accepted your team invitation!");
                }
                else
                {
                    Reply(username, "Could not join team. It might be full.");
                }
            }
            else if (invitation.Type == InvitationType.Deal)
            {
                bool accepted = _factionManager.AcceptDeal(viewer, fromViewer);
                if (accepted)
                {
                    Reply(username, $"Deal with {fromViewer.DisplayName} accepted!");
                    Reply(fromUsername, $"{displayName} accepted your deal!");
                }
                else
                {
                    Reply(username, "Could not accept deal. Check resource requirements.");
                }
            }
        }
        
        private void HandleDecline(string username, string displayName, string[] args)
        {
            if (args.Length == 0)
            {
                Reply(username, "Usage: !decline <username> - Decline a team invitation or deal");
                return;
            }
            
            var viewer = _factionManager.GetOrCreateViewer(username, displayName);
            var fromUsername = args[0].TrimStart('@');
            
            var fromViewer = _factionManager.GetViewer(fromUsername);
            if (fromViewer == null)
            {
                Reply(username, $"Viewer {fromUsername} not found.");
                return;
            }
            
            var invitation = viewer.GetInvitationFrom(fromUsername);
            if (invitation == null || !invitation.IsPending)
            {
                Reply(username, $"No pending invitation from {fromViewer.DisplayName}.");
                return;
            }
            
            if (invitation.Type == InvitationType.Team)
            {
                invitation.Decline();
                viewer.RemoveInvitation(invitation);
                Reply(username, $"You declined {fromViewer.DisplayName}'s team invitation.");
                Reply(fromUsername, $"{displayName} declined your team invitation.");
            }
            else if (invitation.Type == InvitationType.Deal)
            {
                _factionManager.RejectDeal(viewer, fromViewer);
                Reply(username, $"You rejected {fromViewer.DisplayName}'s deal.");
                Reply(fromUsername, $"{displayName} rejected your deal.");
            }
        }
        
        private void HandleStatus(string username, string displayName)
        {
            var viewer = _factionManager.GetViewer(username);
            if (viewer == null)
            {
                Reply(username, "You haven't joined yet! Use !join to create your faction.");
                return;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"🏛️ {viewer.DisplayName}'s Faction Status:");
            sb.AppendLine($"  Points: {viewer.Points}");
            
            if (viewer.IsInTeam)
            {
                sb.AppendLine($"  Team: {viewer.Team!.Name} ({viewer.Team.MemberCount} members)");
            }
            else
            {
                sb.AppendLine($"  Team: None");
            }
            
            sb.AppendLine($"  Active Deals: {viewer.ActiveDeals.Count(d => d.IsPending)}");
            sb.AppendLine($"  Pending Invitations: {viewer.PendingInvitations.Count(i => i.IsPending)}");
            
            Reply(username, sb.ToString());
        }
        
        private void HandleAlliances(string username)
        {
            var teams = _factionManager.GetAllTeams();
            
            if (teams.Count == 0)
            {
                Reply(username, "No teams formed yet. Use !team <username> to create one!");
                return;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"🤝 Active Teams ({teams.Count}):");
            
            foreach (var team in teams.Take(5))
            {
                var members = string.Join(", ", team.Members.Select(m => m.DisplayName));
                sb.AppendLine($"  {team.Name}: {members} (Points: {team.CalculateTotalPoints()})");
            }
            
            if (teams.Count > 5)
            {
                sb.AppendLine($"  ... and {teams.Count - 5} more teams");
            }
            
            Reply(username, sb.ToString());
        }
        
        private void HandleFactions(string username)
        {
            var viewers = _factionManager.GetActiveViewers(TimeSpan.FromMinutes(30));
            
            if (viewers.Count == 0)
            {
                Reply(username, "No active factions yet. Use !join to be the first!");
                return;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"🏛️ Active Factions ({viewers.Count}):");
            
            foreach (var viewer in viewers.Take(10))
            {
                var teamInfo = viewer.IsInTeam ? $" [Team: {viewer.Team!.Name}]" : "";
                sb.AppendLine($"  {viewer.DisplayName} - {viewer.Points} pts{teamInfo}");
            }
            
            if (viewers.Count > 10)
            {
                sb.AppendLine($"  ... and {viewers.Count - 10} more factions");
            }
            
            Reply(username, sb.ToString());
        }
        
        private void HandleLeaderboard(string username)
        {
            var topViewers = _factionManager.GetLeaderboard(10);
            
            if (topViewers.Count == 0)
            {
                Reply(username, "No factions yet. Use !join to be the first!");
                return;
            }
            
            var sb = new StringBuilder();
            sb.AppendLine("🏆 Faction Leaderboard:");
            
            for (int i = 0; i < topViewers.Count; i++)
            {
                var viewer = topViewers[i];
                var medal = i switch { 0 => "🥇", 1 => "🥈", 2 => "🥉", _ => $"{i + 1}." };
                var teamInfo = viewer.IsInTeam ? $" [{viewer.Team!.Name}]" : "";
                sb.AppendLine($"  {medal} {viewer.DisplayName}: {viewer.Points} pts{teamInfo}");
            }
            
            Reply(username, sb.ToString());
        }
        
        private void HandleLeave(string username, string displayName)
        {
            var viewer = _factionManager.GetViewer(username);
            if (viewer == null)
            {
                Reply(username, "You haven't joined yet!");
                return;
            }
            
            if (!viewer.IsInTeam)
            {
                Reply(username, "You're not in a team.");
                return;
            }
            
            var teamName = viewer.Team!.Name;
            bool left = _factionManager.LeaveTeam(viewer);
            
            if (left)
            {
                Reply(username, $"You left team {teamName}.");
            }
            else
            {
                Reply(username, "Could not leave team.");
            }
        }
        
        private void HandleHelp(string username)
        {
            var helpText = @"
🎮 Viewer Faction Commands:
  !join - Join as a faction leader
  !team <username> - Invite someone to your team
  !deal <username> <terms> - Propose a deal
  !accept <username> - Accept invitation/deal
  !decline <username> - Decline invitation/deal
  !status - Show your faction status
  !alliances - List all teams
  !factions - List active factions
  !leaderboard - Show top factions
  !leave - Leave your current team
  !help - Show this help
";
            Reply(username, helpText);
        }
        
        // ==================== UTILITY ====================
        
        private void Reply(string username, string message)
        {
            try
            {
                _sendMessage(username, message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending message to {username}: {ex.Message}");
            }
        }
    }
}
