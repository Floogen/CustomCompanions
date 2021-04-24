using CustomCompanions.Framework.Interfaces;
using CustomCompanions.Framework.Models;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework
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

        internal static void UpdateRingEffects(Farmer who, GameLocation location)
        {
            foreach (Ring ring in GetWornRings(true))
            {
                //ring.Update(who, location);
            }
        }

        internal static void HandleEquip(Farmer who, GameLocation location, Ring ring)
        {
            if (ring != null)
            {
                //customRing.HandleEquip(who, location);
            }
        }

        internal static void HandleUnequip(Farmer who, GameLocation location, Ring ring)
        {
            if (ring != null)
            {
                //customRing.HandleUnequip(who, location);
            }
        }

        internal static void HandleNewLocation(Farmer who, GameLocation location, Ring ring)
        {
            if (ring != null)
            {
                //customRing.HandleNewLocation(who, location);
            }
        }

        internal static void HandleLeaveLocation(Farmer who, GameLocation location, Ring ring)
        {
            if (ring != null)
            {
                //customRing.HandleLeaveLocation(who, location);
            }
        }
    }
}
