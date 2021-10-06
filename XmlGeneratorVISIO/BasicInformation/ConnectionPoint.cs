﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class ConnectionPoint
    {
        public ConnectionPoint()
        {
            ID = null;

            ConnetionX = new double();
            ConnetionY = new double();
            ObjCenterX = new double();
            ObjCenterY = new double();
        }

        public string ID
        {
            get;
            set;
        }

        public double ConnetionX
        {
            get;
            set;
        }

        public double ConnetionY
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
