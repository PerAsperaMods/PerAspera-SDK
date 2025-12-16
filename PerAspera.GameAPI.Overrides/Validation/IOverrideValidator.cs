using System;

namespace PerAspera.GameAPI.Overrides.Validation
{
    /// <summary>
    /// Interface for validating override configurations
    /// Allows custom validation logic for different types and use cases
    /// </summary>
    public interface IOverrideValidator<T>
    {
        /// <summary>
        /// Validate a value before it's set as an override
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if valid, false otherwise</returns>
        bool Validate(T value, out string? errorMessage);

        /// <summary>
        /// Description of validation rules
        /// </summary>
        string ValidationDescription { get; }
    }

    /// <summary>
    /// Validator for numeric ranges
    /// </summary>
    public class RangeValidator<T> : IOverrideValidator<T> where T : IComparable<T>
    {
        private readonly T _min;
        private readonly T _max;

        public string ValidationDescription => $"Value must be between {_min} and {_max}";

        public RangeValidator(T min, T max)
        {
            if (min.CompareTo(max) > 0)
                throw new ArgumentException("Min cannot be greater than max");

            _min = min;
            _max = max;
        }

        public bool Validate(T value, out string? errorMessage)
        {
            if (value.CompareTo(_min) < 0)
            {
                errorMessage = $"Value {value} is below minimum {_min}";
                return false;
            }

            if (value.CompareTo(_max) > 0)
            {
                errorMessage = $"Value {value} exceeds maximum {_max}";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Validator for positive numbers
    /// </summary>
    public class PositiveValidator : IOverrideValidator<float>
    {
        private readonly bool _allowZero;

        public string ValidationDescription => _allowZero
            ? "Value must be zero or positive"
            : "Value must be positive (> 0)";

        public PositiveValidator(bool allowZero = true)
        {
            _allowZero = allowZero;
        }

        public bool Validate(float value, out string? errorMessage)
        {
            var isValid = _allowZero ? value >= 0 : value > 0;

            if (!isValid)
            {
                errorMessage = $"Value {value} must be {(_allowZero ? ">= 0" : "> 0")}";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }

    /// <summary>
    /// Validator for boolean values (always passes, for consistency)
    /// </summary>
    public class BooleanValidator : IOverrideValidator<bool>
    {
        public string ValidationDescription => "Any boolean value is valid";

        public bool Validate(bool value, out string? errorMessage)
        {
            errorMessage = null;
            return true;
        }
    }
}
