using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class Instrument
    {
        public Instrument()
        {
            ID = new int();
            Angle = new int();
            Centers = new Center();
            Extents = new Extent();

            ConnectionPoints = new List<ConnectionPoint>();
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

        public int Angle
        {
            get;
            set;
        }

        public List<ConnectionPoint> ConnectionPoints
        {
            get;
            set;
        }
    }
}
