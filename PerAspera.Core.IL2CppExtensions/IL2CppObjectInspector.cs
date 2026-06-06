using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Debug tool to inspect IL2CPP objects and discover their actual properties/fields
    /// Useful for finding where data actually lives when reflection patterns fail
    /// </summary>
    public static class IL2CppObjectInspector
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("IL2CppObjectInspector");

        /// <summary>
        /// Dump all properties and fields of an IL2CPP object
        /// Helps debug why IL2CppPropertyReader can't find something
        /// </summary>
        public static void DumpObject(object instance, string objectName = null)
        {
            if (instance == null)
            {
                Log.LogWarning("Instance is NULL");
                return;
            }

            var type = instance.GetType();
            objectName = objectName ?? type.Name;

            Log.LogInfo($"=== INSPECTING {objectName} ({type.FullName}) ===");

            // Dump all properties
            Log.LogInfo("PROPERTIES:");
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in properties.OrderBy(p => p.Name))
            {
                try
                {
                    var value = prop.GetValue(instance);
                    var valueStr = value?.GetType().Name ?? "null";
                    Log.LogInfo($"  {prop.Name}: {prop.PropertyType.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"  {prop.Name}: ERROR - {ex.GetType().Name}");
                }
            }

            // Dump all fields
            Log.LogInfo("FIELDS:");
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields.OrderBy(f => f.Name))
            {
                try
                {
                    var value = field.GetValue(instance);
                    var valueStr = value?.GetType().Name ?? "null";
                    Log.LogInfo($"  {field.Name}: {field.FieldType.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"  {field.Name}: ERROR - {ex.GetType().Name}");
                }
            }

            Log.LogInfo($"=== END {objectName} ===");
        }

        /// <summary>
        /// Find a property/field by searching for a substring (case-insensitive)
        /// Useful when you don't know the exact name
        /// </summary>
        public static List<string> FindMembersByName(object instance, string searchTerm)
        {
            if (instance == null) return new List<string>();

            var type = instance.GetType();
            var results = new List<string>();

            // Search properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add($"Property: {prop.Name} ({prop.PropertyType.Name})");
                }
            }

            // Search fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add($"Field: {field.Name} ({field.FieldType.Name})");
                }
            }

            if (results.Count > 0)
            {
                Log.LogInfo($"Found {results.Count} members matching '{searchTerm}':");
                foreach (var result in results)
                {
                    Log.LogInfo($"  - {result}");
                }
            }
            else
            {
                Log.LogWarning($"No members found matching '{searchTerm}'");
            }

            return results;
        }

        /// <summary>
        /// Get value of any member (property or field) safely
        /// Returns default if not found
        /// </summary>
        public static T? SafeGetValue<T>(object instance, string memberName)
        {
            if (instance == null)
                return default;

            var type = instance.GetType();

            // Try property first
            try
            {
                var prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null && prop.CanRead)
                {
                    return (T?)prop.GetValue(instance);
                }
            }
            catch { }

            // Try field
            try
            {
                var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (field != null)
                {
                    return (T?)field.GetValue(instance);
                }
            }
            catch { }

            return default;
        }

        /// <summary>
        /// List all public methods (useful for finding getter methods)
        /// </summary>
        public static void ListMethods(object instance)
        {
            if (instance == null) return;

            var type = instance.GetType();
            Log.LogInfo($"METHODS on {type.Name}:");

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods.OrderBy(m => m.Name))
            {
                var paramStr = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
                Log.LogInfo($"  {method.ReturnType.Name} {method.Name}({paramStr})");
            }
        }
    }
}
