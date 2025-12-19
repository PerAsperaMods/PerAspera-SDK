using System;
using System.Collections.Concurrent;
using PerAspera.Core.IL2CPP;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Thread-safe rate limiter for Twitch commands
    /// Prevents spam and ensures smooth performance
    /// </summary>
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastCommandTime = new();
        private readonly TimeSpan _cooldownPeriod;
        private readonly object _cleanupLock = new();
        private readonly LogAspera _log = LogAspera.Create("RateLimiter");
        
        private DateTime _lastCleanup = DateTime.Now;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Initialize rate limiter with specified cooldown
        /// </summary>
        public RateLimiter(TimeSpan cooldownPeriod)
        {
            _cooldownPeriod = cooldownPeriod;
            _log.Info($"RateLimiter initialized with {cooldownPeriod.TotalSeconds}s cooldown");
        }
        
        /// <summary>
        /// Check if user can execute a command (thread-safe)
        /// </summary>
        public bool CheckRateLimit(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;
                
            var now = DateTime.Now;
            var userKey = username.ToLowerInvariant();
            
            // Check if user can execute command
            if (_lastCommandTime.TryGetValue(userKey, out var lastTime))
            {
                var timeSinceLastCommand = now - lastTime;
                if (timeSinceLastCommand < _cooldownPeriod)
                {
                    var remainingCooldown = _cooldownPeriod - timeSinceLastCommand;
                    _log.Debug($"Rate limit hit for user {username}: {remainingCooldown.TotalSeconds:F1}s remaining");
                    return false;
                }
            }
            
            // Update last command time
            _lastCommandTime[userKey] = now;
            
            // Periodic cleanup to prevent memory growth
            PerformCleanupIfNeeded(now);
            
            return true;
        }
        
        /// <summary>
        /// Get remaining cooldown for a user
        /// </summary>
        public TimeSpan GetRemainingCooldown(string username)
        {
            if (string.IsNullOrEmpty(username))
                return TimeSpan.Zero;
                
            var userKey = username.ToLowerInvariant();
            
            if (_lastCommandTime.TryGetValue(userKey, out var lastTime))
            {
                var elapsed = DateTime.Now - lastTime;
                var remaining = _cooldownPeriod - elapsed;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            
            return TimeSpan.Zero;
        }
        
        /// <summary>
        /// Reset cooldown for a specific user (admin/moderator function)
        /// </summary>
        public void ResetUserCooldown(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;
                
            var userKey = username.ToLowerInvariant();
            _lastCommandTime.TryRemove(userKey, out _);
            _log.Info($"Cooldown reset for user: {username}");
        }
        
        /// <summary>
        /// Clear all rate limiting data
        /// </summary>
        public void ClearAll()
        {
            _lastCommandTime.Clear();
            _log.Info("All rate limit data cleared");
        }
        
        /// <summary>
        /// Get current tracked user count
        /// </summary>
        public int GetTrackedUserCount()
        {
            return _lastCommandTime.Count;
        }
        
        /// <summary>
        /// Periodic cleanup to remove old entries and prevent memory growth
        /// </summary>
        private void PerformCleanupIfNeeded(DateTime now)
        {
            // Only one thread should do cleanup at a time
            if (now - _lastCleanup < _cleanupInterval)
                return;
                
            lock (_cleanupLock)
            {
                // Double-check in case another thread already did cleanup
                if (now - _lastCleanup < _cleanupInterval)
                    return;
                    
                var cutoffTime = now - (_cooldownPeriod * 2); // Remove entries older than 2x cooldown
                var removedCount = 0;
                
                var keysToRemove = new List<string>();
                foreach (var kvp in _lastCommandTime)
                {
                    if (kvp.Value < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (var key in keysToRemove)
                {
                    if (_lastCommandTime.TryRemove(key, out _))
                    {
                        removedCount++;
                    }
                }
                
                _lastCleanup = now;
                
                if (removedCount > 0)
                {
                    _log.Debug($"Cleanup completed: removed {removedCount} old entries, {_lastCommandTime.Count} remaining");
                }
            }
        }
        
        /// <summary>
        /// Get rate limiter statistics for debugging
        /// </summary>
        public string GetStats()
        {
            var stats = new System.Text.StringBuilder();
            stats.AppendLine($"Cooldown Period: {_cooldownPeriod.TotalSeconds}s");
            stats.AppendLine($"Tracked Users: {GetTrackedUserCount()}");
            stats.AppendLine($"Last Cleanup: {_lastCleanup:HH:mm:ss}");
            return stats.ToString();
        }
    }
}