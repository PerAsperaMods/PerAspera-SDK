using System;
using System.Collections.Generic;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Blackboard class (variable storage system)
    /// Provides safe access to Blackboard variables used for quest/dialog state management
    /// DOC: Blackboard.md - Variable storage with string/float/bool values
    /// </summary>
    public class BlackBoard : WrapperBase
    {
        /// <summary>
        /// Creates a new BlackBoard wrapper around a native blackboard instance
        /// </summary>
        /// <param name="nativeBlackBoard">Native blackboard object to wrap</param>
        public BlackBoard(object nativeBlackBoard) : base(nativeBlackBoard)
        {
        }
        public  Native.Blackboard ? GetNativeObject()
        {
            return (Native.Blackboard)NativeObject;
        }
        /// <summary>
        /// Get the name of this blackboard instance
        /// Field: name (readonly string)
        /// </summary>
        public string? Name => SafeGetField<string>("name");
        
        // ==================== VALUE ACCESS METHODS ====================
        
        /// <summary>
        /// Try to get a value from the blackboard
        /// Method: TryGetValue(string variableName, out Value value)
        /// </summary>
        public bool TryGetValue(string variableName, out object? value)
        {
            value = null;
            if (!ValidateNativeObject(nameof(TryGetValue)))
                return false;
                
            try
            {
                // Use reflection to call TryGetValue with out parameter
                var method = NativeObject?.GetType().GetMethod("TryGetValue");
                if (method == null) return false;
                
                var parameters = new object?[] { variableName, null };
                var result = (bool)(method.Invoke(NativeObject, parameters) ?? false);
                value = parameters[1]; // out parameter value
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to TryGetValue for {variableName} on BlackBoard: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get a value from the blackboard
        /// Method: GetValue(string variableName)
        /// Returns: Value (native Yarn.Value type)
        /// </summary>
        public object? GetValue(string variableName)
        {
            return SafeInvoke<object>("GetValue", variableName);
        }
        
        /// <summary>
        /// Check if the blackboard contains a specific key
        /// Method: ContainsKey(string variableName)
        /// </summary>
        public bool ContainsKey(string variableName)
        {
            return SafeInvoke<bool>("ContainsKey", variableName);
        }
        
        // ==================== VALUE SETTING METHODS ====================
        
        /// <summary>
        /// Set a string value in the blackboard
        /// Method: SetValue(string variableName, string stringValue)
        /// </summary>
        public void SetValue(string variableName, string stringValue)
        {
            SafeInvokeVoid("SetValue", variableName, stringValue);
        }
        
        /// <summary>
        /// Set a float value in the blackboard
        /// Method: SetValue(string variableName, float floatValue)
        /// </summary>
        public void SetValue(string variableName, float floatValue)
        {
            SafeInvokeVoid("SetValue", variableName, floatValue);
        }
        
        /// <summary>
        /// Set a boolean value in the blackboard
        /// Method: SetValue(string variableName, bool boolValue)
        /// </summary>
        public void SetValue(string variableName, bool boolValue)
        {
            SafeInvokeVoid("SetValue", variableName, boolValue);
        }
        
        /// <summary>
        /// Set a number value using the legacy SetNumber method
        /// Method: SetNumber(string variableName, float number)
        /// Note: Marked as obsolete in native code but still functional
        /// </summary>
        public void SetNumber(string variableName, float number)
        {
            SafeInvokeVoid("SetNumber", variableName, number);
        }
        
        /// <summary>
        /// Get a number value using the legacy GetNumber method
        /// Method: GetNumber(string variableName)
        /// Note: Marked as obsolete in native code but still functional
        /// </summary>
        public float GetNumber(string variableName)
        {
            return SafeInvoke<float>("GetNumber", variableName);
        }
        
        // ==================== COLLECTION METHODS ====================
        
        /// <summary>
        /// Get all variable keys in this blackboard
        /// Method: GetKeys()
        /// Returns: List&lt;string&gt; of variable names
        /// </summary>
        public IList<string>? GetKeys()
        {
            var nativeList = SafeInvoke<object>("GetKeys");
            if (nativeList == null) return null;
            
            try
            {
                // Convert IL2CPP List<string> to managed IList<string>
                return nativeList.ConvertIl2CppList<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to convert GetKeys result: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all dynamic variable keys in this blackboard
        /// Method: GetDynamicKeys()
        /// Returns: List&lt;string&gt; of dynamic variable names
        /// </summary>
        public IList<string>? GetDynamicKeys()
        {
            var nativeList = SafeInvoke<object>("GetDynamicKeys");
            if (nativeList == null) return null;
            
            try
            {
                // Convert IL2CPP List<string> to managed IList<string>
                return nativeList.ConvertIl2CppList<string>();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to convert GetDynamicKeys result: {ex.Message}");
                return null;
            }
        }
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Increment a numeric variable by the specified amount
        /// Method: Increment(string variableName, float amount)
        /// </summary>
        public void Increment(string variableName, float amount)
        {
            SafeInvokeVoid("Increment", variableName, amount);
        }
        
        /// <summary>
        /// Clear all variables from this blackboard
        /// Method: Clear()
        /// </summary>
        public void Clear()
        {
            SafeInvokeVoid("Clear");
        }
        
        /// <summary>
        /// Set a default value for a variable if it doesn't exist
        /// Method: DefaultSet&lt;T&gt;(string variableName, T defaultValue)
        /// Note: Generic method, requires special handling
        /// </summary>
        public void DefaultSetString(string variableName, string defaultValue)
        {
            try
            {
                var method = NativeObject?.GetType().GetMethod("DefaultSet")?.MakeGenericMethod(typeof(string));
                method?.Invoke(NativeObject, new object[] { variableName, defaultValue });
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to DefaultSetString for {variableName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set a default numeric value for a variable if it doesn't exist
        /// Method: DefaultSet&lt;T&gt;(string variableName, T defaultValue)
        /// </summary>
        public void DefaultSetFloat(string variableName, float defaultValue)
        {
            try
            {
                var method = NativeObject?.GetType().GetMethod("DefaultSet")?.MakeGenericMethod(typeof(float));
                method?.Invoke(NativeObject, new object[] { variableName, defaultValue });
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to DefaultSetFloat for {variableName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set a default boolean value for a variable if it doesn't exist
        /// Method: DefaultSet&lt;T&gt;(string variableName, T defaultValue)
        /// </summary>
        public void DefaultSetBool(string variableName, bool defaultValue)
        {
            try
            {
                var method = NativeObject?.GetType().GetMethod("DefaultSet")?.MakeGenericMethod(typeof(bool));
                method?.Invoke(NativeObject, new object[] { variableName, defaultValue });
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to DefaultSetBool for {variableName}: {ex.Message}");
            }
        }
        
        // ==================== DEBUGGING & INFO ====================
        
        /// <summary>
        /// Get a debug-friendly string representation of this blackboard
        /// </summary>
        public override string ToString()
        {
            var name = Name ?? "Unknown";
            var keyCount = GetKeys()?.Count ?? 0;
            var dynamicKeyCount = GetDynamicKeys()?.Count ?? 0;
            
            return $"BlackBoard [{name}]: {keyCount} variables, {dynamicKeyCount} dynamic";
        }
        
        /// <summary>
        /// Get detailed debug information about this blackboard
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $"BlackBoard '{Name ?? "Unknown"}':\n";
            
            var keys = GetKeys();
            if (keys != null && keys.Count > 0)
            {
                info += "  Variables:\n";
                foreach (var key in keys)
                {
                    var value = GetValue(key);
                    info += $"    {key} = {value}\n";
                }
            }
            
            var dynamicKeys = GetDynamicKeys();
            if (dynamicKeys != null && dynamicKeys.Count > 0)
            {
                info += "  Dynamic Variables:\n";
                foreach (var key in dynamicKeys)
                {
                    info += $"    {key} (dynamic)\n";
                }
            }
            
            return info;
        }
    }
}