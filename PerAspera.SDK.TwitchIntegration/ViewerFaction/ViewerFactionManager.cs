// ViewerFactionManager.cs - Manages all viewer factions and their interactions
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration.ViewerFaction
{
    /// <summary>
    /// Manages all viewer factions, teams, and deals
    /// Central system for viewer faction interactions
    /// </summary>
    public class ViewerFactionManager
    {
        private static readonly LogAspera _logger = new LogAspera("ViewerFactionManager");
        
        private readonly Dictionary<string, ViewerFaction> _viewerFactions;
        private readonly Dictionary<string, ViewerTeam> _teams;
        private readonly List<ViewerDeal> _activeDeals;
        private readonly object _lock = new object();
        
        /// <summary>
        /// Maximum number of team members allowed
        /// </summary>
        public int MaxTeamSize { get; set; } = 5;
        
        /// <summary>
        /// Maximum number of active deals per viewer
        /// </summary>
        public int MaxDealsPerViewer { get; set; } = 3;
        
        /// <summary>
        /// Default deal expiration duration
        /// </summary>
        public TimeSpan DefaultDealDuration { get; set; } = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Starting resources for new viewer factions
        /// </summary>
        public Dictionary<string, float> StartingResources { get; set; }
        
        public ViewerFactionManager()
        {
            _viewerFactions = new Dictionary<string, ViewerFaction>(StringComparer.OrdinalIgnoreCase);
            _teams = new Dictionary<string, ViewerTeam>();
            _activeDeals = new List<ViewerDeal>();
            
            // Default starting resources
            StartingResources = new Dictionary<string, float>
            {
                { "resource_metal", 100f },
                { "resource_silicon", 100f },
                { "resource_water", 50f }
            };
        }
        
        // ==================== VIEWER FACTION MANAGEMENT ====================
        
        /// <summary>
        /// Get or create a viewer faction
        /// </summary>
        public ViewerFaction GetOrCreateViewer(string username, string? displayName = null)
        {
            lock (_lock)
            {
                if (_viewerFactions.TryGetValue(username, out var existing))
                {
                    existing.UpdateActivity();
                    return existing;
                }
                
                var viewer = new ViewerFaction(username, displayName);
                
                // Give starting resources
                foreach (var resource in StartingResources)
                {
                    viewer.AddResource(resource.Key, resource.Value);
                }
                
                _viewerFactions[username] = viewer;
                _logger.Info($"Created new viewer faction: {viewer}");
                
                return viewer;
            }
        }
        
        /// <summary>
        /// Get a viewer faction by username
        /// </summary>
        public ViewerFaction? GetViewer(string username)
        {
            lock (_lock)
            {
                return _viewerFactions.TryGetValue(username, out var viewer) ? viewer : null;
            }
        }
        
        /// <summary>
        /// Check if a viewer faction exists
        /// </summary>
        public bool HasViewer(string username)
        {
            lock (_lock)
            {
                return _viewerFactions.ContainsKey(username);
            }
        }
        
        /// <summary>
        /// Get all viewer factions
        /// </summary>
        public List<ViewerFaction> GetAllViewers()
        {
            lock (_lock)
            {
                return _viewerFactions.Values.ToList();
            }
        }
        
        /// <summary>
        /// Get active viewer factions (recently active)
        /// </summary>
        public List<ViewerFaction> GetActiveViewers(TimeSpan inactivityThreshold)
        {
            lock (_lock)
            {
                var cutoff = DateTime.UtcNow - inactivityThreshold;
                return _viewerFactions.Values
                    .Where(v => v.LastActivityAt > cutoff && v.IsActive)
                    .ToList();
            }
        }
        
        /// <summary>
        /// Remove a viewer faction
        /// </summary>
        public bool RemoveViewer(string username)
        {
            lock (_lock)
            {
                if (!_viewerFactions.TryGetValue(username, out var viewer))
                    return false;
                
                // Leave team if in one
                if (viewer.Team != null)
                {
                    LeaveTeam(viewer);
                }
                
                // Cancel all deals
                CancelAllDeals(viewer);
                
                _viewerFactions.Remove(username);
                _logger.Info($"Removed viewer faction: {username}");
                
                return true;
            }
        }
        
        // ==================== TEAM MANAGEMENT ====================
        
        /// <summary>
        /// Create a new team with a leader
        /// </summary>
        public ViewerTeam? CreateTeam(ViewerFaction leader, string? teamName = null)
        {
            lock (_lock)
            {
                if (leader.Team != null)
                {
                    _logger.Warning($"Viewer {leader.Username} is already in a team");
                    return null;
                }
                
                var team = new ViewerTeam(leader, teamName);
                _teams[team.Id] = team;
                
                _logger.Info($"Created new team: {team}");
                return team;
            }
        }
        
        /// <summary>
        /// Send a team invitation
        /// </summary>
        public ViewerInvitation? SendTeamInvitation(ViewerFaction from, ViewerFaction to)
        {
            lock (_lock)
            {
                if (from == to)
                {
                    _logger.Warning("Cannot send team invitation to self");
                    return null;
                }
                
                if (to.Team != null)
                {
                    _logger.Warning($"Viewer {to.Username} is already in a team");
                    return null;
                }
                
                // Create team if sender doesn't have one
                var team = from.Team ?? CreateTeam(from);
                if (team == null)
                    return null;
                
                // Check team size limit
                if (team.MemberCount >= MaxTeamSize)
                {
                    _logger.Warning($"Team {team.Name} is at maximum capacity");
                    return null;
                }
                
                var invitation = new ViewerInvitation(
                    from, to, InvitationType.Team,
                    $"Join team {team.Name}");
                invitation.Team = team;
                
                to.AddInvitation(invitation);
                _logger.Info($"Team invitation sent: {from.Username} → {to.Username}");
                
                return invitation;
            }
        }
        
        /// <summary>
        /// Accept a team invitation
        /// </summary>
        public bool AcceptTeamInvitation(ViewerFaction viewer, ViewerFaction fromViewer)
        {
            lock (_lock)
            {
                var invitation = viewer.GetInvitationFrom(fromViewer.Username);
                if (invitation == null || invitation.Type != InvitationType.Team)
                    return false;
                
                if (!invitation.IsPending)
                    return false;
                
                var team = invitation.Team;
                if (team == null)
                    return false;
                
                // Check team still has space
                if (team.MemberCount >= MaxTeamSize)
                    return false;
                
                invitation.Accept();
                viewer.RemoveInvitation(invitation);
                
                bool joined = team.AddMember(viewer);
                if (joined)
                {
                    _logger.Info($"Viewer {viewer.Username} joined team {team.Name}");
                }
                
                return joined;
            }
        }
        
        /// <summary>
        /// Leave current team
        /// </summary>
        public bool LeaveTeam(ViewerFaction viewer)
        {
            lock (_lock)
            {
                var team = viewer.Team;
                if (team == null)
                    return false;
                
                // If leader, disband the team
                if (team.Leader == viewer)
                {
                    DisbandTeam(team);
                    return true;
                }
                
                bool left = team.RemoveMember(viewer);
                if (left)
                {
                    _logger.Info($"Viewer {viewer.Username} left team {team.Name}");
                }
                
                return left;
            }
        }
        
        /// <summary>
        /// Disband a team
        /// </summary>
        private void DisbandTeam(ViewerTeam team)
        {
            foreach (var member in team.Members.ToList())
            {
                member.Team = null;
            }
            
            _teams.Remove(team.Id);
            _logger.Info($"Team disbanded: {team.Name}");
        }
        
        /// <summary>
        /// Get all teams
        /// </summary>
        public List<ViewerTeam> GetAllTeams()
        {
            lock (_lock)
            {
                return _teams.Values.ToList();
            }
        }
        
        // ==================== DEAL MANAGEMENT ====================
        
        /// <summary>
        /// Propose a deal between two viewers
        /// </summary>
        public ViewerDeal? ProposeDeal(ViewerFaction proposer, ViewerFaction receiver, string terms,
            string? offeredResource = null, float offeredAmount = 0,
            string? requestedResource = null, float requestedAmount = 0)
        {
            lock (_lock)
            {
                if (proposer == receiver)
                {
                    _logger.Warning("Cannot propose deal to self");
                    return null;
                }
                
                // Check max deals limit
                var proposerDeals = _activeDeals.Count(d => 
                    (d.Proposer == proposer || d.Receiver == proposer) && d.IsPending);
                    
                if (proposerDeals >= MaxDealsPerViewer)
                {
                    _logger.Warning($"Viewer {proposer.Username} has too many active deals");
                    return null;
                }
                
                var deal = new ViewerDeal(proposer, receiver, terms, DefaultDealDuration)
                {
                    OfferedResource = offeredResource,
                    OfferedAmount = offeredAmount,
                    RequestedResource = requestedResource,
                    RequestedAmount = requestedAmount
                };
                
                _activeDeals.Add(deal);
                proposer.ActiveDeals.Add(deal);
                receiver.ActiveDeals.Add(deal);
                
                // Create invitation for receiver
                var invitation = new ViewerInvitation(
                    proposer, receiver, InvitationType.Deal,
                    $"Deal: {terms}");
                invitation.Deal = deal;
                receiver.AddInvitation(invitation);
                
                _logger.Info($"Deal proposed: {deal}");
                return deal;
            }
        }
        
        /// <summary>
        /// Accept a deal
        /// </summary>
        public bool AcceptDeal(ViewerFaction viewer, ViewerFaction fromViewer)
        {
            lock (_lock)
            {
                var invitation = viewer.GetInvitationFrom(fromViewer.Username);
                if (invitation == null || invitation.Type != InvitationType.Deal)
                    return false;
                
                var deal = invitation.Deal;
                if (deal == null)
                    return false;
                
                bool accepted = deal.Accept();
                if (accepted)
                {
                    invitation.Accept();
                    viewer.RemoveInvitation(invitation);
                    _logger.Info($"Deal accepted: {deal}");
                }
                
                return accepted;
            }
        }
        
        /// <summary>
        /// Reject a deal
        /// </summary>
        public bool RejectDeal(ViewerFaction viewer, ViewerFaction fromViewer)
        {
            lock (_lock)
            {
                var invitation = viewer.GetInvitationFrom(fromViewer.Username);
                if (invitation == null || invitation.Type != InvitationType.Deal)
                    return false;
                
                var deal = invitation.Deal;
                if (deal == null)
                    return false;
                
                deal.Reject();
                invitation.Decline();
                viewer.RemoveInvitation(invitation);
                
                _logger.Info($"Deal rejected: {deal}");
                return true;
            }
        }
        
        /// <summary>
        /// Cancel all deals for a viewer
        /// </summary>
        private void CancelAllDeals(ViewerFaction viewer)
        {
            var dealsToCancel = _activeDeals
                .Where(d => (d.Proposer == viewer || d.Receiver == viewer) && d.IsPending)
                .ToList();
            
            foreach (var deal in dealsToCancel)
            {
                deal.Cancel();
                deal.Proposer.ActiveDeals.Remove(deal);
                deal.Receiver.ActiveDeals.Remove(deal);
            }
        }
        
        /// <summary>
        /// Clean up expired deals and invitations
        /// </summary>
        public void CleanupExpired()
        {
            lock (_lock)
            {
                // Remove expired deals
                var expiredDeals = _activeDeals.Where(d => d.IsExpired).ToList();
                foreach (var deal in expiredDeals)
                {
                    deal.Status = DealStatus.Expired;
                    _activeDeals.Remove(deal);
                    deal.Proposer.ActiveDeals.Remove(deal);
                    deal.Receiver.ActiveDeals.Remove(deal);
                }
                
                // Remove expired invitations
                foreach (var viewer in _viewerFactions.Values)
                {
                    var expiredInvitations = viewer.PendingInvitations
                        .Where(inv => inv.IsExpired)
                        .ToList();
                        
                    foreach (var inv in expiredInvitations)
                    {
                        inv.Status = InvitationStatus.Expired;
                        viewer.RemoveInvitation(inv);
                    }
                }
                
                if (expiredDeals.Count > 0)
                {
                    _logger.Debug($"Cleaned up {expiredDeals.Count} expired deals");
                }
            }
        }
        
        /// <summary>
        /// Get all active deals
        /// </summary>
        public List<ViewerDeal> GetActiveDeals()
        {
            lock (_lock)
            {
                return _activeDeals.Where(d => d.IsPending).ToList();
            }
        }
        
        // ==================== STATISTICS ====================
        
        /// <summary>
        /// Get total number of viewer factions
        /// </summary>
        public int TotalViewers => _viewerFactions.Count;
        
        /// <summary>
        /// Get total number of teams
        /// </summary>
        public int TotalTeams => _teams.Count;
        
        /// <summary>
        /// Get total number of active deals
        /// </summary>
        public int TotalActiveDeals => _activeDeals.Count(d => d.IsPending);
        
        /// <summary>
        /// Get leaderboard (top viewers by points)
        /// </summary>
        public List<ViewerFaction> GetLeaderboard(int count = 10)
        {
            lock (_lock)
            {
                return _viewerFactions.Values
                    .OrderByDescending(v => v.Points)
                    .Take(count)
                    .ToList();
            }
        }
    }
}
