using CustomCompanions.Framework.Managers;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Patches
{
    internal class UtilityPatch
    {
        private static IMonitor monitor;
        private readonly Type _utility = typeof(Utility);

        internal UtilityPatch(IMonitor modMonitor)
        {
            monitor = modMonitor;
        }

        internal void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_utility, nameof(Utility.isThereAFarmerOrCharacterWithinDistance), new[] { typeof(Vector2), typeof(int), typeof(GameLocation) }), postfix: new HarmonyMethod(GetType(), nameof(IsThereAFarmerOrCharacterWithinDistancePostfix)));
        }

        private static void IsThereAFarmerOrCharacterWithinDistancePostfix(Utility __instance, ref Character __result, Vector2 tileLocation, int tilesAway, GameLocation environment)
        {
            if (environment is Town && __result != null && CompanionManager.IsCustomCompanion(__result))
            {
                __result = null;
            }
        }
    }
}
