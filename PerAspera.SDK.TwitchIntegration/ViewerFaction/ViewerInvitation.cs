// ViewerInvitation.cs - Represents an invitation (team or deal) between viewers
using System;

namespace PerAspera.SDK.TwitchIntegration.ViewerFaction
{
    /// <summary>
    /// Represents an invitation from one viewer to another (team join or deal)
    /// </summary>
    public class ViewerInvitation
    {
        /// <summary>
        /// Unique identifier for this invitation
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Viewer who sent the invitation
        /// </summary>
        public ViewerFaction FromViewer { get; }
        
        /// <summary>
        /// Viewer who receives the invitation
        /// </summary>
        public ViewerFaction ToViewer { get; }
        
        /// <summary>
        /// Type of invitation
        /// </summary>
        public InvitationType Type { get; }
        
        /// <summary>
        /// Message or description of the invitation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Associated team (for team invitations)
        /// </summary>
        public ViewerTeam? Team { get; set; }
        
        /// <summary>
        /// Associated deal (for deal invitations)
        /// </summary>
        public ViewerDeal? Deal { get; set; }
        
        /// <summary>
        /// When the invitation was created
        /// </summary>
        public DateTime CreatedAt { get; }
        
        /// <summary>
        /// When the invitation expires
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Current status of the invitation
        /// </summary>
        public InvitationStatus Status { get; set; }
        
        public ViewerInvitation(ViewerFaction from, ViewerFaction to, InvitationType type, string message, TimeSpan? duration = null)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
                
            Id = Guid.NewGuid().ToString();
            FromViewer = from;
            ToViewer = to;
            Type = type;
            Message = message ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.Add(duration ?? TimeSpan.FromMinutes(5));
            Status = InvitationStatus.Pending;
        }
        
        /// <summary>
        /// Accept the invitation
        /// </summary>
        public void Accept()
        {
            if (Status == InvitationStatus.Pending && !IsExpired)
            {
                Status = InvitationStatus.Accepted;
            }
        }
        
        /// <summary>
        /// Decline the invitation
        /// </summary>
        public void Decline()
        {
            if (Status == InvitationStatus.Pending)
            {
                Status = InvitationStatus.Declined;
            }
        }
        
        /// <summary>
        /// Cancel the invitation (by sender)
        /// </summary>
        public void Cancel()
        {
            if (Status == InvitationStatus.Pending)
            {
                Status = InvitationStatus.Cancelled;
            }
        }
        
        /// <summary>
        /// Check if invitation has expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == InvitationStatus.Pending;
        
        /// <summary>
        /// Check if invitation is still pending
        /// </summary>
        public bool IsPending => Status == InvitationStatus.Pending && !IsExpired;
        
        public override string ToString()
        {
            return $"Invitation [{Type}] {FromViewer.DisplayName} → {ToViewer.DisplayName}: {Message} ({Status})";
        }
    }
    
    /// <summary>
    /// Type of invitation
    /// </summary>
    public enum InvitationType
    {
        Team,
        Deal
    }
    
    /// <summary>
    /// Status of an invitation
    /// </summary>
    public enum InvitationStatus
    {
        Pending,
        Accepted,
        Declined,
        Cancelled,
        Expired
    }
}
