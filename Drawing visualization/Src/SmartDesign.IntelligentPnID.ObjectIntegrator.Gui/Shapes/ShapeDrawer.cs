using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    static class ShapeDrawer
    {
        public static double PixelsPerDip { get; set; } = 1.25;

        public static void DrawBox(DrawingContext drawingContext, Obb2 extent, Color border)
        {
            DrawBox(drawingContext, extent, border, 1.0);
        }

        public static void DrawAxes(DrawingContext drawingContext, Axis22 axes)
        {
            const double size = 30.0;

            Position2 xAxis = axes.Location + size * axes.XDirection;
            Position2 yAxis = axes.Location + size * axes.YDirection;

            DrawLine(drawingContext, axes.Location, xAxis, Color.FromRgb(255, 0, 0), 3.0);
            DrawLine(drawingContext, axes.Location, yAxis, Color.FromRgb(0, 255, 0), 3.0);
        }

        public static void DrawBox(DrawingContext drawingContext, Obb2 extent, Color border, double thickness)
        {
            Pen borderPen = new Pen();
            borderPen.Thickness = thickness;
            borderPen.Brush = new SolidColorBrush(border);

            Vector2 xDirection = extent.CoordinateSystem.XDirection;
            Angle angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, xDirection));

            RotateTransform transform = new RotateTransform(angle.Degree, extent.CoordinateSystem.Location.X, extent.CoordinateSystem.Location.Y);
            drawingContext.PushTransform(transform);

            Rect rect = new Rect(extent.CoordinateSystem.Location.X + extent.LocalMin.X, extent.CoordinateSystem.Location.Y + extent.LocalMin.Y, extent.Width, extent.Height);

            double halfPenWidth = borderPen.Thickness / 2;

            // Create a guidelines set
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rect.Left + halfPenWidth);
            guidelines.GuidelinesX.Add(rect.Right + halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Top + halfPenWidth);
            guidelines.GuidelinesY.Add(rect.Bottom + halfPenWidth);

            drawingContext.PushGuidelineSet(guidelines);

            drawingContext.DrawRectangle(null, borderPen, rect);

            drawingContext.Pop();
            drawingContext.Pop();

            // 디버깅을 위해 Global Min/Max를 표시
            /*
            Point globalMin = new Point(extent.GlobalMin.X, extent.GlobalMin.Y);
            Point globalMax = new Point(extent.GlobalMax.X, extent.GlobalMax.Y);
            drawingContext.DrawLine(borderPen, globalMin, globalMax);
            */
        }

        public static void DrawFilledBox(DrawingContext drawingContext, Obb2 extent, Color color)
        {
            Brush brush = new SolidColorBrush(color);

            Vector2 xDirection = extent.CoordinateSystem.XDirection;
            Angle angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, xDirection));

            RotateTransform transform = new RotateTransform(angle.Degree, extent.CoordinateSystem.Location.X, extent.CoordinateSystem.Location.Y);
            drawingContext.PushTransform(transform);

            Rect rect = new Rect(extent.CoordinateSystem.Location.X + extent.LocalMin.X, extent.CoordinateSystem.Location.Y + extent.LocalMin.Y, extent.Width, extent.Height);

            // Create a guidelines set
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rect.Left);
            guidelines.GuidelinesX.Add(rect.Right);
            guidelines.GuidelinesY.Add(rect.Top);
            guidelines.GuidelinesY.Add(rect.Bottom);

            drawingContext.PushGuidelineSet(guidelines);

            drawingContext.DrawRectangle(brush, null, rect);
            
            drawingContext.Pop();
            drawingContext.Pop();
        }

        public static void DrawLine(DrawingContext drawingContext, Position2 start, Position2 end, Color color, double thickness)
        {
            DrawLine(drawingContext, start, end, color, DashStyles.Solid, thickness);
        }

        public static void DrawLine(DrawingContext drawingContext, Position2 start, Position2 end, Color color, DashStyle dashStyle, double thickness)
        {
            Pen pen = new Pen();
            pen.Thickness = thickness;
            pen.DashStyle = dashStyle;
            pen.Brush = new SolidColorBrush(color); //주황            

            var startPointX = start.X;
            var startPointY = start.Y;
            var endPointX = end.X;
            var endPointY = end.Y;

            Point startPointShape = new Point(startPointX, startPointY);
            Point endPointShape = new Point(endPointX, endPointY);

            double halfPenWidth = pen.Thickness / 2;

            // Create a guidelines set
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(startPointX + halfPenWidth);
            guidelines.GuidelinesX.Add(endPointX + halfPenWidth);
            guidelines.GuidelinesY.Add(startPointY + halfPenWidth);
            guidelines.GuidelinesY.Add(endPointY + halfPenWidth);

            drawingContext.PushGuidelineSet(guidelines);

            drawingContext.DrawLine(pen, startPointShape, endPointShape);

            drawingContext.Pop();
        }

        public static void DrawCenterLine(DrawingContext drawingContext, CenterLine line, Color color, double thickness)
        {
            DrawCenterLine(drawingContext, line, color, DashStyles.Solid, thickness);
        }

        public static void DrawCenterLine(DrawingContext drawingContext, CenterLine line, Color color, DashStyle dashStyle, double thickness)
        {
            for(int i = 0; i < line.Coordinates.Count - 1; ++i)
            {
                Position2 start = line.Coordinates[i];
                Position2 end = line.Coordinates[i + 1];

                DrawLine(drawingContext, start, end, color, dashStyle, thickness);
            }
        }

        public static void DrawNode(DrawingContext drawingContext, Node node, bool fill)
        {
            const double NodeSize = 2.0;

            //Node
            Pen nodePen = new Pen();
            nodePen.Thickness = 1;
            nodePen.Brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            Brush nodeBrush = null;
            if(fill)
                nodeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            Point point = new Point(node.Coordinate.X, node.Coordinate.Y);

            drawingContext.DrawEllipse(nodeBrush, nodePen, point, NodeSize, NodeSize);
        }

        public static void DrawText(DrawingContext drawingContext, Obb2 extent, string text)
        {
            DrawText(drawingContext, extent, text, ShapeColors.Text);
        }

        public static void DrawText(DrawingContext drawingContext, Obb2 extent, string text, Color color)
        {
            Vector2 xDirection = extent.CoordinateSystem.XDirection;
            Angle angle = Angle.FromRadian(Vector2.Angle(Vector2.OX, xDirection));

            RotateTransform rotation = new RotateTransform(angle.Degree, extent.CoordinateSystem.Location.X, extent.CoordinateSystem.Location.Y);
            drawingContext.PushTransform(rotation);

            Rect rect = new Rect(extent.CoordinateSystem.Location.X + extent.LocalMin.X, extent.CoordinateSystem.Location.Y + extent.LocalMin.Y, extent.Width, extent.Height);

            FormattedText formattedText = 
                new FormattedText(text, CultureInfo.GetCultureInfo("ko-KR"), FlowDirection.LeftToRight, new Typeface("Verdana"), 9, new SolidColorBrush(color), PixelsPerDip);
            formattedText.TextAlignment = TextAlignment.Center;

            double textStandardX = rect.Left + rect.Width / 2;
            double textStandardY = rect.Top;

            Point point = new Point(textStandardX, textStandardY);

            // 텍스트가 뒤집어져 있기 때문에 다시 뒤집어야 한다.
            MatrixTransform mirror = new MatrixTransform(1.0, 0.0, 0.0, -1.0, 0.0, 2.0 * (rect.Y + rect.Height * 0.5));
            drawingContext.PushTransform(mirror);
            drawingContext.DrawText(formattedText, point);
            drawingContext.Pop();

            drawingContext.Pop();
        }

        public static void DrawArrow(DrawingContext drawingContext, Position2 position, Vector2 length, Color color, DashStyle dashStyle, double thickness)
        {
            Pen pen = new Pen();
            pen.Thickness = thickness;
            pen.DashStyle = dashStyle;
            pen.Brush = new SolidColorBrush(color);

            Position2 tipPosition = position + length;

            Vector2 end1Vector = new Vector2(-length.Y, length.X);
            end1Vector = end1Vector * 0.5;
            Vector2 end2Vector = -end1Vector;

            Position2 end1Position = position + end1Vector;
            Position2 end2Position = position + end2Vector;

            double halfPenWidth = pen.Thickness / 2;

            // Create a guidelines set
            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(position.X + halfPenWidth);
            guidelines.GuidelinesX.Add(tipPosition.X + halfPenWidth);
            guidelines.GuidelinesX.Add(end1Position.X + halfPenWidth);
            guidelines.GuidelinesX.Add(end2Position.X + halfPenWidth);
            guidelines.GuidelinesY.Add(position.Y + halfPenWidth);
            guidelines.GuidelinesY.Add(tipPosition.Y + halfPenWidth);
            guidelines.GuidelinesY.Add(end1Position.Y + halfPenWidth);
            guidelines.GuidelinesY.Add(end2Position.Y + halfPenWidth);

            drawingContext.PushGuidelineSet(guidelines);

            drawingContext.DrawLine(pen, new Point(position.X, position.Y), new Point(tipPosition.X, tipPosition.Y));
            drawingContext.DrawLine(pen, new Point(tipPosition.X, tipPosition.Y), new Point(end1Position.X, end1Position.Y));
            drawingContext.DrawLine(pen, new Point(tipPosition.X, tipPosition.Y), new Point(end2Position.X, end2Position.Y));

            drawingContext.Pop();
        }
    }
}
