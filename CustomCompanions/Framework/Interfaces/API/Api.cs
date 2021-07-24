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
    }

    public class Api : IApi
    {
        private readonly CustomCompanions _framework;

        public Api(CustomCompanions customCompanionsMod)
        {
            _framework = customCompanionsMod;
        }
}
