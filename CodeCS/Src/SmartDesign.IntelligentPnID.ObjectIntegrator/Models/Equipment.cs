using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class Equipment : PlantItem
    {
        public Equipment()
        {
        }

        protected Equipment(Equipment other) : base(other)
        {
        }

        [ReadOnly(true)]
        public List<Nozzle> Nozzles
        {
            get { return Children.OfType<Nozzle>().ToList(); }
        }

        public override PlantEntity Clone()
        {
            return new Equipment(this);
        }
    }
}
