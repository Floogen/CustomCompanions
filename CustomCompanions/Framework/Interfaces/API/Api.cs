using CustomCompanions.Framework.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Interfaces.API
{
    public interface IApi
    {
        void ReloadContentPack(string packUniqueId);
    }

    public class Api : IApi
    {
        private readonly CustomCompanions _framework;

        public Api(CustomCompanions customCompanionsMod)
        {
            _framework = customCompanionsMod;
        }

        public void ReloadContentPack(string packUniqueId)
        {
            if (!CompanionManager.companionModels.Any(c => c.Owner.Equals(packUniqueId, StringComparison.OrdinalIgnoreCase)))
            {
                CustomCompanions.monitor.Log($"A mod attempted to reload a non-existent CC pack of the following unique ID: {packUniqueId}");
                return;
            }

            _framework.ManualReload(packUniqueId);
        }
    }
}
