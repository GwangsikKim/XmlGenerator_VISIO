using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PipingComponent : PlantItem, IHasNodes
    {
        public PipingComponent()
        {
            Nodes = new List<Node>();
            Association = null;
        }

        protected PipingComponent(PipingComponent other) : base(other)
        {
            Nodes = other.Nodes.Select(n => n.Clone()).ToList();
            Association = other.Association;
        }

        public List<Node> Nodes
        {
            get;
            set;
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Instrument Association { get; set; }

        public override PlantEntity Clone()
        {
            return new PipingComponent(this);
        }
    }
}
