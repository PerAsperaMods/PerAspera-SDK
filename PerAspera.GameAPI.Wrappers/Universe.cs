using System;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Universe class
    /// Provides safe access to time, game state and universe-level properties
    /// </summary>
    public class Universe : WrapperBase
    {
        public Universe(object nativeUniverse) : base(nativeUniverse)
        {
        }
        
        /// <summary>
        /// Get the current universe instance
        /// </summary>
        public static Universe? GetCurrent()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe != null ? new Universe(universe) : null;
        }
        
        // ==================== TIME PROPERTIES ====================
        
        /// <summary>
        /// Get current Martian sol (days passed)
        /// </summary>
        public int CurrentSol => SafeInvoke<int?>("GetDaysPassed") ?? 0;
        
        /// <summary>
        /// Get current game speed multiplier
        /// </summary>
        public float GameSpeed
        {
            get => SafeInvoke<float?>("GetGameSpeed") ?? 1.0f;
            set => SafeInvokeVoid("SetGameSpeed", value);
        }
        
        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsPaused => SafeInvoke<bool>("IsPaused");
        
        // ==================== GAME STATE ====================
        
        /// <summary>
        /// Get the planet instance
        /// </summary>
        public object? GetPlanet()
        {
            return SafeInvoke<object>("GetPlanet");
        }
        
        /// <summary>
        /// Get the base game instance
        /// </summary>
        public object? GetBaseGame()
        {
            return SafeInvoke<object>("GetBaseGame");
        }
        
        // ==================== ACTIONS ====================
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void Pause()
        {
            SafeInvokeVoid("SetPaused", true);
        }
        
        /// <summary>
        /// Unpause the game
        /// </summary>
        public void Unpause()
        {
            SafeInvokeVoid("SetPaused", false);
        }
        
        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SafeInvokeVoid("SetPaused", !IsPaused);
        }
        
        // ==================== INFO ====================
        
        public override string ToString()
        {
            return $"Universe: Sol {CurrentSol}, Speed={GameSpeed}x, Paused={IsPaused}";
        }
    }
}
