using PerAspera.Core.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class GameEventBusWrapper : WrapperBase
    {
        public GameEventBusWrapper(object nativeBaseGame) : base(nativeBaseGame)
        {
        }

        public KeeperWrapper? GetKeeper()
        {
            return new KeeperWrapper(NativeObject.GetFieldValue<Keeper>("_keeper")); //private Keeper _keeper;
        }
    }
    
}
