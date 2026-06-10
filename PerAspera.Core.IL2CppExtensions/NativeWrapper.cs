#nullable enable
using System;
using System.Reflection;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Generic base class for all native IL2CPP object wrappers.
    /// Provides safe method invocation, field access, and debug utilities.
    /// Zero game-specific logic - usable for any IL2CPP object.
    ///
    /// A wrapper built around a null native object is allowed: <see cref="IsValidWrapper"/>
    /// reports false and every CallNative/GetNativeField returns default without throwing.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyWrapper : NativeWrapper&lt;object&gt;
    /// {
    ///     public MyWrapper(object native) : base(native) { }
    ///     public string GetName() => CallNative&lt;string&gt;("get_Name") ?? "Unknown";
    ///     public void SetActive(bool v) => CallNativeVoid("SetActive", v);
    /// }
    /// </code>
    /// </example>
    public abstract class NativeWrapper<T> where T : class
    {
        protected static readonly ManualLogSource Log = Logger.CreateLogSource("NativeWrapper");

        protected T? _nativeObject;

        public T? GetNativeObject() => _nativeObject;

        /// <summary>True when the wrapped native object is non-null. Honest: a wrapper
        /// constructed with null reports false (no placeholder object is substituted).</summary>
        public bool IsValidWrapper => _nativeObject != null;

        public System.Type? GetNativeType() => _nativeObject?.GetType();

        /// <summary>
        /// Wraps the given native object. Null is tolerated (the wrapper is then invalid:
        /// <see cref="IsValidWrapper"/> = false and all accessors return default).
        /// </summary>
        protected NativeWrapper(T? nativeObject)
        {
            _nativeObject = nativeObject;
        }

        // ==================== INVOCATION ====================

        protected TResult? CallNative<TResult>(string methodName, params object[] parameters)
        {
            var native = _nativeObject;
            if (native == null)
            {
                Log.LogWarning($"[{GetType().Name}] {methodName}: native object is null");
                return default;
            }
            try { return native.InvokeMethod<TResult>(methodName, parameters); }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] {methodName}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Invoke a void method on the native object.
        /// </summary>
        /// <returns>True when the method was found and invoked without error —
        /// callers implementing a fallback (e.g. direct field write) must check this.</returns>
        protected bool CallNativeVoid(string methodName, params object[] parameters)
        {
            var native = _nativeObject;
            if (native == null)
            {
                Log.LogWarning($"[{GetType().Name}] {methodName}: native object is null");
                return false;
            }
            try { return native.InvokeMethod(methodName, parameters); }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] {methodName}: {ex.Message}");
                return false;
            }
        }

        // ==================== FIELD ACCESS ====================

        protected TField? GetNativeField<TField>(string fieldName, BindingFlags? bindingFlags = null)
        {
            var native = _nativeObject;
            if (native == null)
            {
                Log.LogWarning($"[{GetType().Name}] GetField {fieldName}: native object is null");
                return default;
            }
            try
            {
                if (bindingFlags.HasValue)
                {
                    var field = native.GetType().GetField(fieldName, bindingFlags.Value);
                    return field != null ? (TField?)field.GetValue(native) : default;
                }
                return native.GetFieldValue<TField>(fieldName);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] GetField {fieldName}: {ex.Message}");
                return default;
            }
        }

        /// <returns>True when the field was found and written.</returns>
        protected bool SetNativeField<TField>(string fieldName, TField value, BindingFlags? bindingFlags = null)
        {
            var native = _nativeObject;
            if (native == null)
            {
                Log.LogWarning($"[{GetType().Name}] SetField {fieldName}: native object is null");
                return false;
            }
            try
            {
                if (bindingFlags.HasValue)
                {
                    var field = native.GetType().GetField(fieldName, bindingFlags.Value);
                    if (field == null) return false;
                    field.SetValue(native, value);
                    return true;
                }
                native.SetFieldValue(fieldName, value);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] SetField {fieldName}: {ex.Message}");
                return false;
            }
        }

        // ==================== PROPERTY ACCESS ====================

        // CallNative/CallNativeVoid already guard against null and exceptions —
        // no extra try/catch needed here.

        protected TProp? GetNativeProperty<TProp>(string propertyName)
            => CallNative<TProp>($"get_{propertyName}");

        protected bool SetNativeProperty<TProp>(string propertyName, TProp value)
            => CallNativeVoid($"set_{propertyName}", value!);

        // ==================== STATIC HELPERS ====================

        protected static TResult? CallStaticNative<TResult>(System.Type nativeType, string methodName, params object[] parameters)
        {
            try
            {
                var method = nativeType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                var result = method?.Invoke(null, parameters);
                return result is TResult typed ? typed : default;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[static] {nativeType.Name}.{methodName}: {ex.Message}");
                return default;
            }
        }

        // ==================== GUARD ====================

        protected bool ValidateNativeObject(string operationName = "")
        {
            if (_nativeObject == null)
            {
                Log.LogWarning($"[{GetType().Name}] Native object is null" +
                    (string.IsNullOrEmpty(operationName) ? "" : $" during {operationName}"));
                return false;
            }
            return true;
        }

        // ==================== DEBUG ====================

        public void DebugNativeStructure(bool includeInherited = true, bool includePrivate = true)
        {
            var native = _nativeObject;
            if (native == null) { Log.LogWarning($"[{GetType().Name}] null - cannot debug"); return; }
            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (includePrivate) flags   |= BindingFlags.NonPublic;
            if (includeInherited) flags |= BindingFlags.FlattenHierarchy;
            var t = native.GetType();
            Log.LogInfo($"[DEBUG] {GetType().Name} -> {t.FullName}");
            foreach (var f in t.GetFields(flags))
                Log.LogInfo($"[DEBUG]   {(f.IsPublic?"public":"private")} {f.FieldType.Name} {f.Name}");
            foreach (var m in t.GetMethods(flags))
            {
                var ps = string.Join(", ", System.Array.ConvertAll(m.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                Log.LogInfo($"[DEBUG]   {(m.IsPublic?"public":"private")} {m.ReturnType.Name} {m.Name}({ps})");
            }
        }

        public void DebugGameEventBus()
        {
            var native = _nativeObject;
            if (native == null) { Log.LogWarning($"[{GetType().Name}] null - cannot search"); return; }
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var t = native.GetType();
            Log.LogInfo($"[DEBUG_GEB] Searching {t.FullName}");
            foreach (var f in t.GetFields(flags))
                if (f.Name.ToLower().Contains("eventbus"))
                {
                    var v = f.GetValue(native);
                    Log.LogInfo($"[DEBUG_GEB] {f.FieldType.Name} {f.Name} = {v?.GetType().Name ?? "null"}");
                }
            foreach (var m in t.GetMethods(flags))
                if (m.Name.ToLower().Contains("eventbus"))
                    Log.LogInfo($"[DEBUG_GEB] {m.ReturnType.Name} {m.Name}(...)");
        }
    }
}
