using PerAspera.Core;

namespace PerAspera.GameAPI.ResourceInit
{
    internal static class ResourceInitLog
    {
        private static readonly LogAspera _log = new LogAspera("ResourceInit");
        internal static void Info(string msg) => _log.Info(msg);
        internal static void Warning(string msg) => _log.Warning(msg);
        internal static void Error(string msg) => _log.Error(msg);
    }
}
