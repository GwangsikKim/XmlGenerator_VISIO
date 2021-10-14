using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class UnknownLine : LineItem
    {
        public UnknownLine()
        {
        }

        protected UnknownLine(UnknownLine other) : base(other)
        {

        }

        public override PlantEntity Clone()
        {
            return new UnknownLine(this);
        }
    }
}
