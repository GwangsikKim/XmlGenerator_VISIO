using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Matrix33
    {
        public static readonly Matrix33 Identity = new Matrix33() {
            M00 = 1.0, M01 = 0.0, M02 = 0.0,
            M10 = 0.0, M11 = 1.0, M12 = 0.0,
            M20 = 0.0, M21 = 0.0, M22 = 1.0
        };

        public Matrix33(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            M00 = m00; M01 = m01; M02 = m02;
            M10 = m10; M11 = m11; M12 = m12;
            M20 = m20; M21 = m21; M22 = m22;
        }

        public double M00 { get; set; }
        public double M01 { get; set; }
        public double M02 { get; set; }
        public double M10 { get; set; }
        public double M11 { get; set; }
        public double M12 { get; set; }
        public double M20 { get; set; }
        public double M21 { get; set; }
        public double M22 { get; set; }

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
                            case 2: return M02;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 1:
                        switch (j)
                        {
                            case 0: return M10;
                            case 1: return M11;
                            case 2: return M12;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 2:
                        switch (j)
                        {
                            case 0: return M20;
                            case 1: return M21;
                            case 2: return M22;
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
                            case 2: M02 = value; return;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 1:
                        switch (j)
                        {
                            case 0: M10 = value; return;
                            case 1: M11 = value; return;
                            case 2: M12 = value; return;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    case 2:
                        switch (j)
                        {
                            case 0: M20 = value; return;
                            case 1: M21 = value; return;
                            case 2: M22 = value; return;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static Matrix33 operator +(Matrix33 lhs, Matrix33 rhs)
        {
            return new Matrix33(
                lhs.M00 + rhs.M00, lhs.M01 + rhs.M01, lhs.M02 + rhs.M02,
                lhs.M10 + rhs.M10, lhs.M11 + rhs.M11, lhs.M12 + rhs.M12,
                lhs.M20 + rhs.M20, lhs.M21 + rhs.M21, lhs.M22 + rhs.M22
                );
        }

        public static Matrix33 operator -(Matrix33 lhs, Matrix33 rhs)
        {
            return new Matrix33(
                lhs.M00 - rhs.M00, lhs.M01 - rhs.M01, lhs.M02 - rhs.M02,
                lhs.M10 - rhs.M10, lhs.M11 - rhs.M11, lhs.M12 - rhs.M12,
                lhs.M20 - rhs.M20, lhs.M21 - rhs.M21, lhs.M22 - rhs.M22
                );
        }

        public static Matrix33 operator *(Matrix33 lhs, Matrix33 rhs)
        {
            return new Matrix33(
                lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10 + lhs.M02 * rhs.M20,
                lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11 + lhs.M02 * rhs.M21,
                lhs.M00 * rhs.M02 + lhs.M01 * rhs.M12 + lhs.M02 * rhs.M22,
                lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10 + lhs.M12 * rhs.M20,
                lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21,
                lhs.M10 * rhs.M02 + lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22,
                lhs.M20 * rhs.M00 + lhs.M21 * rhs.M10 + lhs.M22 * rhs.M20,
                lhs.M20 * rhs.M01 + lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21,
                lhs.M20 * rhs.M02 + lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22
            );
        }

        public static Matrix33 operator *(Matrix33 m, double d)
        {
            return new Matrix33(
                m.M00 * d, m.M01 * d, m.M02 * d,
                m.M10 * d, m.M11 * d, m.M12 * d,
                m.M20 * d, m.M21 * d, m.M22 * d
                );
        }

        public static Matrix33 operator *(double d, Matrix33 m)
        {
            return m * d;
        }

        public static Vector3 operator *(Matrix33 m, Vector3 v)
        {
            return new Vector3(
                m.M00 * v.X + m.M01 * v.Y + m.M02 * v.Z,
                m.M10 * v.X + m.M11 * v.Y + m.M12 * v.Z,
                m.M20 * v.X + m.M21 * v.Y + m.M22 * v.Z
            );
        }

        public static Position3 operator *(Matrix33 m, Position3 p)
        {
            return new Position3(
                m.M00 * p.X + m.M01 * p.Y + m.M02 * p.Z,
                m.M10 * p.X + m.M11 * p.Y + m.M12 * p.Z,
                m.M20 * p.X + m.M21 * p.Y + m.M22 * p.Z
            );
        }

        public static Matrix33 operator -(Matrix33 m)
        {
            return -1.0 * m;
        }
    }
}
