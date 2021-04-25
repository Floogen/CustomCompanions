using StardewModdingAPI;
using CustomCompanions.Framework.Interfaces;

namespace CustomCompanions.Framework.Managers
{
    internal static class ApiManager
    {
        private static IMonitor monitor = CustomCompanions.monitor;
        private static IJsonAssetsApi jsonAssetsApi;
        private static IWearMoreRingsApi wearMoreRingsApi;

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

        internal static bool HookIntoIWMR(IModHelper helper)
        {
            wearMoreRingsApi = helper.ModRegistry.GetApi<IWearMoreRingsApi>("bcmpinc.WearMoreRings");

            if (wearMoreRingsApi is null)
            {
                monitor.Log("Failed to hook into bcmpinc.WearMoreRings.", LogLevel.Error);
                return false;
            }

            monitor.Log("Successfully hooked into bcmpinc.WearMoreRings.", LogLevel.Debug);
            return true;
        }

        internal static IJsonAssetsApi GetJsonAssetsApi()
        {
            return jsonAssetsApi;
        }

        internal static IWearMoreRingsApi GetIWMRApi()
        {
            return wearMoreRingsApi;
        }
    }
}
