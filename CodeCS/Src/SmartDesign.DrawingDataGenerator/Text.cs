using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class Text
    {
        public Text()
        {
            ID = null;
            Angle = new int();
            Centers = new Center();
            Extents = new Extent();

            ConnectionPoints = new List<ConnectionPoint>();
        }

        public string Contents
        {
            get;
            set;
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
