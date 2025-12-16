using System;
using System.Linq;
using System.Reflection;
using PerAspera.Core.IL2CPP;

namespace PerAspera.Core
{
    /// <summary>
    /// Helper for creating and manipulating CargoQuantity objects
    /// Provides safe IL2CPP access to the game's cargo/resource quantity system
    /// </summary>
    public static class CargoQuantityHelper
    {
        private static System.Type? _cargoType;
        private static MethodInfo? _fromUnitFloat;
        private static MethodInfo? _fromMilli;
        private static MethodInfo? _toFloat;
        private static MethodInfo? _toMilli;
        private static MethodInfo? _toString;
        private static PropertyInfo? _emptyProperty;
        private static bool _isInitialized = false;
        private static readonly object _lock = new object();

        /// <summary>
        /// Check if CargoQuantity system is available
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                return _cargoType != null;
            }
        }

        /// <summary>
        /// Initialize the CargoQuantity reflection bindings
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_isInitialized)
                return;

            lock (_lock)
            {
                if (_isInitialized)
                    return;

                try
                {
                    LocalLogDebug("CargoQuantityHelper", "Initializing CargoQuantity reflection bindings...");

                    // Find CargoQuantity type
                    _cargoType = ReflectionHelpers.FindType("CargoQuantity");
                    if (_cargoType == null)
                    {
                        LocalLogError("CargoQuantityHelper", "CargoQuantity type not found");
                        _isInitialized = true;
                        return;
                    }

                    LocalLogDebug("CargoQuantityHelper", $"Found CargoQuantity type: {_cargoType.FullName}");

                    // Find factory methods
                    _fromUnitFloat = _cargoType.GetMethod("FromUnitFloat", BindingFlags.Public | BindingFlags.Static);
                    _fromMilli = _cargoType.GetMethod("FromMilli", BindingFlags.Public | BindingFlags.Static);

                    // Find conversion methods
                    _toFloat = _cargoType.GetMethod("ToFloat", BindingFlags.Public | BindingFlags.Instance);
                    _toMilli = _cargoType.GetMethod("ToMilli", BindingFlags.Public | BindingFlags.Instance);

                    // Find ToString method
                    _toString = _cargoType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);

                    // Find Empty property
                    _emptyProperty = _cargoType.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);

                    LocalLogDebug("CargoQuantityHelper", $"Reflection setup complete: FromUnitFloat={_fromUnitFloat != null}, ToFloat={_toFloat != null}");
                }
                catch (Exception ex)
                {
                    LocalLogError("CargoQuantityHelper", $"Initialization failed: {ex.Message}");
                }
                finally
                {
                    _isInitialized = true;
                }
            }
        }

        /// <summary>
        /// Create CargoQuantity from float units
        /// </summary>
        /// <param name="units">Amount in float units</param>
        /// <returns>CargoQuantity instance or null</returns>
        public static object? CreateFromUnits(float units)
        {
            EnsureInitialized();
            if (_cargoType == null || _fromUnitFloat == null)
            {
                LocalLogError("CargoQuantityHelper", "CargoQuantity system not available for CreateFromUnits");
                return null;
            }

            try
            {
                var cargo = _fromUnitFloat.Invoke(null, new object[] { units });
                LocalLogDebug("CargoQuantityHelper", $"Created CargoQuantity from {units} units");
                return cargo;
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"CreateFromUnits({units}) failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create CargoQuantity from milli units
        /// </summary>
        /// <param name="milli">Amount in milli units</param>
        /// <returns>CargoQuantity instance or null</returns>
        public static object? CreateFromMilli(long milli)
        {
            EnsureInitialized();
            if (_cargoType == null || _fromMilli == null)
            {
                LocalLogError("CargoQuantityHelper", "CargoQuantity system not available for CreateFromMilli");
                return null;
            }

            try
            {
                // Check parameter type and convert accordingly
                var paramType = _fromMilli.GetParameters().FirstOrDefault()?.ParameterType;
                object param = paramType == typeof(int) ? (int)milli : milli;

                var cargo = _fromMilli.Invoke(null, new object[] { param });
                LocalLogDebug("CargoQuantityHelper", $"Created CargoQuantity from {milli} milli");
                return cargo;
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"CreateFromMilli({milli}) failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get empty CargoQuantity instance
        /// </summary>
        /// <returns>Empty CargoQuantity or null</returns>
        public static object? GetEmpty()
        {
            EnsureInitialized();
            if (_cargoType == null || _emptyProperty == null)
            {
                LocalLogError("CargoQuantityHelper", "CargoQuantity system not available for GetEmpty");
                return null;
            }

            try
            {
                return _emptyProperty.GetValue(null);
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"GetEmpty() failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to convert CargoQuantity to float
        /// </summary>
        /// <param name="cargoObj">CargoQuantity instance</param>
        /// <param name="value">Output float value</param>
        /// <returns>True if successful</returns>
        public static bool TryToFloat(object cargoObj, out float value)
        {
            value = 0f;
            EnsureInitialized();

            if (_cargoType == null || cargoObj == null || _toFloat == null)
                return false;

            try
            {
                var result = _toFloat.Invoke(cargoObj, System.Array.Empty<object>());
                if (result != null)
                {
                    value = Utilities.ToFloat(result, 0f);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"TryToFloat failed: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Try to convert CargoQuantity to milli units
        /// </summary>
        /// <param name="cargoObj">CargoQuantity instance</param>
        /// <param name="milli">Output milli value</param>
        /// <returns>True if successful</returns>
        public static bool TryToMilli(object cargoObj, out long milli)
        {
            milli = 0;
            EnsureInitialized();

            if (_cargoType == null || cargoObj == null || _toMilli == null)
                return false;

            try
            {
                var result = _toMilli.Invoke(cargoObj, System.Array.Empty<object>());
                if (result != null)
                {
                    if (result is long l)
                    {
                        milli = l;
                        return true;
                    }
                    if (result is int i)
                    {
                        milli = i;
                        return true;
                    }
                    if (long.TryParse(result.ToString(), out var parsed))
                    {
                        milli = parsed;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"TryToMilli failed: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Convert CargoQuantity to debug string
        /// </summary>
        /// <param name="cargoObj">CargoQuantity instance</param>
        /// <returns>String representation</returns>
        public static string ToDebugString(object? cargoObj)
        {
            if (cargoObj == null)
                return "<null>";

            EnsureInitialized();
            if (_cargoType == null)
                return "<no-cargo-type>";

            try
            {
                if (_toString != null)
                {
                    var result = _toString.Invoke(cargoObj, System.Array.Empty<object>());
                    return result?.ToString() ?? "<null-toString>";
                }

                // Fallback to basic ToString
                return cargoObj.ToString() ?? "<null-fallback>";
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"ToDebugString failed: {ex.Message}");
                return $"<error: {ex.Message}>";
            }
        }

        /// <summary>
        /// Check if an object is a CargoQuantity
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if it's a CargoQuantity</returns>
        public static bool IsCargoQuantity(object? obj)
        {
            if (obj == null)
                return false;

            EnsureInitialized();
            return _cargoType != null && _cargoType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Get CargoQuantity type information
        /// </summary>
        /// <returns>Type info or null</returns>
        public static System.Type? GetCargoQuantityType()
        {
            EnsureInitialized();
            return _cargoType;
        }

        /// <summary>
        /// Add two CargoQuantity objects if possible
        /// </summary>
        /// <param name="cargo1">First cargo</param>
        /// <param name="cargo2">Second cargo</param>
        /// <returns>Sum cargo or null</returns>
        public static object? Add(object? cargo1, object? cargo2)
        {
            if (cargo1 == null || cargo2 == null)
                return cargo1 ?? cargo2;

            EnsureInitialized();
            if (_cargoType == null)
                return null;

            try
            {
                // Try to find Add method or operator
                var addMethod = _cargoType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                if (addMethod != null)
                {
                    return addMethod.Invoke(cargo1, new object[] { cargo2 });
                }

                // Try operator+
                var opAdd = _cargoType.GetMethod("op_Addition", BindingFlags.Public | BindingFlags.Static);
                if (opAdd != null)
                {
                    return opAdd.Invoke(null, new object[] { cargo1, cargo2 });
                }

                // Fallback: convert to float, add, convert back
                if (TryToFloat(cargo1, out var f1) && TryToFloat(cargo2, out var f2))
                {
                    return CreateFromUnits(f1 + f2);
                }
            }
            catch (Exception ex)
            {
                LocalLogError("CargoQuantityHelper", $"Add failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Minimal logging without external dependencies
        /// </summary>
        private static void LocalLogError(string component, string message)
        {
            try
            {
                // Try LogAspera first if available
                LogAspera.LogError($"[{component}] {message}");
            }
            catch
            {
                // Fallback to console
                try
                {
                    global::System.Console.WriteLine($"[{component}] ERROR: {message}");
                }
                catch { /* swallow */ }
            }
        }

        /// <summary>
        /// Minimal debug logging
        /// </summary>
        private static void LocalLogDebug(string component, string message)
        {
            try
            {
                // Only log debug in debug builds
#if DEBUG
                LogAspera.LogDebug($"[{component}] {message}");
#endif
            }
            catch
            {
                // Swallow debug logging errors
            }
        }
    }
}
