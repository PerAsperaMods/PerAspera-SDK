namespace PerAspera.GameAPI.Events.Constants
{
    /// <summary>
    /// Event name constants for SDK custom events.
    /// These are events created by the SDK itself for modder convenience.
    /// </summary>
    public static class SDKEventConstants
    {
        // ==================== SYSTEM EVENTS ====================
        
        /// <summary>Event: Early mods can start loading (GameHub ready, BaseGame available)</summary>
        public const string EarlyModsReady = "EarlyModsReady";
        
        /// <summary>Event: Base game finished loading (returns SDK wrappers)</summary>
        public const string BaseGameDetected = "BaseGameDetected";
        
        /// <summary>Event: GameHub/GameHubManager initialized (early game access)</summary>
        public const string GameHubInitialized = "GameHubInitialized";
        
        /// <summary>Event: Game fully loaded with all systems ready</summary>
        public const string GameFullyLoaded = "GameFullyLoaded";
        
        /// <summary>Event: Blackboard system initialized (variable storage ready)</summary>
        public const string BlackboardInitialized = "BlackboardInitialized";
        
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
        
        // ==================== TWITCH INTEGRATION EVENTS ====================
        
        /// <summary>Event: Someone followed the Twitch channel</summary>
        public const string TwitchFollow = "TwitchFollow";
        
        /// <summary>Event: Someone cheered bits in Twitch chat</summary>
        public const string TwitchBits = "TwitchBits";
        
        /// <summary>Event: Someone subscribed to the Twitch channel</summary>
        public const string TwitchSubscription = "TwitchSubscription";
        
        /// <summary>Event: Someone redeemed channel points on Twitch</summary>
        public const string TwitchChannelPoints = "TwitchChannelPoints";
        
        /// <summary>Event: Someone raided the Twitch channel</summary>
        public const string TwitchRaid = "TwitchRaid";
        
        /// <summary>Event: Someone hosted the Twitch channel</summary>
        public const string TwitchHost = "TwitchHost";
        
        /// <summary>Event: Twitch chat command was processed</summary>
        public const string TwitchChatCommand = "TwitchChatCommand";
        
        /// <summary>Event: Twitch integration status changed</summary>
        public const string TwitchStatusChanged = "TwitchStatusChanged";
    }
}
