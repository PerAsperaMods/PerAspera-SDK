#nullable enable
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Helpers
{
    /// <summary>
    /// Helper that casts native IL2CPP instances to their concrete types for events.
    /// After the Wrappers removal migration, events expose native IL2CPP types directly.
    /// </summary>
    public static class EventWrapperHelper
    {
        private static readonly LogAspera _logger = new LogAspera("EventWrapperHelper");

        public static Building? CreateBuildingWrapper(object? native) => native as Building;
        public static BuildingType? CreateBuildingTypeWrapper(object? native) => native as BuildingType;
        public static Faction? CreateFactionWrapper(object? native) => native as Faction;
        public static ResourceType? CreateResourceTypeWrapper(object? native) => native as ResourceType;
        public static Technology? CreateTechnologyWrapper(object? native) => native as Technology;
        public static Drone? CreateDroneWrapper(object? native) => native as Drone;
        public static KnowledgeType? CreateKnowledgeWrapper(object? native) => native as KnowledgeType;
    }
}
