using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Models
{
    public class RingModel
    {
        public string Name { get; set; }
        public int ObjectID { get; set; }
        public string Owner { get; set; }
        public string CompanionName { get; set; }
        public int NumberOfCompanionsToSummon { get; set; } = 1;
    }
}
