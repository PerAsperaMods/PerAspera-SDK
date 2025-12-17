using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    public class SetClimateCommand : GameCommandBase
    {
        public float Temperature { get; }
        public float Pressure { get; }
        public float Oxygen { get; }
        
        public SetClimateCommand(float temperature, float pressure, float oxygen) : base(NativeCommandTypes.SetClimate)
        {
            Temperature = temperature; Pressure = pressure; Oxygen = oxygen;
            Parameters[ParameterNames.Temperature] = temperature;
            Parameters[ParameterNames.Pressure] = pressure;
            Parameters[ParameterNames.Oxygen] = oxygen;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (!float.IsFinite(Temperature) || !float.IsFinite(Pressure) || !float.IsFinite(Oxygen))
            { errorMessage = "Climate values must be finite"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Set climate: T={Temperature:F1}°C, P={Pressure:F1}, O2={Oxygen:F1}%";
        
        public static SetClimateCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var temp = Convert.ToSingle(parameters[ParameterNames.Temperature]);
            var press = Convert.ToSingle(parameters[ParameterNames.Pressure]);
            var oxy = Convert.ToSingle(parameters[ParameterNames.Oxygen]);
            return new SetClimateCommand(temp, press, oxy);
        }
    }

    public class TriggerEventCommand : GameCommandBase
    {
        public string EventName { get; }
        public object Target { get; }
        
        public TriggerEventCommand(string eventName, object target = null) : base(NativeCommandTypes.TriggerEvent)
        {
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
            Target = target;
            Parameters[ParameterNames.Key] = eventName;
            if (target != null) Parameters[ParameterNames.Value] = target;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(EventName)) { errorMessage = "Event name cannot be null or empty"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Trigger event '{EventName}'" + (Target != null ? $" on {Target}" : "");
        
        public static TriggerEventCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var eventName = parameters[ParameterNames.Key]?.ToString();
            var target = parameters.TryGetValue(ParameterNames.Value, out var t) ? t : null;
            return new TriggerEventCommand(eventName, target);
        }
    }

    public class SpawnUnitCommand : GameCommandBase
    {
        public object Faction { get; }
        public object UnitType { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        
        public SpawnUnitCommand(object faction, object unitType, float x, float y, float z) : base(NativeCommandTypes.SpawnUnit)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            UnitType = unitType ?? throw new ArgumentNullException(nameof(unitType));
            X = x; Y = y; Z = z;
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.UnitType] = unitType;
            Parameters[ParameterNames.X] = x;
            Parameters[ParameterNames.Y] = y;
            Parameters[ParameterNames.Z] = z;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Faction == null) { errorMessage = "Faction cannot be null"; return false; }
            if (UnitType == null) { errorMessage = "UnitType cannot be null"; return false; }
            if (!float.IsFinite(X) || !float.IsFinite(Y) || !float.IsFinite(Z)) { errorMessage = "Coordinates must be finite"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Spawn {UnitType} for {Faction} at ({X:F1}, {Y:F1}, {Z:F1})";
        
        public static SpawnUnitCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var faction = parameters[ParameterNames.Faction];
            var unitType = parameters[ParameterNames.UnitType];
            var x = Convert.ToSingle(parameters[ParameterNames.X]);
            var y = Convert.ToSingle(parameters[ParameterNames.Y]);
            var z = Convert.ToSingle(parameters[ParameterNames.Z]);
            return new SpawnUnitCommand(faction, unitType, x, y, z);
        }
    }

    public class DestroyUnitCommand : GameCommandBase
    {
        public object Unit { get; }
        
        public DestroyUnitCommand(object unit) : base(NativeCommandTypes.DestroyUnit)
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            Parameters[ParameterNames.Unit] = unit;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Unit == null) { errorMessage = "Unit cannot be null"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Destroy unit {Unit}";
        
        public static DestroyUnitCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new DestroyUnitCommand(parameters[ParameterNames.Unit]);
        }
    }

    public class MoveUnitCommand : GameCommandBase
    {
        public object Unit { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        
        public MoveUnitCommand(object unit, float x, float y, float z) : base(NativeCommandTypes.MoveUnit)
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            X = x; Y = y; Z = z;
            
            Parameters[ParameterNames.Unit] = unit;
            Parameters[ParameterNames.X] = x;
            Parameters[ParameterNames.Y] = y;
            Parameters[ParameterNames.Z] = z;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Unit == null) { errorMessage = "Unit cannot be null"; return false; }
            if (!float.IsFinite(X) || !float.IsFinite(Y) || !float.IsFinite(Z)) { errorMessage = "Coordinates must be finite"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Move unit {Unit} to ({X:F1}, {Y:F1}, {Z:F1})";
        
        public static MoveUnitCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var unit = parameters[ParameterNames.Unit];
            var x = Convert.ToSingle(parameters[ParameterNames.X]);
            var y = Convert.ToSingle(parameters[ParameterNames.Y]);
            var z = Convert.ToSingle(parameters[ParameterNames.Z]);
            return new MoveUnitCommand(unit, x, y, z);
        }
    }

    public class SetFactionRelationCommand : GameCommandBase
    {
        public object Faction1 { get; }
        public object Faction2 { get; }
        public float Relation { get; }
        
        public SetFactionRelationCommand(object faction1, object faction2, float relation) : base(NativeCommandTypes.SetFactionRelation)
        {
            Faction1 = faction1 ?? throw new ArgumentNullException(nameof(faction1));
            Faction2 = faction2 ?? throw new ArgumentNullException(nameof(faction2));
            Relation = relation;
            
            Parameters[ParameterNames.Faction] = faction1;
            Parameters[ParameterNames.TargetFaction] = faction2;
            Parameters[ParameterNames.Value] = relation;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Faction1 == null) { errorMessage = "Faction1 cannot be null"; return false; }
            if (Faction2 == null) { errorMessage = "Faction2 cannot be null"; return false; }
            if (!float.IsFinite(Relation)) { errorMessage = "Relation must be finite"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Set relation between {Faction1} and {Faction2} to {Relation}";
        
        public static SetFactionRelationCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var faction1 = parameters[ParameterNames.Faction];
            var faction2 = parameters[ParameterNames.TargetFaction];
            var relation = Convert.ToSingle(parameters[ParameterNames.Value]);
            return new SetFactionRelationCommand(faction1, faction2, relation);
        }
    }

    public class AddPointsCommand : GameCommandBase
    {
        public object Faction { get; }
        public int Points { get; }
        
        public AddPointsCommand(object faction, int points) : base(NativeCommandTypes.AddPoints)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Points = points;
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Points] = points;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Faction == null) { errorMessage = "Faction cannot be null"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Add {Points} points to {Faction}";
        
        public static AddPointsCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var faction = parameters[ParameterNames.Faction];
            var points = Convert.ToInt32(parameters[ParameterNames.Points]);
            return new AddPointsCommand(faction, points);
        }
    }

    public class SetAIAggressionCommand : GameCommandBase
    {
        public object Faction { get; }
        public float Aggression { get; }
        
        public SetAIAggressionCommand(object faction, float aggression) : base(NativeCommandTypes.SetAIAggression)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Aggression = aggression;
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Value] = aggression;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Faction == null) { errorMessage = "Faction cannot be null"; return false; }
            if (!float.IsFinite(Aggression)) { errorMessage = "Aggression must be finite"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Set AI aggression for {Faction} to {Aggression}";
        
        public static SetAIAggressionCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var faction = parameters[ParameterNames.Faction];
            var aggression = Convert.ToSingle(parameters[ParameterNames.Value]);
            return new SetAIAggressionCommand(faction, aggression);
        }
    }
}