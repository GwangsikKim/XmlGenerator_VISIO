using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Axis22
    {
        public static readonly Axis22 OXY = new Axis22(Position2.O, Vector2.OX);

        public Axis22(Position2 location, Vector2 xDirection)
        {
            Location = location;
            this.xDirection = Vector2.Normalize(xDirection);
            this.yDirection = Vector2.Normalize(new Vector2(-xDirection.Y, xDirection.X));
        }

        public Position2 Location { get; set; }

        private Vector2 xDirection;
        public Vector2 XDirection
        {
            get { return xDirection; }
            set
            {
                xDirection = Vector2.Normalize(value);
                yDirection = Vector2.Normalize(new Vector2(-xDirection.Y, xDirection.X));
            }
        }

        private Vector2 yDirection;
        public Vector2 YDirection
        {
            get { return yDirection; }
            set
            {
                yDirection = Vector2.Normalize(value);
                xDirection = Vector2.Normalize(new Vector2(yDirection.Y, -yDirection.X));
            }
        }
    }
}
