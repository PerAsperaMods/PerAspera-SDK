using System;

namespace PerAspera.GameAPI.Overrides.Models
{
    /// <summary>
    /// Strategy interface for applying override transformations
    /// Allows complex behaviors: replace, multiply, clamp, conditional, etc.
    /// </summary>
    /// <typeparam name="T">The type of value to override</typeparam>
    public interface IOverrideStrategy<T>
    {
        /// <summary>
        /// Apply the override transformation to the original value
        /// </summary>
        /// <param name="originalValue">The original value from the game method</param>
        /// <param name="overrideConfig">The override configuration containing settings</param>
        /// <param name="instance">The instance object (e.g., Planet, Building) for context-aware overrides</param>
        /// <returns>The transformed value to use</returns>
        T Apply(T originalValue, GetterOverride<T> overrideConfig, object? instance = null);

        /// <summary>
        /// Validate if this strategy can be applied to the given configuration
        /// </summary>
        bool CanApply(GetterOverride<T> overrideConfig);

        /// <summary>
        /// Human-readable description of what this strategy does
        /// </summary>
        string Description { get; }
    }
}
