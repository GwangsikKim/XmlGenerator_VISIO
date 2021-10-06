﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class Equipment
    {
        public Equipment()
        {
            ID = null;
            Angle = new int();
            Centers = new Center();
            Extents = new Extent();

            Text = new List<Text>();
            ConnectionPoints = new List<ConnectionPoint>();
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