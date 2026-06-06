using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Debug dumper for IL2CPP objects — writes complete structure to file
    /// Useful for understanding object layout when reflection strategies fail
    /// </summary>
    public static class IL2CppDebugDumper
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("IL2CppDebugDumper");
        private static readonly string DebugPath = Path.Combine(BepInEx.Paths.GameRootPath, "BepInEx", "Debug");

        static IL2CppDebugDumper()
        {
            if (!Directory.Exists(DebugPath))
                Directory.CreateDirectory(DebugPath);
        }

        /// <summary>
        /// Dump complete object structure to file
        /// Creates: BepInEx/Debug/IL2CPP-ObjectName-timestamp.txt
        /// </summary>
        public static string DumpObjectToFile(object instance, string objectName = null)
        {
            if (instance == null)
                return null;

            objectName = objectName ?? instance.GetType().Name;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filename = $"IL2CPP-{objectName}-{timestamp}.txt";
            var filePath = Path.Combine(DebugPath, filename);

            var sb = new StringBuilder();
            sb.AppendLine($"=== IL2CPP OBJECT DUMP ===");
            sb.AppendLine($"Object: {objectName}");
            sb.AppendLine($"Type: {instance.GetType().FullName}");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine();

            DumpProperties(instance, sb);
            sb.AppendLine();
            DumpFields(instance, sb);
            sb.AppendLine();
            DumpMethods(instance, sb);

            try
            {
                File.WriteAllText(filePath, sb.ToString());
                Log.LogInfo($"Dumped to: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to write dump: {ex.Message}");
                return null;
            }
        }

        private static void DumpProperties(object instance, StringBuilder sb)
        {
            sb.AppendLine("PROPERTIES:");
            var type = instance.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (props.Length == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var prop in props)
            {
                try
                {
                    var value = prop.GetValue(instance);
                    var valueStr = value?.GetType().Name ?? "null";
                    sb.AppendLine($"  {prop.Name}: {prop.PropertyType.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  {prop.Name}: {prop.PropertyType.Name} [ERROR: {ex.GetType().Name}]");
                }
            }
        }

        private static void DumpFields(object instance, StringBuilder sb)
        {
            sb.AppendLine("FIELDS:");
            var type = instance.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fields.Length == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(instance);
                    var valueStr = value?.GetType().Name ?? "null";
                    sb.AppendLine($"  {field.Name}: {field.FieldType.Name} = {valueStr}");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  {field.Name}: {field.FieldType.Name} [ERROR: {ex.GetType().Name}]");
                }
            }
        }

        private static void DumpMethods(object instance, StringBuilder sb)
        {
            sb.AppendLine("METHODS:");
            var type = instance.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (methods.Length == 0)
            {
                sb.AppendLine("  (none)");
                return;
            }

            foreach (var method in methods)
            {
                var paramStr = string.Join(", ", method.GetParameters());
                sb.AppendLine($"  {method.ReturnType.Name} {method.Name}({paramStr})");
            }
        }

        /// <summary>
        /// Search for member names matching a pattern and save to file
        /// Creates: BepInEx/Debug/IL2CPP-Search-pattern-timestamp.txt
        /// </summary>
        public static string FindMembersToFile(object instance, string searchTerm)
        {
            if (instance == null)
                return null;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filename = $"IL2CPP-Search-{searchTerm}-{timestamp}.txt";
            var filePath = Path.Combine(DebugPath, filename);

            var sb = new StringBuilder();
            sb.AppendLine($"=== IL2CPP MEMBER SEARCH ===");
            sb.AppendLine($"Type: {instance.GetType().FullName}");
            sb.AppendLine($"Search term: \"{searchTerm}\"");
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine();

            var type = instance.GetType();
            var found = new List<string>();

            // Search properties
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add($"Property: {prop.Name} ({prop.PropertyType.Name})");
                }
            }

            // Search fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add($"Field: {field.Name} ({field.FieldType.Name})");
                }
            }

            // Search methods
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add($"Method: {method.Name}");
                }
            }

            if (found.Count == 0)
            {
                sb.AppendLine($"No members found matching \"{searchTerm}\"");
            }
            else
            {
                sb.AppendLine($"Found {found.Count} member(s):");
                sb.AppendLine();
                foreach (var item in found)
                {
                    sb.AppendLine($"  - {item}");
                }
            }

            try
            {
                File.WriteAllText(filePath, sb.ToString());
                Log.LogInfo($"Search results saved to: {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to write search results: {ex.Message}");
                return null;
            }
        }
    }
}
