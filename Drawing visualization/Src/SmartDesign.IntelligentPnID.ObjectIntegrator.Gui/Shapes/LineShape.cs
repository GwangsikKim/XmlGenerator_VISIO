using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class LineShape : ShapeItem
    {
        private const double ArrowSize = 6.0;

        public LineShape(LineItem lineItem) : base(lineItem)
        {
            LineStyle = DashStyles.Solid;
        }

        protected LineItem LineItem
        {
            get { return (LineItem)PlantEntity; }
        }

        public DashStyle LineStyle
        {
            get;
            set;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);

            ShapeDrawer.DrawCenterLine(drawingContext, LineItem.CenterLine, DrawingColor, LineStyle, DrawingThickness);            

            if (LineItem.StartShape == LineEndShape.Arrow)
            {
                Vector2 dir = new Vector2(LineItem.CenterLine.Start, LineItem.CenterLine.Coordinates[1]);
                dir.Normalize();

                Position2 startArrowPosition = LineItem.CenterLine.Start + dir * ArrowSize;
                Vector2 startArrowLength = new Vector2(startArrowPosition, LineItem.CenterLine.Start);

                ShapeDrawer.DrawArrow(drawingContext, startArrowPosition, startArrowLength, DrawingColor, LineStyle, DrawingThickness);
            }

            if (LineItem.EndShape == LineEndShape.Arrow)
            {
                Vector2 dir = new Vector2(LineItem.CenterLine.Coordinates[LineItem.CenterLine.Coordinates.Count - 2], LineItem.CenterLine.End);
                dir.Normalize();

                Position2 endArrowPosition = LineItem.CenterLine.End - dir * ArrowSize;
                Vector2 endArrowLength = new Vector2(endArrowPosition, LineItem.CenterLine.End);

                ShapeDrawer.DrawArrow(drawingContext, endArrowPosition, endArrowLength, DrawingColor, LineStyle, DrawingThickness);
            }
        }

        public override bool HitTest(double x, double y)
        {
            const double Tolerance = 3.0;

            var coordinates = LineItem.CenterLine.Coordinates;
            for (int i = 0; i < coordinates.Count - 1; ++i)
            {
                Position2 start = coordinates[i];
                Position2 end = coordinates[i + 1];
                LineSegment2 line = new LineSegment2(start, end);

                if (LinePointIntersector.Intersect(line, new Position2(x, y), Tolerance, out double t))
                    return true;
            }

            return false;
        }
    }
}
