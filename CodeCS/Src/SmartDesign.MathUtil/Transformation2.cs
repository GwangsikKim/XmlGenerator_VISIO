using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Transformation2
    {
        public Transformation2(Matrix22 rotation, Vector2 translation)
        {
            Rotation = rotation;
            Translation = translation;
        }

        public const int ColumnSize = 3;
        public const int RowSize = 2;

        public Matrix22 Rotation { get; set; }
        public Vector2 Translation { get; set; }

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

        public static Transformation2 CreateIdentity()
        {
            return new Transformation2() { Rotation = Matrix22.Identity, Translation = Vector2.O };
        }

        public static Transformation2 CreateTranslation(Vector2 displacement)
        {
            return new Transformation2() { Rotation = Matrix22.Identity, Translation = displacement };
        }

        public static Transformation2 CreateScale(double xScaleFactor, double yScaleFactor)
        {
            Matrix22 rotation = new Matrix22(
                xScaleFactor, 0.0,
                0.0, yScaleFactor
                );

            return new Transformation2(rotation, Vector2.O);
        }

        public static Transformation2 CreateScale(Position2 fixedPoint, double xScaleFactor, double yScaleFactor)
        {
            Matrix22 rotation = new Matrix22(xScaleFactor, 0.0, 0.0, yScaleFactor);
            Vector2 translation = new Vector2(
                fixedPoint.X * (1.0 - xScaleFactor)
                , fixedPoint.Y * (1.0 - yScaleFactor)
            );

            return new Transformation2(rotation, translation);
        }

        public static Transformation2 CreateRotation(double angle)
        {
            double cosine = System.Math.Cos(angle);
            double sine = System.Math.Sin(angle);
            double oneCosine = 1.0 - cosine;

            Matrix22 rot = new Matrix22(
                cosine, -sine,
                sine, cosine
                );

            return new Transformation2(rot, Vector2.O);
        }

        public static Transformation2 CreateMirror(Position2 center)
        {
            return new Transformation2(-Matrix22.Identity, (Vector2)(2 * center));
        }

        public static Transformation2 CreateMirror(Axis12 axis)
        {
            Vector2 n = axis.Direction;
            double nx = n.X, ny = n.Y;
            double nxny = 2.0 * nx * ny;

            Matrix22 mirror = new Matrix22(
                2.0 * nx * nx - 1.0, nxny,
                nxny, 2.0 * ny * ny - 1.0
                );

            Vector2 p0 = (Vector2)axis.Location;
            Vector2 v0 = mirror * (-p0);
            v0 += p0;

            return new Transformation2(mirror, v0);
        }

        public static Transformation2 operator *(Transformation2 lhs, Transformation2 rhs)
        {
            Transformation2 trsf = new Transformation2();
            trsf.Translation = lhs.Translation + lhs.Rotation * rhs.Translation;
            trsf.Rotation = lhs.Rotation * rhs.Rotation;

            return trsf;
        }

        public Position2 Transform(Position2 p)
        {
            double x = Rotation.M00 * p.X + Rotation.M01 * p.Y + Translation.X;
            double y = Rotation.M10 * p.X + Rotation.M11 * p.Y + Translation.Y;
            return new Position2(x, y);
        }
    }
}