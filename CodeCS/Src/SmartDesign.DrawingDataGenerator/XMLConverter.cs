using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SmartDesign.DrawingDataGenerator
{
    class XMLConverter
    {
        public static double DistanceTolerance = 3.0;

        public XMLConverter()
        {            

        }

        public XDocument ConvertToXML(PlantModel plantModel, string path)
        {
            XDocument xmlDocument = new XDocument();

            XElement plantModelElement = new XElement("annotation");
            xmlDocument.Add(plantModelElement);

            XElement drawinginformationElement = new XElement("basic_drawing_information");

            XElement folderPathElement = new XElement("folder");
            drawinginformationElement.Add(folderPathElement);

            XElement fileNameElement = new XElement("filename");
            drawinginformationElement.Add(fileNameElement);

            XElement pathElement = new XElement("path", path);
            drawinginformationElement.Add(pathElement);

            XElement basicSizeElement = new XElement("size");
            XElement widthElement = new XElement("width", 1500);
            basicSizeElement.Add(widthElement);
            XElement heightElement = new XElement("height", 1000);
            basicSizeElement.Add(heightElement);
            XElement depthElement = new XElement("depth", 4);
            basicSizeElement.Add(depthElement);
            drawinginformationElement.Add(basicSizeElement);

            XElement borderLineElement = new XElement("external_border_line");
            XElement borderbndboxElement = new XElement("bndbox");
            var bnbboxBorderElement = BasicbnbBoxInformation(borderbndboxElement);
            borderLineElement.Add(bnbboxBorderElement);
            drawinginformationElement.Add(borderLineElement);

            XElement drawingAreaElement = new XElement("pure_drawing_area");
            XElement drawingbndboxElement = new XElement("bndbox");
            var bnbboxDrawingElement = BasicbnbBoxInformation(drawingbndboxElement);
            drawingAreaElement.Add(bnbboxDrawingElement);
            drawinginformationElement.Add(drawingAreaElement);

            XElement noteElement = new XElement("note_area");
            XElement noteBndboxElement = new XElement("bndbox");
            var bnbboxNoteElement = BasicbnbBoxInformation(noteBndboxElement);
            noteElement.Add(bnbboxNoteElement);
            drawinginformationElement.Add(noteElement);

            XElement titleElement = new XElement("title_area");
            XElement titleBndboxElement = new XElement("bndbox");
            var bnbboxTitleElement = BasicbnbBoxInformation(titleBndboxElement);
            titleElement.Add(bnbboxTitleElement);
            drawinginformationElement.Add(titleElement);

            XElement separatorElement = new XElement("drawing_area_separator");
            XElement separatorBndboxElement = new XElement("edge");
            XElement xminElement = new XElement("xstart", 0);
            separatorBndboxElement.Add(xminElement);
            XElement yminElement = new XElement("ystart", 0);
            separatorBndboxElement.Add(yminElement);
            XElement xmaxElement = new XElement("xend", 0);
            separatorBndboxElement.Add(xmaxElement);
            XElement ymaxElement = new XElement("yend", 0);
            separatorBndboxElement.Add(ymaxElement);
            separatorElement.Add(separatorBndboxElement);
            drawinginformationElement.Add(separatorElement);

            plantModelElement.Add(drawinginformationElement);

            foreach (var equipment in plantModel.Equipments)
            {
                XElement shapeElement = new XElement("symbol_object");

                string id = equipment.ID;
                string type = "equipment_symbol";
                string className = equipment.ClassName;
                string flip = "n";
                double degree = equipment.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, equipment.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var instrument in plantModel.Instruments)
            {
                XElement shapeElement = new XElement("symbol_object");

                string id = instrument.ID;
                string type = "instrument_symbol";
                string className = instrument.ClassName;
                string flip = "n";
                double degree = instrument.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, instrument.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var pipingComponent in plantModel.PipingComponents)
            {
                XElement shapeElement = new XElement("symbol_object");

                string id = pipingComponent.ID;
                string type = "pipe_symbol";
                string className = pipingComponent.ClassName;
                string flip = "n";
                double degree = pipingComponent.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, pipingComponent.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var text in plantModel.Texts)
            {
                XElement shapeElement = new XElement("symbol_object");

                string id = text.ID;
                string type = "text";
                string className = text.Contents;
                string flip = "n";
                int degree = text.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, text.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var pipeLine in plantModel.PipeLines)
            {
                XElement shapeElement = new XElement("line_object");

                string id = pipeLine.ID;
                string type = "unspecified_line";
                string className = "solid";
                int minX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                int minY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                int maxX = pipeLine.LineEndPoints.EndPoints.EndX;
                int maxY = pipeLine.LineEndPoints.EndPoints.EndY;
                string lineStartType = "none";
                string lineEndType = "none";

                CreateXmlLineStructure(shapeElement, id, type, className, minX, minY, maxX, maxY, lineStartType, lineEndType);
                plantModelElement.Add(shapeElement);
            }

            foreach (var signalLine in plantModel.SignalLines)
            {
                XElement shapeElement = new XElement("line_object");

                string id = signalLine.ID;
                string type = "unspecified_line";
                string className = "dashed";
                int minX = signalLine.LineEndPoints.BeginPoints.BeginX;
                int minY = signalLine.LineEndPoints.BeginPoints.BeginY;
                int maxX = signalLine.LineEndPoints.EndPoints.EndX;
                int maxY = signalLine.LineEndPoints.EndPoints.EndY;
                string lineStartType = "none";
                string lineEndType = "none";

                CreateXmlLineStructure(shapeElement, id, type, className, minX, minY, maxX, maxY, lineStartType, lineEndType);
                plantModelElement.Add(shapeElement);
            }

            foreach (var connectionPoint in plantModel.ConnectionPoints)
            {
                XElement shapeElement = new XElement("connection_object");
                CreateXmlConnectionStructure(shapeElement, plantModel, connectionPoint);
                plantModelElement.Add(shapeElement);
            }

            foreach (var connectionLine in plantModel.ConnectionLines)
            {
                var shapeElements = CheckePipeTee(plantModel, connectionLine);
                foreach (var item in shapeElements)
                {
                    plantModelElement.Add(item);
                }
            }

            return xmlDocument;
        }

        private void CreateXmlLineStructure(XElement xElement, string id, string type, string className, int minX, int minY, int maxX, int maxY, string lineStartType, string lineEndType)
        {
            XElement idElement = new XElement("id", id);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", type);
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", className);
            xElement.Add(classElement);

            XElement extentElement = new XElement("edge");
            XElement xStartElement = new XElement("xstart", minX);
            extentElement.Add(xStartElement);
            XElement yStartElement = new XElement("ystart", minY);
            extentElement.Add(yStartElement);
            XElement xEndElement = new XElement("xend", maxX);
            extentElement.Add(xEndElement);
            XElement yEndElement = new XElement("yend", maxY);
            extentElement.Add(yEndElement);
            xElement.Add(extentElement);

            XElement endtypeElement = new XElement("endtype");
            XElement startElement = new XElement("start", lineStartType);
            endtypeElement.Add(startElement);
            XElement endElement = new XElement("end", lineEndType);
            endtypeElement.Add(endElement);
            xElement.Add(endtypeElement);
        }

        private void CreateXmlSymbolStructure(XElement xElement, string id, string type, string className, Obb2 extent, string flip, double degree)
        {
            XElement idElement = new XElement("id", id);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", type);
            xElement.Add(typeElement);

            if (typeElement.Name == "text")
            {
                XElement textclassElement = new XElement("class", className);
                xElement.Add(textclassElement);

                XElement classElement = new XElement("class", "none");
                xElement.Add(classElement);
            }
            else
            {
                XElement classElement = new XElement("class", className);
                xElement.Add(classElement);
            }

            XElement extentElement = new XElement("bndbox");
            XElement xMinElement = new XElement("xmin", Math.Ceiling(extent.GlobalMin.X));
            extentElement.Add(xMinElement);
            XElement yMinElement = new XElement("ymin", Math.Ceiling(extent.GlobalMin.Y));
            extentElement.Add(yMinElement);
            XElement xMaxElement = new XElement("xmax", Math.Ceiling(extent.GlobalMax.X));
            extentElement.Add(xMaxElement);
            XElement yMaxElement = new XElement("ymax", Math.Ceiling(extent.GlobalMax.Y));
            extentElement.Add(yMaxElement);
            xElement.Add(extentElement);

            XElement degreeElement = new XElement("degree", degree);
            xElement.Add(degreeElement);

            XElement flipElement = new XElement("flip", flip);
            xElement.Add(flipElement);
        }

        private List<XElement> CheckePipeTee(PlantModel plantModel, ConnectionLine connectionLine)
        {
            List<XElement> xElements = new List<XElement>();

            int standardStartCnnX = connectionLine.LineEndPoints.BeginPoints.BeginX;
            int standardStartCnnY = connectionLine.LineEndPoints.BeginPoints.BeginY;
            int standardEndCnnX = connectionLine.LineEndPoints.EndPoints.EndX;
            int standardEndCnnY = connectionLine.LineEndPoints.EndPoints.EndY;

            foreach (var pipeLine in plantModel.PipeLines)
            {
                if (connectionLine.ID != pipeLine.ID)
                {
                    int startX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                    int startY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                    int endX = pipeLine.LineEndPoints.EndPoints.EndX;
                    int endY = pipeLine.LineEndPoints.EndPoints.EndY;

                    var startConnectPositon = new Position2(standardStartCnnX, standardStartCnnY);
                    var endConnectPositon = new Position2(standardEndCnnX, standardEndCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, startPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, pipeLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, EndPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, pipeLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, startPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, pipeLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, EndPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, pipeLine.ID);
                        xElements.Add(xElement);
                    }
                }
            }

            foreach (var signalLine in plantModel.SignalLines)
            {
                if (connectionLine.ID != signalLine.ID)
                {
                    int startX = signalLine.LineEndPoints.BeginPoints.BeginX;
                    int startY = signalLine.LineEndPoints.BeginPoints.BeginY;
                    int endX = signalLine.LineEndPoints.EndPoints.EndX;
                    int endY = signalLine.LineEndPoints.EndPoints.EndY;

                    var startConnectPositon = new Position2(standardStartCnnX, standardStartCnnY);
                    var endConnectPositon = new Position2(standardEndCnnX, standardEndCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, startPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, signalLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, EndPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, signalLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, startPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, signalLine.ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, EndPositon), DistanceTolerance))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, signalLine.ID);
                        xElements.Add(xElement);
                    }
                }
            }

            return xElements;
        }

        private XElement CreateXmlConnectionLineStructure(ConnectionLine connectionLine, Position2 position2, string id)
        {
            XElement xElement = new XElement("connection_object");

            XElement idElement = new XElement("id", connectionLine.ID +"-" +"Connect"+ "-" + id);
            xElement.Add(idElement);

            XElement connectLocation = new XElement("connection_point");
            XElement x = new XElement("x", (int)position2.X);
            connectLocation.Add(x);
            XElement y = new XElement("y", (int)position2.Y);
            connectLocation.Add(y);
            xElement.Add(connectLocation);

            XElement connectElement = new XElement("connection");
            XAttribute fromAttribute = new XAttribute("From", connectionLine.ID);
            connectElement.Add(fromAttribute);

            XAttribute toAttribute = new XAttribute("To", id);
            connectElement.Add(toAttribute);
            xElement.Add(connectElement);

            return xElement;
        }

        private XElement CreateXmlConnectionStructure(XElement xElement, PlantModel plantModel, ConnectionPoint connectionPoint)
        {
            XElement idElement = new XElement("id", connectionPoint.ID + "-" + "Connect" + "-" +1);
            xElement.Add(idElement);

            XElement connectLocation = new XElement("connection_point");
            XElement x = new XElement("x", (int)connectionPoint.ConnetionX);
            connectLocation.Add(x);
            XElement y = new XElement("y", (int)connectionPoint.ConnetionY);
            connectLocation.Add(y);
            xElement.Add(connectLocation);

            XElement connectElement = new XElement("connection");
            XAttribute fromAttribute = new XAttribute("From", connectionPoint.ID);
            connectElement.Add(fromAttribute);

            var standardCnnX = connectionPoint.ConnetionX;
            var standardCnnY = connectionPoint.ConnetionY;

            foreach (var equipment in plantModel.Equipments)
            {
                if (connectionPoint.ID != equipment.ID)
                    foreach (var targetConnetionPoint in equipment.ConnectionPoints)
                    {
                        double targetStartX = targetConnetionPoint.ConnetionX;
                        double targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon), DistanceTolerance))
                        {
                            XAttribute toAttribute = new XAttribute("To", equipment.ID);
                            connectElement.Add(toAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            foreach (var instrument in plantModel.Instruments)
            {
                if (connectionPoint.ID != instrument.ID)
                    foreach (var targetConnetionPoint in instrument.ConnectionPoints)
                    {
                        double targetStartX = targetConnetionPoint.ConnetionX;
                        double targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon), DistanceTolerance))
                        {
                            XAttribute toAttribute = new XAttribute("To", instrument.ID);
                            connectElement.Add(toAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            foreach (var pipingComponent in plantModel.PipingComponents)
            {
                if (connectionPoint.ID != pipingComponent.ID)
                    foreach (var targetConnetionPoint in pipingComponent.ConnectionPoints)
                    {
                        double targetStartX = targetConnetionPoint.ConnetionX;
                        double targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon), DistanceTolerance))
                        {
                            XAttribute toAttribute = new XAttribute("To", pipingComponent.ID);
                            connectElement.Add(toAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            foreach (var pipeLine in plantModel.PipeLines)
            {
                if (connectionPoint.ID != pipeLine.ID)
                {
                    int startX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                    int startY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                    int endX = pipeLine.LineEndPoints.EndPoints.EndX;
                    int endY = pipeLine.LineEndPoints.EndPoints.EndY;

                    var symbolConnectPositon = new Position2(standardCnnX, standardCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, startPositon), DistanceTolerance))
                    {
                        XAttribute toAttribute = new XAttribute("To", pipeLine.ID);
                        connectElement.Add(toAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, EndPositon), DistanceTolerance))
                    {
                        XAttribute toAttribute = new XAttribute("To", pipeLine.ID);
                        connectElement.Add(toAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                }
            }

            foreach (var signalLine in plantModel.SignalLines)
            {
                if (connectionPoint.ID != signalLine.ID)
                {
                    int startX = signalLine.LineEndPoints.BeginPoints.BeginX;
                    int startY = signalLine.LineEndPoints.BeginPoints.BeginY;
                    int endX = signalLine.LineEndPoints.EndPoints.EndX;
                    int endY = signalLine.LineEndPoints.EndPoints.EndY;

                    var symbolConnectPositon = new Position2(standardCnnX, standardCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, startPositon), DistanceTolerance))
                    {
                        XAttribute toAttribute = new XAttribute("To", signalLine.ID);
                        connectElement.Add(toAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, EndPositon), DistanceTolerance))
                    {
                        XAttribute toAttribute = new XAttribute("To", signalLine.ID);
                        connectElement.Add(toAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                }
            }

            return xElement;
        }

        private XElement BasicbnbBoxInformation(XElement bndboxElement)
        {
            XElement xminElement = new XElement("xmin", 0);
            bndboxElement.Add(xminElement);
            XElement yminElement = new XElement("ymin", 0);
            bndboxElement.Add(yminElement);
            XElement xmaxElement = new XElement("xmax", 0);
            bndboxElement.Add(xmaxElement);
            XElement ymaxElement = new XElement("ymax", 0);
            bndboxElement.Add(ymaxElement);

            return bndboxElement;
        }
    }

}