using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLGeneratorVISIO;

namespace XmlGeneratorVISIO
{
    class ConnectionLine
    {
        public ConnectionLine()
        {
            ID = null;
            LineEndPoints = new LineEndPoint();

            ObjCenterX = new double();
            ObjCenterY = new double();
        }

        public string ID
        {
            get;
            set;
        }

        public LineEndPoint LineEndPoints
        {
            get;
            set;
        }

        public double ObjCenterX
        {
            get;
            set;
        }

        public double ObjCenterY
        {
            get;
            set;
        }

    }
}
