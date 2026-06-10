using System;

namespace PerAspera.GameAPI.Overrides.Models.OverrideStrategies
{
    /// <summary>
    /// Clamp strategy - clamps value between min and max
    /// Override value is used as max, min is stored in metadata
    /// </summary>
    public class ClampStrategy : IOverrideStrategy<float>
    {
        private readonly float _minValue;
        private readonly float _maxValue;

        public string Description => $"Clamp value between {_minValue} and {_maxValue}";

        public ClampStrategy(float minValue, float maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("Min value cannot be greater than max value");

            _minValue = minValue;
            _maxValue = maxValue;
        }

        public float Apply(float originalValue, GetterOverride<float> overrideConfig, object? instance = null)
        {
            return Math.Clamp(originalValue, _minValue, _maxValue);
        }

        public bool CanApply(GetterOverride<float> overrideConfig)
        {
            return true;
        }
    }

    /// <summary>
    /// Integer version of clamp strategy
    /// </summary>
    public class ClampStrategyInt : IOverrideStrategy<int>
    {
        private readonly int _minValue;
        private readonly int _maxValue;

        public string Description => $"Clamp value between {_minValue} and {_maxValue}";

        public ClampStrategyInt(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("Min value cannot be greater than max value");

            _minValue = minValue;
            _maxValue = maxValue;
        }

        public int Apply(int originalValue, GetterOverride<int> overrideConfig, object? instance = null)
        {
            return Math.Clamp(originalValue, _minValue, _maxValue);
        }

        public bool CanApply(GetterOverride<int> overrideConfig)
        {
            return true;
        }
    }
}
