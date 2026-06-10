using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class HazardsManagerWrapper : WrapperBase
    {

        private static readonly LogAspera Log = new LogAspera("HazardsManagerWrapper");
        public HazardsManagerWrapper(object nativeObject) : base(nativeObject)
        {

        }

        public List<object> getHazardsList()
        {
            try
            {
                List<object> hazards = (List<object>)(((HazardsManager)NativeObject).hazards).ConvertIl2CppList<object>(); // Access to ensure the field is loaded
                return hazards;

            }
            catch (Exception ex)
            {
                Log.Error($"Error getting hazards list: {ex.Message}");
                return new List<object>();
            }
        }


    }
}
