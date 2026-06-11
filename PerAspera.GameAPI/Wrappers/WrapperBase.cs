using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Base class for all game object wrappers.
    /// Inherits reflection helpers from NativeWrapper&lt;object&gt; and adds BepInEx logging,
    /// safe convenience accessors (SafeInvoke / SafeGetField), and debug dump utilities.
    /// </summary>
    public abstract class WrapperBase : NativeWrapper<object>
    {
        protected static readonly LogAspera WrapperLog = new LogAspera("Wrappers");

        /// <summary>The native IL2CPP game object being wrapped.</summary>
        protected object? NativeObject
        {
            get => GetNativeObject();
            set => _nativeObject = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>True when the native object is non-null.</summary>
        protected bool IsValid => GetNativeObject() != null;

        /// <summary>
        /// Wraps the given native object. Null is tolerated but the wrapper is then
        /// invalid: <see cref="IsValid"/> = false and every Safe* accessor returns default.
        /// (Previously a placeholder object was substituted, which made IsValid lie.)
        /// </summary>
        protected WrapperBase(object? nativeObject) : base(nativeObject)
        {
            if (nativeObject == null)
                WrapperLog.Warning($"Creating {GetType().Name} with null native object");
        }

        // ==================== SAFE HELPERS ====================

        /// <summary>
        /// Invoke a method on the native object, returning default on failure.
        /// </summary>
        protected T? SafeInvoke<T>(string methodName, params object[] args)
            => CallNative<T>(methodName, args);

        /// <summary>
        /// Invoke a void method on the native object, swallowing exceptions.
        /// </summary>
        protected void SafeInvokeVoid(string methodName, params object[] args)
            => CallNativeVoid(methodName, args);

        /// <summary>
        /// Invoke a void method on the native object and report success.
        /// Use this instead of SafeInvokeVoid when a fallback is needed —
        /// SafeInvokeVoid never throws, so try/catch around it is dead code.
        /// </summary>
        /// <example>if (!TryInvokeVoid("set_waterStock", v)) TrySetField("waterStock", v);</example>
        protected bool TryInvokeVoid(string methodName, params object[] args)
            => CallNativeVoid(methodName, args);

        /// <summary>
        /// Read a field value from the native object, returning default on failure.
        /// </summary>
        protected T? SafeGetField<T>(string fieldName)
            => GetNativeField<T>(fieldName);

        /// <summary>
        /// Write a field value on the native object, swallowing exceptions.
        /// </summary>
        protected void SafeSetField<T>(string fieldName, T value)
            => SetNativeField(fieldName, value);

        /// <summary>
        /// Write a field value on the native object and report success.
        /// </summary>
        protected bool TrySetField<T>(string fieldName, T value)
            => SetNativeField(fieldName, value);

        // ==================== DEBUG FILE DUMPS ====================

        /// <summary>
        /// Dumps the full native object structure to a file under BepInEx/Debug/.
        /// </summary>
        /// <example>wrapper.DumpToFile("FactionWrapper") → BepInEx/Debug/IL2CPP-FactionWrapper-timestamp.txt</example>
        public string? DumpToFile(string? objectName = null)
        {
            var nativeObj = GetNativeObject();
            if (nativeObj == null)
            {
                WrapperLog.Warning($"{GetType().Name}: Cannot dump - native object is null");
                return null;
            }

            objectName ??= GetType().Name;
            var filePath = IL2CppDebugDumper.DumpObjectToFile(nativeObj, objectName);
            WrapperLog.Info($"Dumped {objectName} structure to: {filePath}");
            return filePath;
        }

        /// <summary>
        /// Search for members matching a pattern and save results to BepInEx/Debug/.
        /// </summary>
        /// <example>wrapper.SearchMembersInFile("type") → BepInEx/Debug/IL2CPP-Search-type-timestamp.txt</example>
        public string? SearchMembersInFile(string searchTerm)
        {
            var nativeObj = GetNativeObject();
            if (nativeObj == null)
            {
                WrapperLog.Warning($"{GetType().Name}: Cannot search - native object is null");
                return null;
            }

            var filePath = IL2CppDebugDumper.FindMembersToFile(nativeObj, searchTerm);
            WrapperLog.Info($"Search results saved to: {filePath}");
            return filePath;
        }
    }
}
#pragma warning restore CS1591
