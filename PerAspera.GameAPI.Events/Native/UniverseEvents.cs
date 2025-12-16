// DOC REFERENCES:
// - BaseGame.md: OnEvent_UniverseExplosion, OnEvent_UniverseHideVein
// - Universe.md: GevUniverse* GameEventType static fields

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== UNIVERSE EVENTS ====================
    // DOC: BaseGame.md - OnEvent_UniverseExplosion, OnEvent_UniverseHideVein
    
    /// <summary>
    /// Native event: Explosion in universe
    /// DOC: BaseGame.md - OnEvent_UniverseExplosion (Universe sender, ref GameEvent evt)
    /// </summary>
    public class UniverseExplosionNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseExplosion";
        
        public object? Universe { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        public float? PositionZ { get; set; }
        public float Magnitude { get; set; }

        public override string ToString() => 
            $"Explosion: Magnitude {Magnitude:F1} at ({PositionX:F1}, {PositionY:F1}, {PositionZ:F1}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Vein hidden in universe
    /// DOC: BaseGame.md - OnEvent_UniverseHideVein (Universe sender, ref GameEvent evt)
    /// </summary>
    public class UniverseHideVeinNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseHideVein";
        
        public object? Universe { get; set; }
        public object? Vein { get; set; }
        public string ResourceType { get; set; } = string.Empty;

        public override string ToString() => 
            $"VeinHidden: {ResourceType} - Sol {MartianSol}";
    }

    // ==================== UNIVERSE GLOBAL EVENTS ====================
    // DOC: Universe.md - GevUniverse* GameEventType static fields
    
    /// <summary>
    /// Native event: Universe stats updated
    /// DOC: Universe.md - GevUniverseStatsUpdated
    /// </summary>
    public class UniverseStatsUpdatedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseStatsUpdated";
        
        public object? Universe { get; set; }
        public object? StatsData { get; set; }

        public override string ToString() => 
            $"UniverseStatsUpdated - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Faction swapped
    /// DOC: Universe.md - GevUniverseSwapFaction
    /// </summary>
    public class UniverseSwapFactionNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseSwapFaction";
        
        public object? Universe { get; set; }
        public object? PreviousFaction { get; set; }
        public object? NewFaction { get; set; }
        public string PreviousFactionName { get; set; } = string.Empty;
        public string NewFactionName { get; set; } = string.Empty;

        public override string ToString() => 
            $"FactionSwapped: {PreviousFactionName} → {NewFactionName} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Game over
    /// DOC: Universe.md - GevUniverseGameOver
    /// </summary>
    public class UniverseGameOverNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseGameOver";
        
        public object? Universe { get; set; }
        public string GameOverReason { get; set; } = string.Empty;
        public bool Victory { get; set; }

        public override string ToString() => 
            $"GameOver: {GameOverReason} ({(Victory ? "Victory" : "Defeat")}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: New game started
    /// DOC: Universe.md - GevUniverseNewGameStarted
    /// </summary>
    public class UniverseNewGameStartedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseNewGameStarted";
        
        public object? Universe { get; set; }
        public string GameMode { get; set; } = string.Empty;
        public object? InitialSettings { get; set; }

        public override string ToString() => 
            $"NewGameStarted: {GameMode} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Continue ended game
    /// DOC: Universe.md - GevUniverseContinueEndedGame
    /// </summary>
    public class UniverseContinueEndedGameNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:UniverseContinueEndedGame";
        
        public object? Universe { get; set; }
        public string SaveGameName { get; set; } = string.Empty;

        public override string ToString() => 
            $"ContinueEndedGame: {SaveGameName} - Sol {MartianSol}";
    }
}
