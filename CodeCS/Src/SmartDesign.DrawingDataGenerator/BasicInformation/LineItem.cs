using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.DrawingDataGenerator
{
    class LineItem
    {
        public LineItem()
        {
            X = new List<int>();
            Y = new List<int>();
        }


        public List<int> X
        {
            get;
            set;
        }

        public List<int> Y
        {
            get;
            set;
        }

    }
}
