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
        private List<CompanionModel> models;

        internal static Dictionary<string, string> manifestIdToAssetToken = new Dictionary<string, string>();

        public AssetManager(List<CompanionModel> models, Dictionary<string, string> uniqueIdToAsset)
        {
            this.models = models;
            manifestIdToAssetToken = uniqueIdToAsset;
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("CustomCompanions/Companions/ExampleAuthor.ExamplePack");
        }

        public T Load<T>(IAssetInfo asset)
        {
            var namesToModelData = new Dictionary<string, object>();
            foreach (var model in models.Where(m => !namesToModelData.ContainsKey(m.Name)))
            {
                namesToModelData.Add(model.Name, JsonParser.Serialize<object>(model));
            }

            //var model = JsonParser.Serialize(new Dictionary<string, object>() { { "testKey", new CompanionModel { Name = "test" } } });
            return (T)(object)namesToModelData;
        }
    }
}