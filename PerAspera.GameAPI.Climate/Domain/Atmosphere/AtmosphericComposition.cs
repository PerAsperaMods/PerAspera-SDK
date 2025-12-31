using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Climate;
using PerAspera.GameAPI.Wrappers;

// Alias pour éviter le conflit avec la classe Atmosphere du jeu
using SDKAtmosphere = PerAspera.GameAPI.Climate.Domain.Atmosphere.PlanetaryAtmosphere;

/// <summary>
/// Collection of atmospheric gases with dynamic composition and cargo integration
/// Provides access to atmospheric gases as Cargo objects for building interactions
/// </summary>
public class AtmosphericComposition
{
    private readonly Dictionary<string, AtmosphericGas> _gases;
    private readonly Dictionary<string, Cargo> _gasCargos;
    private readonly Func<float> _getTotalPressure;
    private readonly object _nativePlanet;

    internal AtmosphericComposition(Dictionary<string, AtmosphericGas> gases, Func<float> getTotalPressure, object nativePlanet)
    {
        _gases = gases;
        _gasCargos = new Dictionary<string, Cargo>();
        _getTotalPressure = getTotalPressure;
        _nativePlanet = nativePlanet;

        // Initialize cargo representations for atmospheric gases
        InitializeGasCargos();
    }

    /// <summary>
    /// Initialize Cargo objects for each atmospheric gas to enable building interactions
    /// </summary>
    private void InitializeGasCargos()
    {
        foreach (var gasEntry in _gases)
        {
            var gas = gasEntry.Value;
            // Create a cargo representation for this gas
            // Note: This is a conceptual representation - actual cargo creation would need building context
            _gasCargos[gasEntry.Key] = CreateAtmosphericCargo(gas);
        }
    }

    /// <summary>
    /// Create a conceptual Cargo object representing atmospheric gas
    /// This allows atmospheric gases to interact with building mechanics
    /// </summary>
    private Cargo CreateAtmosphericCargo(AtmosphericGas gas)
    {
        // Find the corresponding ResourceType for this gas
        var resourceType = FindAtmosphericResourceType(gas.Symbol);
        if (resourceType == null) return null;

        // Create cargo with current gas pressure as quantity
        // Note: In a real implementation, this would be managed by the planet's atmosphere system
        var cargo = new Cargo();
        cargo._resource = resourceType;
        cargo._quantity = CargoQuantity.FromUnitFloat(gas.PartialPressure);

        return cargo;
    }

