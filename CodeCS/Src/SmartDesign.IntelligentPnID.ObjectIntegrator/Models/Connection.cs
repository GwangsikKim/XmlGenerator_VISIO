using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator
{
    public class Connection : PlantItem
    {
        public Connection()
        {
            From = string.Empty;
            To = string.Empty;
        }

        protected Connection(Connection other) : base(other)
        {
        }

        public string From
        {
            get;
            set;
        }

        public string To
        {
            get;
            set;
        }

        public override PlantEntity Clone()
        {
            return new Connection(this);
        }
    }
}
