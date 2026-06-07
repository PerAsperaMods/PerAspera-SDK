#nullable enable
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    public class Swarm : WrapperBase
    {
        public Swarm(object nativeSwarm) : base(nativeSwarm) { }

        public static Swarm? FromNative(object? native)
            => native != null ? new Swarm(native) : null;

        public KeeperWrapper? GetKeeper()
        {
            var keeper = SafeGetField<object>("_keeper")
                      ?? SafeInvoke<object>("get_keeper");
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
    }
}
