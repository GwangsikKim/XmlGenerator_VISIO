using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLGeneratorVISIO
{
    class Extent
    {
        public Extent()
        {
            Max = new Max();
            Min = new Min();
        }

        public Max Max
        {
            get;
            set;
        }

        public Min Min
        {
            get;
            set;
        }
    }
}
