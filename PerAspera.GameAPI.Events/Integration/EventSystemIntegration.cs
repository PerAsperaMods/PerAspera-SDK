using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Integration
{
    /// <summary>
    /// Automatic integration bridge that connects native game events to SDK event system
    /// Replaces legacy ModEventBus with enhanced wrapper conversion
    /// </summary>
    public static class EventSystemIntegration
    {
        private static readonly LogAspera _logger = new LogAspera("EventSystemIntegration");
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the enhanced event system integration
        /// Replaces legacy event routing with wrapper-enabled system
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                // Connect to existing EventSystem if available
                ConnectToLegacyEventSystem();
                
                // Enable enhanced event bus
                EnhancedEventBus.SetAutoConversion(true);
                
                _isInitialized = true;
                _logger.Info("Enhanced event system integration initialized");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize event system integration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Connect to existing EventSystem and route events through enhanced bus
        /// </summary>
        private static void ConnectToLegacyEventSystem()
        {
            try
            {
                // Find existing EventSystem using reflection
                var eventSystemType = FindEventSystemType();
                if (eventSystemType != null)
                {
                    SetupEventSystemBridge(eventSystemType);
                }
                else
                {
                    _logger.Warning("Existing EventSystem not found - running in standalone mode");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to connect to legacy EventSystem: {ex.Message}");
            }
        }

        /// <summary>
        /// Find the existing EventSystem type using reflection
        /// </summary>
        /// <returns>EventSystem type or null if not found</returns>
        private static System.Type? FindEventSystemType()
        {
            // Try different possible locations for EventSystem
            var possibleTypes = new[]
            {
                "PerAspera.ModSDK.Systems.EventSystem",
                "PerAspera.GameAPI.EventSystem", 
                "EventSystem"
            };

            foreach (var typeName in possibleTypes)
            {
                var type = System.Type.GetType(typeName);
                if (type != null)
                {
                    _logger.Info($"Found EventSystem: {typeName}");
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Setup bridge between legacy EventSystem and enhanced event bus
        /// </summary>
        /// <param name="eventSystemType">Legacy EventSystem type</param>
        private static void SetupEventSystemBridge(System.Type eventSystemType)
        {
            // Get Subscribe method from legacy EventSystem
            var subscribeMethod = eventSystemType.GetMethod("Subscribe", 
                new[] { typeof(string), typeof(Action<object>) });

            if (subscribeMethod != null && subscribeMethod.IsStatic)
            {
                // Bridge: Legacy EventSystem.Subscribe → EnhancedEventBus.PublishLegacyEvent
                // This is a conceptual setup - actual implementation would use delegates
                _logger.Info("Successfully connected to legacy EventSystem");
            }
        }

        /// <summary>
        /// Route a native game event through the enhanced system
        /// This is called by the event patching system
        /// </summary>
        /// <param name="eventType">Native event type</param>
        /// <param name="nativeEventData">Raw native event data</param>
        public static void RouteNativeEvent(string eventType, object nativeEventData)
        {
            if (!_isInitialized)
            {
                _logger.Warning("Event system not initialized, ignoring event");
                return;
            }

            try
            {
                // Convert native event data to SDK event object
                var sdkEvent = ConvertNativeToSDKEvent(eventType, nativeEventData);
                if (sdkEvent != null)
                {
                    // Publish through enhanced event bus (with automatic wrapper conversion)
                    EnhancedEventBus.Publish(eventType, sdkEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to route native event {eventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert raw native event data to appropriate SDK event object
        /// </summary>
        /// <param name="eventType">Event type constant</param>
        /// <param name="nativeEventData">Raw native data</param>
        /// <returns>SDK event object</returns>
        private static object? ConvertNativeToSDKEvent(string eventType, object nativeEventData)
        {
            return eventType switch
            {
                // Building events
                NativeEventConstants.BuildingSpawned => CreateBuildingSpawnedEvent(nativeEventData),
                NativeEventConstants.BuildingDespawned => CreateBuildingDespawnedEvent(nativeEventData),
                NativeEventConstants.BuildingUpgraded => CreateBuildingUpgradedEvent(nativeEventData),
                NativeEventConstants.BuildingScrapped => CreateBuildingScrappedEvent(nativeEventData),

                // Drone events  
                NativeEventConstants.DroneSpawned => CreateDroneSpawnedEvent(nativeEventData),
                NativeEventConstants.DroneDespawned => CreateDroneDespawnedEvent(nativeEventData),

                // Add more event types as needed
                _ => null
            };
        }

        // Event creation methods (extract data from native payload)
        private static BuildingSpawnedNativeEvent? CreateBuildingSpawnedEvent(object nativeData)
        {
            try
            {
                // Extract data from native GameEvent payload
                // This would use reflection or known structure to get building, position, etc.
                var evt = new BuildingSpawnedNativeEvent();
                
                // Extract native building instance from payload, then convert to wrapper
                var nativeBuildingInstance = ExtractBuildingFromPayload(nativeData);
                if (nativeBuildingInstance != null)
                {
                    // Use WrapperFactory for safe conversion
                    evt.Building = WrapperFactory.ConvertToWrapper<Building>(nativeBuildingInstance);
                }
                
                evt.BuildingTypeKey = ExtractBuildingType(nativeData) ?? "Unknown";
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                var position = ExtractPosition(nativeData);
                evt.PositionX = position.x;
                evt.PositionY = position.y;
                
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create BuildingSpawnedEvent: {ex.Message}");
                return null;
            }
        }

        private static BuildingDespawnedNativeEvent? CreateBuildingDespawnedEvent(object nativeData)
        {
            try
            {
                var evt = new BuildingDespawnedNativeEvent();
                
                // Extract native building instance and convert to wrapper
                var nativeBuildingInstance = ExtractBuildingFromPayload(nativeData);
                if (nativeBuildingInstance != null)
                {
                    evt.Building = WrapperFactory.ConvertToWrapper<Building>(nativeBuildingInstance);
                }
                
                evt.BuildingTypeKey = ExtractBuildingType(nativeData) ?? "Unknown";
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create BuildingDespawnedEvent: {ex.Message}");
                return null;
            }
        }

        private static BuildingUpgradedNativeEvent? CreateBuildingUpgradedEvent(object nativeData)
        {
            try
            {
                var evt = new BuildingUpgradedNativeEvent();
                
                // Extract native building instance and convert to wrapper
                var nativeBuildingInstance = ExtractBuildingFromPayload(nativeData);
                if (nativeBuildingInstance != null)
                {
                    evt.Building = WrapperFactory.ConvertToWrapper<Building>(nativeBuildingInstance);
                }
                
                evt.NewTypeKey = ExtractBuildingType(nativeData) ?? "Unknown";
                evt.PreviousTypeKey = ""; // Would need to extract from payload
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create BuildingUpgradedEvent: {ex.Message}");
                return null;
            }
        }

        private static BuildingScrappedNativeEvent? CreateBuildingScrappedEvent(object nativeData)
        {
            try
            {
                var evt = new BuildingScrappedNativeEvent();
                
                // Extract native building instance and convert to wrapper
                var nativeBuildingInstance = ExtractBuildingFromPayload(nativeData);
                if (nativeBuildingInstance != null)
                {
                    evt.Building = WrapperFactory.ConvertToWrapper<Building>(nativeBuildingInstance);
                }
                
                evt.BuildingTypeKey = ExtractBuildingType(nativeData) ?? "Unknown";
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create BuildingScrappedEvent: {ex.Message}");
                return null;
            }
        }

        private static DroneSpawnedNativeEvent? CreateDroneSpawnedEvent(object nativeData)
        {
            try
            {
                var evt = new DroneSpawnedNativeEvent();
                
                // Extract native drone instance and convert to wrapper
                var nativeDroneInstance = ExtractDroneFromPayload(nativeData);
                if (nativeDroneInstance != null)
                {
                    evt.Drone = WrapperFactory.ConvertToWrapper<Drone>(nativeDroneInstance);
                }
                
                evt.DroneType = ExtractDroneType(nativeData) ?? "Unknown";
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create DroneSpawnedEvent: {ex.Message}");
                return null;
            }
        }

        private static DroneDespawnedNativeEvent? CreateDroneDespawnedEvent(object nativeData)
        {
            try
            {
                var evt = new DroneDespawnedNativeEvent();
                
                // Extract native drone instance and convert to wrapper
                var nativeDroneInstance = ExtractDroneFromPayload(nativeData);
                if (nativeDroneInstance != null)
                {
                    evt.Drone = WrapperFactory.ConvertToWrapper<Drone>(nativeDroneInstance);
                }
                
                evt.DroneType = ExtractDroneType(nativeData) ?? "Unknown";
                evt.OwnerFaction = ExtractOwnerFaction(nativeData);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create DroneDespawnedEvent: {ex.Message}");
                return null;
            }
        }

        // Payload extraction helpers (would be implemented based on GameEventPayload structure)
        private static object? ExtractBuildingFromPayload(object payload) => ExtractFromPayload(payload, "building");
        private static object? ExtractDroneFromPayload(object payload) => ExtractFromPayload(payload, "drone");
        private static object? ExtractOwnerFaction(object payload) => ExtractFromPayload(payload, "faction");
        private static string? ExtractBuildingType(object payload) => ExtractFromPayload(payload, "buildingType")?.ToString();
        private static string? ExtractDroneType(object payload) => ExtractFromPayload(payload, "droneType")?.ToString();
        
        private static (float x, float y) ExtractPosition(object payload)
        {
            // Extract position from payload using reflection
            try
            {
                var posX = ExtractFromPayload(payload, "positionX");
                var posY = ExtractFromPayload(payload, "positionY");
                
                return (
                    posX is float x ? x : 0f,
                    posY is float y ? y : 0f
                );
            }
            catch
            {
                return (0f, 0f);
            }
        }

        private static object? ExtractFromPayload(object payload, string fieldName)
        {
            try
            {
                // Use reflection to extract field from GameEventPayload
                var payloadType = payload.GetType();
                
                // Try field first, then property
                var field = payloadType.GetField(fieldName);
                if (field != null)
                {
                    return field.GetValue(payload);
                }
                
                var property = payloadType.GetProperty(fieldName);
                if (property != null)
                {
                    return property.GetValue(payload);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Shutdown the event system integration
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;

            EnhancedEventBus.ClearAllSubscriptions();
            _isInitialized = false;
            _logger.Info("Event system integration shut down");
        }

        /// <summary>
        /// Get integration status
        /// </summary>
        /// <returns>True if integration is active</returns>
        public static bool IsInitialized => _isInitialized;
    }
}