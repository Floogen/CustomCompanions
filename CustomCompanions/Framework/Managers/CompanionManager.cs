using CustomCompanions.Framework.Companions;
using CustomCompanions.Framework.Models;
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
    internal class BoundCompanions
    {
        public RingModel SummoningRing { get; set; }
        public List<Companion> Companions { get; set; }
    }

    internal class SceneryCompanions
    {
        public GameLocation Location { get; set; }
        public Vector2 Tile { get; set; }
        public List<Companion> Companions { get; set; }
    }

    internal static class CompanionManager
    {
        internal static List<CompanionModel> companionModels;
        internal static List<BoundCompanions> activeCompanions;
        internal static List<SceneryCompanions> sceneryCompanions;

        internal static void SummonCompanions(CompanionModel model, int numberToSummon, RingModel summoningRing, Farmer who, GameLocation location)
        {
            if (location.characters is null)
            {
                CustomCompanions.monitor.Log($"Unable to summon {model.Name} due to the location {location.Name} not having an instantiated GameLocation.characters!");
                return;
            }

            List<Companion> companions = new List<Companion>();
            for (int x = 0; x < numberToSummon; x++)
            {
                Companion companion = new Companion(model, who);
                location.characters.Add(companion);
                companions.Add(companion);
            }

            activeCompanions.Add(new BoundCompanions() { SummoningRing = summoningRing, Companions = companions });
        }

        internal static void SummonCompanions(CompanionModel model, int numberToSummon, Vector2 tile, GameLocation location)
        {
            if (location.characters is null)
            {
                CustomCompanions.monitor.Log($"Unable to summon {model.Name} due to the location {location.Name} not having an instantiated GameLocation.characters!");
                return;
            }

            List<Companion> companions = new List<Companion>();
            for (int x = 0; x < numberToSummon; x++)
            {
                Companion companion = new Companion(model, tile, location);
                companions.Add(companion);
            }

            var sceneryCompanion = new SceneryCompanions() { Location = location, Tile = tile, Companions = companions };
            if (!sceneryCompanions.Any(s => s.Tile == tile && s.Location == location))
            {
                companions.ForEach(c => location.characters.Add(c));
                sceneryCompanions.Add(sceneryCompanion);
            }
            else
            {
                var existingSceneryCompanions = sceneryCompanions.First(s => s.Tile == tile && s.Location == location);
                companions.ForEach(c => existingSceneryCompanions.Companions.Add(c));
            }

            // Ensures each collision based companion is moved to an empty tile
            foreach (var companion in companions.Where(c => c.collidesWithOtherCharacters))
            {
                companion.PlaceInEmptyTile();
            }
        }

        internal static void RespawnCompanions(RingModel summoningRing, Farmer who, GameLocation location, bool removeFromActive = true)
        {
            var boundCompanions = activeCompanions.FirstOrDefault(a => a.SummoningRing == summoningRing);
            if (boundCompanions is null)
            {
                CustomCompanions.monitor.Log($"Unable to find summoning ring match to {summoningRing.Name}, will be unable to respawn companions!");
                return;
            }

            foreach (var companion in boundCompanions.Companions)
            {
                if (location.characters != null && !location.characters.Contains(companion))
                {
                    location.characters.Add(companion);
                    companion.ResetForNewLocation(location, who.getTileLocation());
                }
            }
        }

        internal static void RemoveCompanions(RingModel summoningRing, GameLocation location, bool removeFromActive = true)
        {
            var boundCompanions = activeCompanions.FirstOrDefault(a => a.SummoningRing == summoningRing);
            if (boundCompanions is null)
            {
                CustomCompanions.monitor.Log($"Unable to find summoning ring match to {summoningRing.Name}, will be unable to despawn companions!");
                return;
            }

            foreach (var companion in boundCompanions.Companions)
            {
                if (location.characters != null && location.characters.Contains(companion))
                {
                    location.characters.Remove(companion);
                }
            }

            if (removeFromActive)
            {
                activeCompanions.Remove(boundCompanions);
            }
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
