using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Node
    {
        public Node()
        {
            ID = Guid.NewGuid().ToString("N").ToUpper();
            Coordinate = new Position2();
        }

        protected Node(Node other)
        {
            ID = other.ID;
            Coordinate = other.Coordinate;
        }

        [ReadOnly(true)]
        public string ID
        {
            get;
            set;
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Position2 Coordinate
        {
            get;
            set;
        }

        public virtual Node Clone()
        {
            return new Node(this);
        }

        public override string ToString()
        {
            return string.Format("Node: {0}", ID);
        }
    }
}
