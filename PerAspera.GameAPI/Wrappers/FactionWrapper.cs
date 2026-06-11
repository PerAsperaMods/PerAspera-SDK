#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerAspera.Commands;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Faction class (typed interop access).
    /// Provides safe access to faction properties and operations.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::Faction"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\Faction.cs. Nombreuses API fantômes
    /// corrigées : displayName/factionType/isPlayerFaction/mainStockpile/aiDifficulty/
    /// aiPersonality/color n'existent pas ; GetResourceStock et HasTechnology étaient des
    /// chaînes de noms devinés qui échouaient toutes — réimplémentées sur les vrais membres.
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class FactionWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native faction (compat). Prefer the typed overload.</summary>
        public FactionWrapper(object nativeFaction) : base(nativeFaction) { }

        /// <summary>Wraps a typed interop Faction proxy.</summary>
        public FactionWrapper(Faction nativeFaction) : base(nativeFaction) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>var bb = faction.NativeFaction?.blackboardFaction;</example>
        public Faction? NativeFaction => GetNativeObject() as Faction;

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static FactionWrapper? FromNative(object? nativeFaction)
            => nativeFaction != null ? new FactionWrapper(nativeFaction) : null;

        // ==================== CONSOLE COMMANDS ====================

        /// <summary>Add resources distributed across faction buildings via console command.</summary>
        /// <example>faction.FactionAddResourceDistributed("resource_water", "500");</example>
        public bool FactionAddResourceDistributed(string resourceString, string amountString)
        {
            var consoleWrapper = ConsoleWrapper.GetInstance();
            if (consoleWrapper == null)
            {
                WrapperLog.Warning("Console wrapper not available for FactionAddResourceDistributed");
                return false;
            }
            try
            {
                return consoleWrapper.ExecuteCommandString(
                    $"factionaddresourcedistributed {resourceString} {amountString}");
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"FactionAddResourceDistributed failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>List available console commands in the log.</summary>
        public static void ListAvailableConsoleCommands()
            => ConsoleWrapper.GetInstance()?.ListCommands();

        // ==================== NATIVE METHODS ====================

        /// <summary>Add research points to this faction (typed).</summary>
        /// <example>faction.AddResearchPoints(100f);</example>
        public bool AddResearchPoints(float amount)
        {
            var f = NativeFaction;
            if (f == null) return false;
            f.AddResearchPoints(amount);
            return true;
        }

        /// <summary>N'a jamais fonctionné — méthode d'instance inexistante.</summary>
        [Obsolete("Invocation string-based de « FactionAddResourceDistributed » en instance — n'a jamais fonctionné. Utiliser AddResource() (typé) ou FactionAddResourceDistributed() (console).", false)]
        public bool TestAddResource(string resourceType, float amount)
            => AddResource(resourceType, amount);

        /// <summary>
        /// Sets a bool variable on the faction blackboard (typed).
        /// Used by YAML rules/criteria: <c>$key == true</c>.
        /// </summary>
        /// <example>faction.SetBlackboardBool("mon_flag", true);</example>
        public bool SetBlackboardBool(string key, bool value)
        {
            var bb = NativeFaction?.blackboardFaction;
            if (bb == null) return false;
            bb.SetValue(key, value);
            return true;
        }

        // ==================== HANDLE / INTERACTION ====================

        /// <summary>Get the interaction manager (typed read of Faction.interactionManager).</summary>
        public InteractionManagerWrapper GetInteractionManager()
            => new InteractionManagerWrapper(NativeFaction?.interactionManager);

        /// <summary>Cast the native faction to IHandleable.</summary>
        public IHandleable? GetAsIHandleable()
        {
            try { return (IHandleable?)GetNativeObject(); }
            catch (InvalidCastException)
            {
                WrapperLog.Warning("Cannot cast Faction to IHandleable in IL2CPP context");
                return null;
            }
        }

        /// <summary>Get the faction handle (typed read of Faction.handle).</summary>
        public HandleWrapper? GetHandle()
        {
            var h = NativeFaction?.handle;
            return h != null ? HandleWrapper.FromNative(h) : null;
        }

        /// <summary>Get the raw native handle object.</summary>
        public object? GetRawHandle() => NativeFaction?.handle;

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>Faction name (typed read of Faction.name).</summary>
        public string Name => NativeFaction?.name ?? "Unknown";

        /// <summary>N'a jamais existé — retournait toujours Name.</summary>
        [Obsolete("Faction.displayName n'existe pas dans le jeu — retournait toujours Name. Utiliser Name.", false)]
        public string DisplayName => Name;

        /// <summary>N'a jamais existé sur Faction.</summary>
        [Obsolete("Faction.factionType n'existe pas dans le jeu — retournait toujours null.", false)]
        public object? FactionType => null;

        /// <summary>
        /// True when this faction is the player faction.
        /// (L'ancienne lecture de « isPlayerFaction » n'existait pas — toujours false.
        /// Réimplémenté par comparaison avec Universe.GetPlayerFaction().)
        /// </summary>
        public bool IsPlayerFaction
        {
            get
            {
                var me = NativeFaction;
                if (me == null) return false;
                var player = UniverseWrapper.GetCurrent()?.NativeUniverse?.GetPlayerFaction();
                return player != null && me.Pointer == player.Pointer;
            }
        }

        // ==================== RESOURCES ====================

        /// <summary>N'a jamais existé sur Faction.</summary>
        [Obsolete("Faction.mainStockpile n'existe pas dans le jeu — retournait toujours null. Les stocks sont par bâtiment : GetResourceStock() les agrège.", false)]
        public object? MainStockpile => null;

        /// <summary>
        /// Total resource stock across all faction buildings (units).
        /// (L'ancienne chaîne GetResourceStock/GetStock/GetResourceAmount n'existait pas —
        /// retournait toujours 0. Réimplémenté en agrégeant les stockpiles des bâtiments.)
        /// </summary>
        /// <example>float water = faction.GetResourceStock("resource_water");</example>
        public float GetResourceStock(string resourceKey)
        {
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey) as ResourceType;
            var buildings = NativeFaction?.buildings;
            if (resourceType == null || buildings == null) return 0f;

            float total = 0f;
            foreach (var b in buildings)
            {
                var stockpile = b?.GetStockpile();
                if (stockpile != null)
                    total += stockpile.GetTotalStock(resourceType).ToFloat();
            }
            return total;
        }

        /// <summary>
        /// Add resource to faction (typed Faction.AddResource(ResourceType, int)).
        /// Use negative amount to remove. Amount is truncated to whole units.
        /// </summary>
        /// <example>faction.AddResource("resource_water", 500f);</example>
        public bool AddResource(string resourceKey, float amount)
        {
            var f = NativeFaction;
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey) as ResourceType;
            if (f == null || resourceType == null) return false;
            f.AddResource(resourceType, (int)amount);
            return true;
        }

        /// <summary>
        /// Stock total de la faction pour une ressource, via <c>Faction.GetResourceQuantity(ResourceType)</c>.
        /// Plus performant que <see cref="GetResourceStock"/> (pas d'agrégation bâtiment-par-bâtiment).
        /// Retourne 0 si la faction ou le type de ressource est introuvable.
        /// </summary>
        /// <example>float iron = faction.GetResourceStockTyped("resource_iron");</example>
        public float GetResourceStockTyped(string resourceKey)
        {
            var f = NativeFaction;
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey) as ResourceType;
            if (f == null || resourceType == null) return 0f;
            return f.GetResourceQuantity(resourceType);
        }

        /// <summary>
        /// Retire des ressources de la faction de manière atomique côté SDK.
        /// Vérifie le stock avant de débiter ; retourne false sans modifier le stock si insuffisant.
        /// Utilise <c>Faction.AddResource(ResourceType, int)</c> avec montant négatif.
        /// </summary>
        /// <param name="resourceKey">Clé YAML de la ressource (ex: "resource_iron").</param>
        /// <param name="amount">Quantité à retirer (doit être &gt; 0).</param>
        /// <returns>True si le débit a été effectué, false si stock insuffisant ou paramètres invalides.</returns>
        /// <example>
        /// if (!faction.TryRemoveResource("resource_iron", 100f))
        ///     return TradeResult.Fail("Stock insuffisant");
        /// </example>
        public bool TryRemoveResource(string resourceKey, float amount)
        {
            if (amount <= 0f) return false;
            var f = NativeFaction;
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey) as ResourceType;
            if (f == null || resourceType == null) return false;

            var current = f.GetResourceQuantity(resourceType);
            if (current < amount) return false;

            f.AddResource(resourceType, -(int)amount);
            return true;
        }

        // ==================== RELATIONS ====================

        /// <summary>N'a jamais fonctionné — aucun de ces noms n'existe.</summary>
        [Obsolete("GetRelationship/GetDiplomacyStatus/GetStanding n'existent pas sur Faction — retournait toujours null. Pas de système de diplomatie exposé.", false)]
        public float? GetRelationshipWith(FactionWrapper otherFaction) => null;

        // ==================== BUILDINGS ====================

        /// <summary>All buildings of this faction (typed read of Faction.buildings).</summary>
        /// <example>foreach (var b in faction.GetBuildings()) { ... }</example>
        public List<BuildingWrapper> GetBuildings()
        {
            var result = new List<BuildingWrapper>();
            var buildings = NativeFaction?.buildings;
            if (buildings == null) return result;
            foreach (var b in buildings)
                if (b != null) result.Add(new BuildingWrapper(b));
            return result;
        }

        // ==================== AI BEHAVIOR ====================

        /// <summary>N'a jamais existé sur Faction.</summary>
        [Obsolete("Faction.aiDifficulty/difficultyLevel n'existent pas — retournait toujours 0. La difficulté globale : BaseGameWrapper.GameDifficulty.", false)]
        public int AIDifficulty => 0;

        /// <summary>True when this faction is AI-controlled (= not the player faction).</summary>
        public bool IsAI => !IsPlayerFaction;

        /// <summary>N'a jamais existé sur Faction.</summary>
        [Obsolete("Faction.aiPersonality/behaviorType n'existent pas — retournait toujours \"default\".", false)]
        public string AIPersonality => "default";

        // ==================== TECHNOLOGY ====================

        /// <summary>
        /// True when the technology is researched.
        /// (L'ancienne chaîne HasTechnology/IsTechResearched/HasResearched n'existait pas —
        /// retournait toujours false. Réimplémenté via Faction.GetResearchedTechnologies().)
        /// </summary>
        /// <example>bool done = faction.HasTechnology("tech_solar_panels");</example>
        public bool HasTechnology(string technologyKey)
        {
            var f = NativeFaction;
            if (f == null || string.IsNullOrEmpty(technologyKey)) return false;
            var researched = f.GetResearchedTechnologies();
            if (researched == null) return false;

            // IEnumerable<T> IL2CPP : MoveNext vit sur l'interface non-générique → Cast explicite
            var enumerator = researched.GetEnumerator();
            var iterator = enumerator.Cast<Il2CppSystem.Collections.IEnumerator>();
            while (iterator.MoveNext())
            {
                var tech = enumerator.Current;
                if (tech?.TechnologyType?.key == technologyKey) return true;
            }
            return false;
        }

        /// <summary>
        /// Research a technology immediately (typed Faction.ResearchTechnology(TechnologyType)).
        /// </summary>
        /// <example>faction.ResearchTechnology("tech_solar_panels");</example>
        public bool ResearchTechnology(string technologyKey)
        {
            var f = NativeFaction;
            if (f == null || !TechnologyType.Has(technologyKey)) return false;
            var techType = TechnologyType.Get(technologyKey);
            if (techType == null) return false;
            f.ResearchTechnology(techType);
            return true;
        }

        // ==================== UTILITIES ====================

        /// <summary>N'a jamais fonctionné — Faction n'a pas de couleur exposée.</summary>
        [Obsolete("Faction.color/factionColor n'existent pas — retournait toujours Gray.", false)]
        public System.Drawing.Color GetColor() => System.Drawing.Color.Gray;

        // ==================== GAME EVENT BUS ====================

        /// <summary>Game event bus of this faction (typed read of Faction._gameEventBus).</summary>
        public GameEventBus? GetGameEventBus() => NativeFaction?._gameEventBus;

        // ==================== COMMAND EXECUTION ====================

        /// <summary>Execute a resource import command via the interaction manager.</summary>
        /// <example>faction.ExecuteResourceImportCommand("resource_water", 1000f);</example>
        public bool ExecuteResourceImportCommand(string resourceType, float amount = 1000f)
        {
            var handleable = GetAsIHandleable();
            if (handleable == null) return false;
            var textAction = ResourceCommandHelper.CreateNativeTextAction(resourceType, amount);
            if (textAction == null) return false;
            var bus = NativeFaction?._gameEventBus;
            InteractionManager.DispatchAction(handleable, bus, textAction, "ExecuteResourceImportCommand");
            return true;
        }

        /// <summary>Execute a custom resource command by type name.</summary>
        public bool ExecuteCustomCommand(string commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                var handleable = GetAsIHandleable();
                if (handleable == null)
                {
                    WrapperLog.Error($"Cannot get IHandleable for faction {Name}");
                    return false;
                }
                float amount = parameters?.ContainsKey("amount") == true
                    ? Convert.ToSingle(parameters["amount"]) : 1000f;
                return ResourceCommandHelper.ExecuteResourceImportCommand(handleable, commandType, amount, NativeFaction?._gameEventBus);
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"ExecuteCustomCommand {commandType} failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generic command dispatch via CommandBus. Avoids creating a wrapper per command type.
        /// ⚠️ Réflexion managed (RS0030) — à migrer vers GameAPI.Commands à terme.
        /// </summary>
        /// <param name="commandTypeName">Full type name (e.g., "PerAspera.Commands.CmdFactionResourceAllocation")</param>
        /// <param name="constructorArgs">Constructor arguments for the command.</param>
        public bool DispatchCommand(string commandTypeName, params object[] constructorArgs)
        {
            try
            {
                var scriptsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly");
                var cmdType = scriptsAssembly?.GetType(commandTypeName);
                if (cmdType == null)
                {
                    WrapperLog.Error($"Command type not found: {commandTypeName}");
                    return false;
                }

                var constructor = cmdType.GetConstructor(constructorArgs.Select(a => a.GetType()).ToArray());
                if (constructor == null)
                {
                    WrapperLog.Error($"Constructor not found for command: {commandTypeName}");
                    return false;
                }

                var cmdInstance = constructor.Invoke(constructorArgs);
                var commandBus = UniverseWrapper.GetCurrent()?.GetCommandBus();
                if (commandBus == null)
                {
                    WrapperLog.Error("CommandBus not available");
                    return false;
                }

                // IL2CppExtensions.InvokeMethod — RS0030-exempt (Core)
                commandBus.InvokeMethod("Dispatch", cmdInstance);
                return true;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"DispatchCommand {commandTypeName} failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Debug summary of command-related state.</summary>
        public string GetCommandDebugInfo()
        {
            var handle = GetHandle();
            return $"Faction Command Debug:\n" +
                   $"  Name: {Name}\n" +
                   $"  Handle: {(handle != null ? "Available" : "Not Available")}\n" +
                   $"  IsPlayer: {IsPlayerFaction}  IsValid: {IsValidWrapper}";
        }

        /// <summary>Human-readable faction summary.</summary>
        public override string ToString()
            => $"Faction[{Name}] (Valid:{IsValidWrapper} Player:{IsPlayerFaction} AI:{IsAI})";
    }
}
