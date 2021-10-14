using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Vector2
    {
        public static readonly Vector2 O = new Vector2();
        public static readonly Vector2 OX = new Vector2(1.0, 0.0);
        public static readonly Vector2 OY = new Vector2(0.0, 1.0);

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2(Position2 start, Position2 end)
        {
            X = end.X - start.X;
            Y = end.Y - start.Y;
        }

        public static Vector2 Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");

            string[] valueStrings = s.Split(',');
            if (valueStrings.Length != 2)
                throw new FormatException();

            var values = valueStrings.Select(x => Convert.ToDouble(x)).ToArray();

            return new Vector2(values[0], values[1]);
        }

        public Vector2(double[] coordinates)
        {
            if (coordinates.Length != 2)
                throw new ArgumentException(nameof(coordinates) + "의 크기가 2가 아닙니다.");

            X = coordinates[0];
            Y = coordinates[1];
        }

        public static bool TryParse(string s, out Vector2 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new Vector2();
            }

            return false;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double NormSquared()
        {
            return X * X + Y * Y;
        }

        public double Norm()
        {
            return System.Math.Sqrt(NormSquared());
        }

        public void Normalize()
        {
            double l = Norm();
            if (l == 0)
            {
                l = 1;
            }

            if (Tolerance.IsZeroDistance(l))
                throw new ArgumentOutOfRangeException();

            X = X / l;
            Y = Y / l;
        }

        public static Vector2 Normalize(Vector2 v)
        {
            v.Normalize();
            return v;
        }

        public bool IsZero()
        {
            double l = Norm();
            return Tolerance.IsZeroDistance(l);
        }

        public static double Dot(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static double Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        public static double Angle(Vector2 v1, Vector2 v2)
        {
            if (v1.IsZero() || v2.IsZero())
                return 0.0;

            v1.Normalize();
            v2.Normalize();

            double cosine = Dot(v1, v2);

            if (cosine > -MathConstants.Sqrt1_2 && cosine < MathConstants.Sqrt1_2)
                return Math.Acos(cosine);

            double z = v1.X * v2.Y - v1.Y * v2.X;
            double sine = Math.Sqrt(z * z);

            return (cosine < 0.0) ? Math.PI - Math.Asin(sine) : Math.Asin(sine);
        }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (i)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vector2 operator *(double d, Vector2 rhs)
        {
            return new Vector2(d * rhs.X, d * rhs.Y);
        }

        public static Vector2 operator *(Vector2 lhs, double d)
        {
            return d * lhs;
        }

        public static Vector2 operator /(Vector2 lhs, double d)
        {
            return new Vector2(lhs.X / d, lhs.Y / d);
        }

        public static Vector2 operator -(Vector2 v)
        {
            return -1.0 * v;
        }

        public static explicit operator Vector2(Position2 p)
        {
            return new Vector2(p.X, p.Y);
        }

        public override string ToString()
        {
            return string.Format("{0:f}, {1:f}", X, Y);
        }

        public static bool IsEqual(Vector2 v1, Vector2 v2)
        {
            double mag1 = v1.Norm();
            double mag2 = v2.Norm();

            Vector2 v = v1;
            v -= v2;

            if (Tolerance.IsZeroDistance(mag1) || Tolerance.IsZeroDistance(mag2))
                return Tolerance.IsZeroDistance(v.Norm());
            else
                return Tolerance.IsZeroDistance(v.Norm()) && Tolerance.IsZeroAngle(Angle(v1, v2));
        }

        public static bool IsParallel(Vector2 v1, Vector2 v2)
        {
            double mag1 = v1.Norm();
            double mag2 = v2.Norm();

            if (Tolerance.IsZeroDistance(mag1) || Tolerance.IsZeroDistance(mag2))
                return false;

            double angle = Angle(v1, v2);

            return Tolerance.IsZeroAngle(angle) || Tolerance.IsZeroAngle(Math.Abs(angle - Math.PI));
        }

        public static bool IsOpposite(Vector2 v1, Vector2 v2)
        {
            double mag1 = v1.Norm();
            double mag2 = v2.Norm();

            if (Tolerance.IsZeroDistance(mag1) || Tolerance.IsZeroDistance(mag2))
                return false;

            double angle = Angle(v1, v2);

            return Tolerance.IsZeroAngle(Math.Abs(angle - Math.PI));
        }
    }
}
