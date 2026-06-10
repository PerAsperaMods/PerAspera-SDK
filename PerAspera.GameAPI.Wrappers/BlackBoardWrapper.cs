#nullable enable
using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Blackboard class (variable storage system).
    /// Provides safe access to Blackboard variables used for quest/dialog state management.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::Blackboard"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\Blackboard.cs — tous les membres
    /// existent typés, y compris TryGetValue(out Value) et DefaultSet&lt;T&gt;.
    ///
    /// Rappel scope : les règles YAML MISSION lisent Universe.blackboardMain (« main.X »),
    /// pas le blackboard de faction — voir /per-aspera-wrappers-sdk.
    /// </summary>
    public class BlackBoardWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native blackboard (compat). Prefer the typed overload.</summary>
        public BlackBoardWrapper(object nativeBlackBoard) : base(nativeBlackBoard) { }

        /// <summary>Wraps a typed interop Blackboard proxy.</summary>
        public BlackBoardWrapper(Blackboard nativeBlackBoard) : base(nativeBlackBoard) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>bb.NativeBlackboard?.SetValue("flag", true);</example>
        public Blackboard? NativeBlackboard => base.GetNativeObject() as Blackboard;

        /// <summary>
        /// Get the native blackboard object (safely typed).
        /// Conservé pour compatibilité — équivalent à <see cref="NativeBlackboard"/>.
        /// </summary>
        public new Blackboard? GetNativeObject() => NativeBlackboard;

        /// <summary>Name of this blackboard instance (typed read of Blackboard.name).</summary>
        public string? Name => NativeBlackboard?.name;

        // ==================== VALUE ACCESS METHODS ====================

        /// <summary>
        /// Try to get a value from the blackboard (typed call with out parameter).
        /// ⚠️ L'ancienne version passait par MakeGenericMethod/Invoke — le paramètre out
        /// ne remontait pas correctement à travers la réflexion IL2CPP.
        /// </summary>
        /// <example>if (bb.TryGetValue("mon_flag", out var v)) { ... }</example>
        public bool TryGetValue(string variableName, out object? value)
        {
            value = null;
            var bb = NativeBlackboard;
            if (bb == null) return false;
            var found = bb.TryGetValue(variableName, out var nativeValue);
            if (found) value = nativeValue;
            return found;
        }

        /// <summary>Get a value from the blackboard (typed — returns the native Value).</summary>
        public object? GetValue(string variableName)
            => NativeBlackboard?.GetValue(variableName);

        /// <summary>Check if the blackboard contains a specific key (typed).</summary>
        public bool ContainsKey(string variableName)
            => NativeBlackboard?.ContainsKey(variableName) ?? false;

        // ==================== VALUE SETTING METHODS ====================

        /// <summary>Set a string value (typed Blackboard.SetValue overload).</summary>
        /// <example>bb.SetValue("mon_texte", "bonjour");</example>
        public void SetValue(string variableName, string stringValue)
            => NativeBlackboard?.SetValue(variableName, stringValue);

        /// <summary>Set a float value (typed Blackboard.SetValue overload).</summary>
        /// <example>bb.SetValue("mon_compteur", 42f);</example>
        public void SetValue(string variableName, float floatValue)
            => NativeBlackboard?.SetValue(variableName, floatValue);

        /// <summary>Set a boolean value (typed Blackboard.SetValue overload).</summary>
        /// <example>bb.SetValue("mon_flag", true);</example>
        public void SetValue(string variableName, bool boolValue)
            => NativeBlackboard?.SetValue(variableName, boolValue);

        /// <summary>Set a number value via the legacy native SetNumber (typed).</summary>
        public void SetNumber(string variableName, float number)
            => NativeBlackboard?.SetNumber(variableName, number);

        /// <summary>Get a number value via the legacy native GetNumber (typed).</summary>
        public float GetNumber(string variableName)
            => NativeBlackboard?.GetNumber(variableName) ?? 0f;

        // ==================== COLLECTION METHODS ====================

        /// <summary>All variable keys in this blackboard (typed read).</summary>
        public IList<string> GetKeys()
        {
            var result = new List<string>();
            var keys = NativeBlackboard?.GetKeys();
            if (keys == null) return result;
            foreach (var k in keys) result.Add(k);
            return result;
        }

        /// <summary>All dynamic variable keys in this blackboard (typed read).</summary>
        public IList<string> GetDynamicKeys()
        {
            var result = new List<string>();
            var keys = NativeBlackboard?.GetDynamicKeys();
            if (keys == null) return result;
            foreach (var k in keys) result.Add(k);
            return result;
        }

        // ==================== UTILITY METHODS ====================

        /// <summary>Increment a numeric variable by the specified amount (typed).</summary>
        public void Increment(string variableName, float amount)
            => NativeBlackboard?.Increment(variableName, amount);

        /// <summary>Clear all variables from this blackboard (typed).</summary>
        public void Clear() => NativeBlackboard?.Clear();

        /// <summary>Set a default string value if the variable doesn't exist (typed generic).</summary>
        public void DefaultSetString(string variableName, string defaultValue)
            => NativeBlackboard?.DefaultSet(variableName, defaultValue);

        /// <summary>Set a default float value if the variable doesn't exist (typed generic).</summary>
        public void DefaultSetFloat(string variableName, float defaultValue)
            => NativeBlackboard?.DefaultSet(variableName, defaultValue);

        /// <summary>Set a default bool value if the variable doesn't exist (typed generic).</summary>
        public void DefaultSetBool(string variableName, bool defaultValue)
            => NativeBlackboard?.DefaultSet(variableName, defaultValue);

        // ==================== DEBUGGING & INFO ====================

        /// <summary>Debug-friendly string representation of this blackboard.</summary>
        public override string ToString()
        {
            var name = Name ?? "Unknown";
            var keyCount = GetKeys().Count;
            var dynamicKeyCount = GetDynamicKeys().Count;
            return $"BlackBoard [{name}]: {keyCount} variables, {dynamicKeyCount} dynamic";
        }

        /// <summary>Detailed debug information about this blackboard.</summary>
        public string GetDebugInfo()
        {
            var info = $"BlackBoard '{Name ?? "Unknown"}':\n";

            var keys = GetKeys();
            if (keys.Count > 0)
            {
                info += "  Variables:\n";
                foreach (var key in keys)
                    info += $"    {key} = {GetValue(key)}\n";
            }

            var dynamicKeys = GetDynamicKeys();
            if (dynamicKeys.Count > 0)
            {
                info += "  Dynamic Variables:\n";
                foreach (var key in dynamicKeys)
                    info += $"    {key} (dynamic)\n";
            }

            return info;
        }
    }
}
