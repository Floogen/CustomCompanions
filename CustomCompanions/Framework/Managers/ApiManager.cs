﻿using StardewModdingAPI;
using CustomCompanions.Framework.Interfaces;

namespace CustomCompanions.Framework.Managers
{
    internal static class ApiManager
    {
        private static IMonitor monitor = CustomCompanions.monitor;
        private static ISaveAnywhereApi saveAnywhereApi;
        private static IContentPatcherAPI contentPatcherApi;
        private static IJsonAssetsApi jsonAssetsApi;

        internal static bool HookIntoJsonAssets(IModHelper helper)
        {
            jsonAssetsApi = helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            if (jsonAssetsApi is null)
            {
                monitor.Log("Failed to hook into spacechase0.JsonAssets.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into spacechase0.JsonAssets.", LogLevel.Debug);
            return true;
        }

        internal static bool HookIntoContentPatcher(IModHelper helper)
        {
            contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

            if (contentPatcherApi is null)
            {
                monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;
        }

        internal static bool HookIntoSaveAnywhere(IModHelper helper)
        {
            saveAnywhereApi = helper.ModRegistry.GetApi<ISaveAnywhereApi>("Omegasis.SaveAnywhere");

            if (saveAnywhereApi is null)
            {
                monitor.Log("Failed to hook into Omegasis.SaveAnywhere.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into Omegasis.SaveAnywhere.", LogLevel.Debug);
            return true;
        }

        public static IContentPatcherAPI GetContentPatcherInterface()
        {
            return contentPatcherApi;
        }

        internal static IJsonAssetsApi GetJsonAssetsApi()
        {
            return jsonAssetsApi;
        }

        internal static ISaveAnywhereApi GetSaveAnywhereApi()
        {
            return saveAnywhereApi;
        }
    }
}
