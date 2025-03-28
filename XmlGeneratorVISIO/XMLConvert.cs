﻿using Visio = Microsoft.Office.Interop.Visio;
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

namespace XMLGeneratorVISIO
{
    class VisioReader
    {
        private readonly string[] EquipmentSymbolTypes =
        {
            "none",
            "Vessel", "Selectablecompressor", "Selectablecompressor1"
        };

        private readonly string[] PipeSymbolTypes =
        {
            "none",
            "Flangedvalve","Flanged/bolted", "Gatevalve", "Relief", "Junction", "Globevalve", "Checkvalve", "Poweredvalve", "Reducer",
            "Diaphragmvalve", "Endcaps", "Endcaps2", "Relief(angle)", "Off-SheetLabel3"
        };

        //Valve 값 고민중
        private readonly string[] ValveSymbolTypes =
        {
           "Gatevalve", "Relief", "Diaphragmvalve", "Globevalve", "Checkvalve", "Poweredvalve"
        };

        private readonly string[] InstrumentSymbolTypes =
        {
            "none",
            "Flangedaccesspoint", "Indicator", "CRT", "Diamond", "GenericUtility"
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
            List<PlantModel> plantModels = new List<PlantModel>();

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
                    text.ID = shape.ID;
                    text.Contents = shape.Text;
                    TextAttribution(text, shape);

                    plantModel.Texts.Add(text);
                }

                if (EquipmentSymbolTypes.Contains(shapeReplace))
                {
                    Equipment equipment = new Equipment();
                    equipment.ID = shape.ID;
                    EquipmentAttribution(plantModel, equipment, shape);
                }
                else if (PipeSymbolTypes.Contains(shapeReplace))
                {
                    PipingComponent pipingComponent = new PipingComponent();
                    pipingComponent.ID = shape.ID;
                    PipingComponentAttribution(plantModel, pipingComponent, shape);
                }
                else if (InstrumentSymbolTypes.Contains(shapeReplace))
                {
                    Instrument instrument = new Instrument();
                    instrument.ID = shape.ID;
                    InstrumentAttribution(plantModel, instrument, shape);
                }
                else if (PipeLineTypes.Contains(shapeReplace))
                {
                    PipeLine pipeLine = new PipeLine();
                    pipeLine.ID = shape.ID;
                    PipeLineAttribution(plantModel, pipeLine, shape);
                }
                else if (InstrumentLineTypes.Contains(shapeReplace))
                {
                    SignalLine signalLine = new SignalLine();
                    signalLine.ID = shape.ID;
                    SignalAttribution(plantModel, signalLine, shape);
                }
                else if (Enum.IsDefined(typeof(IConnectorLineType), shapeReplace))
                {
                    Console.WriteLine("connect");
                    //ConnectionPoint connectionPoint = new ConnectionPoint();
                    //ConnectAttribution(connectionPoint, shape);

                    //plantModel.ConnectionPoints.Add(connectionPoint);
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
            equipment.Angle = angle.ObjAngle;
            equipment.Centers = center;
            equipment.Extents = extent;

            ConnectionPoint connectionPoint = new ConnectionPoint();

            ExtractObjectConnectionInformation(shape, connectionPoint);
            equipment.ConnectionPoints.Add(connectionPoint);

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

            ConnectionPoint connectionPoint = new ConnectionPoint();

            ExtractObjectConnectionInformation(shape, connectionPoint);
            pipingComponent.ConnectionPoints.Add(connectionPoint);

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

            ConnectionPoint connectionPoint = new ConnectionPoint();

            ExtractObjectConnectionInformation(shape, connectionPoint);
            instrument.ConnectionPoints.Add(connectionPoint);

            plantModel.Instruments.Add(instrument);
        }

        private void PipeLineAttribution(PlantModel plantModel, PipeLine pipeLine, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ExtractObjectBoxInformation(shape, extent, center, angle);

            pipeLine.Centers = center;
            pipeLine.Extents = extent;

            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (lineItem.X[i] != lineItem.X.Last())
                {
                    pipeLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    pipeLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    pipeLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    pipeLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    plantModel.PipeLines.Add(pipeLine);
                }
            }
        }


