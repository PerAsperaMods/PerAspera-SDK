using System;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Overrides.Validation
{
    /// <summary>
    /// Runtime validation of override compatibility with target methods
    /// Checks if the override type matches the method return type
    /// </summary>
    public static class TypeCompatibilityChecker
    {
        private static readonly LogAspera Log = new LogAspera("Overrides.Validation");

        /// <summary>
        /// Check if an override is compatible with a target method
        /// </summary>
        public static bool IsCompatible(global::System.Type targetType, string methodName, global::System.Type overrideType)
        {
            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                if (method == null)
                {
                    Log.Warning($"Method not found: {targetType.Name}.{methodName}");
                    return false;
                }

                var returnType = method.ReturnType;

                // Exact match
                if (returnType == overrideType)
                    return true;

                // Nullable value types
                if (Nullable.GetUnderlyingType(returnType) == overrideType)
                    return true;

                // Assignable (inheritance/interface)
                if (returnType.IsAssignableFrom(overrideType))
                    return true;

                Log.Warning($"Type mismatch: {targetType.Name}.{methodName} returns {returnType.Name}, override is {overrideType.Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error checking compatibility: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate override configuration at registration time
        /// </summary>
        public static ValidationResult ValidateOverride(string className, string methodName, global::System.Type overrideType)
        {
            try
            {
                // Try to find the class type
                var classType = global::System.Type.GetType(className);

                if (classType == null)
                {
                    // Try searching in loaded assemblies
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        classType = assembly.GetType(className);
                        if (classType != null) break;
                    }
                }

                if (classType == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Class not found: {className}",
                        WarningLevel = WarningLevel.Error
                    };
                }

                if (!IsCompatible(classType, methodName, overrideType))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Type mismatch for {className}.{methodName}",
                        WarningLevel = WarningLevel.Error
                    };
                }

                return new ValidationResult
                {
                    IsValid = true,
                    WarningLevel = WarningLevel.None
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Validation error: {ex.Message}",
                    WarningLevel = WarningLevel.Warning
                };
            }
        }
    }

    /// <summary>
    /// Result of validation check
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public WarningLevel WarningLevel { get; set; }

        public override string ToString()
        {
            return IsValid ? "✅ Valid" : $"❌ {ErrorMessage} [{WarningLevel}]";
        }
    }

    /// <summary>
    /// Warning levels for validation
    /// </summary>
    public enum WarningLevel
    {
        None,
        Info,
        Warning,
        Error
    }
}
