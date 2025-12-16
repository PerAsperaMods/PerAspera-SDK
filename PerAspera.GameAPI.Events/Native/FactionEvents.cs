// DOC REFERENCES:
// - BaseGame.md: OnEvent_FactionCloseAllWindows, OnEvent_OnVeinOrSiteRevealed

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== FACTION EVENTS ====================
    // DOC: BaseGame.md - OnEvent_FactionCloseAllWindows, OnEvent_OnVeinOrSiteRevealed
    
    /// <summary>
    /// Native event: Faction requested close all windows
    /// DOC: BaseGame.md - OnEvent_FactionCloseAllWindows (Faction sender, ref GameEvent evt)
    /// </summary>
    public class FactionCloseAllWindowsNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:FactionCloseAllWindows";
        
        public object? Faction { get; set; }
        public string FactionName { get; set; } = string.Empty;

        public override string ToString() => 
            $"FactionCloseAllWindows: {FactionName} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Vein or site revealed to faction
    /// DOC: BaseGame.md - OnEvent_OnVeinOrSiteRevealed (Faction sender, ref GameEvent evt)
    /// </summary>
    public class VeinOrSiteRevealedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:VeinOrSiteRevealed";
        
        public object? Faction { get; set; }
        public object? VeinOrSite { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"VeinOrSiteRevealed: {ResourceType} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
    }
}