        private void SignalAttribution(PlantModel plantModel, SignalLine signalLine, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            Angle angle = new Angle();

            ExtractObjectBoxInformation(shape, extent, center, angle);

            signalLine.Centers = center;
            signalLine.Extents = extent;

            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (lineItem.X[i] != lineItem.X.Last())
                {
                    signalLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    signalLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    signalLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    signalLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    plantModel.SignalLines.Add(signalLine);
                }
            }
        }


        private void ConnectAttribution(ConnectionPoint connectionPoint, Shape shape)
        {
            short iRow = (short)VisRowIndices.visRowConnectionPts;

            var strPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionConnectionPts,
                    iRow,
                    (short)VisCellIndices.visCnnctX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinX = RemoveUnits(strPinX);
            connectionPoint.ConnetionX = pinX;

            string strPinY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionConnectionPts,
                    iRow,
                    (short)VisCellIndices.visCnnctY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinY = RemoveUnits(strPinY);
            connectionPoint.ConnetionY = pinY;
        }

        private void TextAttribution(Text text, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();

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

            Extentmaker(extent, textcenterX, textcenterY, width, height);

            string strAngle = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow2,
                    (short)VisCellIndices.visXFormAngle
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int angle = RemoveUnits(strAngle);
            text.Angle = angle;

            text.Centers = center;
            text.Extents = extent;


            short iRowCnn = (short)VisRowIndices.visRowConnectionPts;

            while (shape.get_CellsSRCExists(
                  (short)VisSectionIndices.visSectionConnectionPts,
                  (short)iRowCnn,
                  (short)VisCellIndices.visCnnctX,
                  (short)0) != 0)
            {
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

                connectionPoint.ConnetionX = cnnPinX;
                connectionPoint.ConnetionY = cnnPinY;
                text.ConnectionPoints.Add(connectionPoint);

                iRowCnn++;
            }

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

            Extentmaker(extent, pinX, pinY, width, height);

            string strAngle = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormAngle
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            angle.ObjAngle = RemoveUnits(strAngle);
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

                if (lineEndPointX > Math.Abs(2.5))
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

                if (lineEndPointY > Math.Abs(2.5))
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

        private void ExtractObjectConnectionInformation(Shape shape, ConnectionPoint connectionPoint)
        {
            short iRowCnn = (short)VisRowIndices.visRowConnectionPts;

            while (shape.get_CellsSRCExists(
                  (short)VisSectionIndices.visSectionConnectionPts,
                  (short)iRowCnn,
                  (short)VisCellIndices.visCnnctX,
                  (short)0) != 0)
            {
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

                connectionPoint.ConnetionX = cnnPinX;
                connectionPoint.ConnetionY = cnnPinY;

                iRowCnn++;
            }
        }

        private Extent Extentmaker(Extent extent, int pinX, int pinY, int width, int height)
        {
            Min min = new Min();
            Max max = new Max();

            var minX = pinX - width * 0.5;
            var minY = pinY - height * 0.5;
            var maxX = pinX + width * 0.5;
            var maxY = pinY + height * 0.5;

            min.X = (int)minX;
            min.Y = (int)minY;
            max.X = (int)maxX;
            max.Y = (int)maxY;

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
            string[] charsToRemove = new string[] { " mm", " 도" };

            foreach (var chars in charsToRemove)
            {
                if (strValue.Contains(chars))
                {
                    strValue = strValue.Replace(chars, string.Empty);
                    double doubleValue = double.Parse(strValue);
                    int intValue = (int)doubleValue;

                    return intValue;
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
            XElement widthElement = new XElement("width", 3536); //paper 읽어 적용할 것
            basicSizeElement.Add(widthElement);
            XElement heightElement = new XElement("height", 2500); //paper 읽어 적용할 것
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

            return xmlDocument;
        }

        //수정 필요 PlantModel 리스트 만들것 (09.04)

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

        public XElement CreateXmlLineStructure1(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.PipeLines[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        public XElement CreateXmlLineStructure2(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.SignalLines[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        public XElement CreateXmlSymbolStructure1(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.Equipments[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        public XElement CreateXmlSymbolStructure2(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.Instruments[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        public XElement CreateXmlSymbolStructure3(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.PipingComponents[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        public XElement CreateXmlSymbolStructure4(XElement xElement, PlantModel plantModel, int i)
        {
            XElement idElement = new XElement("iD", plantModel.Texts[i].ID);
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

            XElement etcElement = new XElement("etc", "none");
            xElement.Add(etcElement);

            return xElement;
        }

        //public XElement CreateXmlSymbolStructure4(XElement xElement, PlantModel plantModel, int i)
        //{
        //    XElement idElement = new XElement("iD", plantModel.Texts[i].ID);
        //    xElement.Add(idElement);

        //    XElement typeElement = new XElement("type", "text");
        //    xElement.Add(typeElement);

        //    XElement textclassElement = new XElement("class", plantModel.Texts[i].Contents);
        //    xElement.Add(textclassElement);

        //    //XElement typeElement = new XElement("type", "unspecified_symbol");
        //    //shapeElement.Add(typeElement);

        //    //if (plantModel.Description[i] == "Insturment")
        //    //{
        //    //    XElement typeElement = new XElement("type", "instrument_symbol");
        //    //    shapeElement.Add(typeElement);
        //    //}
        //    //else if (plantModel.Description[i] == "Equipment")
        //    //{
        //    //    XElement typeElement = new XElement("type", "equipment_symbol");
        //    //    shapeElement.Add(typeElement);
        //    //}
        //    //else if (plantModel.Description[i] == "Valve")
        //    //{
        //    //    XElement typeElement = new XElement("type", "pipe_symbol");
        //    //    shapeElement.Add(typeElement);
        //    //}
        //    //else
        //    //{
        //    //    XElement typeElement = new XElement("type", "unspecified_symbol");
        //    //    shapeElement.Add(typeElement);
        //    //}

        //    //if (plantModel.Text[i] != null)
        //    //{
        //    //    XElement typeElement = new XElement("type", "text");
        //    //    shapeElement.Add(typeElement);

        //    //    XElement textclassElement = new XElement("class", plantModel.Text[i]);
        //    //    shapeElement.Add(textclassElement);
        //    //}

        //    XElement classElement = new XElement("class", "none");
        //    xElement.Add(classElement);

        //    //if (plantModel.ValveType[i] != null)
        //    //{
        //    //    XElement classElement = new XElement("class", plantModel.ValveType[i]);
        //    //    shapeElement.Add(classElement);
        //    //}
        //    //else
        //    //{
        //    //    XElement classElement = new XElement("class", plantModel.ShapeClass[i]);
        //    //    shapeElement.Add(classElement);
        //    //}

        //    XElement extentElement = new XElement("bndbox");
        //    XElement xMinElement = new XElement("xmin", plantModel.Texts[i].Extents.Min.X);
        //    extentElement.Add(xMinElement);
        //    XElement yMinElement = new XElement("ymin", plantModel.Texts[i].Extents.Min.Y);
        //    extentElement.Add(yMinElement);
        //    XElement xMaxElement = new XElement("xmax", plantModel.Texts[i].Extents.Max.X);
        //    extentElement.Add(xMaxElement);
        //    XElement yMaxElement = new XElement("ymax", plantModel.Texts[i].Extents.Max.Y);
        //    extentElement.Add(yMaxElement);
        //    xElement.Add(extentElement);

        //    XElement degreeElement = new XElement("degree", plantModel.Texts[i].Angle);
        //    xElement.Add(degreeElement);

        //    XElement flipElement = new XElement("flip", "n");
        //    xElement.Add(flipElement);

        //    XElement etcElement = new XElement("etc", "none");
        //    xElement.Add(etcElement);

        //    //if (plantModel.symbolObjects[i].ConnectionPoints != null)
        //    //{
        //    //    for (int j = 0; j < plantModel.symbolObjects[i].ConnectionPoints[0].X.Count; j++)
        //    //    {
        //    //        XElement connectionElement = new XElement("connetion");
        //    //        XElement coordinateElement = new XElement("coordinate");
        //    //        //XAttribute xAttribute = new XAttribute("ID", plantModel.ID[k]);
        //    //        //coordinateElement.Add(xAttribute);
        //    //        XElement XElement = new XElement("X", plantModel.symbolObjects[i].ConnectionPoints[0].X[j]);
        //    //        coordinateElement.Add(XElement);
        //    //        XElement YElement = new XElement("Y", plantModel.symbolObjects[i].ConnectionPoints[0].Y[j]);
        //    //        coordinateElement.Add(YElement);
        //    //        connectionElement.Add(coordinateElement);
        //    //        shapeElement.Add(connectionElement);

        //    //    }
        //    //}

        //    return xElement;
        //}

    }
}
