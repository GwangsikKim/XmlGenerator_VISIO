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
using SmartDesign.DrawingDataGenerator;
using System.Text.RegularExpressions;
using SmartDesign.MathUtil;

namespace SmartDesign.DrawingDataGenerator
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


        public VisioReader(string path)
        {
            this.path = path;
        }

        public XDocument GenerateXML()
        {
            Visio.Application app = new Visio.Application();
            Visio.Document visioDoc = app.Documents.Add(path);
            Visio.Page visioPage = app.ActivePage;

            PlantModel plantModel = new PlantModel();
            ClassifyPlantType(plantModel, visioPage);

            XMLConverter xMLConverter = new XMLConverter();
            var xmlData = xMLConverter.ConvertToXML(plantModel, path);

            return xmlData;
        }

        public void ClassifyPlantType(PlantModel plantModel, Visio.Page visioPage)
        {
            foreach (Shape shape in visioPage.Shapes)
            {
                string shapeDeleteSpace = Regex.Replace(shape.NameU, @"\s", "");

                string shapeTypeName = RemoveSpecificCharacters(shapeDeleteSpace);

                if (shape.Text != null && shape.Text != "" && shape.Text != "￼") //string.IsNullOrEmpty(shape.Text))//
                {
                    Text text = new Text();
                    text.ID = shape.ID.ToString();
                    text.Contents = shape.Text;
                    CreateTextProperties(plantModel, text, shape);

                    plantModel.Texts.Add(text);
                }

                if (EquipmentSymbolTypes.Contains(shapeTypeName))
                {
                    Equipment equipment = new Equipment();
                    equipment.ID = shape.ID.ToString();
                    CreateEquipmentProperties(plantModel, equipment, shape);
                }
                else if (PipeSymbolTypes.Contains(shapeTypeName))
                {
                    PipingComponent pipingComponent = new PipingComponent();
                    pipingComponent.ID = shape.ID.ToString();
                    CreatePipingComponentProperties(plantModel, pipingComponent, shape);
                }
                else if (InstrumentSymbolTypes.Contains(shapeTypeName))
                {
                    Instrument instrument = new Instrument();
                    instrument.ID = shape.ID.ToString();
                    CreateInstrumentProperties(plantModel, instrument, shape);
                }
                else if (PipeLineTypes.Contains(shapeTypeName))
                {
                    CreatePipeLineProperties(plantModel, shape);
                }
                else if (InstrumentLineTypes.Contains(shapeTypeName))
                {
                    CreateSignalLineProperties(plantModel, shape);
                }
                else
                {
                    Console.WriteLine(shape.NameU);
                }
            }
        }

        private void CreateEquipmentProperties(PlantModel plantModel, Equipment equipment, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            int angle = 0;

            ExtractObjectBoxInformation(shape, extent, center, angle);
            equipment.Centers = center;
            equipment.Extents = extent;
            equipment.Angle = angle;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, equipment.ConnectionPoints);

            plantModel.Equipments.Add(equipment);
        }

        private void CreatePipingComponentProperties(PlantModel plantModel, PipingComponent pipingComponent, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            int angle = 0;

            ExtractObjectBoxInformation(shape, extent, center, angle);
            pipingComponent.Angle = angle;
            pipingComponent.Centers = center;
            pipingComponent.Extents = extent;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, pipingComponent.ConnectionPoints);

            plantModel.PipingComponents.Add(pipingComponent);
        }

        private void CreateInstrumentProperties(PlantModel plantModel, Instrument instrument, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            int angle = 0;

            ExtractObjectBoxInformation(shape, extent, center, angle);
            instrument.Angle = angle;
            instrument.Centers = center;
            instrument.Extents = extent;

            ExtractObjectConnectionInformation(shape, plantModel, extent, center, angle, instrument.ConnectionPoints);

            plantModel.Instruments.Add(instrument);
        }

        private void CreatePipeLineProperties(PlantModel plantModel, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            int angle = 0;

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

        private void CreateSignalLineProperties(PlantModel plantModel, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();
            int angle = 0;

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
                }
            }
        }

        private void CreateTextProperties(PlantModel plantModel, Text text, Shape shape)
        {
            Extent extent = new Extent();
            Center center = new Center();

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
            int angle = RemoveUnits(strAngle);

            CreateBox(extent, textcenterX, textcenterY, width, height, angle);

            text.Centers = center;
            text.Extents = extent;
            text.Angle = angle;

            ConnectionLine connectionLine = new ConnectionLine();
            connectionLine.ID = shape.ID.ToString() + 'T';
            connectionLine.LineEndPoints.BeginPoints.BeginX = center.PinX;
            connectionLine.LineEndPoints.BeginPoints.BeginY = center.PinY;
            connectionLine.LineEndPoints.EndPoints.EndX = shapePinX;
            connectionLine.LineEndPoints.EndPoints.EndY = shapePinY;

            plantModel.ConnectionLines.Add(connectionLine);
        }

        private void ExtractObjectBoxInformation(Shape shape, Extent extent, Center center, int angle)
        {
           // Obb2 obb2 = new Obb2();


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
            angle = RemoveUnits(strAngle);

            CreateBox(extent, pinX, pinY, width, height, angle);
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

        private void ExtractObjectConnectionInformation(Shape shape, PlantModel plantModel, Extent extent, Center center, int angle, List<ConnectionPoint> connectionPoints)
        {
            short iRowCnn = (short)VisRowIndices.visRowConnectionPts;
            double radian = Math.PI * Convert.ToDouble(angle) / 180.0;

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

                if (angle == 90) // 아래 각도별 인식으로 바꾸기(임시로 생성)
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

        private Extent CreateBox(Extent extent, int pinX, int pinY, int width, int height, int angle)
        {
            Min min = new Min();
            Max max = new Max();

            if (angle == 90 || angle == -90)
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
            int index = shapeDeleteSpace.IndexOf('.');

            if (index > 0)
            {
                string shapeReplace = shapeDeleteSpace.Substring(0, index);
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
            string[] charsToRemove = new string[] { " mm", " 도", " pt"};

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
    }
}
