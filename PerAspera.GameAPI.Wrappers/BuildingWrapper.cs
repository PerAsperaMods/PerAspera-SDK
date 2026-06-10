#nullable enable
using System;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Building class (typed interop access).
    /// Provides safe access to building properties and operations.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::Building"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\Building.cs.
    /// Les proxies interop exposent même les champs privés natifs (_built, _health…) en typé.
    /// Anciens noms fantômes (StartScrapping/CancelScrapping n'existaient pas) → [Obsolete]
    /// avec redirection vers les vrais membres (ToggleScrap/ForceScrapStart).
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class BuildingWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native building (compat). Prefer the typed overload.</summary>
        public BuildingWrapper(object nativeBuilding) : base(nativeBuilding) { }

        /// <summary>Wraps a typed interop Building proxy.</summary>
        public BuildingWrapper(Building nativeBuilding) : base(nativeBuilding) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>var hp = building.NativeBuilding?._health;</example>
        public Building? NativeBuilding => GetNativeObject() as Building;

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static BuildingWrapper? FromNative(object? native)
            => native != null ? new BuildingWrapper(native) : null;

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>Unique building identifier number.</summary>
        public int Number => NativeBuilding?.number ?? 0;

        /// <summary>Building type definition (factory, hab, etc.)</summary>
        public BuildingTypeWrapper GetBuildingType()
            => new BuildingTypeWrapper(NativeBuilding?.buildingType);

        /// <summary>Building type key (e.g., "solar_panel", "mine").</summary>
        public string TypeKey => NativeBuilding?.buildingType?.key ?? "Unknown";

        /// <summary>Building name from its type.</summary>
        public string BuildingTypeName => NativeBuilding?.buildingType?.name ?? "Unknown";

        // ==================== BASIC PROPERTIES ====================

        /// <summary>2D position on the planet surface.</summary>
        public (float x, float y) Position
        {
            get
            {
                var b = NativeBuilding;
                if (b == null) return (0f, 0f);
                var pos = b.position;
                return (pos.x, pos.y);
            }
        }

        /// <summary>Is the building alive (not destroyed). Typed read of Building._alive.</summary>
        public bool IsAlive => NativeBuilding?._alive ?? false;

        /// <summary>Is the building operational. Typed read of Building._activated.</summary>
        public bool IsOperative => NativeBuilding?._activated ?? false;

        /// <summary>Is the building fully built (not under construction).</summary>
        public bool IsBuilt => NativeBuilding?._built ?? false;

        /// <summary>Is the building powered.</summary>
        public bool IsPowered => NativeBuilding?._powered ?? false;

        /// <summary>Is the building broken.</summary>
        public bool IsBroken => NativeBuilding?._broken ?? false;

        /// <summary>Current health (0.0 to 1.0).</summary>
        public float Health => NativeBuilding?._health ?? 0f;

        /// <summary>Is the building active (alive, built, operational and not broken).</summary>
        public bool IsActive => IsAlive && IsBuilt && IsOperative && !IsBroken;

        // ==================== FACTION ====================

        /// <summary>Get the faction that owns this building.</summary>
        public FactionWrapper? GetFaction()
            => FactionWrapper.FromNative(NativeBuilding?.faction);

        // ==================== STOCKPILE ====================

        /// <summary>Get the building's stockpile (native Stockpile proxy).</summary>
        public Stockpile? GetStockpile() => NativeBuilding?.GetStockpile();

        /// <summary>
        /// Get resource quantity in stockpile (total stock, in units).
        /// Typed: Stockpile.GetTotalStock(ResourceType).ToFloat().
        /// (L'ancien binding « get_Item » n'existait pas — retournait toujours 0.)
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <example>float water = building.GetResourceStock("resource_water");</example>
        public float GetResourceStock(string resourceKey)
        {
            var stockpile = GetStockpile();
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey) as ResourceType;
            if (stockpile == null || resourceType == null) return 0f;
            return stockpile.GetTotalStock(resourceType).ToFloat();
        }

        // ==================== ACTIONS ====================

        /// <summary>
        /// Toggle operative state (typed write of Building.activated).
        /// ⚠️ L'ancienne implémentation visait « ToggleOperative » qui n'existe pas — elle
        /// n'a jamais rien fait. Celle-ci fonctionne réellement.
        /// </summary>
        /// <example>building.ToggleOperative();</example>
        public void ToggleOperative()
        {
            var b = NativeBuilding;
            if (b != null) b.activated = !b.activated;
        }

        /// <summary>Toggle pending-scrap state (native Building.ToggleScrap).</summary>
        /// <example>building.ToggleScrap();</example>
        public void ToggleScrap() => NativeBuilding?.ToggleScrap();

        /// <summary>Force scrapping to start immediately (native Building.ForceScrapStart).</summary>
        /// <example>building.ForceScrapStart();</example>
        public void ForceScrapStart() => NativeBuilding?.ForceScrapStart();

        /// <summary>True when the building can be scrapped (native Building.CanScrap).</summary>
        public bool CanScrap() => NativeBuilding?.CanScrap() ?? false;

        /// <summary>N'a jamais fonctionné — « StartScrapping » n'existe pas côté jeu.</summary>
        [Obsolete("Building.StartScrapping n'existe pas dans le jeu — n'a jamais rien fait. Utiliser ToggleScrap() ou ForceScrapStart().", false)]
        public void StartScrapping() => ToggleScrap();

        /// <summary>N'a jamais fonctionné — « CancelScrapping » n'existe pas côté jeu.</summary>
        [Obsolete("Building.CancelScrapping n'existe pas dans le jeu — n'a jamais rien fait. Utiliser ToggleScrap() (bascule l'état pending-scrap).", false)]
        public void CancelScrapping() => ToggleScrap();

        // ==================== INFO ====================

        /// <summary>Human-readable building summary.</summary>
        public override string ToString()
        {
            var pos = Position;
            return $"Building #{Number}: {TypeKey} at ({pos.x:F1}, {pos.y:F1}) — " +
                   $"Alive:{IsAlive} Built:{IsBuilt} Operative:{IsOperative}";
        }
    }
}
