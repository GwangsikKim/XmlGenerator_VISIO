using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class ConnectionPoint
    {
        public ConnectionPoint()
        {
            ID = null;
            ConnetionID = Guid.NewGuid().ToString("N").ToUpper();

            ConnetionX = 0;
            ConnetionY = 0;
            ObjCenterX = 0;
            ObjCenterY = 0;
        }

        public string ID
        {
            get;
            set;
        }

        public string ConnetionID
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
