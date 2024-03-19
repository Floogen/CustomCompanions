using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Models
{
    public class CompanionData
    {
        public int NumberToSummon { get; set; } = 1;
    }

    public class RingModel
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public Dictionary<string, CompanionData> Companions { get; set; }
    }
}
