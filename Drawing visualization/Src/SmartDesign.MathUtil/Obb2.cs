using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Obb2
    {
        public static Obb2 Create(Position2 start, Position2 end)
        {
            Position2 min = start;
            Position2 max = end;

            Position2 centerInDrawing = min;
            Vector2 xAxis = new Vector2(centerInDrawing, max);
            if (xAxis.IsZero())
                xAxis = Vector2.OX;

            Axis22 coordinateSystem = new Axis22(centerInDrawing, xAxis);

            double angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, xAxis)).Radian;

            double localMinX = min.X * Math.Cos(-angle) - min.Y * Math.Sin(-angle) - centerInDrawing.X * Math.Cos(-angle) + centerInDrawing.Y * Math.Sin(-angle);
            double localMinY = min.X * Math.Sin(-angle) + min.Y * Math.Cos(-angle) - centerInDrawing.X * Math.Sin(-angle) - centerInDrawing.Y * Math.Cos(-angle);

            double localMaxX = max.X * Math.Cos(-angle) - max.Y * Math.Sin(-angle) - centerInDrawing.X * Math.Cos(-angle) + centerInDrawing.Y * Math.Sin(-angle);
            double localMaxY = max.X * Math.Sin(-angle) + max.Y * Math.Cos(-angle) - centerInDrawing.X * Math.Sin(-angle) - centerInDrawing.Y * Math.Cos(-angle);

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = new Position2(Math.Min(localMinX, localMaxX), Math.Min(localMinY, localMaxY)),
                LocalMax = new Position2(Math.Max(localMinX, localMaxX), Math.Max(localMinY, localMaxY))
            };

            return obb;
        }

        public static Obb2 Create(Position2[] positions)
        {
            double minX = positions[0].X;
            double minY = positions[0].Y;
            double maxX = positions[0].X;
            double maxY = positions[0].Y;

            foreach (var position in positions.Skip(1))
            {
                if (minX > position.X)
                    minX = position.X;

                if (maxX < position.X)
                    maxX = position.X;

                if (minY > position.Y)
                    minY = position.Y;

                if (maxY < position.Y)
                    maxY = position.Y;
            }

            Position2 min = new Position2(minX, minY);
            Position2 max = new Position2(maxX, maxY);

            Position2 centerInDrawing = min;
            Vector2 xAxis = Vector2.OX;

            Axis22 coordinateSystem = new Axis22(min, xAxis);

            double angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, xAxis)).Radian;

            double localMinX = min.X * Math.Cos(-angle) - min.Y * Math.Sin(-angle) - centerInDrawing.X * Math.Cos(-angle) + centerInDrawing.Y * Math.Sin(-angle);
            double localMinY = min.X * Math.Sin(-angle) + min.Y * Math.Cos(-angle) - centerInDrawing.X * Math.Sin(-angle) - centerInDrawing.Y * Math.Cos(-angle);

            double localMaxX = max.X * Math.Cos(-angle) - max.Y * Math.Sin(-angle) - centerInDrawing.X * Math.Cos(-angle) + centerInDrawing.Y * Math.Sin(-angle);
            double localMaxY = max.X * Math.Sin(-angle) + max.Y * Math.Cos(-angle) - centerInDrawing.X * Math.Sin(-angle) - centerInDrawing.Y * Math.Cos(-angle);

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = new Position2(Math.Min(localMinX, localMaxX), Math.Min(localMinY, localMaxY)),
                LocalMax = new Position2(Math.Max(localMinX, localMaxX), Math.Max(localMinY, localMaxY))
            };

            return obb;
        }

        public static Obb2 Create(Position2 position, double size)
        {
            Axis22 coordinateSystem = new Axis22(position, Vector2.OX);

            double localMinX = -size * 0.5;
            double localMinY = -size * 0.5;

            double localMaxX = size * 0.5;
            double localMaxY = size * 0.5;

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = new Position2(Math.Min(localMinX, localMaxX), Math.Min(localMinY, localMaxY)),
                LocalMax = new Position2(Math.Max(localMinX, localMaxX), Math.Max(localMinY, localMaxY))
            };

            return obb;
        }

        public static Obb2 Create(Axis22 coordinateSystem, double size)
        {
            double localMinX = -size * 0.5;
            double localMinY = -size * 0.5;

            double localMaxX = size * 0.5;
            double localMaxY = size * 0.5;

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = new Position2(Math.Min(localMinX, localMaxX), Math.Min(localMinY, localMaxY)),
                LocalMax = new Position2(Math.Max(localMinX, localMaxX), Math.Max(localMinY, localMaxY))
            };

            return obb;
        }

        public Axis22 CoordinateSystem { get; set; }        

        private Position2 min;
        private Position2 max;

        /// <summary>
        /// 지역좌표계에서의 바운딩 박스 왼쪽 아래 좌표
        /// </summary>
        public Position2 LocalMin
        {
            get { return min; }
            set { min = value; }
        }

        /// <summary>
        /// 지역좌표계에서의 바운딩 박스 오른쪽 위 좌표
        /// </summary>
        public Position2 LocalMax
        {
            get { return max; }
            set { max = value; }
        }

        /// <summary>
        /// LocalMin의 전역 좌표값
        /// </summary>
        public Position2 GlobalMin
        {
            get
            {
                // [1 0 Cx]   [cos -sin 0]   [cos  -sin  Cx]
                // [0 1 Cy] X [sin  cos 0] = [sin   cos  Cy]
                // [0 0  1]   [ 0    0  1]   [ 0     0    1]
                // y' = x cos - y sin + Cx
                // y' = x sin + y cos + Cy

                Angle angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, CoordinateSystem.XDirection));
                Position2 center = CoordinateSystem.Location;
                double x = LocalMin.X * Math.Cos(angle.Radian) - LocalMin.Y * Math.Sin(angle.Radian) + center.X;
                double y = LocalMin.X * Math.Sin(angle.Radian) + LocalMin.Y * Math.Cos(angle.Radian) + center.Y;

                return new Position2(x, y);
            }
        }

        /// <summary>
        /// LocalMax의 전역 좌표값
        /// </summary>
        public Position2 GlobalMax
        {
            get
            {
                // [1 0 Cx]   [cos -sin 0]   [cos  -sin  Cx]
                // [0 1 Cy] X [sin  cos 0] = [sin   cos  Cy]
                // [0 0  1]   [ 0    0  1]   [ 0     0    1]
                // y' = x cos - y sin + Cx
                // y' = x sin + y cos + Cy

                Angle angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, CoordinateSystem.XDirection));
                Position2 center = CoordinateSystem.Location;
                double x = LocalMax.X * Math.Cos(angle.Radian) - LocalMax.Y * Math.Sin(angle.Radian) + center.X;
                double y = LocalMax.X * Math.Sin(angle.Radian) + LocalMax.Y * Math.Cos(angle.Radian) + center.Y;

                return new Position2(x, y);
            }
        }

        public Position2 GlobalTopLeft
        {
            get
            {
                return new Position2(Math.Min(GlobalMin.X, GlobalMax.X), Math.Max(GlobalMin.Y, GlobalMax.Y));
            }
        }

        public Position2 GlobalTopRight
        {
            get
            {
                return new Position2(Math.Max(GlobalMin.X, GlobalMax.X), Math.Max(GlobalMin.Y, GlobalMax.Y));
            }
        }

        public Position2 GlobalBottomLeft
        {
            get
            {
                return new Position2(Math.Min(GlobalMin.X, GlobalMax.X), Math.Min(GlobalMin.Y, GlobalMax.Y));
            }
        }

        public Position2 GlobalBottomRight
        {
            get
            {
                return new Position2(Math.Max(GlobalMin.X, GlobalMax.X), Math.Min(GlobalMin.Y, GlobalMax.Y));
            }
        }

        /// <summary>
        /// 전역 좌표계에서 바운딩 박스 중심의 좌표. CoordinateSystem의 Location과 다른 것임.
        /// </summary>
        public Position2 Center
        {
            get
            {
                return (GlobalMin + GlobalMax) * 0.5;
            }
        }
        
        public double Width
        {
            get
            {
                return Math.Abs(LocalMax.X - LocalMin.X);
            }
        }

        public double Height
        {
            get
            {
                return Math.Abs(LocalMax.Y - LocalMin.Y);
            }
        }

        public double AspectRatio
        {
            get
            {
                if (Width == 0)
                    throw new InvalidOperationException("폭이 0이기 때문에 종횡비를 계산할 수 없습니다.");

                return Height / Width;
            }
        }

        public double Area()
        {
            return Math.Abs((LocalMax.X - LocalMin.X) * (LocalMax.Y - LocalMin.Y));
        }

        public bool Contains(Position2 p)
        {
            return Contains(p.X, p.Y);
        }

        public bool Contains(double x, double y)
        {
            double minX = Math.Min(LocalMin.X, LocalMax.X);
            double maxX = Math.Max(LocalMin.X, LocalMax.X);
            double minY = Math.Min(LocalMin.Y, LocalMax.Y);
            double maxY = Math.Max(LocalMin.Y, LocalMax.Y);

            Position2 p = new Position2(x, y);
            p = p - CoordinateSystem.Location;

            double angle = Vector2.Angle(Vector2.OX, CoordinateSystem.XDirection);
            Transformation2 rotation = Transformation2.CreateRotation(-angle);
            Position2 pInLocal = rotation.Transform(p);

            if((pInLocal.X >= minX) && (pInLocal.X <= maxX) && (pInLocal.Y >= minY) && (pInLocal.Y <= maxY))
            {
                return true;
            }

            return false;
        }

        public void Expand(double size)
        {
            Expand(size, size, size, size);
        }

        public void Expand(double left, double top, double right, double bottom)
        {
            double minX = LocalMin.X - left;
            double minY = LocalMin.Y - bottom;
            double maxX = LocalMax.X + right;
            double maxY = LocalMax.Y + top;

            if(minX > maxX)
            {
                minX = 0.0;
                maxX = 0.0;
            }    

            if(minY > maxY)
            {
                minY = 0.0;
                maxY = 0.0;
            }

            LocalMin = new Position2(minX, minY);
            LocalMax = new Position2(maxX, maxY);
        }

        public static Obb2 Expand(Obb2 obb, double size)
        {
            obb.Expand(size);
            return obb;
        }

        public static bool IsContainedIn(Obb2 obb1, Obb2 obb2)
        {
            if (obb2.Contains(obb1.GlobalMin.X, obb1.GlobalMin.Y) &&
                obb2.Contains(obb1.GlobalMin.X, obb1.GlobalMax.Y) &&
                obb2.Contains(obb1.GlobalMax.X, obb1.GlobalMin.Y) &&
                obb2.Contains(obb1.GlobalMax.X, obb1.GlobalMax.Y))
                return true;
            else
                return false;
        }

        public static double IntersectingArea(Obb2 obb1, Obb2 obb2)
        {
            double x1 = Math.Max(obb1.GlobalBottomLeft.X, obb2.GlobalBottomLeft.X);
            double y1 = Math.Max(obb1.GlobalBottomLeft.Y, obb2.GlobalBottomLeft.Y);
            double x2 = Math.Min(obb1.GlobalTopRight.X, obb2.GlobalTopRight.X);
            double y2 = Math.Min(obb1.GlobalTopRight.Y, obb2.GlobalTopRight.Y);

            double intersectingArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            return intersectingArea;
        }

        public static double IoU(Obb2 obb1, Obb2 obb2)
        {
            double x1 = Math.Max(obb1.GlobalBottomLeft.X, obb2.GlobalBottomLeft.X);
            double y1 = Math.Max(obb1.GlobalBottomLeft.Y, obb2.GlobalBottomLeft.Y);
            double x2 = Math.Min(obb1.GlobalTopRight.X, obb2.GlobalTopRight.X);
            double y2 = Math.Min(obb1.GlobalTopRight.Y, obb2.GlobalTopRight.Y);

            double intersectingArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            double iou = intersectingArea / (obb1.Area() + obb2.Area() - intersectingArea);

            return iou;
        }

        public enum Side
        {
            Left, Top, Right, Bottom
        }

        private struct SideDistanceData
        {
            public Side side;
            public double distance;
        }

        public static Side FindClosestSide(Position2 position, Obb2 obb2)
        {
            List<SideDistanceData> distances = new List<SideDistanceData>();

            Line2 leftLine = new Line2(obb2.GlobalBottomLeft, obb2.GlobalTopLeft);
            distances.Add(new SideDistanceData() { side = Side.Left, distance = leftLine.Distance(position) });

            Line2 rightLine = new Line2(obb2.GlobalBottomRight, obb2.GlobalTopRight);
            distances.Add(new SideDistanceData() { side = Side.Right, distance = rightLine.Distance(position) });

            Line2 bottomLine = new Line2(obb2.GlobalBottomLeft, obb2.GlobalBottomRight);
            distances.Add(new SideDistanceData() { side = Side.Bottom, distance = bottomLine.Distance(position) });

            Line2 topLine = new Line2(obb2.GlobalTopLeft, obb2.GlobalTopRight);
            distances.Add(new SideDistanceData() { side = Side.Top, distance = topLine.Distance(position) });

            var minSide = distances.OrderBy(x => x.distance).First();
            return minSide.side;
        }
    }
}
