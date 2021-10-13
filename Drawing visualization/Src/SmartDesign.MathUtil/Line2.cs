using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct Line2
    {
        public Line2(Position2 location, Vector2 velocity)
        {
            Location = location;
            Velocity = velocity;
        }

        public Line2(Position2 p1, Position2 p2)
        {
            Location = p1;
            Velocity = new Vector2(p1, p2);
        }

        public Line2(Axis12 axis)
        {
            Location = axis.Location;
            Velocity = axis.Direction;
        }

        public Position2 Location { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Direction
        {
            get
            {
                return Vector2.Normalize(Velocity);
            }
        }

        public double EvaluateXFromY(double y)
        {
            double t = (y - Location.Y) / Velocity.Y;

            return Location.X + t * Velocity.X;
        }

        public double EvaluateYFromX(double x)
        {
            double t = (x - Location.X) / Velocity.X;

            return Location.Y + t * Velocity.Y;
        }

        public Position2 Position(double t)
        {
            return Location + t * Velocity;
        }

        public Vector2 Normal(double t)
        {
            Vector2 dir = Direction;
            Vector2 normal = new Vector2(-dir.Y, dir.X);
            return Vector2.Normalize(normal);
        }

        public Vector2 Tangent(double t)
        {
            return Vector2.Normalize(D1(t));
        }

        public Vector2 D1(double t)
        {
            return Velocity;
        }

        public double ParameterOfPoint(Position2 p)
        {
            Position2 p1 = Location;
            Position2 p2 = Location + Velocity;

            Vector2 v01 = new Vector2(p, p1);
            Vector2 v12 = new Vector2(p1, p2);

            double t = -Vector2.Dot(v01, v12)/ v12.NormSquared();
            return t;
        }

        public double SignedDistance(Position2 p)
        {
            Position2 p1 = Location;
            Position2 p2 = Location + Velocity;

            Vector2 v10 = new Vector2(p1, p);
            Vector2 v20 = new Vector2(p2, p);
            Vector2 v12 = new Vector2(p1, p2);

            double d = Vector2.Cross(v10, v20) / v12.Norm();
            return d;
        }

        public double Distance(Position2 p)
        {
            return Math.Abs(SignedDistance(p));
        }
    }
}
