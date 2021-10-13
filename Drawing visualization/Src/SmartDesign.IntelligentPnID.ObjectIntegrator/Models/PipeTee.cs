using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PipeTee : PipingComponent        
    {
        public PipeTee()
        {
        }

        protected PipeTee(PipeTee other) : base(other)
        {

        }

        public override PlantEntity Clone()
        {
            return new PipeTee(this);
        }
    }
}
