using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Position2
    {
        public static readonly Position2 O = new Position2();

        public Position2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Position2(double[] coordinates)
        {
            if (coordinates.Length != 2)
                throw new ArgumentException(nameof(coordinates) + "의 크기가 2가 아닙니다.");

            X = coordinates[0];
            Y = coordinates[1];
        }

        public static Position2 Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");

            string[] valueStrings = s.Split(',');
            if (valueStrings.Length != 2)
                throw new FormatException();

            var values = valueStrings.Select(x => Convert.ToDouble(x)).ToArray();

            return new Position2(values[0], values[1]);
        }

        public static bool TryParse(string s, out Position2 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new Position2();
            }

            return false;
        }
        
        public double X { get; set; }

        public double Y { get; set; }
        
        public static Position2 operator +(Position2 lhs, Position2 rhs)
        {
            return new Position2(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Position2 operator +(Position2 lhs, Vector2 rhs)
        {
            return new Position2(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Position2 operator -(Position2 lhs, Position2 rhs)
        {
            return new Position2(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Position2 operator -(Position2 p, Vector2 v)
        {
            return new Position2(p.X - v.X, p.Y - v.Y);
        }

        public static Position2 operator *(double d, Position2 rhs)
        {
            return new Position2(d * rhs.X, d * rhs.Y);
        }

        public static Position2 operator *(Position2 lhs, double d)
        {
            return d * lhs;
        }

        public static Position2 operator /(Position2 lhs, double d)
        {
            return new Position2(lhs.X / d, lhs.Y / d);
        }

        public static Position2 operator -(Position2 v)
        {
            return -1.0 * v;
        }

        public static explicit operator Position2(Vector2 v)
        {
            return new Position2(v.X, v.Y);
        }

        public override string ToString()
        {
            return string.Format("{0:f}, {1:f}", X, Y);
        }

        public static double Distance(Position2 p1, Position2 p2)
        {
            return System.Math.Sqrt(SquaredDistance(p1, p2));
        }

        public static double SquaredDistance(Position2 p1, Position2 p2)
        {
            double ds = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            return ds;
        }

        public static bool IsEqual(Position2 p1, Position2 p2, double tolerance)
        {
            return Tolerance.IsZeroDistance(Distance(p1, p2), tolerance);
        }

        public static bool IsEqual(Position2 p1, Position2 p2)
        {
            return Tolerance.IsZeroDistance(Distance(p1, p2));
        }

        public bool IsLess(Position2 p)
        {
            return Tolerance.IsLessDistance(X, p.X) || (Tolerance.IsEqualDistance(X, p.X) && Tolerance.IsLessDistance(Y, p.Y));
        }

        public static bool IsLess(Position2 lhs, Position2 rhs)
        {
            return lhs.IsLess(rhs);
        }
    }
}
