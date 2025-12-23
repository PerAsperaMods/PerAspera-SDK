#nullable enable
using System;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers.Enhanced
{
    /// <summary>
    /// Wrapper for the abstract ABCDroneState class
    /// Provides safe access to drone state machine functionality
    /// DOC: F:\ModPeraspera\Internal_doc\Decompiled_ILcpp2_Class\ABCDroneState.cs
    /// </summary>
    public class ABCDroneStateWrapper : WrapperBase
    {
        private static readonly string LogPrefix = "[ABCDroneStateWrapper]";
        
        /// <summary>
        /// Initialize ABCDroneStateWrapper with native ABCDroneState instance
        /// </summary>
        /// <param name="nativeDroneState">Native ABCDroneState from drone state machine</param>
        public ABCDroneStateWrapper(object nativeDroneState) : base(nativeDroneState)
        {
        }
        
        /// <summary>
        /// Create wrapper from native drone state object
        /// </summary>
        public static ABCDroneStateWrapper? FromNative(object? nativeDroneState)
        {
            return nativeDroneState != null ? new ABCDroneStateWrapper(nativeDroneState) : null;
        }
        
        // ==================== STATE MACHINE METHODS ====================
        
        /// <summary>
        /// Enter the drone state
        /// Maps to: ABCDroneState.Enter() - virtual void method
        /// </summary>
        public void Enter()
        {
            try
            {
                SafeInvoke<object>("Enter");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to enter state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Exit the drone state  
        /// Maps to: ABCDroneState.Exit() - virtual void method
        /// </summary>
        public void Exit()
        {
            try
            {
                SafeInvoke<object>("Exit");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to exit state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tick the drone state with delta time
        /// Maps to: ABCDroneState.OnTick(float deltaDays) - abstract method
        /// </summary>
        /// <param name="deltaDays">Time delta in game days</param>
        public void OnTick(float deltaDays)
        {
            try
            {
                SafeInvoke<object>("OnTick", deltaDays);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to tick state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Feed input to the drone state machine
        /// Maps to: ABCDroneState.OnFeed(InputEvent input) - abstract method
        /// Returns: StateID for next state transition
        /// </summary>
        /// <param name="inputEvent">Input event for state machine</param>
        /// <returns>StateID enum value or null if error</returns>
        public object? OnFeed(object inputEvent)
        {
            try
            {
                return SafeInvoke<object>("OnFeed", inputEvent);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to feed input: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get detailed debug information for this state
        /// Maps to: ABCDroneState.DebugDetailedInfo() - abstract method
        /// </summary>
        /// <returns>Debug string information</returns>
        public string GetDebugInfo()
        {
            try
            {
                return SafeInvoke<string>("DebugDetailedInfo") ?? "No debug info available";
            }
            catch (Exception ex)
            {
                return $"Error getting debug info: {ex.Message}";
            }
        }
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Get the type name of this drone state
        /// </summary>
        public string GetStateTypeName()
        {
            return NativeObject?.GetType().Name ?? "Unknown";
        }
        
        /// <summary>
        /// Check if this is a specific state type
        /// </summary>
        /// <param name="expectedTypeName">Expected state type name</param>
        /// <returns>True if matches expected type</returns>
        public bool IsStateType(string expectedTypeName)
        {
            return GetStateTypeName().Equals(expectedTypeName, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Get runtime information about the state
        /// </summary>
        public StateInfo GetStateInfo()
        {
            return new StateInfo
            {
                TypeName = GetStateTypeName(),
                IsValid = IsValid,
                DebugInfo = GetDebugInfo(),
                LastUpdated = DateTime.Now
            };
        }
    }
    
    /// <summary>
    /// Information about a drone state for diagnostics
    /// </summary>
    public struct StateInfo
    {
        public string TypeName { get; set; }
        public bool IsValid { get; set; }
        public string DebugInfo { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public override string ToString()
        {
            return $"{TypeName} (Valid: {IsValid}, Updated: {LastUpdated:HH:mm:ss})";
        }
    }
}