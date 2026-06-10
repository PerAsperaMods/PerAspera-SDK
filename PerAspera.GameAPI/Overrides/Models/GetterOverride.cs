using System;

namespace PerAspera.GameAPI.Overrides.Models
{
    /// <summary>
    /// Generic getter override configuration for any type
    /// Supports type-safe value overrides with validation and events
    /// </summary>
    /// <typeparam name="T">The return type of the getter method to override</typeparam>
    public class GetterOverride<T>
    {
        private T _currentValue;
        private bool _isEnabled;

        /// <summary>
        /// Event raised when the override value changes
        /// </summary>
        public event EventHandler<OverrideValueChangedEventArgs<T>>? ValueChanged;

        /// <summary>
        /// Event raised when the enabled state changes
        /// </summary>
        public event EventHandler<OverrideEnabledChangedEventArgs>? EnabledChanged;

        /// <summary>
        /// Unique key identifying this override (ClassName.MethodName)
        /// </summary>
        public string Key => $"{ClassName}.{MethodName}";

        /// <summary>
        /// Display name for UI/logging
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Class name of the method to override
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Method name to override
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Current override value (used when enabled)
        /// </summary>
        public T CurrentValue
        {
            get => _currentValue;
            private set
            {
                if (Equals(_currentValue, value)) return;

                var oldValue = _currentValue;
                _currentValue = value;
                ValueChanged?.Invoke(this, new OverrideValueChangedEventArgs<T>(oldValue, value));
            }
        }

        /// <summary>
        /// Default value (fallback when override is disabled)
        /// </summary>
        public T DefaultValue { get; }

        /// <summary>
        /// Whether this override is currently active
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            private set
            {
                if (_isEnabled == value) return;

                var oldState = _isEnabled;
                _isEnabled = value;
                EnabledChanged?.Invoke(this, new OverrideEnabledChangedEventArgs(oldState, value));
            }
        }

        /// <summary>
        /// Effective value (CurrentValue if enabled, DefaultValue otherwise)
        /// </summary>
        public T EffectiveValue => IsEnabled ? CurrentValue : DefaultValue;

        /// <summary>
        /// Category for grouping overrides (e.g., "Climate", "Energy", "Buildings")
        /// </summary>
        public string Category { get; set; } = "General";

        /// <summary>
        /// Optional description for documentation
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional units (e.g., "°C", "mbar", "MW")
        /// </summary>
        public string? Units { get; set; }

        /// <summary>
        /// Custom validation function (return false to reject value)
        /// </summary>
        public Func<T, bool>? Validator { get; set; }

        /// <summary>
        /// Override strategy (Replace, Multiply, Clamp, etc.)
        /// </summary>
        public IOverrideStrategy<T>? Strategy { get; set; }

        /// <summary>
        /// Metadata storage for custom properties
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> Metadata { get; } = new();

        /// <summary>
        /// Create a new getter override
        /// </summary>
        public GetterOverride(
            string className,
            string methodName,
            string displayName,
            T defaultValue,
            IOverrideStrategy<T>? strategy = null)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentException("Class name cannot be empty", nameof(className));
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("Method name cannot be empty", nameof(methodName));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name cannot be empty", nameof(displayName));

            ClassName = className;
            MethodName = methodName;
            DisplayName = displayName;
            DefaultValue = defaultValue;
            _currentValue = defaultValue;
            _isEnabled = false;
            Strategy = strategy;
        }

        /// <summary>
        /// Set the override value (with optional validation)
        /// </summary>
        public void SetValue(T value)
        {
            if (Validator != null && !Validator(value))
            {
                throw new ArgumentException($"Validation failed for override {Key}: {value}");
            }

            CurrentValue = value;
        }

        /// <summary>
        /// Enable or disable this override
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }

        /// <summary>
        /// Reset to default value and disable
        /// </summary>
        public void Reset()
        {
            CurrentValue = DefaultValue;
            IsEnabled = false;
        }

        /// <summary>
        /// Apply the override strategy to an original value
        /// </summary>
        public T ApplyStrategy(T originalValue, object? instance = null)
        {
            if (!IsEnabled)
                return originalValue;

            if (Strategy != null && Strategy.CanApply(this))
                return Strategy.Apply(originalValue, this, instance);

            // Default strategy: simple replacement
            return CurrentValue;
        }

        public override string ToString()
        {
            var status = IsEnabled ? "ON" : "OFF";
            var value = IsEnabled ? CurrentValue?.ToString() : DefaultValue?.ToString();
            var units = Units != null ? $" {Units}" : "";
            return $"[{status}] {DisplayName}: {value}{units}";
        }
    }

    /// <summary>
    /// Event args for value changes
    /// </summary>
    public class OverrideValueChangedEventArgs<T>
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public OverrideValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event args for enabled state changes
    /// </summary>
    public class OverrideEnabledChangedEventArgs
    {
        public bool OldState { get; }
        public bool NewState { get; }

        public OverrideEnabledChangedEventArgs(bool oldState, bool newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
