using CustomCompanions.Framework.Models.Companion;
using CustomCompanions.Framework.Utilities;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Managers
{
    internal class AssetManager : IAssetLoader
    {

        internal static Dictionary<string, string> manifestIdToAssetToken;

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return manifestIdToAssetToken.Keys.Any(i => asset.AssetNameEquals($"CustomCompanions/Companions/{i}"));
        }

        public T Load<T>(IAssetInfo asset)
        {
            var namesToModelData = new Dictionary<string, object>();
            foreach (var model in CompanionManager.companionModels.Where(m => !namesToModelData.ContainsKey(m.Name)))
            {
                namesToModelData.Add(model.Name, JsonParser.Serialize<object>(model));
            }

            //var model = JsonParser.Serialize(new Dictionary<string, object>() { { "testKey", new CompanionModel { Name = "test" } } });
            return (T)(object)namesToModelData;
        }
    }
}