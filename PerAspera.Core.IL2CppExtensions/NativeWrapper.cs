#nullable enable
using System;
using System.Reflection;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Generic base class for all native IL2CPP object wrappers.
    /// Provides safe method invocation, field access, and debug utilities.
    /// Zero game-specific logic â€” usable for any IL2CPP object.
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

        protected T _nativeObject;

        public T GetNativeObject() => _nativeObject;

        public bool IsValidWrapper => _nativeObject != null;

        public System.Type? GetNativeType() => _nativeObject?.GetType();

        protected NativeWrapper(T nativeObject)
        {
            _nativeObject = nativeObject ?? throw new ArgumentNullException(nameof(nativeObject));
        }

        // ==================== INVOCATION ====================

        protected TResult? CallNative<TResult>(string methodName, params object[] parameters)
        {
            try { return _nativeObject.InvokeMethod<TResult>(methodName, parameters); }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] {methodName}: {ex.Message}");
                return default;
            }
        }

        protected void CallNativeVoid(string methodName, params object[] parameters)
        {
            try { _nativeObject.InvokeMethod(methodName, parameters); }
            catch (Exception ex) { Log.LogWarning($"[{GetType().Name}] {methodName}: {ex.Message}"); }
        }

        // ==================== FIELD ACCESS ====================

        protected TField? GetNativeField<TField>(string fieldName, BindingFlags? bindingFlags = null)
        {
            try
            {
                if (bindingFlags.HasValue)
                {
                    var field = _nativeObject.GetType().GetField(fieldName, bindingFlags.Value);
                    return field != null ? (TField?)field.GetValue(_nativeObject) : default;
                }
                return _nativeObject.GetFieldValue<TField>(fieldName);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] GetField {fieldName}: {ex.Message}");
                return default;
            }
        }

        protected void SetNativeField<TField>(string fieldName, TField value, BindingFlags? bindingFlags = null)
        {
            try
            {
                if (bindingFlags.HasValue)
                {
                    _nativeObject.GetType().GetField(fieldName, bindingFlags.Value)?.SetValue(_nativeObject, value);
                }
                else { _nativeObject.SetFieldValue(fieldName, value); }
            }
            catch (Exception ex) { Log.LogWarning($"[{GetType().Name}] SetField {fieldName}: {ex.Message}"); }
        }

        // ==================== PROPERTY ACCESS ====================

        protected TProp? GetNativeProperty<TProp>(string propertyName)
        {
            try { return CallNative<TProp>($"get_{propertyName}"); }
            catch (Exception ex)
            {
                Log.LogWarning($"[{GetType().Name}] GetProperty {propertyName}: {ex.Message}");
                return default;
            }
        }

        protected void SetNativeProperty<TProp>(string propertyName, TProp value)
        {
            try { CallNativeVoid($"set_{propertyName}", value!); }
            catch (Exception ex) { Log.LogWarning($"[{GetType().Name}] SetProperty {propertyName}: {ex.Message}"); }
        }

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
            if (_nativeObject == null) { Log.LogWarning($"[{GetType().Name}] null â€” cannot debug"); return; }
            var flags = BindingFlags.Instance | BindingFlags.Public;
            if (includePrivate) flags   |= BindingFlags.NonPublic;
            if (includeInherited) flags |= BindingFlags.FlattenHierarchy;
            var t = _nativeObject.GetType();
            Log.LogInfo($"[DEBUG] {GetType().Name} â†’ {t.FullName}");
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
            if (_nativeObject == null) { Log.LogWarning($"[{GetType().Name}] null â€” cannot search"); return; }
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var t = _nativeObject.GetType();
            Log.LogInfo($"[DEBUG_GEB] Searching {t.FullName}");
            foreach (var f in t.GetFields(flags))
                if (f.Name.ToLower().Contains("eventbus"))
                {
                    var v = f.GetValue(_nativeObject);
                    Log.LogInfo($"[DEBUG_GEB] {f.FieldType.Name} {f.Name} = {v?.GetType().Name ?? "null"}");
                }
            foreach (var m in t.GetMethods(flags))
                if (m.Name.ToLower().Contains("eventbus"))
                    Log.LogInfo($"[DEBUG_GEB] {m.ReturnType.Name} {m.Name}(...)");
        }
    }
}

