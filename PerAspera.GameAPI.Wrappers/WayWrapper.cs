using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    public class Swarm : WrapperBase
    {
        private Native.SwarmNative? _nativeSwarm;

        public Swarm(object nativeSwarm) : base(nativeSwarm)
        {
            try
            {
                _nativeSwarm = new Native.SwarmNative(nativeSwarm);
            }
            catch (Exception)
            {
                _nativeSwarm = null;
            }
        }

        public KeeperWrapper? GetKeeper()
        {
            try
            {
                var keeper = _nativeSwarm?.NativeInstance.GetFieldValue<object>("_keeper");
                return keeper != null ? new KeeperWrapper(keeper) : null;
            }
            catch
            {
                return new KeeperWrapper(SafeInvoke<Keeper>("get_keeper"));
            }
        }
    }
    
}
