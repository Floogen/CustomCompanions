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
                MapCompanion companion = new MapCompanion(model, tile, location);
                companions.Add(companion);
            }

            var sceneryCompanion = new SceneryCompanions() { Location = location, Tile = tile, Companions = companions };
            if (sceneryCompanions.Any(s => s.Tile == tile && s.Location == location))
            {
                // Clear out old companions, add in the new ones
                foreach (var scenery in sceneryCompanions.Where(s => s.Tile == tile && s.Location == location))
                {
                    scenery.Companions.ForEach(c => location.characters.Remove(c));
                    scenery.Companions = companions;
                }
            }
            else
            {
                sceneryCompanions.Add(sceneryCompanion);
            }

            // Ensures each collision based companion is moved to an empty tile
            companions.ForEach(c => location.characters.Add(c));
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
                    companion.ResetForNewLocation(location, who.getTileLocation());
                    location.characters.Add(companion);
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
                    companion.PrepareForDeletion();
                    location.characters.Remove(companion);
                }
            }

            if (removeFromActive)
            {
                activeCompanions.Remove(boundCompanions);
            }
        }

        internal static void RemoveSceneryCompanionsAtTile(GameLocation location, Vector2 tile)
        {
            sceneryCompanions = sceneryCompanions.Where(s => !(s.Location == location && s.Tile == tile)).ToList();
        }

        internal static bool UpdateCompanionModel(CompanionModel model)
        {
            var cachedModel = companionModels.FirstOrDefault(c => c.GetId() == model.GetId());
            if (cachedModel is null)
            {
                CustomCompanions.monitor.Log($"No companion match found for {model.GetId()}; Did a content patch update the name?", StardewModdingAPI.LogLevel.Trace);
                return false;
            }

            // Update our cached model
            companionModels[companionModels.IndexOf(cachedModel)] = model;

            // Update any existing companions using this model
            foreach (var activeCompanion in activeCompanions.SelectMany(c => c.Companions).Where(c => c.model.GetId() == model.GetId()))
            {
                // Do model update
                activeCompanion.UpdateModel(model);
            }

            foreach (var sceneryCompanion in sceneryCompanions.SelectMany(c => c.Companions).Where(c => c.model.GetId() == model.GetId()))
            {
                // Do model update
                sceneryCompanion.UpdateModel(model);
            }

            return true;
        }

        internal static void RefreshLights()
        {
            foreach (var companion in sceneryCompanions.SelectMany(c => c.Companions).Where(c => c.light != null))
            {
                if (!Game1.currentLightSources.Contains(companion.light))
                {
                    Game1.currentLightSources.Add(companion.light);
                }
            }
        }

        internal static bool IsCustomCompanion(Character follower)
        {
            if (follower != null && follower is Companion)
            {
                return true;
            }

            return false;
        }

        internal static bool IsSceneryCompanion(Character follower)
        {
            if (follower != null && follower is MapCompanion)
            {
                return true;
            }

            return false;
        }

        internal static bool IsOrphan(Character follower, GameLocation location)
        {
            if (follower != null && follower is MapCompanion)
            {
                MapCompanion companion = follower as MapCompanion;
                return !sceneryCompanions.Any(c => c.Location == location && c.Tile == companion.targetTile / 64f && c.Companions.Contains(companion));
            }

            return false;
        }
    }
}
