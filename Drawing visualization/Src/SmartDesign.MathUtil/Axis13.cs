using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Axis13
    {
        public static readonly Axis13 OX = new Axis13(Position3.O, Vector3.OX);
        public static readonly Axis13 OY = new Axis13(Position3.O, Vector3.OY);
        public static readonly Axis13 OZ = new Axis13(Position3.O, Vector3.OZ);

        public Axis13(Position3 location, Vector3 direction)
        {
            Location = location;
            this.direction = Vector3.Normalize(direction);
        }

        public Position3 Location { get; set; }

        private Vector3 direction;
        public Vector3 Direction
        {
            get { return direction; }
            set
            {
                direction = Vector3.Normalize(value);
            }
        }
    }
}
