using System;
using System.Collections.Generic;

namespace CustomCompanions.Framework.Interfaces
{
    public interface IJsonAssetsApi
    {
        void LoadAssets(string path);
        IDictionary<string, int> GetAllObjectIds();
        int GetObjectId(string name);

        event EventHandler IdsAssigned;
    }
}
