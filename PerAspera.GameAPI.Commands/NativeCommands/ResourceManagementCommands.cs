using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    /// <summary>
    /// Import resource command for adding resources to a faction's inventory
    /// Adds specified quantity of resource to the faction's resource pool
    /// </summary>
    /// <example>
    /// <code>
    /// // Import 1000 water for player faction
    /// var result = new ImportResourceCommand(playerFaction, ResourceType.Water, 1000).Execute();
    /// 
    /// // Using builder pattern
    /// var result = Commands.Create(NativeCommandTypes.ImportResource)
    ///     .WithFaction(playerFaction)
    ///     .WithParameter("resource", ResourceType.Water)
    ///     .WithParameter("quantity", 1000)
    ///     .Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.ImportResource(playerFaction, ResourceType.Water, 1000);
    /// </code>
    /// </example>
    public class ImportResourceCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will receive the resources
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The type of resource to import (e.g., Water, Oxygen, Iron, etc.)
        /// </summary>
        public object Resource { get; }
        
        /// <summary>
        /// The quantity of resource to add (must be positive)
        /// </summary>
        public int Quantity { get; }
        
        /// <summary>
        /// Create a new ImportResource command
        /// </summary>
        /// <param name="faction">Faction to receive resources</param>
        /// <param name="resource">Type of resource to import</param>
        /// <param name="quantity">Amount to import (must be positive)</param>
        /// <exception cref="ArgumentNullException">If faction or resource is null</exception>
        /// <exception cref="ArgumentException">If quantity is not positive</exception>
        public ImportResourceCommand(object faction, object resource, int quantity)
            : base(NativeCommandTypes.ImportResource)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
                
            Quantity = quantity;
            
            // Set parameters for native command execution
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Resource] = resource;
            Parameters[ParameterNames.Quantity] = quantity;
        }
        
        /// <summary>
        /// Validate the command before execution
        /// </summary>
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            // Validate faction
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            // Validate resource
            if (Resource == null)
            {
                errorMessage = "Resource cannot be null";
                return false;
            }
            
            // Validate quantity
            if (Quantity <= 0)
            {
                errorMessage = $"Quantity must be positive, got {Quantity}";
                return false;
            }
            
            // Additional validation could check if resource type is valid
            // This would require access to game's resource definitions
            
            return true;
        }
        
        /// <summary>
        /// Get a human-readable description of this command
        /// </summary>
        public override string GetDescription()
        {
            return $"Import {Quantity} {Resource} to faction {Faction}";
        }
        
        /// <summary>
        /// Create ImportResource command from parameters dictionary
        /// </summary>
        public static ImportResourceCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Resource, out var resource))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Resource}");
                
            if (!parameters.TryGetValue(ParameterNames.Quantity, out var quantityObj))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Quantity}");
                
            if (!(quantityObj is int quantity))
            {
                if (int.TryParse(quantityObj?.ToString(), out quantity))
                {
                    // Conversion successful
                }
                else
                {
                    throw new ArgumentException($"Parameter {ParameterNames.Quantity} must be an integer, got {quantityObj}");
                }
            }
            
            return new ImportResourceCommand(faction, resource, quantity);
        }
    }
    
    /// <summary>
    /// Export resource command for removing resources from a faction's inventory
    /// Removes specified quantity of resource from the faction's resource pool
    /// </summary>
    /// <example>
    /// <code>
    /// // Export 500 iron from player faction
    /// var result = new ExportResourceCommand(playerFaction, ResourceType.Iron, 500).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.ExportResource(playerFaction, ResourceType.Iron, 500);
    /// </code>
    /// </example>
    public class ExportResourceCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will lose the resources
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The type of resource to export
        /// </summary>
        public object Resource { get; }
        
        /// <summary>
        /// The quantity of resource to remove (must be positive)
        /// </summary>
        public int Quantity { get; }
        
        /// <summary>
        /// Create a new ExportResource command
        /// </summary>
        /// <param name="faction">Faction to lose resources</param>
        /// <param name="resource">Type of resource to export</param>
        /// <param name="quantity">Amount to export (must be positive)</param>
        public ExportResourceCommand(object faction, object resource, int quantity)
            : base(NativeCommandTypes.ExportResource)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
                
            Quantity = quantity;
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Resource] = resource;
            Parameters[ParameterNames.Quantity] = quantity;
        }
        
        public override bool IsValid()
        {
            return Faction != null && Resource != null && Quantity > 0;
        }
        
        public override string GetDescription()
        {
            return $"Export {Quantity} {Resource} from faction {Faction}";
        }
        
        public static ExportResourceCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Resource, out var resource))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Resource}");
                
            if (!parameters.TryGetValue(ParameterNames.Quantity, out var quantityObj) ||
                !(quantityObj is int quantity && int.TryParse(quantityObj?.ToString(), out quantity)))
                throw new ArgumentException($"Parameter {ParameterNames.Quantity} must be a valid integer");
            
            return new ExportResourceCommand(faction, resource, quantity);
        }
    }
    
    /// <summary>
    /// Set resource amount command for setting exact resource quantity
    /// Sets the faction's resource amount to the specified value (not additive)
    /// </summary>
    /// <example>
    /// <code>
    /// // Set oxygen amount to exactly 2500.5 for player faction
    /// var result = new SetResourceAmountCommand(playerFaction, ResourceType.Oxygen, 2500.5f).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.SetResourceAmount(playerFaction, ResourceType.Oxygen, 2500.5f);
    /// </code>
    /// </example>
    public class SetResourceAmountCommand : GameCommandBase
    {
        /// <summary>
        /// The faction whose resource amount will be set
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The type of resource to set
        /// </summary>
        public object Resource { get; }
        
        /// <summary>
        /// The exact amount to set (must be non-negative)
        /// </summary>
        public float Amount { get; }
        
        /// <summary>
        /// Create a new SetResourceAmount command
        /// </summary>
        /// <param name="faction">Faction whose resource will be set</param>
        /// <param name="resource">Type of resource to set</param>
        /// <param name="amount">Exact amount to set (must be non-negative)</param>
        public SetResourceAmountCommand(object faction, object resource, float amount)
            : base(NativeCommandTypes.SetResourceAmount)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            
            if (amount < 0)
                throw new ArgumentException("Amount must be non-negative", nameof(amount));
            if (!float.IsFinite(amount))
                throw new ArgumentException("Amount must be a finite number", nameof(amount));
                
            Amount = amount;
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Resource] = resource;
            Parameters[ParameterNames.Amount] = amount;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Resource == null)
            {
                errorMessage = "Resource cannot be null";
                return false;
            }
            
            if (Amount < 0)
            {
                errorMessage = $"Amount must be non-negative, got {Amount}";
                return false;
            }
            
            if (!float.IsFinite(Amount))
            {
                errorMessage = $"Amount must be a finite number, got {Amount}";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Set {Resource} amount to {Amount} for faction {Faction}";
        }
        
        public static SetResourceAmountCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Resource, out var resource))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Resource}");
                
            if (!parameters.TryGetValue(ParameterNames.Amount, out var amountObj))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Amount}");
                
            float amount;
            if (amountObj is float f)
            {
                amount = f;
            }
            else if (float.TryParse(amountObj?.ToString(), out amount))
            {
                // Conversion successful
            }
            else
            {
                throw new ArgumentException($"Parameter {ParameterNames.Amount} must be a valid number, got {amountObj}");
            }
            
            return new SetResourceAmountCommand(faction, resource, amount);
        }
    }
}