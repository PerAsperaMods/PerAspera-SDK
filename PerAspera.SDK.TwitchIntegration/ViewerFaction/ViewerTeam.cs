// ViewerTeam.cs - Represents a team/alliance of viewer factions
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.SDK.TwitchIntegration.ViewerFaction
{
    /// <summary>
    /// Represents a team or alliance of viewer factions
    /// Team members can share resources and coordinate actions
    /// </summary>
    public class ViewerTeam
    {
        /// <summary>
        /// Unique identifier for this team
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Team name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Members of this team
        /// </summary>
        public List<ViewerFaction> Members { get; }
        
        /// <summary>
        /// The viewer who created/leads this team
        /// </summary>
        public ViewerFaction Leader { get; }
        
        /// <summary>
        /// When this team was created
        /// </summary>
        public DateTime CreatedAt { get; }
        
        /// <summary>
        /// Shared team resources
        /// </summary>
        public Dictionary<string, float> SharedResources { get; }
        
        /// <summary>
        /// Team points/score
        /// </summary>
        public int TeamPoints { get; set; }
        
        public ViewerTeam(ViewerFaction leader, string? name = null)
        {
            if (leader == null)
                throw new ArgumentNullException(nameof(leader));
                
            Id = Guid.NewGuid().ToString();
            Leader = leader;
            Name = name ?? $"Team {leader.DisplayName}";
            Members = new List<ViewerFaction> { leader };
            CreatedAt = DateTime.UtcNow;
            SharedResources = new Dictionary<string, float>();
            TeamPoints = 0;
            
            leader.Team = this;
        }
        
        /// <summary>
        /// Add a member to the team
        /// </summary>
        public bool AddMember(ViewerFaction viewer)
        {
            if (viewer == null)
                return false;
                
            if (Members.Contains(viewer))
                return false;
                
            if (viewer.Team != null)
                return false;
                
            Members.Add(viewer);
            viewer.Team = this;
            return true;
        }
        
        /// <summary>
        /// Remove a member from the team
        /// </summary>
        public bool RemoveMember(ViewerFaction viewer)
        {
            if (viewer == null || viewer == Leader)
                return false;
                
            if (Members.Remove(viewer))
            {
                viewer.Team = null;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all members except the specified viewer
        /// </summary>
        public List<ViewerFaction> GetOtherMembers(ViewerFaction viewer)
        {
            return Members.Where(m => m != viewer).ToList();
        }
        
        /// <summary>
        /// Get total team size
        /// </summary>
        public int MemberCount => Members.Count;
        
        /// <summary>
        /// Check if viewer is in this team
        /// </summary>
        public bool HasMember(ViewerFaction viewer)
        {
            return Members.Contains(viewer);
        }
        
        /// <summary>
        /// Add resources to shared pool
        /// </summary>
        public void AddSharedResource(string resourceKey, float amount)
        {
            if (!SharedResources.ContainsKey(resourceKey))
                SharedResources[resourceKey] = 0f;
            SharedResources[resourceKey] += amount;
        }
        
        /// <summary>
        /// Remove resources from shared pool
        /// </summary>
        public bool RemoveSharedResource(string resourceKey, float amount)
        {
            if (!SharedResources.ContainsKey(resourceKey))
                return false;
                
            if (SharedResources[resourceKey] < amount)
                return false;
                
            SharedResources[resourceKey] -= amount;
            return true;
        }
        
        /// <summary>
        /// Get shared resource amount
        /// </summary>
        public float GetSharedResource(string resourceKey)
        {
            return SharedResources.ContainsKey(resourceKey) ? SharedResources[resourceKey] : 0f;
        }
        
        /// <summary>
        /// Calculate total team points (sum of all members)
        /// </summary>
        public int CalculateTotalPoints()
        {
            return TeamPoints + Members.Sum(m => m.Points);
        }
        
        public override string ToString()
        {
            return $"{Name} (Leader: {Leader.DisplayName}, Members: {MemberCount}, Points: {CalculateTotalPoints()})";
        }
    }
}
