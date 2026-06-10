using System;
using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Data
{
    /// <summary>
    /// Building event data
    /// Contains information about building-related events
    /// </summary>
    public class BuildingEventData : GameEventBase
    {
        public override string EventType => "BuildingEvent";

        // === BUILDING IDENTITY ===
        public string BuildingTypeKey { get; set; } = string.Empty;
        public string BuildingName { get; set; } = string.Empty;
        public object? BuildingInstance { get; set; }

        // === LOCATION ===
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public int? RegionId { get; set; }

        // === OWNERSHIP ===
        public object? OwnerFaction { get; set; }
        public string? OwnerFactionName { get; set; }

        // === STATUS ===
        public string Status { get; set; } = string.Empty; // "Spawned", "Upgraded", "Scrapped"
        public bool IsOperational { get; set; } = true;
        public float? ProductionRate { get; set; }

        // === NATIVE INTEGRATION ===
        public object? Payload { get; set; }
        public int MartianSol { get; set; }

        public BuildingEventData() { }

        public BuildingEventData(string buildingTypeKey, string status)
        {
            BuildingTypeKey = buildingTypeKey;
            Status = status;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"BuildingEvent: {Status} - {BuildingTypeKey} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
        }
    }
}
