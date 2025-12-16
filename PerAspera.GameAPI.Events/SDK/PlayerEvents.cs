using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== PLAYER EVENTS ====================

    /// <summary>
    /// Event triggered when player faction is detected
    /// </summary>
    public class PlayerFactionDetectedEvent : SDKEventBase
    {
        public override string EventType => "PlayerFactionDetected";
        
        public object PlayerFaction { get; }
        public string FactionName { get; }

        public PlayerFactionDetectedEvent(object playerFaction, string factionName = "")
        {
            PlayerFaction = playerFaction;
            FactionName = factionName;
        }
    }

    /// <summary>
    /// Event triggered when player executes an action
    /// </summary>
    public class PlayerActionExecutedEvent : SDKEventBase
    {
        public override string EventType => "PlayerActionExecuted";
        
        public string ActionType { get; }
        public object? ActionData { get; }
        public bool Success { get; }

        public PlayerActionExecutedEvent(string actionType, object? actionData = null, bool success = true)
        {
            ActionType = actionType;
            ActionData = actionData;
            Success = success;
        }
    }
}
