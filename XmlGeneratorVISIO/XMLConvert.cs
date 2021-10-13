using Visio = Microsoft.Office.Interop.Visio;
using Microsoft.Office.Interop.Visio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using XMLGeneratorVISIO;
using XmlGeneratorVISIO;
using System.Text.RegularExpressions;
using SmartDesign.MathUtil;

namespace XMLGeneratorVISIO
{
    class VisioReader
    {
        private readonly string[] EquipmentSymbolTypes =
        {
            "none",
            "Vessel", "Selectablecompressor", "Selectablecompressor1", "Heatexchanger1" ,"Fluidcontacting", "Barrel"
        };

        private readonly string[] PipeSymbolTypes =
        {
            "none",
            "Flangedvalve","Flanged/bolted", "Gatevalve", "Relief", "Junction", "Globevalve", "Checkvalve", "Poweredvalve", "Reducer",
            "Diaphragmvalve", "Endcaps", "Endcaps2", "Relief(angle)", "Off-SheetLabel3", "Butterflyvalve", "Callout3", "Ballvalve",
            "Screw-downvalve","CapillaryTube", "Sleevejoint"
        };

        //Valve 값 고민중
        private readonly string[] ValveSymbolTypes =
        {
           "Gatevalve", "Relief", "Diaphragmvalve", "Globevalve", "Checkvalve", "Poweredvalve", "Butterflyvalve", "Ballvalve"
        };

        private readonly string[] InstrumentSymbolTypes =
        {
            "none",
            "Flangedaccesspoint", "Indicator", "CRT", "Diamond", "GenericUtility", "Generaljoint", "Filter2"
        };

        private readonly string[] PipeLineTypes =
        {
            "MajorPipeline", "MinorPipeline", "MajorPipelineR", "MajorPipelineL", "MinorPipelineR", "MinorPipelineL"
            //,"Dynamicconnector"
        };

        private readonly string[] InstrumentLineTypes =
        {
            "none", "Electric", "Data", "Electric3"
        };

        string path = "";

        Visio.Application app = new Visio.Application();
        Visio.Document visioDoc;
        Visio.Page visioPage;

        public VisioReader(string path)
        {
            visioDoc = app.Documents.Add(path);
            visioPage = app.ActivePage;
        }

        public XDocument XMLGenerator()
        {
            PlantModel plantModel = new PlantModel();
            PlantTypeClassification(plantModel);

            var xmlData = XmlMaker(plantModel);

            return xmlData;
        }

        public void PlantTypeClassification(PlantModel plantModel)
        {
            foreach (Shape shape in visioPage.Shapes)
            {
                string shapeDeleteSpace = Regex.Replace(shape.NameU, @"\s", "");

                var shapeReplace = RemoveSpecificCharacters(shapeDeleteSpace);

                if (shape.Text != null && shape.Text != "" && shape.Text != "￼")
                {
                    Text text = new Text();
                    text.ID = shape.ID.ToString();
                    text.Contents = shape.Text;
                    TextAttribution(plantModel, text, shape);

                    plantModel.Texts.Add(text);
                }

                if (EquipmentSymbolTypes.Contains(shapeReplace))
                {
                    Equipment equipment = new Equipment();
                    equipment.ID = shape.ID.ToString();
                    EquipmentAttribution(plantModel, equipment, shape);
                }
                else if (PipeSymbolTypes.Contains(shapeReplace))
                {
                    PipingComponent pipingComponent = new PipingComponent();
                    pipingComponent.ID = shape.ID.ToString();
                    PipingComponentAttribution(plantModel, pipingComponent, shape);
                }
                else if (InstrumentSymbolTypes.Contains(shapeReplace))
                {
                    Instrument instrument = new Instrument();
                    instrument.ID = shape.ID.ToString();
                    InstrumentAttribution(plantModel, instrument, shape);
                }
                else if (PipeLineTypes.Contains(shapeReplace))
                {
                    PipeLineAttribution(plantModel, shape);
                }
                else if (InstrumentLineTypes.Contains(shapeReplace))
                {
                    SignalAttribution(plantModel, shape);
                }
                else
                {
                    Console.WriteLine(shape.NameU);
                }
            }
        }

