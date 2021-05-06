using CustomCompanions.Framework;
using CustomCompanions.Framework.Companions;
using CustomCompanions.Framework.Interfaces;
using CustomCompanions.Framework.Managers;
using CustomCompanions.Framework.Models;
using CustomCompanions.Framework.Models.Companion;
using CustomCompanions.Framework.Patches;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions
{
    public class CustomCompanions : Mod
    {
        internal static IMonitor monitor;
        internal static IModHelper modHelper;

        private IJsonAssetsApi _jsonAssetsApi;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor and helper
            monitor = Monitor;
            modHelper = helper;

            // Set up the mod's resources
            this.Reset(true);

            // Load our Harmony patches
            try
            {
                var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

                // Apply our patches
                new RingPatch(monitor).Apply(harmony);
                new UtilityPatch(monitor).Apply(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            // Add in our debug commands
            helper.ConsoleCommands.Add("cc_spawnCompanion", "Spawns in a specific companion.\n\nUsage: cc_spawnCompanion [QUANTITY] UNIQUE_ID.COMPANION_NAME", this.DebugSpawnCompanion);
            helper.ConsoleCommands.Add("cc_removeAll", "Removes all map-based custom companions at the current location.\n\nUsage: cc_removeAll", this.DebugRemoveAll);

            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;

            // Hook into Player events
            helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("bcmpinc.WearMoreRings") && ApiManager.HookIntoIWMR(Helper))
            {
                RingManager.wearMoreRingsApi = ApiManager.GetIWMRApi();
            }

            if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets") && ApiManager.HookIntoJsonAssets(Helper))
            {
                _jsonAssetsApi = ApiManager.GetJsonAssetsApi();

                // Hook into IdsAssigned
                _jsonAssetsApi.IdsAssigned += this.IdsAssigned;
            }

            // Load any owned content packs
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Loading companions from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                var companionFolders = new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Companions")).GetDirectories();
                if (companionFolders.Count() == 0)
                {
                    Monitor.Log($"No sub-folders found under Companions for the content pack {contentPack.Manifest.Name}!", LogLevel.Warn);
                    continue;
                }

                // Load in the companions
                foreach (var companionFolder in companionFolders)
                {
                    if (!File.Exists(Path.Combine(companionFolder.FullName, "companion.json")))
                    {
                        Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a companion.json under {companionFolder.Name}!", LogLevel.Warn);
                        continue;
                    }

                    CompanionModel companion = contentPack.ReadJsonFile<CompanionModel>(Path.Combine(companionFolder.Parent.Name, companionFolder.Name, "companion.json"));
                    companion.Name = companion.Name.Replace(" ", "");
                    companion.Owner = contentPack.Manifest.UniqueID;
                    Monitor.Log(companion.ToString(), LogLevel.Trace);

                    // Save the TileSheet, if one is given
                    if (String.IsNullOrEmpty(companion.TileSheetPath) && !File.Exists(Path.Combine(companionFolder.FullName, "companion.png")))
                    {
                        Monitor.Log($"Unable to add companion {companion.Name} from {contentPack.Manifest.Name}: No associated companion.png or TileSheetPath given", LogLevel.Warn);
                        continue;
                    }
                    else if (String.IsNullOrEmpty(companion.TileSheetPath))
                    {
                        companion.TileSheetPath = contentPack.GetActualAssetKey(Path.Combine(companionFolder.Parent.Name, companionFolder.Name, "companion.png"));
                    }

                    if (contentPack.Translation != null)
                    {
                        companion.Translations = contentPack.Translation;
                    }

                    // Add the companion to our cache
                    CompanionManager.companionModels.Add(companion);
                }

                if (_jsonAssetsApi != null)
                {
                    // Load in the rings that will be paired to a companion
                    if (!Directory.Exists(Path.Combine(contentPack.DirectoryPath, "Objects")))
                    {
                        Monitor.Log($"No summoning rings available from {contentPack.Manifest.Name}, this may be intended", LogLevel.Trace);
                        continue;
                    }

                    foreach (var ringFolder in new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Objects")).GetDirectories())
                    {
                        if (!File.Exists(Path.Combine(ringFolder.FullName, "object.json")))
                        {
                            Monitor.Log($"Content pack {contentPack.Manifest.Name} is missing a object.json under {ringFolder.Name}!", LogLevel.Warn);
                            continue;
                        }

                        RingModel ring = contentPack.ReadJsonFile<RingModel>(Path.Combine(ringFolder.Parent.Name, ringFolder.Name, "object.json"));
                        ring.Owner = contentPack.Manifest.UniqueID;

                        RingManager.rings.Add(ring);
                    }

                    // Generate content.json for Json Assets
                    contentPack.WriteJsonFile("content-pack.json", new ContentPackModel
                    {
                        Name = contentPack.Manifest.Name,
                        Author = contentPack.Manifest.Author,
                        Version = contentPack.Manifest.Version.ToString(),
                        Description = contentPack.Manifest.Description,
                        UniqueID = contentPack.Manifest.UniqueID,
                        UpdateKeys = contentPack.Manifest.UpdateKeys,
                    });

                    // Load in the associated rings objects (via JA)
                    _jsonAssetsApi.LoadAssets(contentPack.DirectoryPath);
                }
            }
        }

        private void IdsAssigned(object sender, EventArgs e)
        {
            // Get the ring IDs loaded in by JA from our owned content packs
            foreach (var ring in RingManager.rings)
            {
                int objectID = _jsonAssetsApi.GetObjectId(ring.Name);
                if (objectID == -1)
                {
                    continue;
                }

                ring.ObjectID = objectID;
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            RingManager.LoadWornRings();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            // Go through all game locations and purge any of custom creatures
            foreach (GameLocation location in Game1.locations.Where(l => l != null))
            {
                if (location.characters != null)
                {
                    foreach (var creature in location.characters.Where(c => CompanionManager.IsCustomCompanion(c)).ToList())
                    {
                        location.characters.Remove(creature);
                    }
                }
            }

            CompanionManager.sceneryCompanions = new List<SceneryCompanions>();
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.Reset();
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            var backLayer = e.NewLocation.map.GetLayer("Back");
            for (int x = 0; x < backLayer.LayerWidth; x++)
            {
                for (int y = 0; y < backLayer.LayerHeight; y++)
                {
                    var tile = backLayer.Tiles[x, y];
                    if (tile is null)
                    {
                        continue;
                    }

                    if (tile.Properties.ContainsKey("CustomCompanions"))
                    {
                        string command = tile.Properties["CustomCompanions"].ToString();
                        if (command.Split(' ')[0].ToUpper() != "SPAWN")
                        {
                            if (!String.IsNullOrEmpty(command))
                            {
                                Monitor.Log($"Unknown CustomCompanions command ({command.Split(' ')[0]}) given on tile ({x}, {y}) for map {e.NewLocation.NameOrUniqueName}!", LogLevel.Warn);
                            }
                            continue;
                        }

                        string companionKey = command.Substring(command.IndexOf(' ') + 2).TrimStart();
                        if (!Int32.TryParse(command.Split(' ')[1], out int amountToSummon))
                        {
                            amountToSummon = 1;
                            companionKey = command.Substring(command.IndexOf(' ') + 1);
                        }

                        var companion = CompanionManager.companionModels.FirstOrDefault(c => String.Concat(c.Owner, ".", c.Name) == companionKey);
                        if (companion is null)
                        {
                            Monitor.Log($"Unable to find companion match for {companionKey} given on tile ({x}, {y}) for map {e.NewLocation.NameOrUniqueName}!", LogLevel.Warn);
                            continue;
                        }

                        Monitor.Log($"Spawning [{companionKey}] x{amountToSummon} on tile ({x}, {y}) for map {e.NewLocation.NameOrUniqueName}");
                        CompanionManager.SummonCompanions(companion, amountToSummon, new Vector2(x, y), e.NewLocation);
                    }
                }
            }

            CompanionManager.RefreshLights();
        }

        private void Reset(bool isFirstRun = false)
        {
            if (isFirstRun)
            {
                // Set up the companion models
                CompanionManager.companionModels = new List<CompanionModel>();

                // Set up the RingManager
                RingManager.rings = new List<RingModel>();
            }

            // Set up the CompanionManager
            CompanionManager.activeCompanions = new List<BoundCompanions>();
            CompanionManager.sceneryCompanions = new List<SceneryCompanions>();
        }

        private void DebugSpawnCompanion(string command, string[] args)
        {
            if (args.Length == 0)
            {
                Monitor.Log($"Missing required arguments: [QUANTITY] UNIQUE_ID.COMPANION_NAME", LogLevel.Warn);
                return;
            }

            int amountToSummon = 1;
            string companionKey = args[0];
            if (args.Length > 1 && Int32.TryParse(args[0], out amountToSummon))
            {
                companionKey = args[1];
            }

            if (!CompanionManager.companionModels.Any(c => String.Concat(c.Owner, ".", c.Name) == companionKey) && !CompanionManager.companionModels.Any(c => String.Concat(c.Name) == companionKey))
            {
                Monitor.Log($"No match found for the companion name {companionKey}.", LogLevel.Warn);
                return;
            }
            if (CompanionManager.companionModels.Where(c => String.Concat(c.Name) == companionKey).Count() > 1)
            {
                Monitor.Log($"There was more than one match to the companion name {companionKey}. Use exact name (UNIQUE_ID.COMPANION_NAME) to resolve this issue.", LogLevel.Warn);
                return;
            }

            var companion = CompanionManager.companionModels.Where(c => String.Concat(c.Name) == companionKey).Count() > 1 ? CompanionManager.companionModels.FirstOrDefault(c => String.Concat(c.Owner, ".", c.Name) == companionKey) : CompanionManager.companionModels.FirstOrDefault(c => String.Concat(c.Name) == companionKey);
            if (companion is null)
            {
                Monitor.Log($"An error has occured trying to spawn {companionKey}: Command failed!", LogLevel.Warn);
                return;
            }

            Monitor.Log($"Spawning {companionKey} x{amountToSummon} at {Game1.currentLocation} on tile {Game1.player.getTileLocation()}!", LogLevel.Debug);
            CompanionManager.SummonCompanions(companion, amountToSummon, Game1.player.getTileLocation(), Game1.currentLocation);
        }

        private void DebugRemoveAll(string command, string[] args)
        {
            var currentLocation = Game1.player.currentLocation;
            foreach (var companion in currentLocation.characters.Where(c => CompanionManager.IsCustomCompanion(c)).ToList())
            {
                currentLocation.characters.Remove(companion);
            }

            CompanionManager.sceneryCompanions.Clear();
        }

        internal static bool IsSoundValid(string soundName, bool logFailure = false)
        {
            try
            {
                Game1.soundBank.GetCue(soundName);
            }
            catch (Exception)
            {
                if (logFailure)
                {
                    monitor.Log($"Attempted to get a sound that doesn't exist: {soundName}", LogLevel.Debug);
                }

                return false;
            }

            return true;
        }

        internal static bool CompanionHasFullMovementSet(CompanionModel model)
        {
            if (model.UpAnimation is null)
            {
                return false;
            }
            if (model.RightAnimation is null)
            {
                return false;
            }
            if (model.DownAnimation is null)
            {
                return false;
            }
            if (model.LeftAnimation is null)
            {
                return false;
            }

            return true;
        }

        internal static Color GetColorFromArray(int[] colorArray)
        {
            if (colorArray.Length < 3)
            {
                return Color.White;
            }

            // Verify alpha is given
            int alpha = 255;
            if (colorArray.Length >= 4)
            {
                alpha = colorArray[3];
            }

            return new Color(colorArray[0], colorArray[1], colorArray[2], alpha);
        }
    }
}
