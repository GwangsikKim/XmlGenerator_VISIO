using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Axis12
    {
        public static readonly Axis12 OX = new Axis12(Position2.O, Vector2.OX);
        public static readonly Axis12 OY = new Axis12(Position2.O, Vector2.OY);

        public Axis12(Position2 location, Vector2 direction)
        {
            Location = location;
            this.direction = Vector2.Normalize(direction);
        }

        public Position2 Location { get; set; }

        private Vector2 direction;
        public Vector2 Direction
        {
            get { return direction; }
            set
            {
                direction = Vector2.Normalize(value);
            }
        }
    }
}
