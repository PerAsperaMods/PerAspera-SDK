using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    public class MeshRendererWrapper : WrapperBase
    {
        private MeshRenderer? _native;

        public MeshRendererWrapper(object? nativeObject) : base(nativeObject)
        {
            _native = nativeObject as MeshRenderer;
        }

        public Material? SharedMaterial => _native?.sharedMaterial;
        public Material? Material => _native?.material;
        public bool Enabled
        {
            get => _native?.enabled ?? false;
            set { if (_native != null) _native.enabled = value; }
        }
    }
}
