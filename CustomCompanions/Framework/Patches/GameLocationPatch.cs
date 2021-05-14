using CustomCompanions.Framework.Companions;
using CustomCompanions.Framework.Managers;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    internal class GameLocationPatch
    {
        private static IMonitor monitor;
        private readonly Type _gameLocation = typeof(GameLocation);

        internal GameLocationPatch(IMonitor modMonitor)
        {
            monitor = modMonitor;
        }

        internal void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_gameLocation, nameof(GameLocation.drawWater), new[] { typeof(SpriteBatch) }), prefix: new HarmonyMethod(GetType(), nameof(DrawWaterPostfix)));
        }

        private static bool DrawWaterPostfix(GameLocation __instance, SpriteBatch b)
        {
            foreach (var companion in CustomCompanions.companionManager.sceneryCompanions.Where(c => c.Location.Value == __instance).SelectMany(c => c.Companions))
            {
                if (companion.model.AppearUnderwater)
                {
                    companion.DrawUnderwater(b);
                }
            }

            return true;
        }
    }
}
