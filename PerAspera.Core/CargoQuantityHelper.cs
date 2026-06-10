using System;

namespace PerAspera.Core
{
    /// <summary>
    /// Helper for creating and manipulating CargoQuantity objects.
    /// Uses typed interop proxy access (publicized via BepInEx.AssemblyPublicizer).
    /// </summary>
    public static class CargoQuantityHelper
    {

        /// <summary>CargoQuantity system is always available via typed interop proxy.</summary>
        public static bool IsAvailable => true;

        /// <summary>
        /// Create CargoQuantity from float units.
        /// </summary>
        public static object? CreateFromUnits(float units)
        {
            try   { return CargoQuantity.FromUnitFloat(units); }
            catch (Exception ex) { LogAspera.LogError($"[CargoQuantityHelper] CreateFromUnits({units}) failed: {ex.Message}"); return null; }
        }

        /// <summary>
        /// Create CargoQuantity from milli units.
        /// </summary>
        public static object? CreateFromMilli(long milli)
        {
            try   { return CargoQuantity.FromMilli((int)milli); }
            catch (Exception ex) { LogAspera.LogError($"[CargoQuantityHelper] CreateFromMilli({milli}) failed: {ex.Message}"); return null; }
        }

        /// <summary>
        /// Get the empty CargoQuantity singleton.
        /// </summary>
        public static object? GetEmpty()
        {
            try   { return CargoQuantity.Empty; }
            catch (Exception ex) { LogAspera.LogError($"[CargoQuantityHelper] GetEmpty() failed: {ex.Message}"); return null; }
        }

        /// <summary>
        /// Convert a CargoQuantity to its float value.
        /// </summary>
        public static bool TryToFloat(object? cargoObj, out float value)
        {
            value = 0f;
            if (cargoObj is CargoQuantity cq) { value = cq.ToFloat(); return true; }
            return false;
        }

        /// <summary>
        /// Convert a CargoQuantity to its milli value.
        /// </summary>
        public static bool TryToMilli(object? cargoObj, out long milli)
        {
            milli = 0;
            if (cargoObj is CargoQuantity cq) { milli = cq.ToMilli(); return true; }
            return false;
        }

        /// <summary>
        /// Convert a CargoQuantity to a debug string.
        /// </summary>
        public static string ToDebugString(object? cargoObj)
        {
            if (cargoObj is CargoQuantity cq)
            {
                try { return cq.ToString() ?? "<null-toString>"; }
                catch { return "<ToString-failed>"; }
            }
            return cargoObj?.ToString() ?? "<null>";
        }

        /// <summary>Returns true when obj is a CargoQuantity.</summary>
        public static bool IsCargoQuantity(object? obj) => obj is CargoQuantity;

        /// <summary>Returns the CargoQuantity System.Type.</summary>
        public static System.Type GetCargoQuantityType() => typeof(CargoQuantity);

        /// <summary>
        /// Add two CargoQuantity objects using operator+.
        /// Falls back to float conversion if pattern match fails.
        /// </summary>
        public static object? Add(object? cargo1, object? cargo2)
        {
            if (cargo1 == null) return cargo2;
            if (cargo2 == null) return cargo1;
            try
            {
                if (cargo1 is CargoQuantity c1 && cargo2 is CargoQuantity c2)
                    return c1 + c2;

                // Fallback: convert to float, add, convert back
                if (TryToFloat(cargo1, out var f1) && TryToFloat(cargo2, out var f2))
                    return CreateFromUnits(f1 + f2);
            }
            catch (Exception ex) { LogAspera.LogError($"[CargoQuantityHelper] Add failed: {ex.Message}"); }
            return null;
        }
    }
}
