using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class UnknownSymbol : PlantItem
    {
        public UnknownSymbol()
        {
            ComponentClass = "Unknown";
        }

        protected UnknownSymbol(UnknownSymbol other) : base(other)
        {
        }

        public override PlantEntity Clone()
        {
            return new UnknownSymbol(this);
        }
    }
}
