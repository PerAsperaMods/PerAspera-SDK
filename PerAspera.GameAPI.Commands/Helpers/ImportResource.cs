using PerAspera.GameAPI.Commands.Builders;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.Helpers
{
    /// <summary>
    /// Helper class for creating ImportResource commands with fluent API
    /// MVP implementation for testing Commands Native Bridge
    /// </summary>
    public static class ImportResource
    {
        /// <summary>
        /// Start building an ImportResource command
        /// </summary>
        /// <returns>CommandBuilder configured for ImportResource</returns>
        public static CommandBuilder Create()
        {
            return Commands.Create(NativeCommandTypes.ImportResource);
        }
        
        /// <summary>
        /// Create ImportResource command for specific resource and amount
        /// </summary>
        /// <param name="resourceName">Resource name (e.g., "water", "carbon")</param>
        /// <param name="amount">Amount to import</param>
        /// <returns>Configured CommandBuilder</returns>
        public static CommandBuilder Create(string resourceName, float amount)
        {
            return Create()
                .WithParameter("ResourceName", resourceName)
                .WithParameter("Amount", amount);
        }
    }
}