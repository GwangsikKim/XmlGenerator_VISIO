using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class PipeLine
    {
        public PipeLine()
        {
            ID = new int();
            LineEndPoints = new LineEndPoint();
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

        public LineEndPoint LineEndPoints
        {
            get;
            set;
        }
    }
}
