using CustomCompanions.Framework.Interfaces;
using CustomCompanions.Framework.Patches;
using Harmony;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions
{
    public class CustomCompanions : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        private IWearMoreRingsApi wearMoreRingsApi;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor and helper
            monitor = Monitor;
            modHelper = helper;

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

                // Apply our patches
                //new RingPatch(monitor).Apply(harmony);
                new UtilityPatch(monitor).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Hook into GameLoop events
            //helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            //helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            //helper.Events.GameLoop.Saving += this.OnSaving;
            //helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;
        }
        internal static bool IsCustomCompanion(object follower)
        {
            if (follower != null && follower.GetType().Namespace == "CustomCompanions.Framework.Critters")
            {
                return true;
            }

            return false;
        }
    }
}
