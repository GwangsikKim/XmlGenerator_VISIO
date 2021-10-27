using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class Instrument
    {
        public Instrument()
        {
            ID = null;
            ClassName = "none";
            Angle = 0;
            Extent = new Obb2();

            Text = new List<Text>();
            ConnectionPoints = new List<ConnectionPoint>();
        }

        public string ID
        {
            get;
            set;
        }

        public string ClassName
        {
            get;
            set;
        }

        public Obb2 Extent
        {
            get;
            set;
        }

        public double Angle
        {
            get;
            set;
        }

        public List<Text> Text
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
