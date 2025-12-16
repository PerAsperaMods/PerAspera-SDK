using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== MOD SYSTEM EVENTS ====================

    /// <summary>
    /// Event triggered when a mod system is initialized
    /// </summary>
    public class ModSystemInitializedEvent : SDKEventBase
    {
        public override string EventType => "ModSystemInitialized";
        
        public string ModName { get; }
        public string ModVersion { get; }
        public object? ModInstance { get; }

        public ModSystemInitializedEvent(string modName, string modVersion = "1.0.0", object? modInstance = null)
        {
            ModName = modName;
            ModVersion = modVersion;
            ModInstance = modInstance;
        }
    }

    /// <summary>
    /// Event triggered when a mod system shuts down
    /// </summary>
    public class ModSystemShutdownEvent : SDKEventBase
    {
        public override string EventType => "ModSystemShutdown";
        
        public string ModName { get; }
        public string Reason { get; }

        public ModSystemShutdownEvent(string modName, string reason = "Normal shutdown")
        {
            ModName = modName;
            Reason = reason;
        }
    }

    /// <summary>
    /// Event triggered when command system is ready
    /// </summary>
    public class CommandSystemReadyEvent : SDKEventBase
    {
        public override string EventType => "CommandSystemReady";
        
        public object? CommandBus { get; }
        public bool IsRemoteCapable { get; }

        public CommandSystemReadyEvent(object? commandBus = null, bool isRemoteCapable = false)
        {
            CommandBus = commandBus;
            IsRemoteCapable = isRemoteCapable;
        }
    }
}
