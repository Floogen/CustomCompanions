using CustomCompanions.Framework;
using CustomCompanions.Framework.Companions;
using CustomCompanions.Framework.Interfaces;
using CustomCompanions.Framework.Models;
using CustomCompanions.Framework.Models.Companion;
using CustomCompanions.Framework.Patches;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        internal static List<CompanionModel> companions;
        internal static List<RingModel> rings;

        private IWearMoreRingsApi wearMoreRingsApi;

        public override void Entry(IModHelper helper)
        {
            // Set up the monitor and helper
            monitor = Monitor;
            modHelper = helper;
            companions = new List<CompanionModel>();

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
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("bcmpinc.WearMoreRings") && ApiManager.HookIntoIWMR(Helper))
            {
                wearMoreRingsApi = ApiManager.GetIWMRApi();
            }

            // Hook into the APIs we utilize
            if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets") && ApiManager.HookIntoJsonAssets(Helper))
            {
                var jsonAssetsApi = ApiManager.GetJsonAssetsApi();

                // Load any owned content packs
                foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                {
                    Monitor.Log($"Loading companions from pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} by {contentPack.Manifest.Author}", LogLevel.Debug);

                    // Load in the companions
                    foreach (var companionFolder in new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Companions")).GetDirectories())
                    {
                        CompanionModel companion = contentPack.ReadJsonFile<CompanionModel>(Path.Combine(companionFolder.Parent.Name, companionFolder.Name, "companion.json"));
                        companion.Owner = contentPack.Manifest.UniqueID;
                        Monitor.Log(companion.ToString(), LogLevel.Trace);

                        // Save the TileSheet, if one is given
                        if (String.IsNullOrEmpty(companion.TileSheetPath) && !File.Exists(Path.Combine(companionFolder.FullName, "companion.png")))
                        {
                            Monitor.Log($"Unable to add companion {companion.Name} from {contentPack.Manifest.Name}: No associated companion.png or TileSheetPath given", LogLevel.Warn);
                            continue;
                        }
                        else if (!String.IsNullOrEmpty(companion.TileSheetPath))
                        {
                            companion.TileSheetPath = Path.Combine(companionFolder.FullName, "companion.png");
                        }

                        // Add the companion to our cache
                        companions.Add(companion);
                    }

                    // Load in the rings that will be paired to a companion
                    if (!Directory.Exists(Path.Combine(contentPack.DirectoryPath, "Objects")))
                    {
                        Monitor.Log($"No summoning rings available from {contentPack.Manifest.Name}, this may be intended", LogLevel.Debug);
                        continue;
                    }

                    foreach (var json in new DirectoryInfo(Path.Combine(contentPack.DirectoryPath, "Objects")).GetDirectories().Where(f => f.Name == "object.json"))
                    {
                        RingModel ring = contentPack.ReadJsonFile<RingModel>(json.FullName);
                        ring.ContentPackID = contentPack.Manifest.UniqueID;

                        rings.Add(ring);
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
                    jsonAssetsApi.LoadAssets(contentPack.DirectoryPath);
                }
            }
        }

        internal static bool IsCustomCompanion(object follower)
        {
            if (follower != null && follower is WalkingCompanion)
            {
                return true;
            }

            return false;
        }
    }
}
