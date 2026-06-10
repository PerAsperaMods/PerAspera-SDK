using PerAspera.GameAPI.Commands.Builders;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.Extensions
{
    /// <summary>
    /// Extension methods for CommandBuilder to provide fluent API for common parameters
    /// </summary>
    public static class CommandBuilderExtensions
    {
        /// <summary>
        /// Set resource for ImportResource commands
        /// </summary>
        public static CommandBuilder Resource(this CommandBuilder builder, string resourceName)
        {
            return builder.WithParameter("ResourceName", resourceName);
        }
        
        /// <summary>
        /// Set amount for ImportResource and similar commands
        /// </summary>
        public static CommandBuilder Amount(this CommandBuilder builder, float amount)
        {
            return builder.WithParameter("Amount", amount);
        }
        
        /// <summary>
        /// Set quantity for commands that need integer quantities
        /// </summary>
        public static CommandBuilder Quantity(this CommandBuilder builder, int quantity)
        {
            return builder.WithParameter("Quantity", quantity);
        }
        
        /// <summary>
        /// Set building for UnlockBuilding commands
        /// </summary>
        public static CommandBuilder Building(this CommandBuilder builder, object building)
        {
            return builder.WithParameter("Building", building);
        }
        
        /// <summary>
        /// Set technology for ResearchTechnology commands
        /// </summary>
        public static CommandBuilder Technology(this CommandBuilder builder, object technology)
        {
            return builder.WithParameter("Technology", technology);
        }
    }
}
