using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public static class LineObbIntersector
    {
        public enum Status
        {
            Intersecting,
            NoIntersection
        }

        public static Status Intersect(LineSegment2 line, Obb2 obb, out double[] t)
        {
            if (Intersect(line.BaseLine, obb, out double[] u) == Status.Intersecting)
            {
                t = u.Where(x => x >= line.StartParameter - Tolerance.DistanceTolerance && x <= line.EndParameter + Tolerance.DistanceTolerance).ToArray();
                if (t.Count() == 0)
                {
                    t = null;
                    return Status.NoIntersection;
                }
                else
                    return Status.Intersecting;

            }
            else
            {
                t = null;
                return Status.NoIntersection;
            }
        }

        public static Status Intersect(Line2 line, Obb2 obb, out double[] t)
        {
            Line2 obbLeftLine = new Line2(obb.GlobalBottomLeft, obb.GlobalTopLeft);
            Line2 obbRightLine = new Line2(obb.GlobalBottomRight, obb.GlobalTopRight);
            Line2 obbBottomLine = new Line2(obb.GlobalBottomLeft, obb.GlobalBottomRight);
            Line2 obbTopLine = new Line2(obb.GlobalTopLeft, obb.GlobalTopRight);

            List<double> intersections = new List<double>();

            if(LineLineIntersector.Intersect(line, obbLeftLine, out double t1, out double t2) == LineLineIntersector.Status.Intersecting)
            {
                if(t2 >= -Tolerance.DistanceTolerance && t2 <= 1.0 + Tolerance.DistanceTolerance)
                    intersections.Add(t1);
            }

            if (LineLineIntersector.Intersect(line, obbRightLine, out double t3, out double t4) == LineLineIntersector.Status.Intersecting)
            {
                if (t4 >= -Tolerance.DistanceTolerance && t4 <= 1.0 + Tolerance.DistanceTolerance)
                    intersections.Add(t3);
            }

            if (LineLineIntersector.Intersect(line, obbBottomLine, out double t5, out double t6) == LineLineIntersector.Status.Intersecting)
            {
                if (t6 >= -Tolerance.DistanceTolerance && t6 <= 1.0 + Tolerance.DistanceTolerance)
                    intersections.Add(t5);
            }

            if (LineLineIntersector.Intersect(line, obbTopLine, out double t7, out double t8) == LineLineIntersector.Status.Intersecting)
            {
                if (t8 >= -Tolerance.DistanceTolerance && t8 <= 1.0 + Tolerance.DistanceTolerance)
                    intersections.Add(t7);
            }

            if(intersections.Count > 0)
            {
                intersections.Sort();
                t = intersections.ToArray();
                return Status.Intersecting;
            }

            t = null;
            return Status.NoIntersection;
        }
    }
}
