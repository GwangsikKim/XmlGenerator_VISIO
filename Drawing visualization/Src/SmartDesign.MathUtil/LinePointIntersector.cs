using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public static class LinePointIntersector
    {
        public static bool Intersect(Line2 line, Position2 p, out double t)
        {
            return Intersect(line, p, Tolerance.DistanceTolerance, out t);
        }

        public static bool Intersect(Line2 line, Position2 p, double tolerance, out double t)
        {
            t = 0.0;

            double d = line.SignedDistance(p);
            if (!Tolerance.IsZeroDistance(d, tolerance))
                return false;

            t = line.ParameterOfPoint(p);
            return true;
        }

        public static bool Intersect(LineSegment2 line, Position2 p, out double t)
        {
            return Intersect(line, p, Tolerance.DistanceTolerance, out t);
        }

        public static bool Intersect(LineSegment2 line, Position2 p, double tolerance, out double t)
        {
            t = 0.0;

            double d = line.BaseLine.SignedDistance(p);
            if (!Tolerance.IsZeroDistance(d, tolerance))
                return false;

            t = line.BaseLine.ParameterOfPoint(p);
            if (t < line.StartParameter || t > line.EndParameter)
                return false;
            else
                return true;
        }
    }
}
