using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PipeCross : PipingComponent
    {
        public PipeCross()
        {
        }

        protected PipeCross(PipeCross other) : base(other)
        {

        }

        public override PlantEntity Clone()
        {
            return new PipeCross(this);
        }
    }
}
