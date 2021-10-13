using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PipeConnectorSymbol : PlantItem, IHasNodes
    {
        public PipeConnectorSymbol()
        {
            Nodes = new List<Node>();
            CrossPageConnection = null;
        }

        protected PipeConnectorSymbol(PipeConnectorSymbol other)
        {
            Nodes = other.Nodes.Select(n => n.Clone()).ToList();

            if(CrossPageConnection != null)
                CrossPageConnection = (CrossPageConnection)CrossPageConnection.Clone();
        }

        public List<Node> Nodes
        {
            get;
            set;
        }

        public CrossPageConnection CrossPageConnection
        {
            get;
            set;
        }

        public override PlantEntity Clone()
        {
            return new PipeConnectorSymbol(this);
        }
    }
}
