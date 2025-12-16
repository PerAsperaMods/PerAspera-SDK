namespace PerAspera.GameAPI.Events.Constants
{
    /// <summary>
    /// Event name constants for SDK custom events.
    /// These are events created by the SDK itself for modder convenience.
    /// </summary>
    public static class SDKEventConstants
    {
        // ==================== SYSTEM EVENTS ====================
        
        /// <summary>Event: Base game finished loading</summary>
        public const string BaseGameDetected = "BaseGameDetected";
        
        /// <summary>Event: SDK initialized and ready</summary>
        public const string SDKInitialized = "SDKInitialized";
        
        /// <summary>Event: All mods finished loading</summary>
        public const string ModsLoaded = "ModsLoaded";
        
        // ==================== PLAYER EVENTS ====================
        
        /// <summary>Event: Player interacted with UI element</summary>
        public const string PlayerUIInteraction = "PlayerUIInteraction";
        
        /// <summary>Event: Player selected a building</summary>
        public const string PlayerSelectedBuilding = "PlayerSelectedBuilding";
        
        /// <summary>Event: Player camera moved</summary>
        public const string PlayerCameraMoved = "PlayerCameraMoved";
        
        // ==================== MOD EVENTS ====================
        
        /// <summary>Event: Mod configuration changed</summary>
        public const string ModConfigChanged = "ModConfigChanged";
        
        /// <summary>Event: Mod registered successfully</summary>
        public const string ModRegistered = "ModRegistered";
        
        /// <summary>Event: Mod encountered an error</summary>
        public const string ModError = "ModError";
    }
}
