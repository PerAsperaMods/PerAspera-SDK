using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class BaseGameReferencesWrapper : WrapperBase
    {
        private BaseGameReferences? nativeBaseGameReferences; 
        public BaseGameReferencesWrapper(object? nativeObject) : base(nativeObject)
        {
            nativeBaseGameReferences= nativeObject as BaseGameReferences;
        }

        public MarsManagerWrapper? Mars => new MarsManagerWrapper(SafeInvoke<object>("get_mars"));

    }
}
