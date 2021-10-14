using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Vector3
    {
        public static readonly Vector3 O = new Vector3();
        public static readonly Vector3 OX = new Vector3(1.0, 0.0, 0.0);
        public static readonly Vector3 OY = new Vector3(0.0, 1.0, 0.0);
        public static readonly Vector3 OZ = new Vector3(0.0, 0.0, 1.0);

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s");

            string[] valueStrings = s.Split(',');
            if (valueStrings.Length != 3)
                throw new FormatException();

            var values = valueStrings.Select(x => Convert.ToDouble(x)).ToArray();

            return new Vector3(values[0], values[1], values[2]);
        }

        public Vector3(double[] coordinates)
        {
            if (coordinates.Length != 3)
                throw new ArgumentException(nameof(coordinates) + "의 크기가 3이 아닙니다.");

            X = coordinates[0];
            Y = coordinates[1];
            Z = coordinates[2];
        }

        public static bool TryParse(string s, out Vector3 result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = new Vector3();
            }

            return false;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double Norm()
        {
            return System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public void Normalize()
        {
            double l = Norm();

            if(Tolerance.IsZeroDistance(l))
                throw new ArgumentOutOfRangeException();

            X = X / l;
            Y = Y / l;
            Z = Z / l;
        }

        public static Vector3 Normalize(Vector3 v)
        {
            v.Normalize();
            return v;
        }

        public bool IsZero()
        {
            double l = Norm();
            return Tolerance.IsZeroDistance(l);
        }

        public static double Dot(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            double x = v1.Y * v2.Z - v1.Z * v2.Y;
            double y = v1.Z * v2.X - v1.X * v2.Z;
            double z = v1.X * v2.Y - v1.Y * v2.X;

            return new Vector3(x, y, z);
        }

        public static double Angle(Vector3 v1, Vector3 v2)
        {
            v1.Normalize();
            v2.Normalize();

            double cosine = Dot(v1, v2);

            if (cosine > -MathConstants.Sqrt1_2 && cosine < MathConstants.Sqrt1_2)
                return Math.Acos(cosine);

            double x = v1.Y * v2.Z - v1.Z * v2.Y;
            double y = v1.Z * v2.X - v1.X * v2.Z;
            double z = v1.X * v2.Y - v1.Y * v2.X;
            double sine = Math.Sqrt(x * x + y * y + z * z);

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
                    case 2: return Z;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (i)
                {
                    case 0: X = value;
                        break;
                    case 1: Y = value;
                        break;
                    case 2: Z = value;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        public static Vector3 operator *(double d, Vector3 rhs)
        {
            return new Vector3(d * rhs.X, d * rhs.Y, d * rhs.Z);
        }

        public static Vector3 operator *(Vector3 lhs, double d)
        {
            return d * lhs;
        }

        public static Vector3 operator /(Vector3 lhs, double d)
        {
            return new Vector3(lhs.X / d, lhs.Y / d, lhs.Z / d);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return -1.0 * v;
        }

        public static explicit operator Vector3(Position3 p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public override string ToString()
        {
            return string.Format("{0:f}, {1:f}, {2:f}", X, Y, Z);
        }

        public static bool IsEqual(Vector3 v1, Vector3 v2)
        {
            double mag1 = v1.Norm();
            double mag2 = v2.Norm();

            Vector3 v = v1;
            v -= v2;

            if (Tolerance.IsZeroDistance(mag1) || Tolerance.IsZeroDistance(mag2))
                return Tolerance.IsZeroDistance(v.Norm());
            else
                return Tolerance.IsZeroDistance(v.Norm()) && Tolerance.IsZeroAngle(Angle(v1, v2));
        }

        public static bool IsParallel(Vector3 v1, Vector3 v2)
        {
            double mag1 = v1.Norm();
            double mag2 = v2.Norm();

            if (Tolerance.IsZeroDistance(mag1) || Tolerance.IsZeroDistance(mag2))
                return false;

            double angle = Angle(v1, v2);

            return Tolerance.IsZeroAngle(angle) || Tolerance.IsZeroAngle(Math.Abs(angle - Math.PI));
        }

        public static bool IsOpposite(Vector3 v1, Vector3 v2)
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
