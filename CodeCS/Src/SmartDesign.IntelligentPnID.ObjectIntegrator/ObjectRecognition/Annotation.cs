using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class Annotation
    {
        public Annotation()
        {
            BasicDrawingInformation = new BasicDrawingInformation();
            SymbolObjectList = new List<SymbolObject>();
            LineObjectList = new List<LineObject>();
            ConnectionObjectList = new List<ConnectionObject>();
        }

        public BasicDrawingInformation BasicDrawingInformation { get; set; }

        public List<SymbolObject> SymbolObjectList { get; set; }

        public List<LineObject> LineObjectList { get; set; }

        public List<ConnectionObject> ConnectionObjectList { get; set; }

    }
}
