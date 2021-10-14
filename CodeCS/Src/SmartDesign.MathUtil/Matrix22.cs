using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Matrix22
    {
        public static readonly Matrix22 Identity = new Matrix22()
        {
            M00 = 1.0,
            M01 = 0.0,
            M10 = 0.0,
            M11 = 1.0
        };

        public Matrix22(double m00, double m01, double m10, double m11)
        {
            M00 = m00; M01 = m01;
            M10 = m10; M11 = m11;
        }

        public double M00 { get; set; }
        public double M01 { get; set; }
        public double M10 { get; set; }
        public double M11 { get; set; }

        public double this[int i, int j]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        switch (j)
                        {
                            case 0: return M00;
                            case 1: return M01;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 1:
                        switch (j)
                        {
                            case 0: return M10;
                            case 1: return M11;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            set
            {
                switch (i)
                {
                    case 0:
                        switch (j)
                        {
                            case 0: M00 = value; return;
                            case 1: M01 = value; return;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 1:
                        switch (j)
                        {
                            case 0: M10 = value; return;
                            case 1: M11 = value; return;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Matrix22 operator +(Matrix22 lhs, Matrix22 rhs)
        {
            return new Matrix22(
                lhs.M00 + rhs.M00, lhs.M01 + rhs.M01,
                lhs.M10 + rhs.M10, lhs.M11 + rhs.M11
                );
        }

        public static Matrix22 operator -(Matrix22 lhs, Matrix22 rhs)
        {
            return new Matrix22(
                lhs.M00 - rhs.M00, lhs.M01 - rhs.M01,
                lhs.M10 - rhs.M10, lhs.M11 - rhs.M11
                );
        }

        public static Matrix22 operator *(Matrix22 lhs, Matrix22 rhs)
        {
            return new Matrix22(
                lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10,
                lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11,
                lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10,
                lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11
            );
        }

        public static Matrix22 operator *(Matrix22 m, double d)
        {
            return new Matrix22(
                m.M00 * d, m.M01 * d,
                m.M10 * d, m.M11 * d
                );
        }

        public static Matrix22 operator *(double d, Matrix22 m)
        {
            return m * d;
        }

        public static Vector2 operator *(Matrix22 m, Vector2 v)
        {
            return new Vector2(
                m.M00 * v.X + m.M01 * v.Y,
                m.M10 * v.X + m.M11 * v.Y
            );
        }

        public static Position2 operator *(Matrix22 m, Position2 p)
        {
            return new Position2(
                m.M00 * p.X + m.M01 * p.Y,
                m.M10 * p.X + m.M11 * p.Y
            );
        }

        public static Matrix22 operator -(Matrix22 m)
        {
            return -1.0 * m;
        }
    }
}
