using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Wrappers
{
    public class VisualResourceVeinWrapper : WrapperBase
    {
        private VisualResourceVein? _nativeVisualResourceVein;

        public VisualResourceVeinWrapper(object nativeVisualResourceVein) : base(nativeVisualResourceVein)
        {
            // Try to cast to native type for direct access
            try
            {
                _nativeVisualResourceVein = (VisualResourceVein)nativeVisualResourceVein;
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"Failed to cast to VisualResourceVein, using reflection fallback: {ex.Message}");
            }
        }
    }
}
