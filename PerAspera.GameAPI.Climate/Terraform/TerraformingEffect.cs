using PerAspera.Core.IL2CPP;

/// <summary>
/// Represents a terraforming effect on climate
/// </summary>
public class TerraformingEffect
{
    private readonly object _nativePlanet;
    private readonly string _getterMethod;

    public string Name { get; }
    public string Description { get; }

    internal TerraformingEffect(object nativePlanet, string name, string description, string getterMethod)
    {
        _nativePlanet = nativePlanet;
        Name = name;
        Description = description;
        _getterMethod = getterMethod;
    }

    /// <summary>
    /// Temperature effect in Kelvin
    /// </summary>
    public float TemperatureEffect => _nativePlanet.InvokeMethod<float>(_getterMethod);

    public override string ToString() => $"{Name}: {TemperatureEffect:+0.00;-0.00;0}K";
}