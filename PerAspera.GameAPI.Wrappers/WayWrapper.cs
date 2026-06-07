#nullable enable
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    public class Way : WrapperBase
    {
        public Way(object nativeWay) : base(nativeWay) { }

        public static Way? FromNative(object? native)
            => native != null ? new Way(native) : null;

        public KeeperWrapper? GetKeeper()
        {
            var keeper = SafeGetField<object>("_keeper")
                      ?? SafeInvoke<object>("get_keeper");
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
    }
}
