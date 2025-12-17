using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    /// <summary>
    /// Unlock building command for making a building type available to a faction
    /// Adds the building to the faction's available building list
    /// </summary>
    /// <example>
    /// <code>
    /// // Unlock solar panels for player faction
    /// var result = new UnlockBuildingCommand(playerFaction, BuildingType.SolarPanel).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.UnlockBuilding(playerFaction, BuildingType.SolarPanel);
    /// </code>
    /// </example>
    public class UnlockBuildingCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will gain access to the building
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The building type to unlock
        /// </summary>
        public object Building { get; }
        
        /// <summary>
        /// Create a new UnlockBuilding command
        /// </summary>
        /// <param name="faction">Faction to unlock building for</param>
        /// <param name="building">Building type to unlock</param>
        public UnlockBuildingCommand(object faction, object building)
            : base(NativeCommandTypes.UnlockBuilding)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Building = building ?? throw new ArgumentNullException(nameof(building));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Building] = building;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Building == null)
            {
                errorMessage = "Building cannot be null";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Unlock building {Building} for faction {Faction}";
        }
        
        public static UnlockBuildingCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Building, out var building))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Building}");
            
            return new UnlockBuildingCommand(faction, building);
        }
    }
    
    /// <summary>
    /// Lock building command for removing a building type from a faction's available buildings
    /// Removes the building from the faction's available building list
    /// </summary>
    /// <example>
    /// <code>
    /// // Lock nuclear reactors for AI faction
    /// var result = new LockBuildingCommand(aiFaction, BuildingType.NuclearReactor).Execute();
    /// </code>
    /// </example>
    public class LockBuildingCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will lose access to the building
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The building type to lock
        /// </summary>
        public object Building { get; }
        
        /// <summary>
        /// Create a new LockBuilding command
        /// </summary>
        /// <param name="faction">Faction to lock building for</param>
        /// <param name="building">Building type to lock</param>
        public LockBuildingCommand(object faction, object building)
            : base(NativeCommandTypes.LockBuilding)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Building = building ?? throw new ArgumentNullException(nameof(building));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Building] = building;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Building == null)
            {
                errorMessage = "Building cannot be null";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Lock building {Building} for faction {Faction}";
        }
        
        public static LockBuildingCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Building, out var building))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Building}");
            
            return new LockBuildingCommand(faction, building);
        }
    }
    
    /// <summary>
    /// Add building command for placing a building at specified coordinates
    /// Creates a new building instance on the map for the faction
    /// </summary>
    /// <example>
    /// <code>
    /// // Add solar panel at coordinates (100, 0, 50)
    /// var result = new AddBuildingCommand(playerFaction, BuildingType.SolarPanel, 100f, 0f, 50f).Execute();
    /// 
    /// // Using Vector3-like position
    /// var result = Commands.ForFaction(playerFaction).AddBuilding(BuildingType.SolarPanel, position.x, position.y, position.z).Execute();
    /// </code>
    /// </example>
    public class AddBuildingCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will own the building
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The building type to add
        /// </summary>
        public object Building { get; }
        
        /// <summary>
        /// X coordinate for building placement
        /// </summary>
        public float X { get; }
        
        /// <summary>
        /// Y coordinate for building placement
        /// </summary>
        public float Y { get; }
        
        /// <summary>
        /// Z coordinate for building placement
        /// </summary>
        public float Z { get; }
        
        /// <summary>
        /// Create a new AddBuilding command
        /// </summary>
        /// <param name="faction">Faction to own the building</param>
        /// <param name="building">Building type to add</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public AddBuildingCommand(object faction, object building, float x, float y, float z)
            : base(NativeCommandTypes.AddBuilding)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Building = building ?? throw new ArgumentNullException(nameof(building));
            
            if (!float.IsFinite(x)) throw new ArgumentException("X coordinate must be finite", nameof(x));
            if (!float.IsFinite(y)) throw new ArgumentException("Y coordinate must be finite", nameof(y));
            if (!float.IsFinite(z)) throw new ArgumentException("Z coordinate must be finite", nameof(z));
            
            X = x;
            Y = y;
            Z = z;
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Building] = building;
            Parameters[ParameterNames.X] = x;
            Parameters[ParameterNames.Y] = y;
            Parameters[ParameterNames.Z] = z;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Building == null)
            {
                errorMessage = "Building cannot be null";
                return false;
            }
            
            if (!float.IsFinite(X))
            {
                errorMessage = $"X coordinate must be finite, got {X}";
                return false;
            }
            
            if (!float.IsFinite(Y))
            {
                errorMessage = $"Y coordinate must be finite, got {Y}";
                return false;
            }
            
            if (!float.IsFinite(Z))
            {
                errorMessage = $"Z coordinate must be finite, got {Z}";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Add building {Building} for faction {Faction} at ({X:F1}, {Y:F1}, {Z:F1})";
        }
        
        public static AddBuildingCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Building, out var building))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Building}");
                
            if (!parameters.TryGetValue(ParameterNames.X, out var xObj) ||
                !TryConvertToFloat(xObj, out var x))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.X}");
                
            if (!parameters.TryGetValue(ParameterNames.Y, out var yObj) ||
                !TryConvertToFloat(yObj, out var y))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.Y}");
                
            if (!parameters.TryGetValue(ParameterNames.Z, out var zObj) ||
                !TryConvertToFloat(zObj, out var z))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.Z}");
            
            return new AddBuildingCommand(faction, building, x, y, z);
        }
        
        private static bool TryConvertToFloat(object value, out float result)
        {
            result = 0f;
            
            if (value is float f)
            {
                result = f;
                return float.IsFinite(f);
            }
            
            if (value is double d)
            {
                result = (float)d;
                return float.IsFinite(result);
            }
            
            if (value is int i)
            {
                result = i;
                return true;
            }
            
            return float.TryParse(value?.ToString(), out result) && float.IsFinite(result);
        }
    }
    
    /// <summary>
    /// Remove building command for removing a building from the map
    /// Removes the specified building instance from the faction
    /// </summary>
    /// <example>
    /// <code>
    /// // Remove a specific building instance
    /// var result = new RemoveBuildingCommand(playerFaction, buildingInstance).Execute();
    /// </code>
    /// </example>
    public class RemoveBuildingCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that owns the building
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The building instance to remove
        /// </summary>
        public object Building { get; }
        
        /// <summary>
        /// Create a new RemoveBuilding command
        /// </summary>
        /// <param name="faction">Faction that owns the building</param>
        /// <param name="building">Building instance to remove</param>
        public RemoveBuildingCommand(object faction, object building)
            : base(NativeCommandTypes.RemoveBuilding)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Building = building ?? throw new ArgumentNullException(nameof(building));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Building] = building;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Building == null)
            {
                errorMessage = "Building cannot be null";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Remove building {Building} from faction {Faction}";
        }
        
        public static RemoveBuildingCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Building, out var building))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Building}");
            
            return new RemoveBuildingCommand(faction, building);
        }
    }
}