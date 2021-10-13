using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PipingNetworkSegment : LineItem
    {
        public PipingNetworkSegment()
        {
        }

        protected PipingNetworkSegment(PipingNetworkSegment other) : base(other)
        {
        }

        public override PlantEntity Clone()
        {
            return new PipingNetworkSegment(this);
        }
    }
}
