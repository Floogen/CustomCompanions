using CustomCompanions.Framework.Companions;
using CustomCompanions.Framework.Models.Companion;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Managers
{
    internal static class CompanionManager
    {
        internal static List<CompanionModel> companionModels;
        internal static List<Companion> activeCompanions;

        internal static void SummonCompanion(CompanionModel model, Farmer who, GameLocation location)
        {
            if (location.characters is null)
            {
                CustomCompanions.monitor.Log($"Unable to summon {model.Name} due to the location {location.Name} not having an instantiated GameLocation.characters!");
                return;
            }

            Companion companion = new Companion(model, who);
            location.characters.Add(companion);
            activeCompanions.Add(companion);
        }

        internal static void UpdateCompanions()
        {
            // TODO: Implement game tick updates
        }

        internal static bool IsCustomCompanion(Character follower)
        {
            if (follower != null && follower is Companion)
            {
                return true;
            }

            return false;
        }
    }
}
