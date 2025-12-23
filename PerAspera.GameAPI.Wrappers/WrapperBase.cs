using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Base class for all game object wrappers
    /// Provides common functionality for wrapping native IL2CPP game objects
    /// Now inherits from NativeWrapper<object> for unified method calling
    /// 
    /// 📚 Architecture Guide: F:\ModPeraspera\Organization-Wiki\architecture\SDK-Components.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🔧 Core Utilities: F:\ModPeraspera\SDK\PerAspera.Core\IL2CPP\SafeInvoke patterns
    /// 🎯 Best Practices: Always inherit for new game object wrappers
    /// </summary>
    public abstract class WrapperBase : NativeWrapper<object>
    {
        protected static readonly LogAspera WrapperLog = new LogAspera("Wrappers");
        
        /// <summary>
        /// Legacy compatibility: The native IL2CPP game object being wrapped
        /// Now delegates to base NativeWrapper._nativeObject
        /// </summary>
        protected object? NativeObject 
        {
            get => GetNativeObject();
            set => _nativeObject = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Check if the native object is valid
        /// </summary>
        protected bool IsValid => GetNativeObject() != null;
        
        /// <summary>
        /// Public accessor for IsValid - allows cross-wrapper validation
        /// </summary>
        public bool IsValidWrapper => IsValid;
        
        /// <summary>
        /// Get the Type of the native IL2CPP object (for debugging/inspection)
        /// Returns null if no native object is wrapped
        /// </summary>
        public System.Type? GetNativeType()
        {
            return GetNativeObject()?.GetType();
        }
        
        /// <summary>
        /// Constructor with native object - now uses NativeWrapper base
        /// </summary>
        protected WrapperBase(object? nativeObject) : base(nativeObject ?? new object())
        {
            if (nativeObject == null)
            {
                WrapperLog.Warning($"Creating {GetType().Name} with null native object");
            }
        }
        
        /// <summary>
        /// Validate that the native object exists
        /// </summary>
        protected bool ValidateNativeObject(string operationName = "")
        {
            if (GetNativeObject() == null)
            {
                WrapperLog.Warning($"Native object is null for {GetType().Name}" + 
                           (string.IsNullOrEmpty(operationName) ? "" : $" during {operationName}"));
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Safely invoke a method on the native object - delegates to NativeWrapper
        /// </summary>
        protected T? SafeInvoke<T>(string methodName, params object[] args)
        {
            return CallNative<T>(methodName, args);
        }
        
        /// <summary>
        /// Safely invoke a void method on the native object - delegates to NativeWrapper
        /// </summary>
        protected void SafeInvokeVoid(string methodName, params object[] args)
        {
            CallNativeVoid(methodName, args);
        }
        
        /// <summary>
        /// Safely get a field value from the native object - delegates to NativeWrapper
        /// </summary>
        protected T? SafeGetField<T>(string fieldName)
        {
            return GetNativeField<T>(fieldName);
        }
        
        /// <summary>
        /// Safely set a field value on the native object - delegates to NativeWrapper
        /// </summary>
        protected void SafeSetField<T>(string fieldName, T value)
        {
            SetNativeField(fieldName, value);
        }
        
        /// <summary>
        /// Debug function to inspect native object structure
        /// Lists all fields and methods with their types for debugging
        /// </summary>
        /// <param name="includeInherited">Include fields/methods from base classes</param>
        /// <param name="includePrivate">Include private members</param>
        public void DebugNativeStructure(bool includeInherited = true, bool includePrivate = true)
        {
            var nativeObj = GetNativeObject();
            if (nativeObj == null)
            {
                WrapperLog.Warning($"[DEBUG] {GetType().Name}: Native object is null");
                return;
            }
            
            var objType = nativeObj.GetType();
            WrapperLog.Info($"[DEBUG] {GetType().Name} wrapping native type: {objType.FullName}");
            
            // Binding flags
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            if (includePrivate) flags |= System.Reflection.BindingFlags.NonPublic;
            if (includeInherited) flags |= System.Reflection.BindingFlags.FlattenHierarchy;
            
            // Debug fields
            var fields = objType.GetFields(flags);
            WrapperLog.Info($"[DEBUG] Found {fields.Length} fields:");
            foreach (var field in fields)
            {
                var visibility = field.IsPublic ? "public" : field.IsPrivate ? "private" : "protected";
                var isStatic = field.IsStatic ? "static " : "";
                WrapperLog.Info($"[DEBUG]   {visibility} {isStatic}{field.FieldType.Name} {field.Name}");
                
                // Show GameEventBus related fields with extra detail
                if (field.Name.Contains("gameEventBus") || field.Name.Contains("GameEventBus"))
                {
                    try
                    {
                        var value = field.GetValue(nativeObj);
                        WrapperLog.Info($"[DEBUG]   *** GAME_EVENT_BUS_FIELD: {field.Name} = {value?.GetType().Name ?? "null"} ***");
                    }
                    catch (Exception ex)
                    {
                        WrapperLog.Info($"[DEBUG]   *** GAME_EVENT_BUS_FIELD: {field.Name} - Error accessing: {ex.Message} ***");
                    }
                }
            }
            
            // Debug methods
            var methods = objType.GetMethods(flags);
            WrapperLog.Info($"[DEBUG] Found {methods.Length} methods:");
            var methodGroups = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var method in methods)
            {
                var methodName = method.Name;
                if (methodGroups.ContainsKey(methodName))
                {
                    methodGroups[methodName]++;
                }
                else
                {
                    methodGroups[methodName] = 1;
                    var visibility = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
                    var isStatic = method.IsStatic ? "static " : "";
                    var parameters = string.Join(", ", System.Array.ConvertAll(method.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                    WrapperLog.Info($"[DEBUG]   {visibility} {isStatic}{method.ReturnType.Name} {methodName}({parameters})");
                }
            }
        }
        
        /// <summary>
        /// Quick debug for GameEventBus specifically - searches for GameEventBus related members
        /// </summary>
        public void DebugGameEventBus()
        {
            var nativeObj = GetNativeObject();
            if (nativeObj == null)
            {
                WrapperLog.Warning($"[DEBUG_GEB] {GetType().Name}: Native object is null");
                return;
            }
            
            var objType = nativeObj.GetType();
            WrapperLog.Info($"[DEBUG_GEB] Searching for GameEventBus in {objType.FullName}");
            
            var allFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | 
                          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy;
            
            // Search all fields
            var fields = objType.GetFields(allFlags);
            foreach (var field in fields)
            {
                if (field.Name.ToLower().Contains("gameeventbus") || field.Name.ToLower().Contains("eventbus"))
                {
                    try
                    {
                        var value = field.GetValue(nativeObj);
                        var visibility = field.IsPublic ? "public" : field.IsPrivate ? "private" : "protected";
                        var isStatic = field.IsStatic ? "static " : "";
                        WrapperLog.Info($"[DEBUG_GEB] *** FOUND: {visibility} {isStatic}{field.FieldType.Name} {field.Name} = {value?.GetType().Name ?? "null"} ***");
                    }
                    catch (Exception ex)
                    {
                        WrapperLog.Info($"[DEBUG_GEB] *** FOUND: {field.Name} - Error accessing: {ex.Message} ***");
                    }
                }
            }
            
            // Search all methods
            var methods = objType.GetMethods(allFlags);
            foreach (var method in methods)
            {
                if (method.Name.ToLower().Contains("gameeventbus") || method.Name.ToLower().Contains("eventbus"))
                {
                    var visibility = method.IsPublic ? "public" : method.IsPrivate ? "private" : "protected";
                    var isStatic = method.IsStatic ? "static " : "";
                    var parameters = string.Join(", ", System.Array.ConvertAll(method.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                    WrapperLog.Info($"[DEBUG_GEB] *** FOUND: {visibility} {isStatic}{method.ReturnType.Name} {method.Name}({parameters}) ***");
                }
            }
        }
    }
}
