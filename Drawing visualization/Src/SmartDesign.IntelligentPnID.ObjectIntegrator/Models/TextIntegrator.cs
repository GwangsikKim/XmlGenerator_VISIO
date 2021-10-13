using DevExpress.Mvvm.Native;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    class TextIntegrationData
    {
        internal PlantItem Item;
        internal IHasExtent DistanceItem;
        internal double Distance;
    };

    class TextIntegrator
    {
        public const double DefaultMaximumDistance = 10.0;
        public const double DefaultDistanceTolerance = 2.0;
        public const double FakeLineEndSize = 6.0;

        public TextIntegrator(PlantModel plantModel)
        {
            PlantModel = plantModel;
            MaximumDistance = DefaultMaximumDistance;
            DistanceTolerance = DefaultDistanceTolerance;
        }

        private PlantModel PlantModel
        {
            get;
            set;
        }

        public double MaximumDistance
        {
            get;
            set;
        }

        public double DistanceTolerance
        {
            get;
            set;
        }

        public void Integrate()
        {
            List<PlantItem> targetEntities = new List<PlantItem>();
            targetEntities.AddRange(PlantModel.Instruments);
            targetEntities.AddRange(PlantModel.Equipments);
            targetEntities.AddRange(PlantModel.PipeConnectorSymbols);
            targetEntities.AddRange(PlantModel.PipingComponents);
            targetEntities.AddRange(PlantModel.SignalConnectorSymbols);
            targetEntities.AddRange(PlantModel.SignalLines);
            targetEntities.AddRange(PlantModel.PipingNetworkSystems.SelectMany(x => x.PipingNetworkSegments).ToList());

            targetEntities.AddRange(PlantModel.UnknownSymbols);
            targetEntities.AddRange(PlantModel.UnknownLines);

            var fakeLineItemEnds = MakeFakeLineEndItems();
            targetEntities.AddRange(fakeLineItemEnds);

            Integrate(targetEntities);
        }

        // 디버깅용
        public void IntegrateSymbolText()
        {
            List<PlantItem> targetEntities = new List<PlantItem>();
            targetEntities.AddRange(PlantModel.Instruments);
            targetEntities.AddRange(PlantModel.Equipments);
            targetEntities.AddRange(PlantModel.PipeConnectorSymbols);
            targetEntities.AddRange(PlantModel.PipingComponents);
            targetEntities.AddRange(PlantModel.SignalConnectorSymbols);
            targetEntities.AddRange(PlantModel.UnknownSymbols);

            Integrate(targetEntities);
        }

        // 디버깅용
        public void IntegrateLineText()
        {
            List<PlantItem> targetEntities = new List<PlantItem>();
            targetEntities.AddRange(PlantModel.SignalLines);
            targetEntities.AddRange(PlantModel.PipingNetworkSystems.SelectMany(x => x.PipingNetworkSegments).ToList());

            targetEntities.AddRange(PlantModel.UnknownLines);

            var fakeLineItemEnds = MakeFakeLineEndItems();
            targetEntities.AddRange(fakeLineItemEnds);

            Integrate(targetEntities);
        }

        private List<FakeLineEndItem> MakeFakeLineEndItems()
        {
            List<LineItem> lineWithArrowEnds = new List<LineItem>();
            var pipeLinesWithArrowEnds = PlantModel.PipingNetworkSystems.SelectMany(x => x.PipingNetworkSegments).Where(x => x.StartShape == LineEndShape.Arrow || x.EndShape == LineEndShape.Arrow);
            lineWithArrowEnds.AddRange(pipeLinesWithArrowEnds);

            var signalLinesWithArrowEnds = PlantModel.SignalLines.Where(x => x.StartShape == LineEndShape.Arrow || x.EndShape == LineEndShape.Arrow);
            lineWithArrowEnds.AddRange(signalLinesWithArrowEnds);

            var unknownLinesWithArrowEnds = PlantModel.UnknownLines.Where(x => x.StartShape == LineEndShape.Arrow || x.EndShape == LineEndShape.Arrow);
            lineWithArrowEnds.AddRange(unknownLinesWithArrowEnds);

            List<FakeLineEndItem> fakeLineEndItems = new List<FakeLineEndItem>();
            foreach (var lineItem in lineWithArrowEnds)
            {
                if(lineItem.StartShape == LineEndShape.Arrow)
                {
                    var item = CreateFakeLineEndItem(lineItem.CenterLine.Start, lineItem, FakeLineEndSize);
                    fakeLineEndItems.Add(item);
                }

                if (lineItem.EndShape == LineEndShape.Arrow)
                {
                    var item = CreateFakeLineEndItem(lineItem.CenterLine.End, lineItem, FakeLineEndSize);
                    fakeLineEndItems.Add(item);
                }
            }

            return fakeLineEndItems;
        }

        private FakeLineEndItem CreateFakeLineEndItem(Position2 position, LineItem lineItem, double size)
        {
            FakeLineEndItem item = new FakeLineEndItem();
            item.LineItem = lineItem;
            item.Parent = lineItem.Parent;
            item.PlantModel = lineItem.PlantModel;

            Obb2 extent = new Obb2();

            Axis22 coordinateSystem = new Axis22(position, Vector2.OX);
            extent.CoordinateSystem = coordinateSystem;
            extent.LocalMin = new Position2(-size * 0.5, -size * 0.5);
            extent.LocalMax = new Position2(size * 0.5, size * 0.5);

            item.Extent = extent;

            return item;
        }

        private void Integrate(List<PlantItem> plantItems)
        {
            // 축 방향 길이를 이용해 거리 측정
            while (true)
            {
                List<Text> texts = PlantModel.Texts.ToList();
                int numberOfIsolatedTexts = texts.Count;

                foreach (var text in texts)
                {
                    // 최대 거리 내에 있는 대상들을 찾는다. 뒤에 나오는 비교를 수행할 때 속도를 향상 시킨다.
                    var candidateItems = FindItemsWithinExtent(text.Extent, plantItems, MaximumDistance);

                    // 텍스트가 심볼에 완전히 포함되는 경우를 처리한다.
                    if (ProcessCaseTextIsCompletelyContainedInSymbol(text, candidateItems))
                        continue;            

                    var overlappedItems = FindItemsWithinExtent(text.Extent, plantItems, 0);

                    // 텍스트가 심볼 또는 라인과 겹치는 경우를 처리한다.
                    if (ProcessCaseTextIsOverlappedWithEitherSymbolOrLine(text, overlappedItems))
                        continue;            
                    
                    List<TextIntegrationData> candidateSymbols = new List<TextIntegrationData>();
                    IHasExtent minItem = null;

                    // 우선 각 텍스트에 대해 축 거리를 기준으로 심볼 또는 배관과의 거리를 계산한다.
                    foreach (var item in candidateItems)
                    {
                        double minDistance = ComputeDistanceWithAxisLine(text.Extent, item, MaximumDistance);
                        minItem = item;

                        foreach(var childSymbol in item.Children.OfType<IHasExtent>())
                        {
                            double distance = ComputeDistanceWithAxisLine(text.Extent, childSymbol, MaximumDistance);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                minItem = childSymbol;
                            }
                        }

                        if (minDistance > MaximumDistance)
                            continue;

                        TextIntegrationData data = new TextIntegrationData()
                        {
                            Item = item,
                            DistanceItem = minItem,
                            Distance = minDistance
                        };

                        candidateSymbols.Add(data);
                    }

                    // 가까운 대상이 없다면 다음 텍스트로 넘어간다.
                    if (candidateSymbols.Count == 0)
                        continue;

                    // 텍스트가 한 개의 라인과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextIntersectsWithALine(text, candidateSymbols))
                        continue;

                    // 텍스트가 한 개의 심볼과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextIntersectsWithASymbol(text, candidateSymbols))
                        continue;            

                    // 텍스트가 여러 개의 라인 또는 심볼과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextIntersectsWithLinesSymbols(text, candidateSymbols))
                        continue;
                }

                // 남아있는 텍스트 갯수에 변화가 없으면 그만 진행한다. 아니면, 다시 반복한다.
                if (PlantModel.Texts.Count == numberOfIsolatedTexts)
                    break;
            }
            
            // OBB 확대를 이용해 거리 측정
            /*
            while (true)
            {
                List<Text> texts = PlantModel.Texts.ToList();
                int numberOfIsolatedTexts = texts.Count;

                foreach (var text in texts)
                {
                    // 최대 거리 내에 있는 대상들을 찾는다. 뒤에 나오는 비교를 수행할 때 속도를 향상 시킨다.
                    var candidateItems = FindItemsWithinExtent(text.Extent, plantItems, MaximumDistance);

                    List<KeyValuePair<PlantItem, double>> candidateSymbols = new List<KeyValuePair<PlantItem, double>>();

                    // 우선 각 텍스트에 대해 축 거리를 기준으로 심볼 또는 배관과의 거리를 계산한다.
                    foreach (var item in candidateItems)
                    {
                        double minDistance = ComputeDistanceWithExapandingBox(text.Extent, item.Extent, MaximumDistance);

                        foreach (var childSymbol in item.Children.OfType<IHasExtent>())
                        {
                            double distance = ComputeDistanceWithExapandingBox(text.Extent, childSymbol.Extent, MaximumDistance);
                            if (distance < minDistance)
                                minDistance = distance;
                        }

                        if (minDistance > MaximumDistance)
                            continue;

                        candidateSymbols.Add(new KeyValuePair<PlantItem, double>(item, minDistance));
                    }

                    // 가까운 대상이 없다면 다음 텍스트로 넘어간다.
                    if (candidateSymbols.Count == 0)
                        continue;

                    // 텍스트가 한 개의 라인과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextIntersectsWithALine(text, candidateSymbols))
                        continue;

                    // 텍스트가 한 개의 심볼과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextIntersectsWithASymbol(text, candidateSymbols))
                        continue;

                    // 텍스트가 여러 개의 라인 또는 심볼과 교차하는 경우를 처리한다.
                    if (ProcessCaseTextExpandedBoxIntersectsWithLinesSymbols(text, candidateSymbols))
                        continue;
                }

                // 남아있는 텍스트 갯수에 변화가 없으면 그만 진행한다. 아니면, 다시 반복한다.
                if (PlantModel.Texts.Count == numberOfIsolatedTexts)
                    break;
            }
            */

            foreach (var fakeLineEndItem in plantItems.OfType<FakeLineEndItem>().ToList())
            {
                foreach(var childItem in fakeLineEndItem.Children.ToList())
                {
                    ChangeParent(childItem, fakeLineEndItem, fakeLineEndItem.LineItem);
                }
            }
        }        

        private List<PlantItem> FindItemsWithinExtent(Obb2 extent, List<PlantItem> items, double maximumDistance)
        {
            List<PlantItem> candidateItems = new List<PlantItem>();

            extent.Expand(maximumDistance);

            foreach(var item in items)
            {
                Obb2 entityExtent = item.Extent;
                if (CollisionDetector.Collide(extent, entityExtent))
                    candidateItems.Add(item);
                else
                {
                    foreach(var childItem in item.Children.OfType<IHasExtent>())
                    {
                        if (CollisionDetector.Collide(extent, childItem.Extent))
                        {
                            candidateItems.Add(item);
                            break;
                        }
                    }
                }
            }

            return candidateItems;
        }

        private bool ProcessCaseTextIsCompletelyContainedInSymbol(Text text, List<PlantItem> plantItems)
        {
            foreach(var plantItem in plantItems)
            {
                // 만약 텍스트가 심볼에 완전히 포함되면 통합한다.
                if (Obb2.IsContainedIn(text.Extent, plantItem.Extent))
                {
                    ChangeParent(text, PlantModel, plantItem);
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIsCompletelyContainedInSymbol);
                    return true;
                }
            }

            return false;
        }

        private bool ProcessCaseTextIsOverlappedWithEitherSymbolOrLine(Text text, List<PlantItem> overlappedItems)
        {
            if (overlappedItems.Count != 1)
                return false;

            PlantItem overlappedItem = overlappedItems.First();

            if(overlappedItem is LineItem lineItem)
            {
                if(IsTextWithinLineWidth(text, lineItem))
                {
                    ChangeParent(text, PlantModel, lineItem);
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIsOverlappedWithEitherSymbolOrLine);
                    return true;
                }
            }
            else if(overlappedItem is PlantItem plantItem)
            {                
                if(IsTextWithinSymbolWidth(text, plantItem))
                {
                    ChangeParent(text, PlantModel, plantItem);
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIsOverlappedWithEitherSymbolOrLine);
                    return true;
                }
            }

            return false;
        }        

        private static bool IsTextWithinLineWidth(Text text, LineItem lineItem)
        {
            if (!Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, lineItem.Extent.CoordinateSystem.XDirection))
                return false;

            Position2 textMin = text.Extent.GlobalMin;
            Position2 textMax = text.Extent.GlobalMax;
            Position2 lineStart = lineItem.CenterLine.Start;
            Position2 lineEnd = lineItem.CenterLine.End;
            Line2 line = new Line2(lineStart, lineEnd);
            double t0 = line.ParameterOfPoint(textMin);
            double t1 = line.ParameterOfPoint(textMax);

            if (t0 < 0.0 || t1 < 0.0 || t0 > 1.0 || t1 > 1.0)
                return false;
            else
                return true;
        }

        private bool IsTextWithinSymbolWidth(Text text, PlantItem plantItem)
        {
            if(Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, plantItem.Extent.CoordinateSystem.XDirection))
            {
                Position2 textMin = text.Extent.GlobalMin;
                Position2 textMax = text.Extent.GlobalMax;

                double y = plantItem.Extent.Center.Y;
                Position2 lineStart = new Position2(plantItem.Extent.GlobalMin.X, y);
                Position2 lineEnd = new Position2(plantItem.Extent.GlobalMax.X, y);
                Line2 line = new Line2(lineStart, lineEnd);
                double t0 = line.ParameterOfPoint(textMin);
                double t1 = line.ParameterOfPoint(textMax);

                if (t0 < 0.0 || t1 < 0.0 || t0 > 1.0 || t1 > 1.0)
                    return false;
                else
                    return true;
            }
            else if(Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, plantItem.Extent.CoordinateSystem.YDirection))
            {
                Position2 textMin = text.Extent.GlobalMin;
                Position2 textMax = text.Extent.GlobalMax;

                double x = plantItem.Extent.Center.X;
                Position2 lineStart = new Position2(x, plantItem.Extent.GlobalMin.Y);
                Position2 lineEnd = new Position2(x, plantItem.Extent.GlobalMax.Y);
                Line2 line = new Line2(lineStart, lineEnd);
                double t0 = line.ParameterOfPoint(textMin);
                double t1 = line.ParameterOfPoint(textMax);

                if (t0 < 0.0 || t1 < 0.0 || t0 > 1.0 || t1 > 1.0)
                    return false;
                else
                    return true;
            }

            return false;
        }

        private bool AddTextToLineIfPossible(Text text, LineItem lineItem)
        {
            // 라인과 교차하는 경우에는 방향을 확인해야 한다.
            if (Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, lineItem.Extent.CoordinateSystem.XDirection))
            {
                ChangeParent(text, PlantModel, lineItem);
                return true;
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithALine(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            // 교차하는 심볼이 한 개 인지 확인
            if (candidateSymbols.Count() == 1)
            {
                // 심볼이 라인인지 확인
                if (candidateSymbols.First().DistanceItem is LineItem)
                {
                    if (AddTextToPlantItem(text, candidateSymbols.First().Item, candidateSymbols.First().Distance))
                    {
                        text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithALine);
                        return true;
                    }                        
                }
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithASymbol(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            // 교차하는 심볼이 한 개 인지 확인
            if (candidateSymbols.Count() == 1)
            {
                // 심볼이 라인이면 무시
                if (candidateSymbols.First().DistanceItem is LineItem)
                    return false;

                // 심볼이 라인이 아니면 통합한다.
                if (AddTextToPlantItem(text, candidateSymbols.First().Item, candidateSymbols.First().Distance))
                {
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithASymbol);
                    return true;
                }
                    
            }

            return false;
        }

        private bool AddTextToPlantItem(Text text, PlantItem plantItem, double expand)
        {
            // 심볼이 라인인지 확인
            if (plantItem is LineItem)
            {
                // 라인과 교차하는 경우에는 방향을 확인해야 한다.
                if (Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, plantItem.Extent.CoordinateSystem.XDirection))
                {
                    // 디버깅을 위해 확대된 OBB를 저장한다.
                    var expandedExtent = text.Extent;
                    expandedExtent.Expand(expand);
                    text.DebugInfo.ExpandedExtent = expandedExtent;

                    ChangeParent(text, PlantModel, plantItem);
                    return true;
                }
            }
            else
            {
                if (Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, plantItem.Extent.CoordinateSystem.XDirection) ||
                        Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, plantItem.Extent.CoordinateSystem.YDirection))
                {
                    // 디버깅을 위해 확대된 OBB를 저장한다.
                    var expandedExtent = text.Extent;
                    expandedExtent.Expand(expand);
                    text.DebugInfo.ExpandedExtent = expandedExtent;

                    ChangeParent(text, PlantModel, plantItem);
                    return true;
                }
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithLinesSymbols(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            // 한 개만 교차하는 경우 끝낸다.
            if (candidateSymbols.Count() <= 1)
                return false;

            // 여러 개가 교차하는 경우 거리 순으로 정렬한다.
            var orderedSymbols = candidateSymbols.OrderBy(x => x.Distance).ToList();
            double minDistance = orderedSymbols.First().Distance;

            // 오차 이내에서 가장 가까운 (거리가 동일한) 심볼들을 골라낸다. 즉, 가장 가까운 심볼보다 DistanceTolerance 이내에 있는 것들을 골라낸다.
            var strongCandidateSymbols = candidateSymbols.Where(x => Tolerance.IsLessValue(x.Distance - minDistance - DistanceTolerance, 0.0));

            Debug.Assert(strongCandidateSymbols.Count() > 0);

            if(strongCandidateSymbols.Count() == 1)
            {
                if (AddTextToPlantItem(text, strongCandidateSymbols.First().Item, strongCandidateSymbols.First().Distance))
                {
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbols);
                    return true;
                }
            }
            
            // 후보가 여러 개일 경우 동일한 방향을 갖는 것만 골라낸다.
            var candidateSymbolsWithSameDirection = strongCandidateSymbols
                .Where(x => Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, x.DistanceItem.Extent.CoordinateSystem.XDirection));
            if(candidateSymbolsWithSameDirection.Count() == 0)
            {
                return false;
            }
            else if(candidateSymbolsWithSameDirection.Count() == 1)
            {
                // 같은 방향을 갖는 후보가 1개라면 그것을 통합한다.
                ChangeParent(text, PlantModel, candidateSymbolsWithSameDirection.First().Item);
                text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbols);
                return true;
            }
            else
            {
                // 같은 방향을 갖는 후보가 두 개 이상인 경우를 처리한다.
                if (ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis(text, candidateSymbolsWithSameDirection))
                    return true;
            }

            return false;
        }        

        /*
        private bool ProcessCaseTextIntersectsWithTwoLinesSymbolsAtSameDistance(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            Debug.Assert(candidateSymbols.Count() == 2);

            if (candidateSymbols.ElementAt(0).DistanceItem is LineItem && candidateSymbols.ElementAt(1).DistanceItem is LineItem)
            {
                if (ProcessCaseTextIntersectsWithTwoLinesAtSameDistance(text, candidateSymbols))
                    return true;
            }
            else if (candidateSymbols.ElementAt(0).DistanceItem is LineItem && !(candidateSymbols.ElementAt(1).DistanceItem is LineItem))
            {
                if (ProcessCaseTextIntersectsWithLineSymbolAtSameDistace(text, candidateSymbols.ElementAt(0), candidateSymbols.ElementAt(1)))
                    return true;
            }
            else if (!(candidateSymbols.ElementAt(0).DistanceItem is LineItem) && candidateSymbols.ElementAt(1).DistanceItem is LineItem)
            {
                if (ProcessCaseTextIntersectsWithLineSymbolAtSameDistace(text, candidateSymbols.ElementAt(1), candidateSymbols.ElementAt(0)))
                    return true;
            }
            else
            {
                if (ProcessCaseTextIntersectsWithMultipleLinesSymbolsAtSameDistance(text, candidateSymbols))
                    return true;
            }

            return false;
        }
        */

        private bool ProcessCaseTextIntersectsWithTwoLinesAtSameDistance(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            Debug.Assert(candidateSymbols.Count() == 2);

            // 텍스트가 두 라인과 동일 거리에 있다면, 텍스트의 수직 아래 방향으로 동일 거리에 있는 심볼을 선택한다.

            double yDistance1 = ComputeSignedDistanceWithYAxisLine(text.Extent, candidateSymbols.ElementAt(0).DistanceItem.Extent, MaximumDistance);
            double yDistance2 = ComputeSignedDistanceWithYAxisLine(text.Extent, candidateSymbols.ElementAt(1).DistanceItem.Extent, MaximumDistance);

            if (Math.Abs(yDistance1) <= MaximumDistance && Math.Abs(yDistance2) > MaximumDistance)
            {
                return AddTextToPlantItem(text, candidateSymbols.ElementAt(0).Item, yDistance1);
            }
            else if (Math.Abs(yDistance1) > MaximumDistance && Math.Abs(yDistance2) <= MaximumDistance)
            {
                return AddTextToPlantItem(text, candidateSymbols.ElementAt(1).Item, yDistance2);
            }
            else if (Math.Abs(yDistance1) <= MaximumDistance && Math.Abs(yDistance2) <= MaximumDistance && Math.Abs(yDistance1) == Math.Abs(yDistance2))
            {
                if (yDistance1 < yDistance2)
                    return AddTextToPlantItem(text, candidateSymbols.ElementAt(0).Item, yDistance1);
                else
                    return AddTextToPlantItem(text, candidateSymbols.ElementAt(1).Item, yDistance1);
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithTwoSymbolsAtSameDistance(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            Debug.Assert(candidateSymbols.Count() == 2);

            // 텍스트가 두 심볼과 동일 거리에 있다면, 텍스트의 수직 위 방향으로 동일 거리에 있는 심볼을 선택한다.

            double yDistance1 = ComputeSignedDistanceWithYAxisLine(text.Extent, candidateSymbols.ElementAt(0).DistanceItem.Extent, MaximumDistance);
            double yDistance2 = ComputeSignedDistanceWithYAxisLine(text.Extent, candidateSymbols.ElementAt(1).DistanceItem.Extent, MaximumDistance);

            if(Math.Abs(yDistance1) <= MaximumDistance && Math.Abs(yDistance2) > MaximumDistance)
            {
                return AddTextToPlantItem(text, candidateSymbols.ElementAt(0).Item, yDistance1);
            }
            else if (Math.Abs(yDistance1) > MaximumDistance && Math.Abs(yDistance2) <= MaximumDistance)
            {
                return AddTextToPlantItem(text, candidateSymbols.ElementAt(1).Item, yDistance2);
            }
            else if(Math.Abs(yDistance1) <= MaximumDistance && Math.Abs(yDistance2) <= MaximumDistance && Math.Abs(yDistance1) == Math.Abs(yDistance2))
            {
                if(yDistance1 > yDistance2)
                    return AddTextToPlantItem(text, candidateSymbols.ElementAt(0).Item, yDistance1);
                else
                    return AddTextToPlantItem(text, candidateSymbols.ElementAt(1).Item, yDistance1);
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis(Text text, IEnumerable<TextIntegrationData> candidateSymbolsWithSameDirection)
        {
            // 먼저 라인을 대상으로 한다. 라인의 경우 텍스트 아랫쪽을 우선 선택한다.

            var candidateLines = candidateSymbolsWithSameDirection
                .Select(x => new { Item = x.Item, DistanceItem = x.DistanceItem })
                .Where(x => x.DistanceItem is LineItem)
                .Where(x => Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, x.DistanceItem.Extent.CoordinateSystem.XDirection));

            if (candidateLines.Count() != 0)
            {
                var orderedCandidateLines = candidateLines
                    .Select(x => new { Item = x.Item, DistanceItem = x.DistanceItem, YDistance = ComputeSignedDistanceWithYAxisLine(text.Extent, x.DistanceItem.Extent, MaximumDistance) })
                    .OrderBy(x => x.YDistance);

                if (orderedCandidateLines.First().YDistance < MaximumDistance)
                {
                    if(AddTextToPlantItem(text, orderedCandidateLines.First().Item, orderedCandidateLines.First().YDistance))
                    {
                        text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis);
                        return true;
                    }
                    
                }
            }

            // 심볼을 대상으로 한다. 심볼의 경우 텍스트 윗쪽을 우선 선택한다.

            var candidateSymbols = candidateSymbolsWithSameDirection
                .Select(x => new { Item = x.Item, DistanceItem = x.DistanceItem })
                .Where(x => !(x.DistanceItem is LineItem));

            if (candidateSymbols.Count() != 0)
            {
                var orderedCandidateSymbols = candidateSymbols
                    .Select(x => new
                    {
                        Item = x.Item,
                        DistanceItem = x.DistanceItem,
                        YDistance = ComputeSignedDistanceWithYAxisLine(text.Extent, x.DistanceItem.Extent, MaximumDistance),
                        XDistance = ComputeSignedDistanceWithXAxisLine(text.Extent, x.DistanceItem.Extent, MaximumDistance)
                    })
                    .OrderBy(x => x.YDistance)
                    .ThenBy(x => x.XDistance);

                if (orderedCandidateSymbols.First().YDistance < MaximumDistance)
                {
                    if (AddTextToPlantItem(text, orderedCandidateSymbols.First().Item, orderedCandidateSymbols.First().YDistance))
                    {
                        text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis);
                        return true;
                    }
                }
                else if(orderedCandidateSymbols.First().YDistance >= MaximumDistance && orderedCandidateSymbols.First().XDistance < MaximumDistance)
                {
                    if (AddTextToPlantItem(text, orderedCandidateSymbols.First().Item, orderedCandidateSymbols.First().XDistance))
                    {
                        text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ProcessCaseTextIntersectsWithLineSymbolAtSameDistace(Text text, TextIntegrationData lineItemData, TextIntegrationData symbolItemData)
        {
            Debug.Assert(lineItemData.DistanceItem is LineItem);

            LineItem lineItem = (LineItem)lineItemData.DistanceItem;

            // 라인과 텍스트 방향이 동일하면 통합한다.
            if (Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, lineItem.Extent.CoordinateSystem.XDirection))
            {
                // 디버깅을 위해 확대된 OBB를 저장한다.
                var expandedExtent = text.Extent;
                expandedExtent.Expand(lineItemData.Distance);
                text.DebugInfo.ExpandedExtent = expandedExtent;

                ChangeParent(text, PlantModel, lineItemData.Item);

                text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLineSymbolAtSameDistace);
                return true;

            }
            else
            {
                // 심볼과 텍스트 방향이 동일하면 통합한다.

                // 디버깅을 위해 확대된 OBB를 저장한다.
                var expandedExtent = text.Extent;
                expandedExtent.Expand(symbolItemData.Distance);
                text.DebugInfo.ExpandedExtent = expandedExtent;

                ChangeParent(text, PlantModel, symbolItemData.Item);
                
                text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLineSymbolAtSameDistace);
                return true;
            }
        }

        /*
        private bool ProcessCaseTextIntersectsWithMultipleLinesSymbolsAtSameDistance(Text text, IEnumerable<TextIntegrationData> candidateSymbols)
        {
            if (candidateSymbols.Count() < 2)
                return false;

            var orderedCandidates = candidateSymbols.OrderByDescending(x => ComputeIoU(Obb2.Expand(text.Extent, x.Distance), x.DistanceItem.Extent));

            double iou1 = ComputeIoU(text.Extent, orderedCandidates.ElementAt(0).DistanceItem.Extent);
            double iou2 = ComputeIoU(text.Extent, orderedCandidates.ElementAt(1).DistanceItem.Extent);

            ChangeParent(text, PlantModel, orderedCandidates.First().Item);

            Obb2 expandedExtent = text.Extent;
            expandedExtent.Expand(orderedCandidates.First().Distance);
            text.DebugInfo.ExpandedExtent = expandedExtent;
            text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithMultipleLinesSymbolsAtSameDistance);

            return true;
        }
        */

        /*
        private bool ProcessCaseTextExpandedBoxIntersectsWithLinesSymbols(Text text, List<TextIntegrationData> candidateSymbols)
        {
            // 한 개만 교차하는 경우 끝낸다.
            if (candidateSymbols.Count() <= 1)
                return false;

            // 여러 개가 교차하는 경우 거리 순으로 정렬한다.
            var orderedSymbols = candidateSymbols.OrderBy(x => x.Distance).ToList();
            double minDistance = orderedSymbols.First().Distance;

            // 오차 이내에서 가장 가까운 (거리가 동일한) 심볼들을 골라낸다. 즉, 가장 가까운 심볼보다 DistanceTolerance 이내에 있는 것들을 골라낸다.
            var strongCandidateSymbols = candidateSymbols.Where(x => Tolerance.IsLessValue(x.Distance - minDistance - DistanceTolerance, 0.0));

            Debug.Assert(strongCandidateSymbols.Count() > 0);

            if (strongCandidateSymbols.Count() == 1)
            {
                if (AddTextToPlantItem(text, strongCandidateSymbols.First().Item, strongCandidateSymbols.First().Distance))
                {
                    text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbols);
                    return true;
                }
            }

            // 후보가 여러 개일 경우 동일한 방향을 갖는 것만 골라낸다.
            var candidateSymbolsWithSameDirection = strongCandidateSymbols.Where(x => Vector2.IsParallel(text.Extent.CoordinateSystem.XDirection, x.DistanceItem.Extent.CoordinateSystem.XDirection));
            if (candidateSymbolsWithSameDirection.Count() == 0)
            {
                // 같은 방향을 갖는 후보가 없다면 모두를 대상으로 한다.
                if (ProcessCaseTextIntersectsWithMultipleLinesSymbolsAtSameDistance(text, strongCandidateSymbols))
                    return true;
            }
            else if (candidateSymbolsWithSameDirection.Count() == 1)
            {
                // 같은 방향을 갖는 후보가 1개라면 그것을 통합한다.
                ChangeParent(text, PlantModel, candidateSymbolsWithSameDirection.First().Item);
                text.DebugInfo.TextIntegrationCategory = nameof(ProcessCaseTextIntersectsWithLinesSymbols);
                return true;
            }
            else
            {
                // 같은 방향을 갖는 후보가 두 개 이상인 경우를 처리한다.
                if (ProcessCaseTextIntersectsWithLinesSymbolsAtSameDistanceAlongAxis(text, candidateSymbolsWithSameDirection))
                    return true;
            }

            return false;
        }
        */

        private void ChangeParent(PlantEntity entity, PlantEntity oldParent, PlantEntity newParent)
        {
            newParent.Children.Add(entity);
            oldParent.Children.Remove(entity);
            entity.Parent = newParent;
        }

        private static double ComputeDistanceWithAxisLine(Obb2 obb1, IHasExtent hasExtent, double maximumDistance)
        {
            if (hasExtent is LineItem lineItem)
            {
                if (Vector2.IsParallel(obb1.CoordinateSystem.XDirection, hasExtent.Extent.CoordinateSystem.XDirection))
                    return ComputeDistanceWithAxisLine(obb1, hasExtent.Extent, maximumDistance);
                else
                    return double.MaxValue;
            }
            else
                return ComputeDistanceWithAxisLine(obb1, hasExtent.Extent, maximumDistance);
        }

        private static double ComputeDistanceWithAxisLine(Obb2 obb1, Obb2 obb2, double maximumDistance)
        {
            // X-축 방향 거리 확인
            Obb2 extentAlongX = obb1;
            extentAlongX.LocalMin = new Position2(obb1.LocalMin.X, 0);
            extentAlongX.LocalMax = new Position2(obb1.LocalMax.X, 0);

            // Y-축 방향 거리 확인
            Obb2 extentAlongY = obb1;
            extentAlongY.LocalMin = new Position2(0, obb1.LocalMin.Y);
            extentAlongY.LocalMax = new Position2(0, obb1.LocalMax.Y);

            for (int i = 0; i < maximumDistance; ++i)
            {
                if (CollisionDetector.Collide(extentAlongX, obb2))
                {
                    return (double)i;
                }

                if (CollisionDetector.Collide(extentAlongY, obb2))
                {
                    return (double)i;
                }

                extentAlongX.Expand(1.0, 0.0, 1.0, 0.0);
                extentAlongY.Expand(0.0, 1.0, 0.0, 1.0);
            }

            return double.MaxValue;
        }

        private static double ComputeSignedDistanceWithXAxisLine(Obb2 obb1, Obb2 obb2, double maximumDistance)
        {
            // X-축 방향 거리 확인
            Obb2 extentAlongPositiveX = obb1;
            extentAlongPositiveX.LocalMin = new Position2(obb1.LocalMin.X, 0);
            extentAlongPositiveX.LocalMax = new Position2(obb1.LocalMax.X, 0);

            Obb2 extentAlongNegativeX = obb1;
            extentAlongNegativeX.LocalMin = new Position2(obb1.LocalMin.X, 0);
            extentAlongNegativeX.LocalMax = new Position2(obb1.LocalMax.X, 0);

            for (int i = 0; i < maximumDistance; ++i)
            {
                if (CollisionDetector.Collide(extentAlongPositiveX, obb2))
                {
                    return (double)i;
                }

                if (CollisionDetector.Collide(extentAlongNegativeX, obb2))
                {
                    return -(double)i;
                }

                extentAlongPositiveX.Expand(1.0, 0.0, 1.0, 0.0);
                extentAlongNegativeX.Expand(1.0, 0.0, 1.0, 0.0);
            }

            return double.MaxValue;
        }

        private static double ComputeSignedDistanceWithYAxisLine(Obb2 obb1, Obb2 obb2, double maximumDistance)
        {
            // Y-축 방향 거리 확인
            Obb2 extentAlongPositiveY = obb1;
            extentAlongPositiveY.LocalMin = new Position2(0, obb1.LocalMin.Y);
            extentAlongPositiveY.LocalMax = new Position2(0, obb1.LocalMax.Y);

            Obb2 extentAlongNegativeY = obb1;
            extentAlongNegativeY.LocalMin = new Position2(0, obb1.LocalMin.Y);
            extentAlongNegativeY.LocalMax = new Position2(0, obb1.LocalMax.Y);

            for (int i = 0; i < maximumDistance; ++i)
            {
                if (CollisionDetector.Collide(extentAlongPositiveY, obb2))
                {
                    return (double)i;
                }

                if (CollisionDetector.Collide(extentAlongNegativeY, obb2))
                {
                    return -(double)i;
                }

                extentAlongPositiveY.Expand(0.0, 1.0, 0.0, 1.0);
                extentAlongNegativeY.Expand(0.0, 1.0, 0.0, 1.0);
            }

            return double.MaxValue;
        }

        /*
        private static double ComputeDistanceWithExapandingBox(Obb2 obb1, Obb2 obb2, double maximumDistance)
        {
            Obb2 extent = obb1;

            for (int i = 0; i < maximumDistance; ++i)
            {                
                if (CollisionDetector.Collide(extent, obb2))
                {
                    return (double)i;
                }

                extent.Expand(1.0);
            }

            return double.MaxValue;
        }
        */

        /*
        private static double ComputeIoU(Obb2 obb1, Obb2 obb2)
        {
            // 현재는 회전한 OBB는 처리하지 못한다.
            Debug.Assert(Vector2.IsParallel(obb1.CoordinateSystem.XDirection, Vector2.OX) || Vector2.IsParallel(obb1.CoordinateSystem.YDirection, Vector2.OX));
            Debug.Assert(Vector2.IsParallel(obb2.CoordinateSystem.XDirection, Vector2.OX) || Vector2.IsParallel(obb2.CoordinateSystem.YDirection, Vector2.OX));

            double minX = Math.Max(obb1.GlobalMin.X, obb2.GlobalMin.X);
            double maxX = Math.Min(obb1.GlobalMax.X, obb2.GlobalMax.X);
            double minY = Math.Max(obb1.GlobalMin.Y, obb2.GlobalMin.Y);
            double maxY = Math.Min(obb1.GlobalMax.Y, obb2.GlobalMax.Y);

            double intersectionArea = (maxX - minX) * (maxY - minY);
            double area1 = obb1.Area();
            double area2 = obb2.Area();

            double iou = intersectionArea / (area1 + area2 - intersectionArea);
            return iou;
        }
        */
    }
}
