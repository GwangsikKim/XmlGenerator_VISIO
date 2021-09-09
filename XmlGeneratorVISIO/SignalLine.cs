using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class SignalLine
    {
        public SignalLine()
        {
            ID = new int();
            LineEndPoints = new LineEndPoint();

            Text = new List<Text>();
        }

        public int ID
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
