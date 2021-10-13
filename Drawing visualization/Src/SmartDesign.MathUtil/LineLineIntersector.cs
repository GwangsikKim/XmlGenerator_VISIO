using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public static class LineLineIntersector
    {
        public enum Status
        {
            Intersecting,
            Parallel,
            Overlapping,
            NoIntersection
        }

        public static Status Intersect(Line2 line1, Line2 line2, out double t1, out double t2)
        {
            Position2 p1 = line1.Position(0.0);
            Position2 p2 = line1.Position(1.0);
            Position2 p3 = line2.Position(0.0);
            Position2 p4 = line2.Position(1.0);

            Vector2 a = new Vector2(p2, p1);
            Vector2 b = new Vector2(p3, p4);
            Vector2 c = new Vector2(p1, p3);

            double cb = Vector2.Cross(c, b);
            double ac = Vector2.Cross(a, c);
            double ba = Vector2.Cross(b, a);

            // 두 직선이 평행 또는 일직선 상에 있는지 확인
            if (Tolerance.IsZeroDistance(ba))
            {
                t1 = 0.0;
                t2 = 0.0;

                // 두 직선이 일직선 상에 있는지 확인
                if (Tolerance.IsZeroDistance(cb))
                    return Status.Overlapping;
                // 아니면 평행
                else
                    return Status.Parallel;
            }

            t1 = cb / ba;
            t2 = ac / ba;

            return Status.Intersecting;
        }

        public static Status Intersect(LineSegment2 line1, LineSegment2 line2, out double t1, out double t2)
        {
            Status status = Intersect(line1.BaseLine, line2.BaseLine, out t1, out t2);

            if (status == Status.Intersecting)
            {
                if (t1 < line1.StartParameter || t1 > line1.EndParameter || t2 < line2.StartParameter || t2 > line2.EndParameter)
                    status = Status.NoIntersection;
            }
            else if (status == Status.Overlapping)
            {
                Position2 left1 = line1.LeftPosition;
                Position2 right1 = line1.RightPosition;
                Position2 left2 = line2.LeftPosition;
                Position2 right2 = line2.RightPosition;

                if (Position2.IsEqual(right1, left2))
                {
                    t1 = line1.EndParameter;
                    t2 = line2.StartParameter;
                    status = Status.Intersecting;
                }
                else if (Position2.IsEqual(left1, right2))
                {
                    t1 = line1.StartParameter;
                    t2 = line2.EndParameter;
                    status = Status.Intersecting;
                }
                else if (right1.IsLess(left2) || right2.IsLess(left1)) // Non-overlapping
                {
                    status = Status.NoIntersection;
                }
            }

            return status;
        }
    }
}
