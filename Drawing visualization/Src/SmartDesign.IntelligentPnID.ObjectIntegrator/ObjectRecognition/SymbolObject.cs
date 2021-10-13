using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class SymbolObject
    {
        public SymbolObject()
        {
            Id = string.Empty;
            BoundingBox = new BoundingBox();
        }

        public string Id { get; set; }
        public SymbolObjectType Type { get; set; }

        public string ClassName { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public Angle Rotation { get; set; }

        public bool Flip { get; set; } // 수평 flip
    }
}
