using System;

namespace PerAspera.GameAPI.Overrides.Patching
{
    /// <summary>
    /// Attribute to mark classes for automatic override patch discovery
    /// Enables reflection-based automatic patching without hardcoding
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AutoOverridePatchAttribute : Attribute
    {
        /// <summary>
        /// Class name to patch (e.g., "Planet", "Building")
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Method name to patch (e.g., "GetAtmosphericPressure")
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Category for grouping (e.g., "Climate", "Energy")
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// Priority for patch application (higher = earlier)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Whether this patch is enabled by default
        /// </summary>
        public bool EnabledByDefault { get; set; } = true;

        public AutoOverridePatchAttribute(string className, string methodName)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Class name cannot be empty", nameof(className));
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be empty", nameof(methodName));

            ClassName = className;
            MethodName = methodName;
        }

        public override string ToString()
        {
            return $"[AutoOverridePatch] {ClassName}.{MethodName} (Category: {Category}, Priority: {Priority})";
        }
    }

    /// <summary>
    /// Attribute to mark individual patch methods for metadata
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OverridePatchMethodAttribute : Attribute
    {
        /// <summary>
        /// Description of what this patch does
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Return type of the method being patched
        /// </summary>
        public global::System.Type? ReturnType { get; set; }
    }
}
