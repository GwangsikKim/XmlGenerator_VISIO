using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class BoundingBox
    {
        public int XMin { get; set; }
        public int YMin { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }

        public bool HasExtent()
        {
            return !(XMin == XMax || YMin == YMax);
        }
    }
}
