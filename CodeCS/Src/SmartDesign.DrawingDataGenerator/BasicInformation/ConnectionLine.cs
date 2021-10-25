using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartDesign.DrawingDataGenerator;
using SmartDesign.MathUtil;

namespace SmartDesign.DrawingDataGenerator
{
    class ConnectionLine
    {
        public ConnectionLine()
        {
            ID = null;
            
            LineEndPoints = new LineEndPoint();

            ObjCenterX = 0;
            ObjCenterY = 0;
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
