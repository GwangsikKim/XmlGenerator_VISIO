using SmartDesign.MathUtil;
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
            Angle = 0;
            Extent = new Obb2(); 

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

        public Obb2 Extent
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
