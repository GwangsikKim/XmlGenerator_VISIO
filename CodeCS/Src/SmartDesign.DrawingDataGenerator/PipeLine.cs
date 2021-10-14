using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class PipeLine
    {
        public PipeLine()
        {
            ID = null;
            LineEndPoints = new LineEndPoint();

            Text = new List<Text>();
        }

        public string ID
        {
            get;
            set;
        }

        public Center Centers
        {
            get;
            set;
        }

        public Extent Extents
        {
            get;
            set;
        }

        public List<Text> Text
        {
            get;
            set;
        }

        public LineEndPoint LineEndPoints
        {
            get;
            set;
        }
    }
}
