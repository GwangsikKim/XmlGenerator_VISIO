using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class SignalConnectorSymbol : PlantItem, IHasNodes
    {
        public SignalConnectorSymbol()
        {
            Nodes = new List<Node>();
            CrossPageConnection = null;
        }

        protected SignalConnectorSymbol(SignalConnectorSymbol other) : base(other)
        {
            Nodes = other.Nodes.Select(n => n.Clone()).ToList();
            if(CrossPageConnection != null)
                CrossPageConnection = (CrossPageConnection)other.CrossPageConnection.Clone();
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
    }
}
