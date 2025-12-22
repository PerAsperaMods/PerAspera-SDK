// ViewerDeal.cs - Represents a deal or agreement between viewer factions
using System;

namespace PerAspera.SDK.TwitchIntegration.ViewerFaction
{
    /// <summary>
    /// Represents a deal or agreement between two viewer factions
    /// </summary>
    public class ViewerDeal
    {
        /// <summary>
        /// Unique identifier for this deal
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Viewer who proposed the deal
        /// </summary>
        public ViewerFaction Proposer { get; }
        
        /// <summary>
        /// Viewer who receives the deal proposal
        /// </summary>
        public ViewerFaction Receiver { get; }
        
        /// <summary>
        /// Terms of the deal (human-readable description)
        /// </summary>
        public string Terms { get; set; }
        
        /// <summary>
        /// Resource being offered by proposer
        /// </summary>
        public string? OfferedResource { get; set; }
        
        /// <summary>
        /// Amount of resource being offered
        /// </summary>
        public float OfferedAmount { get; set; }
        
        /// <summary>
        /// Resource being requested from receiver
        /// </summary>
        public string? RequestedResource { get; set; }
        
        /// <summary>
        /// Amount of resource being requested
        /// </summary>
        public float RequestedAmount { get; set; }
        
        /// <summary>
        /// Current status of the deal
        /// </summary>
        public DealStatus Status { get; set; }
        
        /// <summary>
        /// When the deal was proposed
        /// </summary>
        public DateTime ProposedAt { get; }
        
        /// <summary>
        /// When the deal was accepted/rejected/expired
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
        
        /// <summary>
        /// Deal expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        public ViewerDeal(ViewerFaction proposer, ViewerFaction receiver, string terms, TimeSpan? duration = null)
        {
            if (proposer == null)
                throw new ArgumentNullException(nameof(proposer));
            if (receiver == null)
                throw new ArgumentNullException(nameof(receiver));
            if (string.IsNullOrWhiteSpace(terms))
                throw new ArgumentException("Terms cannot be null or empty", nameof(terms));
                
            Id = Guid.NewGuid().ToString();
            Proposer = proposer;
            Receiver = receiver;
            Terms = terms;
            Status = DealStatus.Pending;
            ProposedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.Add(duration ?? TimeSpan.FromMinutes(5));
        }
        
        /// <summary>
        /// Accept the deal and execute the exchange
        /// </summary>
        public bool Accept()
        {
            if (Status != DealStatus.Pending)
                return false;
                
            if (DateTime.UtcNow > ExpiresAt)
            {
                Status = DealStatus.Expired;
                return false;
            }
            
            // Execute resource exchange if specified
            if (!string.IsNullOrEmpty(OfferedResource) && !string.IsNullOrEmpty(RequestedResource))
            {
                // Check if both parties have required resources
                if (Proposer.GetResource(OfferedResource) < OfferedAmount)
                    return false;
                    
                if (Receiver.GetResource(RequestedResource) < RequestedAmount)
                    return false;
                
                // Execute exchange
                Proposer.RemoveResource(OfferedResource, OfferedAmount);
                Receiver.AddResource(OfferedResource, OfferedAmount);
                
                Receiver.RemoveResource(RequestedResource, RequestedAmount);
                Proposer.AddResource(RequestedResource, RequestedAmount);
            }
            
            Status = DealStatus.Accepted;
            ResolvedAt = DateTime.UtcNow;
            return true;
        }
        
        /// <summary>
        /// Reject the deal
        /// </summary>
        public void Reject()
        {
            if (Status == DealStatus.Pending)
            {
                Status = DealStatus.Rejected;
                ResolvedAt = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Cancel the deal (by proposer)
        /// </summary>
        public void Cancel()
        {
            if (Status == DealStatus.Pending)
            {
                Status = DealStatus.Cancelled;
                ResolvedAt = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Check if deal has expired
        /// </summary>
        public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == DealStatus.Pending;
        
        /// <summary>
        /// Check if deal is still active/pending
        /// </summary>
        public bool IsPending => Status == DealStatus.Pending && !IsExpired;
        
        public override string ToString()
        {
            return $"Deal [{Status}]: {Proposer.DisplayName} → {Receiver.DisplayName} | {Terms}";
        }
    }
    
    /// <summary>
    /// Status of a viewer deal
    /// </summary>
    public enum DealStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled,
        Expired
    }
}
