using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class Way : WrapperBase
    {
        public Way(object nativeBaseGame) : base(nativeBaseGame)
        {
        }

        public KeeperWrapper? GetKeeper()
        {
            return     new KeeperWrapper(SafeInvoke<Keeper>("get_keeper")); // private Keeper _keeper;
        }
    }
    
}
