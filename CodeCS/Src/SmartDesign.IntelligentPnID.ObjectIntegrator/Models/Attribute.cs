using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Attribute
    {
        public Attribute()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        protected Attribute(Attribute other)
        {
            Name = other.Name;
            Value = other.Value;
        }

        public string Name
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public virtual Attribute Clone()
        {
            return new Attribute(this);
        }
    }
}
