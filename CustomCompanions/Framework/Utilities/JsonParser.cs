using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Utilities
{
    public static class JsonParser
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public static string Serialize<Model>(Model model)
        {
            return JsonConvert.SerializeObject(model, Formatting.Indented, settings);
        }

        public static Model Deserialize<Model>(string json)
        {
            return JsonConvert.DeserializeObject<Model>(json, settings);
        }
    }
}
