using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class FinishInjectionContext : WrapperBase
    {
        public FinishInjectionContext(object nativeBaseGame) : base(nativeBaseGame)
        {
        }

        public Keeper? GetKeeper()
        {
            return SafeInvoke<Keeper>("keeper"); // public Keeper keeper;
        }
    }
    
}
