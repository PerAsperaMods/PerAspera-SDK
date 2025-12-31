using BepInEx.Unity.IL2CPP;
using LlockhamIndustries.Decals;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _planet = @event.PlanetWrapper;
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
