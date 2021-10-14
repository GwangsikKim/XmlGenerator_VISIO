using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class Instrument : PlantItem, IHasNodes
    {
        public Instrument()
        {
            Nodes = new List<Node>();
        }

        protected Instrument(Instrument other) : base(other)
        {
            Nodes = other.Nodes.Select(n => n.Clone()).ToList();
        }

        public List<Node> Nodes
        {
            get;
            private set;
        }

        public override PlantEntity Clone()
        {
            return new Instrument(this);
        }
    }
}
