using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class SignalLine : LineItem
    {
        public SignalLine()
        {
        }

        protected SignalLine(SignalLine other) : base(other)
        {

        }

        public override PlantEntity Clone()
        {
            return new SignalLine(this);
        }
    }
}
