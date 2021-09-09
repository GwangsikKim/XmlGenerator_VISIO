using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class LineEndPoint
    {
        public LineEndPoint()
        {
            BeginPoints = new BeginPoint();
            EndPoints = new EndPoint();
        }

        public BeginPoint BeginPoints
        {
            get;
            set;
        }

        public EndPoint EndPoints
        {
            get;
            set;
        }
    }
}
