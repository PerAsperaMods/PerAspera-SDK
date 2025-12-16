namespace PerAspera.GameAPI.Overrides.Models.OverrideStrategies
{
    /// <summary>
    /// Simple replacement strategy - replaces original value with override value
    /// This is the default behavior when no strategy is specified
    /// </summary>
    public class ReplaceStrategy<T> : IOverrideStrategy<T>
    {
        public string Description => "Replace original value with override value";

        public T Apply(T originalValue, GetterOverride<T> overrideConfig, object? instance = null)
        {
            return overrideConfig.CurrentValue;
        }

        public bool CanApply(GetterOverride<T> overrideConfig)
        {
            return true; // Always applicable
        }
    }
}
