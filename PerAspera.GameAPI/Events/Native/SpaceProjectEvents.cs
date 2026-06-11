using System.Collections.Generic;
using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== SPACE PROJECT EVENTS ====================
    // Ancrage natif : PortProject.SetStage, SpecialProject.Complete,
    // SpecialProject.DispatchLaunchActions, SpecialProject.RecoverGatheredResources
    // Vérifiés dans Tools\InteropDump\ScriptsAssembly\PortProject.cs + SpecialProject.cs

    /// <summary>
    /// Émis en Postfix de <c>PortProject.SetStage(SpaceportStage)</c>.
    /// Couvre toutes les transitions Gather → Trade → Complete (et annulation).
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(PortProjectStageChangedNativeEvent.Key, e =>
    /// {
    ///     var ev = (PortProjectStageChangedNativeEvent)e;
    ///     if (ev.NewStageKey == "complete") OrbitalStockService.Instance.Flush(ev.ProjectKey);
    /// });
    /// </example>
    public class PortProjectStageChangedNativeEvent : NativeGameEventBase
    {
        public const string Key = "Native:PortProjectStageChanged";
        public override string EventType => Key;

        /// <summary>Référence au PortProject natif (proxy interop).</summary>
        public PortProject? NativePortProject { get; set; }

        /// <summary>Clé du projet : <c>SpecialProject.type.name</c> (ex: "mission_arean_survey").</summary>
        public string ProjectKey { get; set; } = string.Empty;

        /// <summary>Clé du stage précédent : <c>SpaceportStage.key</c>. Vide si premier stage.</summary>
        public string PreviousStageKey { get; set; } = string.Empty;

        /// <summary>Clé du nouveau stage : <c>SpaceportStage.key</c>.</summary>
        public string NewStageKey { get; set; } = string.Empty;

        public override string ToString() =>
            $"PortProjectStageChanged: [{ProjectKey}] {PreviousStageKey} → {NewStageKey} (Sol {MartianSol})";
    }

    /// <summary>
    /// Émis en Postfix de <c>SpecialProject.DispatchLaunchActions()</c>.
    /// Indique un lancement orbital (rocket fired).
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(SpecialProjectLaunchedNativeEvent.Key, e =>
    /// {
    ///     var ev = (SpecialProjectLaunchedNativeEvent)e;
    ///     LogAspera.Info($"Project {ev.ProjectKey} launched #{ev.Launches}");
    /// });
    /// </example>
    public class SpecialProjectLaunchedNativeEvent : NativeGameEventBase
    {
        public const string Key = "Native:SpecialProjectLaunched";
        public override string EventType => Key;

        /// <summary>Référence au SpecialProject natif (proxy interop).</summary>
        public SpecialProject? NativeSpecialProject { get; set; }

        /// <summary>Clé du projet : <c>SpecialProject.type.name</c>.</summary>
        public string ProjectKey { get; set; } = string.Empty;

        /// <summary>Nombre total de lancements après cet événement (<c>SpecialProject.launches</c>).</summary>
        public int Launches { get; set; }

        public override string ToString() =>
            $"SpecialProjectLaunched: [{ProjectKey}] launch #{Launches} (Sol {MartianSol})";
    }

    /// <summary>
    /// Émis en Postfix de <c>SpecialProject.Complete()</c>.
    /// Fournit un snapshot des ressources collectées au moment de la complétion.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(SpecialProjectCompletedNativeEvent.Key, e =>
    /// {
    ///     var ev = (SpecialProjectCompletedNativeEvent)e;
    ///     foreach (var (res, qty) in ev.GatheredResources)
    ///         OrbitalStock.Credit(res, qty, ev.ProjectKey);
    /// });
    /// </example>
    public class SpecialProjectCompletedNativeEvent : NativeGameEventBase
    {
        public const string Key = "Native:SpecialProjectCompleted";
        public override string EventType => Key;

        /// <summary>Référence au SpecialProject natif (proxy interop).</summary>
        public SpecialProject? NativeSpecialProject { get; set; }

        /// <summary>Clé du projet : <c>SpecialProject.type.name</c>.</summary>
        public string ProjectKey { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot des ressources collectées au moment de Complete().
        /// Clés = <c>ResourceType.name</c>. Quantités en unités (float, via <c>CargoQuantity.ToFloat()</c>).
        /// </summary>
        public IReadOnlyDictionary<string, float> GatheredResources { get; set; }
            = new Dictionary<string, float>();

        public override string ToString() =>
            $"SpecialProjectCompleted: [{ProjectKey}] ({GatheredResources.Count} resource types, Sol {MartianSol})";
    }

    /// <summary>
    /// Émis en Postfix de <c>SpecialProject.RecoverGatheredResources(SpacePortComponent)</c>.
    /// Signale une annulation de projet — les ressources collectées sont remboursées au joueur.
    /// Ne pas créditer le stock orbital sur cet événement.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.Subscribe(PortProjectCancelledNativeEvent.Key, e =>
    /// {
    ///     var ev = (PortProjectCancelledNativeEvent)e;
    ///     OrbitalStockService.Instance.InvalidatePending(ev.ProjectKey);
    /// });
    /// </example>
    public class PortProjectCancelledNativeEvent : NativeGameEventBase
    {
        public const string Key = "Native:PortProjectCancelled";
        public override string EventType => Key;

        /// <summary>Référence au SpecialProject natif (proxy interop).</summary>
        public SpecialProject? NativeSpecialProject { get; set; }

        /// <summary>Clé du projet : <c>SpecialProject.type.name</c>.</summary>
        public string ProjectKey { get; set; } = string.Empty;

        public override string ToString() =>
            $"PortProjectCancelled: [{ProjectKey}] (Sol {MartianSol})";
    }
}
