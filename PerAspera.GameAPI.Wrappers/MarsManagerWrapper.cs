using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class MarsManagerWrapper : WrapperBase
    {
        private MarsManager? nativeMarsManager;
        public MarsManagerWrapper(object? nativeObject) : base(nativeObject)
        {
            nativeMarsManager = nativeObject as MarsManager;
        }

        public MeshRendererWrapper? waterRenderer => new MeshRendererWrapper(SafeInvoke<object>("get_waterRenderer"));
        public object? marsRenderer => SafeInvoke<object>("get_marsRenderer");

    }
}
