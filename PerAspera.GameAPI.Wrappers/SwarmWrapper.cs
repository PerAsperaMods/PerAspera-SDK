using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    public class Way : WrapperBase
    {
        private Native.WayNative? _nativeWay;

        public Way(object nativeWay) : base(nativeWay)
        {
            try
            {
                _nativeWay = new Native.WayNative(nativeWay);
            }
            catch (Exception)
            {
                _nativeWay = null;
            }
        }

        public KeeperWrapper? GetKeeper()
        {
            try
            {
                var keeper = _nativeWay?.NativeInstance.GetFieldValue<object>("_keeper");
                return keeper != null ? new KeeperWrapper(keeper) : null;
            }
            catch
            {
                return new KeeperWrapper(SafeInvoke<Keeper>("get_keeper"));
            }
        }
    }
    
}
