using System;
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
            ConnetionX = new int();
            ConnetionY = new int();
        }

        public int ID
        {
            get;
            set;
        }

        public int ConnetionX
        {
            get;
            set;
        }

        public int ConnetionY
        {
            get;
            set;
        }

    }
}
