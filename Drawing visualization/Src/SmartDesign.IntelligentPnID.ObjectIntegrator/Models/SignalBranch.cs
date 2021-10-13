using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class SignalBranch : PlantItem, IHasNodes
    {
        public SignalBranch()
        {
            Nodes = new List<Node>();
        }

        protected SignalBranch(SignalBranch other) : base(other)
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
            return new SignalBranch(this);
        }
    }
}
