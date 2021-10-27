using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator
{
    public class AnnotationToPlantModelConverter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const double DefaultTextAspectRatioThreshold = 2.0;

        private readonly List<string> UnspecifiedPipingComponentNames = new List<string>()
        {
            "indirect_drain"
        };

        private readonly List<string> UnspecifiedEquipmentNames = new List<string>()
        {
            "typical_detail"
        };

        private readonly List<string> UnspecifiedInstrumentNames = new List<string>()
        {
            "packaged_system_by_plc_of_single_controller", "diaphragm_with_positioner"
        };

        public AnnotationToPlantModelConverter()
        {
            TextAspectRatioThreshold = DefaultTextAspectRatioThreshold;
        }

        public double TextAspectRatioThreshold { get; set; }

        public PlantModel Convert(Annotation annotation)
        {
            PlantModel plantModel = new PlantModel();

            FillExtent(annotation, plantModel);

            ConvertSymbolObjectList(annotation.SymbolObjectList, plantModel);
            ConvertLineObjectList(annotation.LineObjectList, plantModel);

            ConvertConnectionObjectList(annotation.ConnectionObjectList, plantModel);

            return plantModel;
        }
                
        //Connection Information
        private void ConvertConnectionObjectList(List<ConnectionObject> connectionObjectList, PlantModel plantModel)
        {
            foreach (var connectionObject in connectionObjectList)
            {
                ConvertConnectionObject(connectionObject, plantModel);
            }
        }

        private void ConvertConnectionObject(ConnectionObject connectionObject, PlantModel plantModel)
        {
            Connection connection = new Connection();
            ConvertConnectionItem(connectionObject, connection);
                        
            connection.From = connectionObject.From;
            connection.To = connectionObject.To;

            plantModel.Add(connection);
        }

        private void ConvertConnectionItem(ConnectionObject connectionObject, PlantItem plantItem)
        {
            plantItem.OriginalID = connectionObject.Id;
        }

        private void FillExtent(Annotation annotation, PlantModel plantModel)
        {
            Obb2 extent = new Obb2()
            {
                CoordinateSystem = Axis22.OXY,
                LocalMin = new Position2(0.0, 0.0),
                LocalMax = new Position2(annotation.BasicDrawingInformation.Size.Width, annotation.BasicDrawingInformation.Size.Height)
            };

            plantModel.Extent = extent;
        }

        private void ConvertSymbolObjectList(IEnumerable<SymbolObject> symbolObjectList, PlantModel plantModel)
        {
            foreach(var symbolObject in symbolObjectList)
            {
                ConvertSymbolObject(symbolObject, plantModel);
            }
        }

        private void ConvertSymbolObject(SymbolObject symbolObject, PlantModel plantModel)
        {
            if(symbolObject.Type == SymbolObjectType.EquipmentSymbol)
            {
                ConvertEquipmentSymbol(symbolObject, plantModel);
            }
            else if(symbolObject.Type == SymbolObjectType.PipeSymbol)
            {
                ConvertPipeSymbol(symbolObject, plantModel);
            }
            else if (symbolObject.Type == SymbolObjectType.InstrumentSymbol)
            {
                ConvertInstrumentSymbol(symbolObject, plantModel);
            }
            else if (symbolObject.Type == SymbolObjectType.Text)
            {
                ConvertTexts(symbolObject, plantModel);
            }
            else if (symbolObject.Type == SymbolObjectType.UnspecifiedSymbol)
            {
                string message = "unspecified_symbol이 있습니다.";

                if (log.IsInfoEnabled)
                    log.Info(message);

                ConvertUnspecifiedSymbol(symbolObject, plantModel);
            }
            else
            {
                string message = "알 수 없는 심볼 타입입니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }
        }        

        private void ConvertPlantItem(SymbolObject symbolObject, PlantItem plantItem, PlantModel plantModel)
        {
            plantItem.OriginalID = symbolObject.Id;

            if(!string.IsNullOrEmpty(symbolObject.ClassName))
                plantItem.ComponentClass = symbolObject.ClassName;            

            Obb2 obb = ConvertBoundingBox(symbolObject.BoundingBox, symbolObject.Rotation, plantModel.Height);
            plantItem.Extent = obb;
            // 디버깅 목적
            plantItem.ExpandedExtent = obb;

            plantItem.SymbolFlip = symbolObject.Flip ? Flip.Horizontal : Flip.None;
        }

        private void ConvertEquipmentSymbol(SymbolObject symbolObject, PlantModel plantModel)
        {
            if (!symbolObject.BoundingBox.HasExtent())
            {
                string message = "equipment_symbol은 크기를 가져야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }                

            Equipment equipment = new Equipment();
            ConvertPlantItem(symbolObject, equipment, plantModel);
            
            plantModel.Add(equipment);
        }

        private void ConvertPipeSymbol(SymbolObject symbolObject, PlantModel plantModel)
        {
            if (!symbolObject.BoundingBox.HasExtent())
            {
                string message = "pipe_symbol은 크기를 가져야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }

            if (symbolObject.ClassName == "off_page_connector")
            {
                PipeConnectorSymbol pipeConnectorSymbol = new PipeConnectorSymbol();
                ConvertPlantItem(symbolObject, pipeConnectorSymbol, plantModel);

                plantModel.Add(pipeConnectorSymbol);
            }
            else
            {
                PipingComponent pipingComponent = new PipingComponent();
                ConvertPlantItem(symbolObject, pipingComponent, plantModel);

                plantModel.Add(pipingComponent);
            }
        }

        private void ConvertInstrumentSymbol(SymbolObject symbolObject, PlantModel plantModel)
        {
            if (!symbolObject.BoundingBox.HasExtent())
            {
                string message = "instrument_symbol은 크기를 가져야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }                

            Instrument instrument = new Instrument();
            ConvertPlantItem(symbolObject, instrument, plantModel);            

            plantModel.Add(instrument);
        }

        //Text
        private void ConvertTexts(SymbolObject symbolObject, PlantModel plantModel)
        {
            Text text = new Text();
            text.OriginalID = symbolObject.Id;
            text.String = symbolObject.ClassName;

            if (!symbolObject.BoundingBox.HasExtent())
            {
                string message = "Text_symbol은 크기를 가져야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }

            Obb2 obb = ConvertTextBoundingBox(symbolObject.BoundingBox, symbolObject.Rotation, plantModel.Height, TextAspectRatioThreshold);

            text.Extent = obb;
            // 디버깅 목적
            text.DebugInfo.ExpandedExtent = obb;

            plantModel.Add(text);
        }

        private void ConvertUnspecifiedSymbol(SymbolObject symbolObject, PlantModel plantModel)
        {
            if (!symbolObject.BoundingBox.HasExtent())
            {
                string message = "unspecified_symbol은 크기를 가져야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }

            if (UnspecifiedPipingComponentNames.Contains(symbolObject.ClassName))
            {
                string message = "unspecified_symbol을 PipingComponent로 변환합니다.";

                if (log.IsInfoEnabled)
                    log.Info(message);

                ConvertPipeSymbol(symbolObject, plantModel);
            }
            else if (UnspecifiedInstrumentNames.Contains(symbolObject.ClassName))
            {
                string message = "unspecified_symbol을 Instrument로 변환합니다.";

                if (log.IsInfoEnabled)
                    log.Info(message);

                ConvertInstrumentSymbol(symbolObject, plantModel);
            }
            else if (UnspecifiedEquipmentNames.Contains(symbolObject.ClassName))
            {
                string message = "unspecified_symbol을 Equipment로 변환합니다.";

                if (log.IsInfoEnabled)
                    log.Info(message);

                ConvertEquipmentSymbol(symbolObject, plantModel);
            }
            else
            {
                UnknownSymbol unknownSymbol = new UnknownSymbol();
                ConvertPlantItem(symbolObject, unknownSymbol, plantModel);

                plantModel.Add(unknownSymbol);
            }
        }

        private static Position2 TransformImageSpaceToDrawingSpace(Position2 pointInImage, double drawingHeight)
        {
            double xInDrawing = pointInImage.X;
            double yInDrawing = drawingHeight - pointInImage.Y;

            return new Position2(xInDrawing, yInDrawing);
        }

        private static Obb2 ConvertBoundingBox(BoundingBox boundingBox, Angle rotation, double drawingHeight)
        {
            Position2 minInImage = new Position2(boundingBox.XMin, boundingBox.YMin);
            Position2 maxInImage = new Position2(boundingBox.XMax, boundingBox.YMax);            

            Position2 minInDrawing = TransformImageSpaceToDrawingSpace(minInImage, drawingHeight);
            Position2 maxInDrawing = TransformImageSpaceToDrawingSpace(maxInImage, drawingHeight);

            double minX = Math.Min(minInDrawing.X, maxInDrawing.X);
            double minY = Math.Min(minInDrawing.Y, maxInDrawing.Y);
            double maxX = Math.Max(minInDrawing.X, maxInDrawing.X);
            double maxY = Math.Max(minInDrawing.Y, maxInDrawing.Y);

            int width = Math.Abs(boundingBox.XMax - boundingBox.XMin) + 1;
            int height = Math.Abs(boundingBox.YMax - boundingBox.YMin) + 1;
            double aspectRatio = (double)height / (double)width;

            Position2 min = new Position2()
            {
                X = minX,
                Y = minY
            };

            Position2 max = new Position2()
            {
                X = maxX,
                Y = maxY
            };

            Position2 centerInDrawing = (min + max) * 0.5;
            Vector2 xAxis = new Vector2(Math.Cos(rotation.Radian), Math.Sin(rotation.Radian));

            Axis22 coordinateSystem = new Axis22(centerInDrawing, xAxis);

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = min - centerInDrawing,
                LocalMax = max - centerInDrawing
            };

            return obb;  
        }

        private static Obb2 ConvertTextBoundingBox(BoundingBox boundingBox, Angle rotation, double drawingHeight, double aspectRatioThreshold)
        {
            Position2 minInImage = new Position2(boundingBox.XMin, boundingBox.YMin);
            Position2 maxInImage = new Position2(boundingBox.XMax, boundingBox.YMax);

            Position2 minInDrawing = TransformImageSpaceToDrawingSpace(minInImage, drawingHeight);
            Position2 maxInDrawing = TransformImageSpaceToDrawingSpace(maxInImage, drawingHeight);

            double minX = Math.Min(minInDrawing.X, maxInDrawing.X);
            double minY = Math.Min(minInDrawing.Y, maxInDrawing.Y);
            double maxX = Math.Max(minInDrawing.X, maxInDrawing.X);
            double maxY = Math.Max(minInDrawing.Y, maxInDrawing.Y);

            int width = Math.Abs(boundingBox.XMax - boundingBox.XMin) + 1;
            if (width < 2)
            {
                width = 30;
            }
            int height = Math.Abs(boundingBox.YMax - boundingBox.YMin) + 1;

            double aspectRatio = (double)height / (double)width;

            Position2 min = new Position2()
            {
                X = minX,
                Y = minY
            };

            Position2 max = new Position2()
            {
                X = maxX,
                Y = maxY
            };

            Position2 centerInDrawing = (min + max) * 0.5;
            Vector2 xAxis = new Vector2(Math.Cos(rotation.Radian), Math.Sin(rotation.Radian));

            Axis22 coordinateSystem = new Axis22(centerInDrawing, xAxis);

            Obb2 obb = new Obb2()
            {
                CoordinateSystem = coordinateSystem,
                LocalMin = min - centerInDrawing,
                LocalMax = max - centerInDrawing
            };
            
            if (rotation.Degree == 0 && aspectRatio > aspectRatioThreshold)
            {
                Vector2 newXAxis = new Vector2(0.0, 1.0);
                Axis22 newCoordinateSystem = new Axis22(centerInDrawing, newXAxis);

                Position2 newMin = new Position2(obb.LocalMin.Y, obb.LocalMin.X);
                Position2 newMax = new Position2(obb.LocalMax.Y, obb.LocalMax.X);

                obb = new Obb2()
                {
                    CoordinateSystem = newCoordinateSystem,
                    LocalMin = newMin,
                    LocalMax = newMax
                };
            }
            
            return obb;
        }

        //LineObject
        private void ConvertLineObjectList(IEnumerable<LineObject> lineObjectList, PlantModel plantModel)
        {
            foreach (var lineObject in lineObjectList)
            {
                ConvertLineObject(lineObject, plantModel);
            }
        }

        private void ConvertLineObject(LineObject lineObject, PlantModel plantModel)
        {
            if (lineObject.Type == LineObjectType.PipeLine)
            {
                ConvertPipeLine(lineObject, plantModel);
            }
            else if (lineObject.Type == LineObjectType.InstrumentLine)
            {
                ConvertInstrumentLine(lineObject, plantModel);
            }
            else if (lineObject.Type == LineObjectType.UnspecifiedLine)
            {
                string message = "unspecified_line이 있습니다.";

                if (log.IsInfoEnabled)
                    log.Info(message);

                ConvertUnspecifiedLine(lineObject, plantModel);
            }
            else
            {
                string message = "알 수 없는 Line 타입입니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentException(message);
            }
        }        

        //PipeLine
        private void ConvertPipeLine(LineObject lineObject, PlantModel plantModel)
        {            
            PipingNetworkSegment pipingNetworkSegment = new PipingNetworkSegment();
            ConvertLineItem(lineObject, pipingNetworkSegment, plantModel);

            Node startNode = new Node();
            startNode.Coordinate = pipingNetworkSegment.CenterLine.Start;

            Node endNode = new Node();
            endNode.Coordinate = pipingNetworkSegment.CenterLine.End;

            // 순서 중요
            plantModel.Add(pipingNetworkSegment);            
        }

        private void ConvertLineItem(LineObject lineObject, LineItem lineItem, PlantModel plantModel)
        {
            lineItem.OriginalID = lineObject.Id;
            lineItem.ComponentClass = lineObject.ClassName;

            lineItem.CenterLine = new CenterLine();
            ConvertEdge(lineObject.Edge, lineItem.CenterLine, plantModel.Height);

            var extent = Obb2.Create(lineItem.CenterLine.Start, lineItem.CenterLine.End);
            lineItem.Extent = extent;
            lineItem.ExpandedExtent = extent;

            lineItem.StartShape = lineObject.StartEndType;
            lineItem.EndShape = lineObject.EndEndType;
        }

        private void ConvertInstrumentLine(LineObject lineObject, PlantModel plantModel)
        {
            SignalLine signalLine = new SignalLine();
            ConvertLineItem(lineObject, signalLine, plantModel);

            plantModel.Add(signalLine);
        }

        private void ConvertUnspecifiedLine(LineObject lineObject, PlantModel plantModel)
        {
            UnknownLine unknownLine = new UnknownLine();
            ConvertLineItem(lineObject, unknownLine, plantModel);

            plantModel.Add(unknownLine);
        }

        //Edge
        private void ConvertEdge(Edge edge, CenterLine centerLine, double drawingHeight)
        {
            Position2 startInImage = new Position2(edge.XStart, edge.YStart);
            Position2 endInImage = new Position2(edge.XEnd, edge.YEnd);

            Position2 startInDrawing = TransformImageSpaceToDrawingSpace(startInImage, drawingHeight);
            Position2 endInDrawing = TransformImageSpaceToDrawingSpace(endInImage, drawingHeight);

            centerLine.Start = startInDrawing;
            centerLine.End = endInDrawing;
        }
    }
}
