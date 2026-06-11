using System;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Resource command execution utilities.
    /// Creates TextAction instances and dispatches them via InteractionManager (typed, zero reflection).
    ///
    /// MIGRATION 2026-06-10 - rewrite en acces type.
    /// Elimine ~38 RS0030 (AppDomain.GetAssemblies, GetMethod, GetProperty, ctor.Invoke).
    /// Source de verite :
    ///   Tools/InteropDump/ScriptsAssembly/TextAction.cs        : TextAction(string command, [Optional] Il2CppStringArray)
    ///   Tools/InteropDump/ScriptsAssembly/InteractionManager.cs : DispatchAction(IHandleable, GameEventBus, TextAction, string) public static
    ///   Tools/InteropDump/ScriptsAssembly/Faction.cs            : _gameEventBus GameEventBus (public property, ligne 13469)
    /// </summary>
    public static class ResourceCommandHelper
    {
        private static readonly LogAspera _log = new LogAspera("ResourceCommandHelper");

        /// <summary>
        /// Execute a resource import command for the given faction.
        /// Calls InteractionManager.DispatchAction directly (typed, zero reflection).
        /// </summary>
        /// <param name="handleable">The faction as IHandleable.</param>
        /// <param name="resourceType">Resource key (e.g. "WATER", "OXYGEN", "Ice").</param>
        /// <param name="amount">Quantity to add (default 1000).</param>
        /// <param name="bus">GameEventBus from the faction (Faction._gameEventBus). Pass null to dispatch without bus.</param>
        /// <returns>True on success.</returns>
        public static bool ExecuteResourceImportCommand(IHandleable handleable, string resourceType, float amount = 1000f, GameEventBus? bus = null)
        {
            if (handleable == null)
            {
                _log.Error("ResourceCommandHelper: handleable cannot be null");
                return false;
            }

            if (string.IsNullOrEmpty(resourceType))
            {
                _log.Error("ResourceCommandHelper: resourceType cannot be null or empty");
                return false;
            }

            try
            {
                var textAction = CreateNativeTextAction(resourceType, amount);
                if (textAction == null) return false;

                // InteractionManager.DispatchAction : public static void -- called directly, zero reflection
                InteractionManager.DispatchAction(handleable, bus, textAction, "ResourceImport_" + resourceType);
                _log.Info("ResourceImport_" + resourceType + " dispatched");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("ResourceCommandHelper: failed to execute command: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create a TextAction for the given resource type and amount.
        /// Uses the typed TextAction(string command) constructor from the interop proxy.
        /// </summary>
        /// <param name="resourceType">Resource key, will be normalised.</param>
        /// <param name="amount">Quantity.</param>
        /// <returns>A new TextAction, or null on error.</returns>
        public static TextAction? CreateNativeTextAction(string resourceType, float amount)
        {
            try
            {
                var normalizedType = NormalizeResourceType(resourceType);
                var commandString = "FactionAddResourceDistributed\t" + normalizedType + "\t" + (int)amount;
                // TextAction(string command, [Optional] Il2CppStringArray) -- [Optional] defaults to empty array
                return new TextAction(commandString);
            }
            catch (Exception ex)
            {
                _log.Error("ResourceCommandHelper: failed to create TextAction: " + ex.Message);
                return null;
            }
        }

        // --- Private helpers --------------------------------------------------

        private static string NormalizeResourceType(string resourceType)
        {
            return resourceType.ToUpperInvariant() switch
            {
                "WATER" or "H2O"                                     => "WATER",
                "ICE"                                                => "Ice",
                "CHG" or "METAL" or "IRON" or "METHANE" or "CARBON" => "CHG",
                "NITROGEN"                                           => "NITROGEN",
                "OXYGEN"                                             => "OXYGEN",
                _                                                    => resourceType.ToUpperInvariant()
            };
        }
    }
}