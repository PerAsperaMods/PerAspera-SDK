using System;
using System.Collections.Generic;
using System.Globalization;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.Builders
{
    /// <summary>
    /// Type-safe parameter builder with validation and conversion for all Per Aspera types
    /// Provides fluent API for building and validating command parameters
    /// </summary>
    public class ParameterBuilder
    {
        private readonly Dictionary<string, object> _parameters;
        private readonly Dictionary<string, Func<object, bool>> _validators;
        private readonly Dictionary<string, Func<object, object>> _converters;
        
        public ParameterBuilder()
        {
            _parameters = new Dictionary<string, object>();
            _validators = new Dictionary<string, Func<object, bool>>();
            _converters = new Dictionary<string, Func<object, object>>();
            InitializeBuiltInValidators();
            InitializeBuiltInConverters();
        }
        
        /// <summary>
        /// Add a parameter with automatic type validation and conversion
        /// </summary>
        public ParameterBuilder Add(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));
            
            // Apply converter if available
            if (_converters.TryGetValue(name, out var converter))
            {
                try
                {
                    value = converter(value);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Failed to convert parameter '{name}' value '{value}': {ex.Message}", ex);
                }
            }
            
            // Apply validator if available
            if (_validators.TryGetValue(name, out var validator))
            {
                if (!validator(value))
                {
                    throw new ArgumentException($"Parameter '{name}' value '{value}' failed validation");
                }
            }
            
            _parameters[name] = value;
            return this;
        }
        
        /// <summary>
        /// Add resource parameter with validation
        /// </summary>
        public ParameterBuilder Resource(object resource)
        {
            return Add(ParameterNames.Resource, resource);
        }
        
        /// <summary>
        /// Add quantity parameter with validation (must be positive)
        /// </summary>
        public ParameterBuilder Quantity(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            return Add(ParameterNames.Quantity, quantity);
        }
        
        /// <summary>
        /// Add building parameter with validation
        /// </summary>
        public ParameterBuilder Building(object building)
        {
            return Add(ParameterNames.Building, building);
        }
        
        /// <summary>
        /// Add technology parameter with validation
        /// </summary>
        public ParameterBuilder Technology(object technology)
        {
            return Add(ParameterNames.Technology, technology);
        }
        
        /// <summary>
        /// Add knowledge parameter with validation
        /// </summary>
        public ParameterBuilder Knowledge(object knowledge)
        {
            return Add(ParameterNames.Knowledge, knowledge);
        }
        
        /// <summary>
        /// Add person parameter with validation
        /// </summary>
        public ParameterBuilder Person(object person)
        {
            return Add(ParameterNames.Person, person);
        }
        
        /// <summary>
        /// Add dialogue parameter with validation
        /// </summary>
        public ParameterBuilder Dialogue(object dialogue)
        {
            return Add(ParameterNames.Dialogue, dialogue);
        }
        
        /// <summary>
        /// Add position parameters (X, Y, Z) with validation
        /// </summary>
        public ParameterBuilder Position(float x, float y, float z)
        {
            return Add(ParameterNames.X, x)
                  .Add(ParameterNames.Y, y)
                  .Add(ParameterNames.Z, z);
        }
        
        /// <summary>
        /// Add position from Vector3-like object
        /// </summary>
        public ParameterBuilder Position(object vector3)
        {
            if (vector3 == null)
                throw new ArgumentNullException(nameof(vector3));
                
            // Try to extract X, Y, Z from vector3 object using reflection
            var type = vector3.GetType();
            var xProp = type.GetProperty("x") ?? type.GetProperty("X");
            var yProp = type.GetProperty("y") ?? type.GetProperty("Y");
            var zProp = type.GetProperty("z") ?? type.GetProperty("Z");
            
            if (xProp != null && yProp != null && zProp != null)
            {
                var x = Convert.ToSingle(xProp.GetValue(vector3));
                var y = Convert.ToSingle(yProp.GetValue(vector3));
                var z = Convert.ToSingle(zProp.GetValue(vector3));
                return Position(x, y, z);
            }
            
            throw new ArgumentException("Vector3 object must have x, y, z or X, Y, Z properties", nameof(vector3));
        }
        
        /// <summary>
        /// Add key-value pair for generic parameters
        /// </summary>
        public ParameterBuilder KeyValue(string key, object value)
        {
            return Add(ParameterNames.Key, key)
                  .Add(ParameterNames.Value, value);
        }
        
        /// <summary>
        /// Add amount parameter with validation (must be non-negative)
        /// </summary>
        public ParameterBuilder Amount(float amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            return Add(ParameterNames.Amount, amount);
        }
        
        /// <summary>
        /// Add duration parameter with validation (must be positive)
        /// </summary>
        public ParameterBuilder Duration(float duration)
        {
            if (duration <= 0)
                throw new ArgumentException("Duration must be positive", nameof(duration));
            return Add(ParameterNames.Duration, duration);
        }
        
        /// <summary>
        /// Add message parameter with validation (must not be empty)
        /// </summary>
        public ParameterBuilder Message(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            return Add(ParameterNames.Message, message);
        }
        
        /// <summary>
        /// Add custom validator for a parameter
        /// </summary>
        public ParameterBuilder WithValidator(string parameterName, Func<object, bool> validator)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
                
            _validators[parameterName] = validator;
            return this;
        }
        
        /// <summary>
        /// Add custom converter for a parameter
        /// </summary>
        public ParameterBuilder WithConverter(string parameterName, Func<object, object> converter)
        {
            if (string.IsNullOrEmpty(parameterName))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));
                
            _converters[parameterName] = converter;
            return this;
        }
        
        /// <summary>
        /// Check if parameter exists
        /// </summary>
        public bool HasParameter(string name)
        {
            return _parameters.ContainsKey(name);
        }
        
        /// <summary>
        /// Get parameter value
        /// </summary>
        public T GetParameter<T>(string name)
        {
            if (!_parameters.TryGetValue(name, out var value))
                throw new KeyNotFoundException($"Parameter '{name}' not found");
                
            if (value is T typedValue)
                return typedValue;
                
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Cannot convert parameter '{name}' value '{value}' to type {typeof(T).Name}", ex);
            }
        }
        
        /// <summary>
        /// Get all parameters as dictionary
        /// </summary>
        public Dictionary<string, object> Build()
        {
            return new Dictionary<string, object>(_parameters);
        }
        
        /// <summary>
        /// Clear all parameters
        /// </summary>
        public ParameterBuilder Clear()
        {
            _parameters.Clear();
            return this;
        }
        
        /// <summary>
        /// Get parameter count
        /// </summary>
        public int Count => _parameters.Count;
        
        /// <summary>
        /// Validate all parameters against their registered validators
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();
            
            foreach (var kvp in _parameters)
            {
                if (_validators.TryGetValue(kvp.Key, out var validator))
                {
                    try
                    {
                        if (!validator(kvp.Value))
                        {
                            errors.Add($"Parameter '{kvp.Key}' value '{kvp.Value}' failed validation");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Validation error for parameter '{kvp.Key}': {ex.Message}");
                    }
                }
            }
            
            return errors.Count == 0;
        }
        
        private void InitializeBuiltInValidators()
        {
            // Quantity must be positive
            _validators[ParameterNames.Quantity] = value => value is int qty && qty > 0;
            
            // Amount must be non-negative
            _validators[ParameterNames.Amount] = value => value is float amt && amt >= 0;
            
            // Duration must be positive
            _validators[ParameterNames.Duration] = value => value is float dur && dur > 0;
            
            // Message cannot be null or empty
            _validators[ParameterNames.Message] = value => value is string msg && !string.IsNullOrWhiteSpace(msg);
            
            // Key cannot be null or empty
            _validators[ParameterNames.Key] = value => value is string key && !string.IsNullOrWhiteSpace(key);
            
            // Coordinates must be finite numbers
            _validators[ParameterNames.X] = value => value is float x && float.IsFinite(x);
            _validators[ParameterNames.Y] = value => value is float y && float.IsFinite(y);
            _validators[ParameterNames.Z] = value => value is float z && float.IsFinite(z);
        }
        
        private void InitializeBuiltInConverters()
        {
            // String to int conversion for quantity
            _converters[ParameterNames.Quantity] = value =>
            {
                if (value is string str && int.TryParse(str, out var result))
                    return result;
                if (value is int || value is long || value is short)
                    return Convert.ToInt32(value);
                return value;
            };
            
            // String to float conversion for amounts and coordinates
            var floatConverter = new Func<object, object>(value =>
            {
                if (value is string str && float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
                if (value is double || value is decimal || value is int)
                    return Convert.ToSingle(value);
                return value;
            });
            
            _converters[ParameterNames.Amount] = floatConverter;
            _converters[ParameterNames.Duration] = floatConverter;
            _converters[ParameterNames.X] = floatConverter;
            _converters[ParameterNames.Y] = floatConverter;
            _converters[ParameterNames.Z] = floatConverter;
            
            // String trimming for text parameters
            var stringTrimmer = new Func<object, object>(value => 
                value is string str ? str.Trim() : value);
                
            _converters[ParameterNames.Message] = stringTrimmer;
            _converters[ParameterNames.Key] = stringTrimmer;
        }
    }
}