using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class LineItemSplitter
    {
        private class LineIntersectionData
        {
            public LineIntersectionData()
            {
                LineItemAdapter = null;
                Parameters = new List<double>();
            }

            public ILineItemAdapter LineItemAdapter
            {
                get;
                set;
            }

            public List<double> Parameters
            {
                get;
                set;
            }
        }

        private struct ParameterComparer : IEqualityComparer<double>
        {
            public bool Equals(double x, double y)
            {
                return Tolerance.IsEqualValue(x, y);
            }

            public int GetHashCode(double obj)
            {
                return obj.GetHashCode();
            }
        }

        private interface ILineItemAdapter
        {
            PlantEntity PlantEntity { get; }
            CenterLine CenterLine { get; }
            PlantEntity CreateNewLine(Position2 startPosition, Position2 endPosition);
        }

        private class PipingNetworkSystemAdapter : ILineItemAdapter
        {
            public PipingNetworkSystemAdapter(PipingNetworkSystem pipingNetworkSystem)
            {
                this.PipingNetworkSystem = pipingNetworkSystem;
            }

            public PipingNetworkSystem PipingNetworkSystem { get; set; }
            public PipingNetworkSegment PipingNetwrokSegment { get { return PipingNetworkSystem.PipingNetworkSegments[0]; } }

            public PlantEntity PlantEntity { get { return PipingNetworkSystem; } }

            public CenterLine CenterLine { get { return PipingNetwrokSegment.CenterLine; } }

            public PlantEntity CreateNewLine(Position2 startPosition, Position2 endPosition)
            {                
                PipingNetworkSegment pipingNetworkSegment = (PipingNetworkSegment)PipingNetwrokSegment.CloneNew();
                pipingNetworkSegment.CenterLine.Start = startPosition;
                pipingNetworkSegment.CenterLine.End = endPosition;
                pipingNetworkSegment.Extent = Obb2.Create(startPosition, endPosition);
                pipingNetworkSegment.ExpandedExtent = Obb2.Create(startPosition, endPosition);

                PipingNetworkSystem pipingNetworkSystem = new PipingNetworkSystem();
                pipingNetworkSystem.Add(pipingNetworkSegment);
                
                Node startNode = new Node();
                startNode.Coordinate = pipingNetworkSegment.CenterLine.Start;
                pipingNetworkSystem.Nodes[0] = startNode;

                Node endNode = new Node();
                endNode.Coordinate = pipingNetworkSegment.CenterLine.End;
                pipingNetworkSystem.Nodes[1] = endNode;

                pipingNetworkSystem.Extent = pipingNetworkSegment.Extent;
                pipingNetworkSystem.ExpandedExtent = pipingNetworkSegment.ExpandedExtent;

                return pipingNetworkSystem;
            }
        }

        private class SignalLineAdapter : ILineItemAdapter
        {
            public SignalLineAdapter(SignalLine signalLine)
            {
                this.SignalLine = signalLine;
            }

            public SignalLine SignalLine { get; set; }

            public PlantEntity PlantEntity { get { return SignalLine; } }

            public CenterLine CenterLine { get { return SignalLine.CenterLine; } }

            public PlantEntity CreateNewLine(Position2 startPosition, Position2 endPosition)
            {
                SignalLine signalLine = (SignalLine)SignalLine.CloneNew();
                signalLine.CenterLine.Start = startPosition;
                signalLine.CenterLine.End = endPosition;
                signalLine.Extent = Obb2.Create(startPosition, endPosition);
                signalLine.ExpandedExtent = Obb2.Create(startPosition, endPosition);

                return signalLine;
            }
        }

        private class UnknownLineAdapter : ILineItemAdapter
        {
            public UnknownLineAdapter(UnknownLine unknownLine)
            {
                this.UnknownLine = unknownLine;
            }

            public UnknownLine UnknownLine { get; set; }

            public PlantEntity PlantEntity { get { return UnknownLine; } }

            public CenterLine CenterLine { get { return UnknownLine.CenterLine; } }

            public PlantEntity CreateNewLine(Position2 startPosition, Position2 endPosition)
            {
                UnknownLine unknownLine = (UnknownLine)UnknownLine.CloneNew();
                unknownLine.CenterLine.Start = startPosition;
                unknownLine.CenterLine.End = endPosition;
                unknownLine.Extent = Obb2.Create(startPosition, endPosition);
                unknownLine.ExpandedExtent = Obb2.Create(startPosition, endPosition);

                return unknownLine;
            }
        }

        public const double DefaultDistanceTolerance = 2.0;

        public LineItemSplitter(PlantModel plantModel)
        {
            PlantModel = plantModel;
            DistanceTolerance = DefaultDistanceTolerance;
        }

        private PlantModel PlantModel
        {
            get;
            set;
        }

        public double DistanceTolerance
        {
            get;
            set;
        }

        public void Split()
        {
            // 배관 및 신호선 정보를 가져온다.
            List<ILineItemAdapter> targetLines = new List<ILineItemAdapter>();
            targetLines.AddRange(PlantModel.PipingNetworkSystems.Select(x => new PipingNetworkSystemAdapter(x)).ToList());
            targetLines.AddRange(PlantModel.SignalLines.Select(x => new SignalLineAdapter(x)).ToList());
            targetLines.AddRange(PlantModel.UnknownLines.Select(x => new UnknownLineAdapter(x)).ToList());

            // 분할
            List<LineIntersectionData> splitLineData = FindSplitItemsByLine(targetLines, targetLines);
            SplitLineItems(splitLineData);            
        }

        private List<LineIntersectionData> FindSplitItemsByLine(List<ILineItemAdapter> splitLineItems, List<ILineItemAdapter> splittingLineItems)
        {
            List<LineIntersectionData> lineIntersectionDataList = new List<LineIntersectionData>();

            for (int i = 0; i < splitLineItems.Count; i++)
            {
                CenterLine centerLine1 = splitLineItems[i].CenterLine;
                LineSegment2 line1 = new LineSegment2(centerLine1.Start, centerLine1.End);
                var extendedLine1 = line1.ExtendBoth(DistanceTolerance);

                // 분할되는 정보를 저장
                LineIntersectionData lineIntersectionData = new LineIntersectionData();
                lineIntersectionData.LineItemAdapter = splitLineItems[i];

                for (int j = 0; j < splittingLineItems.Count; j++)
                {
                    // 자를 선과 잘릴 선이 동일하면 무시
                    if (splitLineItems[i] == splittingLineItems[j])
                        continue;

                    CenterLine centerLine2 = splittingLineItems[j].CenterLine;
                    LineSegment2 line2 = new LineSegment2(centerLine2.Start, centerLine2.End);
                    var extendedLine2 = line2.ExtendBoth(DistanceTolerance);

                    if (!CheckIntersection(extendedLine1, extendedLine2, out double t1))
                        continue;

                    if (!lineIntersectionData.Parameters.Contains(t1, new ParameterComparer()))
                        lineIntersectionData.Parameters.Add(t1);

                    if (!lineIntersectionDataList.Contains(lineIntersectionData))
                    {
                        lineIntersectionDataList.Add(lineIntersectionData);
                    }
                }
            }
            return lineIntersectionDataList;
        }

        private bool CheckIntersection(LineSegment2 line1, LineSegment2 line2, out double t1)
        {
            var isIntersected = LineLineIntersector.Intersect(line1, line2, out t1, out double t2);

            if (isIntersected != LineLineIntersector.Status.Intersecting)
                return false;

            var intersectedPoint = line1.BaseLine.Position(t1);

            // 시작 점이 만나는 경우는 제외
            if (Position2.Distance(intersectedPoint, line1.StartPosition) < Tolerance.DistanceTolerance)
                return false;

            // 끝 점이 만나는 경우는 제외
            if (Position2.Distance(intersectedPoint, line1.EndPosition) < Tolerance.DistanceTolerance)
                return false;

            return true;
        }

        private void SplitLineItems(List<LineIntersectionData> lineIntersectionDataList)
        {
            for (int i = 0; i < lineIntersectionDataList.Count; i++)
            {
                ILineItemAdapter originalLineAdapter = lineIntersectionDataList[i].LineItemAdapter;

                CenterLine originalCenterLine = originalLineAdapter.CenterLine;
                LineSegment2 originalLine = new LineSegment2(originalCenterLine.Start, originalCenterLine.End);

                var parameters = lineIntersectionDataList[i].Parameters;
                parameters.Sort();

                Position2 intersectionPointFirst = originalLine.BaseLine.Position(parameters.First());
                Position2 intersectionPointLast = originalLine.BaseLine.Position(parameters.Last());

                if (Position2.Distance(originalLine.StartPosition, intersectionPointFirst) > DistanceTolerance)
                {
                    var splitFirstLine = originalLineAdapter.CreateNewLine(originalLine.StartPosition, intersectionPointFirst);
                    splitFirstLine.ID = originalLineAdapter.PlantEntity.ID; // 첫 번째 선을 원래 ID를 유지한다.
                    splitFirstLine.DisplayName = splitFirstLine.ID;
                    PlantModel.Add(splitFirstLine);
                }

                if (parameters.Count > 1)
                {
                    for (int j = 1; j < parameters.Count; j++)
                    {
                        Position2 intersectionPointStart = originalLine.BaseLine.Position(parameters[j - 1]);
                        Position2 intersectionPointEnd = originalLine.BaseLine.Position(parameters[j]);

                        if (Position2.Distance(intersectionPointStart, intersectionPointEnd) > DistanceTolerance)
                        {
                            var splitLine = originalLineAdapter.CreateNewLine(intersectionPointStart, intersectionPointEnd);
                            PlantModel.Add(splitLine);
                        }
                    }
                }

                if (Position2.Distance(intersectionPointLast, originalLine.EndPosition) > DistanceTolerance)
                {
                    var splitLastLine = originalLineAdapter.CreateNewLine(intersectionPointLast, originalLine.EndPosition);
                    PlantModel.Add(splitLastLine);
                }

                PlantModel.Children.Remove(originalLineAdapter.PlantEntity);
            }
        }
    }
}
