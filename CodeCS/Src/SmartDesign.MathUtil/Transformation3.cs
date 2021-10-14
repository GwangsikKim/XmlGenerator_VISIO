using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Transformation3
    {
        public Transformation3(Matrix33 rotation, Vector3 translation)
        {
            Rotation = rotation;
            Translation = translation;
        }

        public const int ColumnSize = 4;
        public const int RowSize = 3;

        public Matrix33 Rotation { get; set; }
        public Vector3 Translation { get; set; }

        public double this[int i, int j]
        {
            get
            {
                if (j == ColumnSize - 1)
                    return Translation[i];
                else
                    return Rotation[i, j];
            }
        }

        public void ToAxisRotation(out Axis13 axis, out Angle angle)
        {
            double a = Rotation[0, 0];
            double b = Rotation[0, 1];
            double c = Rotation[0, 2];
            double d = Rotation[1, 0];
            double e = Rotation[1, 1];
            double f = Rotation[1, 2];
            double g = Rotation[2, 0];            
            double h = Rotation[2, 1];
            double i = Rotation[2, 2];            

            Vector3 dir = new Vector3(h - f, c - g, d - b);
            if (dir.IsZero())
            {
                axis = new Axis13(new Position3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 1.0));
                angle = Angle.FromRadian(0.0);
                return;
            }

            double tr = a + e + i;
            double radian = System.Math.Acos(0.5 * (tr - 1.0));

            dir.Normalize();

            axis = new Axis13((Position3)Translation, dir);
            angle = Angle.FromRadian(radian);
        }

        public static Transformation3 CreateIdentity()
        {
            return new Transformation3() { Rotation = Matrix33.Identity, Translation = new Vector3(0.0, 0.0, 0.0) };
        }

        public static Transformation3 CreateTranslation(Vector3 displacement)
        {
            return new Transformation3() { Rotation = Matrix33.Identity, Translation = displacement };
        }

        public static Transformation3 CreateScale(double xScaleFactor, double yScaleFactor, double zScaleFactor)
        {
            Matrix33 rotation = new Matrix33(
                xScaleFactor, 0.0, 0.0, 
                0.0, yScaleFactor, 0.0, 
                0.0, 0.0, zScaleFactor
                );

            return new Transformation3(rotation, new Vector3(0.0, 0.0, 0.0));
        }

        public static Transformation3 CreateScale(Position3 fixedPoint, double xScaleFactor, double yScaleFactor, double zScaleFactor)
        {
            Matrix33 rotation = new Matrix33(xScaleFactor, 0.0, 0.0, 0.0, yScaleFactor, 0.0, 0.0, 0.0, zScaleFactor);
            Vector3 translation = new Vector3(
                fixedPoint.X * (1.0 - xScaleFactor)    
                , fixedPoint.Y * (1.0 - yScaleFactor)
                , fixedPoint.Z * (1.0 - zScaleFactor)
    		);

            return new Transformation3(rotation, translation);
        }

        public static Transformation3 CreateRotation(Axis13 axis, double angle)
        {
            double cosine = System.Math.Cos(angle);
            double sine = System.Math.Sin(angle);
            double oneCosine = 1.0 - cosine;
            Vector3 n = axis.Direction;
            double nx = n.X, ny = n.Y, nz = n.Z;

            Matrix33 rot = new Matrix33(
                cosine + oneCosine * nx * nx, oneCosine * nx * ny - nz * sine, oneCosine * nx * nz + ny * sine,
                oneCosine * ny * nx + nz * sine, cosine + oneCosine * ny * ny, oneCosine * ny * nz - nx * sine,
                oneCosine * nz * nx - ny * sine, oneCosine * nz * ny + nx * sine, cosine + oneCosine * nz * nz
    		    );

            Vector3 p0 = (Vector3)axis.Location;
            Vector3 v0 = rot * (-p0);
            v0 += p0;

            return new Transformation3(rot, v0);
        }

        public static Transformation3 CreateMirror(Position3 center)
        {
            return new Transformation3(-Matrix33.Identity, (Vector3)(2 * center));
        }

        public static Transformation3 CreateMirror(Axis13 axis)
        {
            Vector3 n = axis.Direction;
            double nx = n.X, ny = n.Y, nz = n.Z;
            double nxny = 2.0 * nx * ny;
            double nxnz = 2.0 * nx * nz;
            double nynz = 2.0 * ny * nz;

            Matrix33 mirror = new Matrix33(
                2.0 * nx * nx - 1.0, nxny, nxnz,
                nxny, 2.0 * ny * ny - 1.0, nynz,
                nxnz, nynz, 2.0 * nz * nz - 1.0
    		    );

            Vector3 p0 = (Vector3)axis.Location;
            Vector3 v0 = mirror * (-p0);
            v0 += p0;

            return new Transformation3(mirror, v0);
        }

        public static Transformation3 CreateMirror(Planes plane)
        {
            double a = 0.0, b = 0.0, c = 1.0;

            switch (plane)
            {
                case Planes.XYPlane:
                    a = 0.0;b = 0.0;c = 1.0;
                    break;
                case Planes.YZPlane:
                    a = 1.0;b = 0.0;c = 0.0;
                    break;
                case Planes.ZXPlane:
                    a = 0.0; b = 1.0; c = 0.0;
                    break;
            }

            Matrix33 rotation = new Matrix33(
                1.0 - 2.0 * a * a, -2.0 * a * b, -2.0 * a * c,
                -2.0 * a * b, 1.0 - 2.0 * b * b, -2.0 * b * c,
                -2.0 * a * c, -2.0 * b * c, 1.0 - 2.0 * c * c);

            return new Transformation3(rotation, new Vector3(0.0, 0.0, 0.0));
        }

        public static Transformation3 operator *(Transformation3 lhs, Transformation3 rhs)
        {
            Transformation3 trsf = new Transformation3();
            trsf.Translation = lhs.Translation + lhs.Rotation * rhs.Translation;
            trsf.Rotation = lhs.Rotation * rhs.Rotation;

            return trsf;
        }
    }
}
