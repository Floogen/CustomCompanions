using CustomCompanions.Framework.Interfaces;
using CustomCompanions.Framework.Models;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Managers
{
    internal static class RingManager
    {
        internal static List<RingModel> rings;
        internal static IWearMoreRingsApi wearMoreRingsApi;

        internal static bool IsSummoningRing(Ring ring)
        {
            if (ring != null && rings.Any(r => r.ObjectID == ring.ParentSheetIndex))
            {
                return true;
            }

            return false;
        }

        internal static bool HasSummoningRingEquipped(Farmer who)
        {
            return GetWornRings().Where(r => IsSummoningRing(r)).Any();
        }

        internal static List<Ring> GetWornRings(bool filterVanillaRings = false)
        {
            List<Ring> wornRings = new List<Ring>() { Game1.player.leftRing, Game1.player.rightRing };
            if (wearMoreRingsApi != null)
            {
                wornRings = wearMoreRingsApi.GetAllRings(Game1.player).ToList();
            }

            return filterVanillaRings ? wornRings.Where(r => r != null && IsSummoningRing(r)).ToList() : wornRings;
        }

        internal static void LoadWornRings()
        {
            foreach (Ring ring in GetWornRings(true))
            {
                HandleEquip(Game1.player, Game1.currentLocation, ring);
            }
        }

        internal static void HandleEquip(Farmer who, GameLocation location, Ring ring)
        {
            var summoningRing = rings.FirstOrDefault(r => r.ObjectID == ring.ParentSheetIndex);
            if (summoningRing is null)
            {
                CustomCompanions.monitor.Log($"Failed to find a summoning ring match to [{ring.Name}]");
                return;
            }

            var selectedCompanionData = summoningRing.Companions.ElementAt(Game1.random.Next(summoningRing.Companions.Count));
            var companion = CompanionManager.companionModels.FirstOrDefault(c => c.Name == selectedCompanionData.Key && c.Owner == summoningRing.Owner);
            if (companion is null)
            {
                CustomCompanions.monitor.Log($"Failed to find a companion match to [{selectedCompanionData}] for the summoning ring [{ring.Name}]");
                return;
            }

            // Create a new Companion and add it to the player's location
            CustomCompanions.monitor.Log($"Spawning [{selectedCompanionData}] x{selectedCompanionData.Value.NumberToSummon} via the summoning ring [{ring.Name}]");
            CompanionManager.SummonCompanions(companion, selectedCompanionData.Value.NumberToSummon, summoningRing, who, location);
        }

        internal static void HandleUnequip(Farmer who, GameLocation location, Ring ring)
        {
            var summoningRing = rings.FirstOrDefault(r => r.ObjectID == ring.ParentSheetIndex);
            if (summoningRing is null)
            {
                CustomCompanions.monitor.Log($"Failed to find a summoning ring match to [{ring.Name}]");
                return;
            }

            // Despawn the summoned companion(s) bound to this ring
            CustomCompanions.monitor.Log($"Despawning companions bound to the summoning ring [{ring.Name}]");
            CompanionManager.RemoveCompanions(summoningRing, location);
        }

        internal static void HandleNewLocation(Farmer who, GameLocation location, Ring ring)
        {
            var summoningRing = rings.FirstOrDefault(r => r.ObjectID == ring.ParentSheetIndex);
            if (summoningRing is null)
            {
                CustomCompanions.monitor.Log($"Failed to find a summoning ring match to [{ring.Name}]");
                return;
            }

            // Create a new Companion and add it to the player's location
            CustomCompanions.monitor.Log($"Respawning companions bound to the summoning ring [{ring.Name}]");
            CompanionManager.RespawnCompanions(summoningRing, who, location);
        }

        internal static void HandleLeaveLocation(Farmer who, GameLocation location, Ring ring)
        {
            var summoningRing = rings.FirstOrDefault(r => r.ObjectID == ring.ParentSheetIndex);
            if (summoningRing is null)
            {
                CustomCompanions.monitor.Log($"Failed to find a summoning ring match to [{ring.Name}]");
                return;
            }

            // Despawn the summoned companion(s) bound to this ring
            CustomCompanions.monitor.Log($"Despawning companions bound to the summoning ring [{ring.Name}]");
            CompanionManager.RemoveCompanions(summoningRing, location, false);
        }
    }
}
