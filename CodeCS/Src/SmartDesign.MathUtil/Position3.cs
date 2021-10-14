using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Position3
    {
        public static readonly Position3 O = new Position3();

        public Position3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position3(double[] coordinates)
        {
            if (coordinates.Length != 3)
                throw new ArgumentException(nameof(coordinates) + "의 크기가 3이 아닙니다.");

            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
        }

        public static Position3 Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");

            string[] valueStrings = s.Split(',');
            if (valueStrings.Length != 3)
                throw new FormatException();

            var values = valueStrings.Select(x => Convert.ToDouble(x)).ToArray();

            return new Position3(values[0], values[1], values[2]);
        }

        public static bool TryParse(string s, out Position3 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new Position3();
            }

            return false;
        }
        
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
        
        public static Position3 operator +(Position3 lhs, Position3 rhs)
        {
            return new Position3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Position3 operator -(Position3 lhs, Position3 rhs)
        {
            return new Position3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Position3 operator -(Position3 p, Vector3 v)
        {
            return new Position3(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Position3 operator *(double d, Position3 rhs)
        {
            return new Position3(d * rhs.X, d * rhs.Y, d * rhs.Z);
        }

        public static Position3 operator *(Position3 lhs, double d)
        {
            return d * lhs;
        }

        public static Position3 operator /(Position3 lhs, double d)
        {
            return new Position3(lhs.X / d, lhs.Y / d, lhs.Z / d);
        }

        public static Position3 operator -(Position3 v)
        {
            return -1.0 * v;
        }

        public static explicit operator Position3(Vector3 v)
        {
            return new Position3(v.X, v.Y, v.Z);
        }

        public override string ToString()
        {
            return string.Format("{0:f}, {1:f}, {2:f}", X, Y, Z);
        }

        public static double Distance(Position3 p1, Position3 p2)
        {
            return System.Math.Sqrt(SquaredDistance(p1, p2));
        }

        public static double SquaredDistance(Position3 p1, Position3 p2)
        {
            double ds = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z);
            return ds;
        }

        public static bool IsEqual(Position3 p1, Position3 p2)
        {
            return Tolerance.IsZeroDistance(Distance(p1, p2));
        }
    }
}
