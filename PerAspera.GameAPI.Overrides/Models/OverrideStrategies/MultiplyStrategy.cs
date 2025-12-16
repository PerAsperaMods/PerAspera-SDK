using System;

namespace PerAspera.GameAPI.Overrides.Models.OverrideStrategies
{
    /// <summary>
    /// Multiply strategy - multiplies original value by override value
    /// Useful for scaling energy production, resource rates, etc.
    /// Only works with numeric types
    /// </summary>
    public class MultiplyStrategy : IOverrideStrategy<float>
    {
        public string Description => "Multiply original value by multiplier";

        public float Apply(float originalValue, GetterOverride<float> overrideConfig, object? instance = null)
        {
            return originalValue * overrideConfig.CurrentValue;
        }

        public bool CanApply(GetterOverride<float> overrideConfig)
        {
            return true;
        }
    }

    /// <summary>
    /// Integer version of multiply strategy
    /// </summary>
    public class MultiplyStrategyInt : IOverrideStrategy<int>
    {
        public string Description => "Multiply original value by multiplier (integer)";

        public int Apply(int originalValue, GetterOverride<int> overrideConfig, object? instance = null)
        {
            return originalValue * overrideConfig.CurrentValue;
        }

        public bool CanApply(GetterOverride<int> overrideConfig)
        {
            return true;
        }
    }
}