    /// <summary>
    /// Find ResourceType corresponding to atmospheric gas symbol
    /// </summary>
    private ResourceType FindAtmosphericResourceType(string symbol)
    {
        // Map gas symbols to resource type constants
        var resourceKey = symbol switch
        {
            "CO2" => "resource_carbon_dioxide_release",
            "O2" => "resource_oxygen_release",
            "N2" => "resource_nitrogen_release",
            "GHG" => "resource_ghg_release",
            "H2O" => "resource_water", // Water vapor maps to water resource
            _ => null
        };

        if (resourceKey != null)
        {
            return (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
        }

        return null;
    }

    /// <summary>
    /// Get gas by symbol (CO2, O2, N2, H2O)
    /// </summary>
    public AtmosphericGas? this[string symbol] => _gases.TryGetValue(symbol, out var gas) ? gas : null;

    /// <summary>
    /// Get cargo representation of atmospheric gas for building interactions
    /// </summary>
    public Cargo? GetGasCargo(string symbol) => _gasCargos.TryGetValue(symbol, out var cargo) ? cargo : null;

    /// <summary>
    /// All atmospheric gases
    /// </summary>
    public IEnumerable<AtmosphericGas> AllGases => _gases.Values;

    /// <summary>
    /// All atmospheric gas cargos for building interactions
    /// </summary>
    public IEnumerable<Cargo> AllGasCargos => _gasCargos.Values.Where(c => c != null);

    /// <summary>
    /// Update percentages based on total pressure and sync cargo quantities
    /// </summary>
    internal void UpdatePercentages()
    {
        var total = _getTotalPressure();
        if (total <= 0) return;

        foreach (var gas in _gases.Values)
        {
            gas.Percentage = (gas.PartialPressure / total) * 100f;
        }

        // Sync cargo quantities with current gas pressures
        SyncCargoQuantities();
    }

    /// <summary>
    /// Sync cargo quantities with current atmospheric gas pressures
    /// This enables buildings to interact with atmospheric gases through cargo system
    /// </summary>
    private void SyncCargoQuantities()
    {
        foreach (var gasEntry in _gases)
        {
            var cargo = _gasCargos[gasEntry.Key];
            if (cargo != null)
            {
                // Update cargo quantity to match current gas pressure
                cargo._quantity = CargoQuantity.FromUnitFloat(gasEntry.Value.PartialPressure);
            }
        }
    }

    /// <summary>
    /// Transfer gas between buildings via cargo system
    /// </summary>
    public bool TransferGasToBuilding(string gasSymbol, Building targetBuilding, float amount)
    {
        var cargo = GetGasCargo(gasSymbol);
        if (cargo == null || targetBuilding == null) return false;

        try
        {
            // Check if building can accept this cargo
            if (!SDKAtmosphere.CanAcceptCargo(targetBuilding, cargo))
            {
                LogAspera.LogWarning($"Building cannot accept {gasSymbol} cargo");
                return false;
            }

            // Check if we have enough gas in atmosphere
            var gas = this[gasSymbol];
            if (gas == null || gas.PartialPressure < amount)
            {
                LogAspera.LogWarning($"Insufficient {gasSymbol} in atmosphere ({gas?.PartialPressure ?? 0} < {amount})");
                return false;
            }

            // Create a cargo object with the transfer amount
            var transferCargo = new Cargo();
            transferCargo._resource = cargo.resource;
            transferCargo._quantity = CargoQuantity.FromUnitFloat(amount);

            // Transfer cargo to building
            if (SDKAtmosphere.AcceptCargo(targetBuilding, transferCargo))
            {
                // Reduce atmospheric gas pressure
                gas.PartialPressure -= amount;
                LogAspera.LogInfo($"Transferred {amount} {gasSymbol} from atmosphere to building");
                return true;
            }
        }
        catch (Exception ex)
        {
            LogAspera.LogWarning($"Failed to transfer {gasSymbol} to building: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Extract gas from building back to atmosphere
    /// </summary>
    public bool ExtractGasFromBuilding(string gasSymbol, Building sourceBuilding, float amount)
    {
        if (sourceBuilding == null) return false;

        try
        {
            // Find cargo in building
            var resourceKey = GetResourceKeyForGas(gasSymbol);
            if (resourceKey == null) return false;

            var resourceType = (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
            var cargo = SDKAtmosphere.FindCargoByResource(sourceBuilding, resourceType);

            if (cargo == null || cargo.quantity.ToFloat() < amount)
            {
                LogAspera.LogWarning($"Building doesn't have enough {gasSymbol} to extract ({cargo?.quantity.ToFloat() ?? 0} < {amount})");
                return false;
            }

            // Remove cargo from building
            if (SDKAtmosphere.RemoveCargo(sourceBuilding, cargo, amount))
            {
                // Increase atmospheric gas pressure
                var gas = this[gasSymbol];
                if (gas != null)
                {
                    gas.PartialPressure += amount;
                    LogAspera.LogInfo($"Extracted {amount} {gasSymbol} from building to atmosphere");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            LogAspera.LogWarning($"Failed to extract {gasSymbol} from building: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Get resource key for gas symbol
    /// </summary>
    private string GetResourceKeyForGas(string gasSymbol)
    {
        return gasSymbol switch
        {
            "CO2" => "resource_carbon_dioxide_release",
            "O2" => "resource_oxygen_release",
            "N2" => "resource_nitrogen_release",
            "GHG" => "resource_ghg_release",
            "H2O" => "resource_water",
            _ => null
        };
    }

    /// <summary>
    /// Get quantity of atmospheric gas by resource key
    /// </summary>
    public float GetGasQuantity(string resourceKey)
    {
        var resourceType = (ResourceType)KeeperTypeRegistry.GetResourceType(resourceKey);
        if (resourceType == null || !_gasCargos.TryGetValue(GetGasSymbolForResource(resourceKey), out var cargo)) return 0f;
        return cargo?._quantity.ToFloat() ?? 0f;
    }

    /// <summary>
    /// Get gas symbol for resource key
    /// </summary>
    private string GetGasSymbolForResource(string resourceKey)
    {
        return resourceKey switch
        {
            "resource_carbon_dioxide_release" => "CO2",
            "resource_oxygen_release" => "O2",
            "resource_nitrogen_release" => "N2",
            "resource_ghg_release" => "GHG",
            "resource_water" => "H2O",
            _ => null
        };
    }

    /// <summary>
    /// Update atmospheric composition over time
    /// </summary>
    public void Tick(float deltaTime)
    {
        // TODO: Implement atmospheric composition dynamics
        // For now, just update gas quantities from native planet
        foreach (var gas in _gases.Values)
        {
            // Update gas quantity from planet getter
            // This would be where diffusion, reactions, etc. happen
        }
    }

    public override string ToString()
    {
        UpdatePercentages();
        return string.Join(", ", _gases.Values.Select(g => $"{g.Symbol}:{g.Percentage:F1}%"));
    }
}