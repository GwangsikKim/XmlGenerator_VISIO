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

                var id = equipment.ID;
                var type = "equipment_symbol";
                var className = "none";
                var flip = "n";
                var degree = equipment.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, equipment.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var instrument in plantModel.Instruments)
            {
                XElement shapeElement = new XElement("symbol_object");

                var id = instrument.ID;
                var type = "instrument_symbol";
                var className = "none";
                var flip = "n";
                var degree = instrument.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, instrument.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var pipingComponent in plantModel.PipingComponents)
            {
                XElement shapeElement = new XElement("symbol_object");

                var id = pipingComponent.ID;
                var type = "pipe_symbol";
                var className = "none";
                var flip = "n";
                var degree = pipingComponent.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, pipingComponent.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var text in plantModel.Texts)
            {
                XElement shapeElement = new XElement("symbol_object");

                var id = text.ID;
                var type = "text";
                var className = text.Contents;
                var flip = "n";
                var degree = text.Angle;

                CreateXmlSymbolStructure(shapeElement, id, type, className, text.Extent, flip, degree);
                plantModelElement.Add(shapeElement);
            }

            foreach (var pipeLine in plantModel.PipeLines)
            {
                XElement shapeElement = new XElement("line_object");

                var id = pipeLine.ID;
                var type = "unspecified_line";
                var className = "solid";
                var minX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                var minY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                var maxX = pipeLine.LineEndPoints.EndPoints.EndX;
                var maxY = pipeLine.LineEndPoints.EndPoints.EndY;
                var lineStartType = "none";
                var lineEndType = "none";

                CreateXmlLineStructure(shapeElement, id, type, className, minX, minY, maxX, maxY, lineStartType, lineEndType);
                plantModelElement.Add(shapeElement);
            }

            foreach (var signalLine in plantModel.SignalLines)
            {
                XElement shapeElement = new XElement("line_object");

                var id = signalLine.ID;
                var type = "unspecified_line";
                var className = "dashed";
                var minX = signalLine.LineEndPoints.BeginPoints.BeginX;
                var minY = signalLine.LineEndPoints.BeginPoints.BeginY;
                var maxX = signalLine.LineEndPoints.EndPoints.EndX;
                var maxY = signalLine.LineEndPoints.EndPoints.EndY;
                var lineStartType = "none";
                var lineEndType = "none";

                CreateXmlLineStructure(shapeElement, id, type, className, minX, minY, maxX, maxY, lineStartType, lineEndType);
                plantModelElement.Add(shapeElement);
            }

            for (int i = 0; i < plantModel.ConnectionPoints.Count; i++)
            {
                XElement shapeElement = new XElement("connection_object");
                CreateXmlConnectionStructure(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }

            for (int i = 0; i < plantModel.ConnectionLines.Count; i++)
            {
                var shapeElements = CheckePipeTee(plantModel, plantModel.ConnectionLines[i]);
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

            var standardStartCnnX = connectionLine.LineEndPoints.BeginPoints.BeginX;
            var standardStartCnnY = connectionLine.LineEndPoints.BeginPoints.BeginY;
            var standardEndCnnX = connectionLine.LineEndPoints.EndPoints.EndX;
            var standardEndCnnY = connectionLine.LineEndPoints.EndPoints.EndY;

            for (int j = 0; j < plantModel.PipeLines.Count; j++)
            {
                if (connectionLine.ID != plantModel.PipeLines[j].ID)
                {
                    var startX = plantModel.PipeLines[j].LineEndPoints.BeginPoints.BeginX;
                    var startY = plantModel.PipeLines[j].LineEndPoints.BeginPoints.BeginY;
                    var endX = plantModel.PipeLines[j].LineEndPoints.EndPoints.EndX;
                    var endY = plantModel.PipeLines[j].LineEndPoints.EndPoints.EndY;

                    var startConnectPositon = new Position2(standardStartCnnX, standardStartCnnY);
                    var endConnectPositon = new Position2(standardEndCnnX, standardEndCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, startPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, plantModel.PipeLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, EndPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, plantModel.PipeLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, startPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, plantModel.PipeLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, EndPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, plantModel.PipeLines[j].ID);
                        xElements.Add(xElement);
                    }
                }
            }

            for (int j = 0; j < plantModel.SignalLines.Count; j++)
            {
                if (connectionLine.ID != plantModel.SignalLines[j].ID)
                {
                    var startX = plantModel.SignalLines[j].LineEndPoints.BeginPoints.BeginX;
                    var startY = plantModel.SignalLines[j].LineEndPoints.BeginPoints.BeginY;
                    var endX = plantModel.SignalLines[j].LineEndPoints.EndPoints.EndX;
                    var endY = plantModel.SignalLines[j].LineEndPoints.EndPoints.EndY;

                    var startConnectPositon = new Position2(standardStartCnnX, standardStartCnnY);
                    var endConnectPositon = new Position2(standardEndCnnX, standardEndCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, startPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, plantModel.SignalLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(startConnectPositon, EndPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, startConnectPositon, plantModel.SignalLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, startPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, plantModel.SignalLines[j].ID);
                        xElements.Add(xElement);
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(endConnectPositon, EndPositon)))
                    {
                        var xElement = CreateXmlConnectionLineStructure(connectionLine, endConnectPositon, plantModel.SignalLines[j].ID);
                        xElements.Add(xElement);
                    }
                }
            }

            return xElements;
        }

        private XElement CreateXmlConnectionLineStructure(ConnectionLine connectionLine, Position2 position2, string id)
        {
            XElement xElement = new XElement("connection_object");

            XElement idElement = new XElement("id", connectionLine.ID + "Connect" + id);
            xElement.Add(idElement);

            XElement classElement = new XElement("class", "connection");
            xElement.Add(classElement);

            XElement connectLocation = new XElement("connectionpoint");
            XElement x = new XElement("X", position2.X);
            connectLocation.Add(x);
            XElement y = new XElement("Y", position2.Y);
            connectLocation.Add(y);
            xElement.Add(connectLocation);

            XElement connectElement = new XElement("connectionobject");
            XElement connectAttribute = new XElement("connection");
            XAttribute fromAttribute = new XAttribute("From", connectionLine.ID);
            connectAttribute.Add(fromAttribute);

            XAttribute toAttribute = new XAttribute("To", id);
            connectAttribute.Add(toAttribute);
            connectElement.Add(connectAttribute);
            xElement.Add(connectElement);

            return xElement;
        }

        private XElement CreateXmlConnectionStructure(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.ConnectionPoints[i].ID + "Connect");
            xElement.Add(idElement);

            XElement classElement = new XElement("class", "connection");
            xElement.Add(classElement);

            XElement connectLocation = new XElement("connectionpoint");
            XElement x = new XElement("X", plantModel.ConnectionPoints[i].ConnetionX);
            connectLocation.Add(x);
            XElement y = new XElement("Y", plantModel.ConnectionPoints[i].ConnetionY);
            connectLocation.Add(y);
            xElement.Add(connectLocation);

            XElement connectElement = new XElement("connectionobject");
            XElement connectAttribute = new XElement("connection");
            XAttribute fromAttribute = new XAttribute("From", plantModel.ConnectionPoints[i].ID);
            connectAttribute.Add(fromAttribute);

            var standardCnnX = plantModel.ConnectionPoints[i].ConnetionX;
            var standardCnnY = plantModel.ConnectionPoints[i].ConnetionY;

            for (int j = 0; j < plantModel.Equipments.Count; j++)
            {
                if (plantModel.ConnectionPoints[i].ID != plantModel.Equipments[j].ID)
                    foreach (var targetConnetionPoint in plantModel.Equipments[j].ConnectionPoints)
                    {
                        var targetStartX = targetConnetionPoint.ConnetionX;
                        var targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon)))
                        {
                            XAttribute toAttribute = new XAttribute("To", plantModel.Equipments[j].ID);
                            connectAttribute.Add(toAttribute);
                            connectElement.Add(connectAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            for (int j = 0; j < plantModel.Instruments.Count; j++)
            {
                if (plantModel.ConnectionPoints[i].ID != plantModel.Instruments[j].ID)
                    foreach (var targetConnetionPoint in plantModel.Instruments[j].ConnectionPoints)
                    {
                        var targetStartX = targetConnetionPoint.ConnetionX;
                        var targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon)))
                        {
                            XAttribute toAttribute = new XAttribute("To", plantModel.Instruments[j].ID);
                            connectAttribute.Add(toAttribute);
                            connectElement.Add(connectAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            for (int j = 0; j < plantModel.PipingComponents.Count; j++)
            {
                if (plantModel.ConnectionPoints[i].ID != plantModel.PipingComponents[j].ID)
                    foreach (var targetConnetionPoint in plantModel.PipingComponents[j].ConnectionPoints)
                    {
                        var targetStartX = targetConnetionPoint.ConnetionX;
                        var targetStartY = targetConnetionPoint.ConnetionY;

                        var startPositon = new Position2(standardCnnX, standardCnnY);
                        var EndPositon = new Position2(targetStartX, targetStartY);

                        if (Tolerance.IsZeroDistance(Position2.Distance(startPositon, EndPositon)))
                        {
                            XAttribute toAttribute = new XAttribute("To", plantModel.PipingComponents[j].ID);
                            connectAttribute.Add(toAttribute);
                            connectElement.Add(connectAttribute);
                            xElement.Add(connectElement);

                            return xElement;
                        }
                    }
            }

            for (int j = 0; j < plantModel.PipeLines.Count; j++)
            {
                if (plantModel.ConnectionPoints[i].ID != plantModel.PipeLines[j].ID)
                {
                    var startX = plantModel.PipeLines[j].LineEndPoints.BeginPoints.BeginX;
                    var startY = plantModel.PipeLines[j].LineEndPoints.BeginPoints.BeginY;
                    var endX = plantModel.PipeLines[j].LineEndPoints.EndPoints.EndX;
                    var endY = plantModel.PipeLines[j].LineEndPoints.EndPoints.EndY;

                    var symbolConnectPositon = new Position2(standardCnnX, standardCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, startPositon)))
                    {
                        XAttribute toAttribute = new XAttribute("To", plantModel.PipeLines[j].ID);
                        connectAttribute.Add(toAttribute);
                        connectElement.Add(connectAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, EndPositon)))
                    {
                        XAttribute toAttribute = new XAttribute("To", plantModel.PipeLines[j].ID);
                        connectAttribute.Add(toAttribute);
                        connectElement.Add(connectAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                }
            }

            for (int j = 0; j < plantModel.SignalLines.Count; j++)
            {
                if (plantModel.ConnectionPoints[i].ID != plantModel.SignalLines[j].ID)
                {
                    var startX = plantModel.SignalLines[j].LineEndPoints.BeginPoints.BeginX;
                    var startY = plantModel.SignalLines[j].LineEndPoints.BeginPoints.BeginY;
                    var endX = plantModel.SignalLines[j].LineEndPoints.EndPoints.EndX;
                    var endY = plantModel.SignalLines[j].LineEndPoints.EndPoints.EndY;

                    var symbolConnectPositon = new Position2(standardCnnX, standardCnnY);
                    var startPositon = new Position2(startX, startY);
                    var EndPositon = new Position2(endX, endY);

                    if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, startPositon)))
                    {
                        XAttribute toAttribute = new XAttribute("To", plantModel.SignalLines[j].ID);
                        connectAttribute.Add(toAttribute);
                        connectElement.Add(connectAttribute);
                        xElement.Add(connectElement);

                        return xElement;
                    }
                    else if (Tolerance.IsZeroDistance(Position2.Distance(symbolConnectPositon, EndPositon)))
                    {
                        XAttribute toAttribute = new XAttribute("To", plantModel.SignalLines[j].ID);
                        connectAttribute.Add(toAttribute);
                        connectElement.Add(connectAttribute);
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