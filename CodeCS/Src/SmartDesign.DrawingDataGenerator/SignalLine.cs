using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class SignalLine
    {
        public SignalLine()
        {
            ID = null;
            LineEndPoints = new LineEndPoint();
            Extent = new Obb2();

            Text = new List<Text>();
        }

        public string ID
        {
            get;
            set;
        }

        public Obb2 Extent
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