        private void EquipmentAttribution(PlantModel plantModel, Equipment equipment, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ExtractObjectBoxInformation(shape, extent, center, angle);
            equipment.Centers = center;
            equipment.Extents = extent;
            equipment.Angle = angle.ObjAngle;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, equipment.ConnectionPoints);

            plantModel.Equipments.Add(equipment);
        }

        private void PipingComponentAttribution(PlantModel plantModel, PipingComponent pipingComponent, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ExtractObjectBoxInformation(shape, extent, center, angle);
            pipingComponent.Angle = angle.ObjAngle;
            pipingComponent.Centers = center;
            pipingComponent.Extents = extent;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, pipingComponent.ConnectionPoints);

            plantModel.PipingComponents.Add(pipingComponent);
        }

        private void InstrumentAttribution(PlantModel plantModel, Instrument instrument, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ExtractObjectBoxInformation(shape, extent, center, angle);
            instrument.Angle = angle.ObjAngle;
            instrument.Centers = center;
            instrument.Extents = extent;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, instrument.ConnectionPoints);

            plantModel.Instruments.Add(instrument);
        }

        private void PipeLineAttribution(PlantModel plantModel, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (i < lineItem.X.Count - 1)
                {
                    PipeLine pipeLine = new PipeLine();
                    ConnectionLine connectionLine = new ConnectionLine();
                    pipeLine.ID = shape.ID.ToString() + "PipeL" + "-" + i;

                    ExtractObjectBoxInformation(shape, extent, center, angle);
                    pipeLine.Extents = extent;

                    pipeLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    pipeLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    pipeLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    pipeLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    double linePinX = (lineItem.X[i] + lineItem.X[i + 1]) * 0.5;
                    double linePinY = (lineItem.Y[i] + lineItem.Y[i + 1]) * 0.5;

                    center.PinX = Math.Abs((int)linePinX);
                    center.PinY = Math.Abs((int)linePinY);

                    pipeLine.Centers = center;

                    plantModel.PipeLines.Add(pipeLine);

                    connectionLine.ID = pipeLine.ID;

                    connectionLine.LineEndPoints.BeginPoints.BeginX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                    connectionLine.LineEndPoints.BeginPoints.BeginY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                    connectionLine.LineEndPoints.EndPoints.EndX = pipeLine.LineEndPoints.EndPoints.EndX;
                    connectionLine.LineEndPoints.EndPoints.EndY = pipeLine.LineEndPoints.EndPoints.EndY;

                    connectionLine.ObjCenterX = center.PinX;
                    connectionLine.ObjCenterY = center.PinY;

                    plantModel.ConnectionLines.Add(connectionLine);
                }
            }
        }

        private void SignalAttribution(PlantModel plantModel, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (i < lineItem.X.Count - 1)
                {
                    SignalLine signalLine = new SignalLine();
                    ConnectionLine connectionLine = new ConnectionLine();
                    signalLine.ID = shape.ID.ToString() + "SignalL" + "-" + i;

                    ExtractObjectBoxInformation(shape, extent, center, angle);
                    signalLine.Extents = extent;

                    signalLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    signalLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    signalLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    signalLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    double linePinX = (lineItem.X[i] + lineItem.X[i + 1]) * 0.5;
                    double linePinY = (lineItem.Y[i] + lineItem.Y[i + 1]) * 0.5;

                    center.PinX = Math.Abs((int)linePinX);
                    center.PinY = Math.Abs((int)linePinY);

                    signalLine.Centers = center;

                    plantModel.SignalLines.Add(signalLine);

                    connectionLine.ID = signalLine.ID;

                    connectionLine.LineEndPoints.BeginPoints.BeginX = signalLine.LineEndPoints.BeginPoints.BeginX;
                    connectionLine.LineEndPoints.BeginPoints.BeginY = signalLine.LineEndPoints.BeginPoints.BeginY;
                    connectionLine.LineEndPoints.EndPoints.EndX = signalLine.LineEndPoints.EndPoints.EndX;
                    connectionLine.LineEndPoints.EndPoints.EndY = signalLine.LineEndPoints.EndPoints.EndY;

                    connectionLine.ObjCenterX = center.PinX;
                    connectionLine.ObjCenterY = center.PinY;

                    plantModel.ConnectionLines.Add(connectionLine);

                    //connectionPoint1.ID = signalLine.ID;

                    //connectionPoint1.ConnetionX = signalLine.LineEndPoints.BeginPoints.BeginX;
                    //connectionPoint1.ConnetionY = signalLine.LineEndPoints.BeginPoints.BeginY;

                    //connectionPoint1.ObjCenterX = center.PinX;
                    //connectionPoint1.ObjCenterY = center.PinY;

                    //connectionPoint2.ID = signalLine.ID;

                    //connectionPoint2.ConnetionX = signalLine.LineEndPoints.EndPoints.EndX;
                    //connectionPoint2.ConnetionY = signalLine.LineEndPoints.EndPoints.EndY;

                    //connectionPoint2.ObjCenterX = center.PinX;
                    //connectionPoint2.ObjCenterY = center.PinY;

                    //plantModel.ConnectionPoints.Add(connectionPoint1);
                    //plantModel.ConnectionPoints.Add(connectionPoint2);
                }
            }
        }

        private void TextAttribution(PlantModel plantModel, Text text, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ConnectionPoint connectionPoint = new ConnectionPoint();

            short iRow1 = (short)VisRowIndices.visRowXFormOut;
            short iRow2 = (short)VisRowIndices.visRowTextXForm;

            string shapeStrWidth = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow1,
                    (short)VisCellIndices.visXFormWidth
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int shapeWidth = RemoveUnits(shapeStrWidth);

            string shapeStrHeight = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow1,
                    (short)VisCellIndices.visXFormHeight
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int shapeHeight = RemoveUnits(shapeStrHeight);

            var shapeStrPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow1,
                    (short)VisCellIndices.visXFormPinX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int shapePinX = RemoveUnits(shapeStrPinX);

            string shapeStrPinY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow1,
                    (short)VisCellIndices.visXFormPinY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int shapePinY = RemoveUnits(shapeStrPinY);

            var strPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow2,
                    (short)VisCellIndices.visXFormPinX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinX = RemoveUnits(strPinX);

            string strPinY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow2,
                    (short)VisCellIndices.visXFormPinY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinY = RemoveUnits(strPinY);

            Min min = new Min();

            var minX = shapePinX - shapeWidth * 0.5;
            var minY = shapePinY - shapeHeight * 0.5;

            min.X = (int)minX;
            min.Y = (int)minY;

            int textcenterX = (int)minX + pinX;
            int textcenterY = (int)minY + pinY;

            center.PinX = textcenterX;
            center.PinY = textcenterY;

            string strWidth = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow2,
                    (short)VisCellIndices.visXFormWidth
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int width = RemoveUnits(strWidth);

            string strHeight = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow2,
                    (short)VisCellIndices.visXFormHeight
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int height = RemoveUnits(strHeight);

            string strAngle = shape.get_CellsSRC(
                   (short)VisSectionIndices.visSectionObject,
                   iRow2,
                   (short)VisCellIndices.visXFormAngle
                   ).get_ResultStr(VisUnitCodes.visNoCast);
            angle.ObjAngle = RemoveUnits(strAngle);

            Extentmaker(extent, textcenterX, textcenterY, width, height, angle);

            text.Centers = center;
            text.Extents = extent;
            text.Angle = angle.ObjAngle;

            ConnectionLine connectionLine = new ConnectionLine();
            connectionLine.ID = shape.ID.ToString() + 'T';
            connectionLine.LineEndPoints.BeginPoints.BeginX = center.PinX;
            connectionLine.LineEndPoints.BeginPoints.BeginY = center.PinY;
            connectionLine.LineEndPoints.EndPoints.EndX = shapePinX;
            connectionLine.LineEndPoints.EndPoints.EndY = shapePinY;

            plantModel.ConnectionLines.Add(connectionLine);
        }

        private void ExtractObjectBoxInformation(Shape shape, Extent extent, Center center, Angle angle)
        {
            short iRow = (short)VisRowIndices.visRowXFormOut;

            var strPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormPinX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinX = RemoveUnits(strPinX);
            center.PinX = pinX;

            string strPinY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormPinY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinY = RemoveUnits(strPinY);
            center.PinY = pinY;

            string strWidth = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormWidth
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int width = RemoveUnits(strWidth);

            string strHeight = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormHeight
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int height = RemoveUnits(strHeight);

            string strAngle = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormAngle
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            angle.ObjAngle = RemoveUnits(strAngle);

            Extentmaker(extent, pinX, pinY, width, height, angle);
        }

        private void ExtractLineInformation(Shape shape, LineItem lineEndPoints)
        {
            short iRow1 = (short)VisRowIndices.visRowXForm1D;
            short iRow2 = (short)VisRowIndices.visRowVertex;

            var strLineBeginPointX = shape.get_CellsSRC(
                  (short)VisSectionIndices.visSectionObject,
                  iRow1,
                  (short)VisCellIndices.vis1DBeginX
                  ).get_ResultStr(VisUnitCodes.visNoCast);
            int lineBeginPointX = RemoveUnits(strLineBeginPointX);

            var strLineBeginPointY = shape.get_CellsSRC(
                  (short)VisSectionIndices.visSectionObject,
                  iRow1,
                  (short)VisCellIndices.vis1DBeginY
                  ).get_ResultStr(VisUnitCodes.visNoCast);
            int lineBeginPointY = RemoveUnits(strLineBeginPointY);

            while (shape.get_CellsSRCExists(
              (short)VisSectionIndices.visSectionFirstComponent,
              (short)iRow2,
              (short)VisCellIndices.visX,
              (short)0) != 0)
            {
                var strLinePointX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionFirstComponent,
                    iRow2,
                    (short)VisCellIndices.visX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
                int lineEndPointX = RemoveUnits(strLinePointX);

                if (Math.Abs(lineEndPointX) > 2.5)
                {
                    lineEndPoints.X.Add(lineBeginPointX + lineEndPointX);
                }
                else
                {
                    lineEndPoints.X.Add(lineBeginPointX);
                }

                string strLinePointY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionFirstComponent,
                    iRow2,
                    (short)VisCellIndices.visY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
                int lineEndPointY = RemoveUnits(strLinePointY);

                if (Math.Abs(lineEndPointY) > 2.5)
                {
                    lineEndPoints.Y.Add(lineBeginPointY + lineEndPointY);
                }
                else
                {
                    lineEndPoints.Y.Add(lineBeginPointY);
                }

                iRow2++;
            }
        }

        private void ExtractObjectConnectionInformation(Shape shape, PlantModel plantModel, Extent extent, Center center, Angle angle, List<ConnectionPoint> connectionPoints)
        {
            short iRowCnn = (short)VisRowIndices.visRowConnectionPts;
            double radian = Math.PI * Convert.ToDouble(angle.ObjAngle) / 180.0;

            while (shape.get_CellsSRCExists(
                  (short)VisSectionIndices.visSectionConnectionPts,
                  (short)iRowCnn,
                  (short)VisCellIndices.visCnnctX,
                  (short)0) != 0)
            {
                ConnectionPoint connectionPoint = new ConnectionPoint();

                var strCnnPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionConnectionPts,
                    iRowCnn,
                    (short)VisCellIndices.visCnnctX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
                int cnnPinX = RemoveUnits(strCnnPinX);

                string strCnnPinY = shape.get_CellsSRC(
                        (short)VisSectionIndices.visSectionConnectionPts,
                        iRowCnn,
                        (short)VisCellIndices.visCnnctY
                        ).get_ResultStr(VisUnitCodes.visNoCast);
                int cnnPinY = RemoveUnits(strCnnPinY);

                if (angle.ObjAngle == 90) // 아래 각도별 인식으로 바꾸기(임시로 생성)
                {
                    //double x2 = Math.Cos(radian) * cnnPinX - Math.Sin(radian) * cnnPinX;
                    //double y2 = Math.Sin(radian) * cnnPinY + Math.Cos(radian) * cnnPinY;

                    double pinX = extent.Min.X + cnnPinY;
                    double pinY = extent.Min.Y + cnnPinX;

                    connectionPoint.ConnetionX = pinX;
                    connectionPoint.ConnetionY = pinY;
                }
                else
                {
                    connectionPoint.ConnetionX = extent.Min.X + cnnPinX;
                    connectionPoint.ConnetionY = extent.Min.Y + cnnPinY;
                }

                connectionPoint.ID = shape.ID.ToString();

                connectionPoint.ObjCenterX = center.PinX;
                connectionPoint.ObjCenterY = center.PinY;

                connectionPoints.Add(connectionPoint);
                plantModel.ConnectionPoints.Add(connectionPoint);

                iRowCnn++;
            }
        }

        private Extent Extentmaker(Extent extent, int pinX, int pinY, int width, int height, Angle angle)
        {
            Min min = new Min();
            Max max = new Max();

            if (angle.ObjAngle == 90 || angle.ObjAngle == -90)
            {
                var minX = pinX - height * 0.5;
                var minY = pinY - width * 0.5;
                var maxX = pinX + height * 0.5;
                var maxY = pinY + width * 0.5;

                min.X = (int)minX;
                min.Y = (int)minY;
                max.X = (int)maxX;
                max.Y = (int)maxY;
            }
            else
            {
                var minX = pinX - width * 0.5;
                var minY = pinY - height * 0.5;
                var maxX = pinX + width * 0.5;
                var maxY = pinY + height * 0.5;

                min.X = (int)minX;
                min.Y = (int)minY;
                max.X = (int)maxX;
                max.Y = (int)maxY;
            }


            extent.Min = min;
            extent.Max = max;

            return extent;
        }

        private string RemoveSpecificCharacters(string shapeDeleteSpace)
        {
            int index1 = shapeDeleteSpace.IndexOf('.');

            if (index1 > 0)
            {
                string shapeReplace = shapeDeleteSpace.Substring(0, index1);
                return shapeReplace;
            }
            else
            {
                string shapeReplace = shapeDeleteSpace;
                return shapeReplace;
            }
        }

        private int RemoveUnits(string strValue)
        {
            string[] charsToRemove = new string[] { " mm", " 도", " pt" };

            foreach (var chars in charsToRemove)
            {
                if (strValue.Contains(chars))
                {
                    if (chars == " pt")
                    {
                        strValue = strValue.Replace(chars, string.Empty);
                        double doubleValue = double.Parse(strValue) * 0.35278;
                        int intValue = (int)doubleValue;

                        return intValue;

                    }
                    else
                    {
                        strValue = strValue.Replace(chars, string.Empty);
                        double doubleValue = double.Parse(strValue);
                        int intValue = (int)doubleValue;

                        return intValue;
                    }
                }
            }

            return 0;
        }

        public XDocument XmlMaker(PlantModel plantModel)
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

            for (int i = 0; i < plantModel.Equipments.Count; i++)
            {
                XElement shapeElement = new XElement("symbol_object");
                CreateXmlSymbolStructure1(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }
            for (int i = 0; i < plantModel.Instruments.Count; i++)
            {
                XElement shapeElement = new XElement("symbol_object");
                CreateXmlSymbolStructure2(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }
            for (int i = 0; i < plantModel.PipingComponents.Count; i++)
            {
                XElement shapeElement = new XElement("symbol_object");
                CreateXmlSymbolStructure3(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }
            for (int i = 0; i < plantModel.PipeLines.Count; i++)
            {
                XElement shapeElement = new XElement("line_object");
                CreateXmlLineStructure1(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }
            for (int i = 0; i < plantModel.SignalLines.Count; i++)
            {
                XElement shapeElement = new XElement("line_object");
                CreateXmlLineStructure2(shapeElement, plantModel, i);
                plantModelElement.Add(shapeElement);
            }
            for (int i = 0; i < plantModel.Texts.Count; i++)
            {
                XElement shapeElement = new XElement("symbol_object");
                CreateXmlSymbolStructure4(shapeElement, plantModel, i);
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

            XElement idElement = new XElement("id", connectionLine.ID + "Connect");
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

        //PipeLine
        public void CreateXmlLineStructure1(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.PipeLines[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "unspecified_line");
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", "solid");
            xElement.Add(classElement);

            XElement extentElement = new XElement("edge");
            XElement xStartElement = new XElement("xstart", plantModel.PipeLines[i].LineEndPoints.BeginPoints.BeginX);
            extentElement.Add(xStartElement);
            XElement yStartElement = new XElement("ystart", plantModel.PipeLines[i].LineEndPoints.BeginPoints.BeginY);
            extentElement.Add(yStartElement);
            XElement xEndElement = new XElement("xend", plantModel.PipeLines[i].LineEndPoints.EndPoints.EndX);
            extentElement.Add(xEndElement);
            XElement yEndElement = new XElement("yend", plantModel.PipeLines[i].LineEndPoints.EndPoints.EndY);
            extentElement.Add(yEndElement);
            xElement.Add(extentElement);

            XElement endtypeElement = new XElement("endtype");
            XElement startElement = new XElement("start", "none");
            endtypeElement.Add(startElement);
            XElement endElement = new XElement("end", "none");
            endtypeElement.Add(endElement);
            xElement.Add(endtypeElement);
        }

        //SignalLine
        public void CreateXmlLineStructure2(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.SignalLines[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "unspecified_line");
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", "dashed");
            xElement.Add(classElement);

            XElement extentElement = new XElement("edge");
            XElement xStartElement = new XElement("xstart", plantModel.SignalLines[i].LineEndPoints.BeginPoints.BeginX);
            extentElement.Add(xStartElement);
            XElement yStartElement = new XElement("ystart", plantModel.SignalLines[i].LineEndPoints.BeginPoints.BeginY);
            extentElement.Add(yStartElement);
            XElement xEndElement = new XElement("xend", plantModel.SignalLines[i].LineEndPoints.EndPoints.EndX);
            extentElement.Add(xEndElement);
            XElement yEndElement = new XElement("yend", plantModel.SignalLines[i].LineEndPoints.EndPoints.EndY);
            extentElement.Add(yEndElement);
            xElement.Add(extentElement);

            XElement endtypeElement = new XElement("endtype");
            XElement startElement = new XElement("start", "none");
            endtypeElement.Add(startElement);
            XElement endElement = new XElement("end", "none");
            endtypeElement.Add(endElement);
            xElement.Add(endtypeElement);
        }

        //Equipment
        public void CreateXmlSymbolStructure1(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.Equipments[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "equipment_symbol");
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", "none");
            xElement.Add(classElement);

            XElement extentElement = new XElement("bndbox");
            XElement xMinElement = new XElement("xmin", plantModel.Equipments[i].Extents.Min.X);
            extentElement.Add(xMinElement);
            XElement yMinElement = new XElement("ymin", plantModel.Equipments[i].Extents.Min.Y);
            extentElement.Add(yMinElement);
            XElement xMaxElement = new XElement("xmax", plantModel.Equipments[i].Extents.Max.X);
            extentElement.Add(xMaxElement);
            XElement yMaxElement = new XElement("ymax", plantModel.Equipments[i].Extents.Max.Y);
            extentElement.Add(yMaxElement);
            xElement.Add(extentElement);

            XElement degreeElement = new XElement("degree", plantModel.Equipments[i].Angle);
            xElement.Add(degreeElement);

            XElement flipElement = new XElement("flip", "n");
            xElement.Add(flipElement);
        }

        //Instrument
        public void CreateXmlSymbolStructure2(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.Instruments[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "instrument_symbol");
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", "none");
            xElement.Add(classElement);

            XElement extentElement = new XElement("bndbox");
            XElement xMinElement = new XElement("xmin", plantModel.Instruments[i].Extents.Min.X);
            extentElement.Add(xMinElement);
            XElement yMinElement = new XElement("ymin", plantModel.Instruments[i].Extents.Min.Y);
            extentElement.Add(yMinElement);
            XElement xMaxElement = new XElement("xmax", plantModel.Instruments[i].Extents.Max.X);
            extentElement.Add(xMaxElement);
            XElement yMaxElement = new XElement("ymax", plantModel.Instruments[i].Extents.Max.Y);
            extentElement.Add(yMaxElement);
            xElement.Add(extentElement);

            XElement degreeElement = new XElement("degree", plantModel.Instruments[i].Angle);
            xElement.Add(degreeElement);

            XElement flipElement = new XElement("flip", "n");
            xElement.Add(flipElement);
        }

        //PipingComponent
        public void CreateXmlSymbolStructure3(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.PipingComponents[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "pipe_symbol");
            xElement.Add(typeElement);

            XElement classElement = new XElement("class", "none");
            xElement.Add(classElement);
            XElement extentElement = new XElement("bndbox");
            XElement xMinElement = new XElement("xmin", plantModel.PipingComponents[i].Extents.Min.X);
            extentElement.Add(xMinElement);
            XElement yMinElement = new XElement("ymin", plantModel.PipingComponents[i].Extents.Min.Y);
            extentElement.Add(yMinElement);
            XElement xMaxElement = new XElement("xmax", plantModel.PipingComponents[i].Extents.Max.X);
            extentElement.Add(xMaxElement);
            XElement yMaxElement = new XElement("ymax", plantModel.PipingComponents[i].Extents.Max.Y);
            extentElement.Add(yMaxElement);
            xElement.Add(extentElement);

            XElement degreeElement = new XElement("degree", plantModel.PipingComponents[i].Angle);
            xElement.Add(degreeElement);

            XElement flipElement = new XElement("flip", "n");
            xElement.Add(flipElement);
        }

        //Text
        public void CreateXmlSymbolStructure4(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("id", plantModel.Texts[i].ID);
            xElement.Add(idElement);

            XElement typeElement = new XElement("type", "text");
            xElement.Add(typeElement);

            XElement textclassElement = new XElement("class", plantModel.Texts[i].Contents);
            xElement.Add(textclassElement);

            XElement classElement = new XElement("class", "none");
            xElement.Add(classElement);

            XElement extentElement = new XElement("bndbox");
            XElement xMinElement = new XElement("xmin", plantModel.Texts[i].Extents.Min.X);
            extentElement.Add(xMinElement);
            XElement yMinElement = new XElement("ymin", plantModel.Texts[i].Extents.Min.Y);
            extentElement.Add(yMinElement);
            XElement xMaxElement = new XElement("xmax", plantModel.Texts[i].Extents.Max.X);
            extentElement.Add(xMaxElement);
            XElement yMaxElement = new XElement("ymax", plantModel.Texts[i].Extents.Max.Y);
            extentElement.Add(yMaxElement);
            xElement.Add(extentElement);

            XElement degreeElement = new XElement("degree", plantModel.Texts[i].Angle);
            xElement.Add(degreeElement);

            XElement flipElement = new XElement("flip", "n");
            xElement.Add(flipElement);
        }
    }
}
