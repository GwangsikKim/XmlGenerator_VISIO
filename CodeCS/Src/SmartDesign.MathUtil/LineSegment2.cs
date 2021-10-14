using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.MathUtil
{
    public struct LineSegment2
    {
        public LineSegment2(Position2 p1, Position2 p2)
        {
            StartParameter = 0.0;
            EndParameter = 1.0;
            BaseLine = new Line2(p1, p2);
        }

        public LineSegment2(Line2 baseLine, double startParameter, double endParameter)
        {
            StartParameter = startParameter;
            EndParameter = endParameter;
            BaseLine = baseLine;
        }

        public Line2 BaseLine { get; set; }

        public double StartParameter { get; set; }
        public double EndParameter { get; set; }

        public Position2 StartPosition
        {
            get
            {
                return BaseLine.Position(StartParameter);
            }
        }

        public Position2 EndPosition
        {
            get
            {
                return BaseLine.Position(EndParameter);
            }
        }

        public Position2 LeftPosition
        {
            get
            {
                if (StartPosition.IsLess(EndPosition))
                    return EndPosition;
                else
                    return EndPosition;
            }
        }

        public Position2 RightPosition
        {
            get
            {
                if (StartPosition.IsLess(EndPosition))
                    return EndPosition;
                else
                    return StartPosition;
            }
        }

        public LineSegment2 ExtendToStart(double increment)
        {
            Vector2 addedVector = increment * BaseLine.Direction;

            var startPosition = StartPosition - addedVector;
            double newStartParameter = BaseLine.ParameterOfPoint(startPosition);

            LineSegment2 newLineSegment = new LineSegment2(BaseLine, newStartParameter, EndParameter);
            return newLineSegment;
        }

        public LineSegment2 ExtendToEnd(double increment)
        {
            Vector2 addedVector = increment * BaseLine.Direction;

            var endPosition = EndPosition + addedVector;
            double newEndParameter = BaseLine.ParameterOfPoint(endPosition);

            LineSegment2 newLineSegment = new LineSegment2(BaseLine, StartParameter, newEndParameter);
            return newLineSegment;
        }

        public LineSegment2 ExtendBoth(double lengthAtEnd)
        {
            Vector2 addedVector = lengthAtEnd * BaseLine.Direction;

            var startPosition = StartPosition - addedVector;
            double newStartParameter = BaseLine.ParameterOfPoint(startPosition);

            var endPosition = EndPosition + addedVector;
            double newEndParameter = BaseLine.ParameterOfPoint(endPosition);

            LineSegment2 newLineSegment = new LineSegment2(BaseLine, newStartParameter, newEndParameter);
            return newLineSegment;
        }

    }
}
