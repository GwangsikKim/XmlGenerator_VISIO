using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CenterLine
    {
        public CenterLine()
        {
            Coordinates = new List<Position2>();
            Coordinates.Add(new Position2());
            Coordinates.Add(new Position2());
        }

        public CenterLine(Position2[] positions)
        {
            Coordinates = positions.ToList();
            
            for(int i = Coordinates.Count; i < 2; ++i)
                Coordinates.Add(new Position2());
        }

        protected CenterLine(CenterLine other)
        {
            Coordinates = other.Coordinates.Select(c => c).ToList();
        }

        public List<Position2> Coordinates
        {
            get;
            private set;
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Position2 Start
        {
            get { return Coordinates.First(); }
            set { Coordinates[0] = value; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Position2 End
        {
            get { return Coordinates.Last(); }
            set { Coordinates[Coordinates.Count - 1] = value; }
        }

        public bool Contains(double x, double y)
        {
            Position2 p = new Position2(x, y);

            for(int i = 0; i < Coordinates.Count - 1; ++i)
            {
                Position2 start = Coordinates[i];
                Position2 end = Coordinates[i + 1];

                LineSegment2 line = new LineSegment2(start, end);
                bool isIntersected = LinePointIntersector.Intersect(line, p, out double t);
                if (isIntersected)
                    return true;
            }

            return false;
        }

        public double GetLength()
        {
            double length = 0.0;

            for (int i = 0; i < Coordinates.Count - 1; ++i)
            {
                Position2 start = Coordinates[i];
                Position2 end = Coordinates[i + 1];
                double subLength = Position2.Distance(start, end);
                length += subLength;
            }

            return length;
        }

        public virtual CenterLine Clone()
        {
            return new CenterLine(this);
        }
    }
}
