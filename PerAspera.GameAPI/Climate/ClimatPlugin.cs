using BepInEx.Unity.IL2CPP;
using LlockhamIndustries.Decals;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Climate
{
    public class ClimatPlugin : BasePlugin
    {
        private LogAspera Log = new LogAspera("Climate.Plugin");

        private PlanetWrapper _planet;
        private AtmosphereGrid _atmosphereGrid;

        public override void Load()
        {
            EnhancedEventBus.SubscribeToGameFullyLoaded(OnLoadFinished);
        }

        private void OnLoadFinished(GameFullyLoadedEvent @event)
        {
            _planet = @event.PlanetWrapper != null ? new PlanetWrapper(@event.PlanetWrapper) : null;
            _atmosphereGrid = new AtmosphereGrid(_planet.GetNativeObject()); // Get native planet object
            _atmosphereGrid.InitializeGrid();
            _atmosphereGrid.EnableClimateControl(); // Enable overrides directly

            // TODO: Register tick handler for cellular updates
            // _tickAdapter = new AtmosphereTickAdapter(_atmosphereGrid);
            // GameEvents.RegisterTick(_tickAdapter);

            Log.Info("Planet loaded: " + _planet.Name);


            //Override Planete GetSet for gas ans plante variables
        }


    }
}
#pragma warning restore CS1591
