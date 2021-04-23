using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Models
{
    public class ContentPackModel
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string UniqueID { get; set; }
        public string[] UpdateKeys { get; set; }
    }
}
