// ViewerFaction.cs - Represents a Twitch viewer as a faction
using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.SDK.TwitchIntegration.ViewerFaction
{
    /// <summary>
    /// Represents a Twitch viewer as a faction in the game
    /// Each viewer can control their own faction, team up with others, and make deals
    /// </summary>
    public class ViewerFaction
    {
        /// <summary>
        /// Twitch username of the viewer
        /// </summary>
        public string Username { get; }
        
        /// <summary>
        /// Display name of the viewer
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Unique identifier for this viewer faction
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Associated game faction wrapper (if connected to game)
        /// </summary>
        public Faction? GameFaction { get; set; }
        
        /// <summary>
        /// Team this viewer belongs to (null if not in a team)
        /// </summary>
        public ViewerTeam? Team { get; set; }
        
        /// <summary>
        /// Resources owned by this viewer faction
        /// </summary>
        public Dictionary<string, float> Resources { get; }
        
        /// <summary>
        /// Active deals this viewer is involved in
        /// </summary>
        public List<ViewerDeal> ActiveDeals { get; }
        
        /// <summary>
        /// Pending invitations (team or deal)
        /// </summary>
        public List<ViewerInvitation> PendingInvitations { get; }
        
        /// <summary>
        /// When this viewer faction was created
        /// </summary>
        public DateTime CreatedAt { get; }
        
        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivityAt { get; set; }
        
        /// <summary>
        /// Whether this viewer faction is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Points or score for this viewer
        /// </summary>
        public int Points { get; set; }
        
        public ViewerFaction(string username, string? displayName = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
                
            Username = username.ToLowerInvariant();
            DisplayName = displayName ?? username;
            Id = Guid.NewGuid().ToString();
            Resources = new Dictionary<string, float>();
            ActiveDeals = new List<ViewerDeal>();
            PendingInvitations = new List<ViewerInvitation>();
            CreatedAt = DateTime.UtcNow;
            LastActivityAt = DateTime.UtcNow;
            IsActive = true;
            Points = 0;
        }
        
        /// <summary>
        /// Update last activity timestamp
        /// </summary>
        public void UpdateActivity()
        {
            LastActivityAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Add resources to this faction
        /// </summary>
        public void AddResource(string resourceKey, float amount)
        {
            if (!Resources.ContainsKey(resourceKey))
                Resources[resourceKey] = 0f;
            Resources[resourceKey] += amount;
        }
        
        /// <summary>
        /// Remove resources from this faction
        /// </summary>
        public bool RemoveResource(string resourceKey, float amount)
        {
            if (!Resources.ContainsKey(resourceKey))
                return false;
                
            if (Resources[resourceKey] < amount)
                return false;
                
            Resources[resourceKey] -= amount;
            return true;
        }
        
        /// <summary>
        /// Get resource amount
        /// </summary>
        public float GetResource(string resourceKey)
        {
            return Resources.ContainsKey(resourceKey) ? Resources[resourceKey] : 0f;
        }
        
        /// <summary>
        /// Add a pending invitation
        /// </summary>
        public void AddInvitation(ViewerInvitation invitation)
        {
            PendingInvitations.Add(invitation);
        }
        
        /// <summary>
        /// Remove a pending invitation
        /// </summary>
        public bool RemoveInvitation(ViewerInvitation invitation)
        {
            return PendingInvitations.Remove(invitation);
        }
        
        /// <summary>
        /// Get invitation from a specific viewer
        /// </summary>
        public ViewerInvitation? GetInvitationFrom(string fromUsername)
        {
            return PendingInvitations.Find(inv => 
                inv.FromViewer.Username.Equals(fromUsername, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Check if viewer is in a team
        /// </summary>
        public bool IsInTeam => Team != null;
        
        /// <summary>
        /// Get team members (excluding self)
        /// </summary>
        public List<ViewerFaction> GetTeamMembers()
        {
            if (Team == null) return new List<ViewerFaction>();
            return Team.GetOtherMembers(this);
        }
        
        public override string ToString()
        {
            return $"{DisplayName} ({Username}) - Points: {Points}, Active: {IsActive}, Team: {(IsInTeam ? Team!.Name : "None")}";
        }
    }
}
