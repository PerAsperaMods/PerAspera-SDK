using System;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers;

public class AtmosphericGas
{
    private readonly object _nativePlanet;
    private readonly string _getterMethod;
    private readonly string? _setterMethod;

    public string Name { get; }
    public string Symbol { get; }

    ResourceTypeWrapper resourceType = null;

    internal AtmosphericGas(object nativePlanet, string name, string symbol, string getterMethod, string? setterMethod = null)
    {
        _nativePlanet = nativePlanet;
        Name = name;
        Symbol = symbol;
        _getterMethod = getterMethod;
        _setterMethod = setterMethod;
    }

    /// <summary>
    /// Partial pressure of this gas (kPa)
    /// </summary>
    public float PartialPressure
    {
        get => _nativePlanet.InvokeMethod<float>(_getterMethod);
        set
        {
            if (_setterMethod == null)
                throw new InvalidOperationException($"{Name} pressure is read-only");
            _nativePlanet.InvokeMethod(_setterMethod, value);
        }
    }

    /// <summary>
    /// Percentage in total atmosphere (0-100%)
    /// </summary>
    public float Percentage { get; internal set; }

    /// <summary>
    /// Check if this gas can be modified
    /// </summary>
    public bool IsReadOnly => _setterMethod == null;

    public override string ToString() => $"{Symbol}: {PartialPressure:F2}kPa ({Percentage:F1}%)";
}