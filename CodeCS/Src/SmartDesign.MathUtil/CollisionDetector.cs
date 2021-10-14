using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public static class CollisionDetector
    {

        public static bool Collide(Obb2 obb1, Obb2 obb2)
        {
            if (IsOverlapped(obb1, obb2, obb1.CoordinateSystem.XDirection) &&
               IsOverlapped(obb1, obb2, obb1.CoordinateSystem.YDirection) &&
               IsOverlapped(obb1, obb2, obb2.CoordinateSystem.XDirection) &&
               IsOverlapped(obb1, obb2, obb2.CoordinateSystem.YDirection))
                return true;
            else
                return false;
        }

        private static bool IsOverlapped(Obb2 obb1, Obb2 obb2, Vector2 projectionAxis)
        {
            Vector2 ax = obb1.CoordinateSystem.XDirection;
            Vector2 ay = obb1.CoordinateSystem.YDirection;
            double wa = obb1.Width * 0.5;
            double ha = obb1.Height * 0.5;

            Vector2 bx = obb2.CoordinateSystem.XDirection;
            Vector2 by = obb2.CoordinateSystem.YDirection;
            double wb = obb2.Width * 0.5;
            double hb = obb2.Height * 0.5;

            double projAx = Math.Abs(Vector2.Dot(wa * ax, projectionAxis));
            double projAy = Math.Abs(Vector2.Dot(ha * ay, projectionAxis));
            double projBx = Math.Abs(Vector2.Dot(wb * bx, projectionAxis));
            double projBy = Math.Abs(Vector2.Dot(hb * by, projectionAxis));
            double projectedLength = projAx + projAy + projBx + projBy;

            Vector2 centerDistance = new Vector2(obb1.Center, obb2.Center);
            double distanceCenters = Math.Abs(Vector2.Dot(centerDistance, projectionAxis));

            return distanceCenters < projectedLength;
        }
    }
}
