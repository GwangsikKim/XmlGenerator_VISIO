using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class Nozzle : PlantItem, IHasNodes
    {
        public Nozzle()
        {
            Nodes = new List<Node>();
        }

        protected Nozzle(Nozzle other) : base(other)
        {
            Nodes = other.Nodes.Select(n => n.Clone()).ToList();
        }

        public List<Node> Nodes
        {
            get;
            set;
        }

        public override PlantEntity Clone()
        {
            return new Nozzle(this);
        }
    }
}
