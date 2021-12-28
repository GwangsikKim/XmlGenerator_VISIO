using Visio = Microsoft.Office.Interop.Visio;
using Microsoft.Office.Interop.Visio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using SmartDesign.MathUtil;

namespace SmartDesign.DrawingDataGenerator
{
    class VisioReader
    {
        private readonly string[] EquipmentSymbolTypes =
        {
            "none",
            "Vessel", "Selectablecompressor", "Selectablecompressor1", "Heatexchanger1" ,"Fluidcontacting", "Barrel", "Tank",
            "Centrifugalpump", "Shellandtube", "Condenser", "Column", "Gasholder", "Motordriventurbine", "Filter1", "Filter2"
        };

        private readonly string[] PipeSymbolTypes =
        {
            "none",
            "Flangedvalve","Flanged/bolted", "Gatevalve", "Relief", "Junction", "Globevalve", "Checkvalve", "Poweredvalve", "Reducer",
            "Diaphragmvalve", "Endcaps", "Endcaps2", "Relief(angle)", "Off-SheetLabel3", "Butterflyvalve", "Callout3", "Ballvalve",
            "Screw-downvalve", "Sleevejoint", "Stopcheckvalve", "Buttweld", "Mixingvalve", "Liquidsealopen/closed", "OperatorBox",
            "Generaljoint", "Thermometers", "Indicator/recorder", "Drainsilencer", "Flowmeter",
            "GenericUtility"
        };

        //Valve 값 고민중
        private readonly string[] ValveSymbolTypes =
        {
           "Gatevalve", "Relief", "Diaphragmvalve", "Globevalve", "Checkvalve", "Poweredvalve", "Butterflyvalve", "Ballvalve", "Mixingvalve"
        };

        private readonly string[] InstrumentSymbolTypes =
        {
            "none",
            "Flangedaccesspoint", "Indicator", "CRT", "Diamond", "Refriger-ators",
            "Steamtraced", "Propellermeter", "Computer", 
            "Electricallybonded"
        };

        private readonly string[] PipeLineTypes =
        {
            "MajorPipeline", "MinorPipeline", "MajorPipelineR", "MajorPipelineL", "MinorPipelineR", "MinorPipelineL",
            "CapillaryTube"
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
                    equipment.ClassName = shapeTypeName.ToLower();
                    CreateEquipmentProperties(plantModel, equipment, shape);
                }
                else if (PipeSymbolTypes.Contains(shapeTypeName))
                {
                    PipingComponent pipingComponent = new PipingComponent();
                    pipingComponent.ID = shape.ID.ToString();
                    pipingComponent.ClassName = shapeTypeName.ToLower();
                    CreatePipingComponentProperties(plantModel, pipingComponent, shape);
                }
                else if (InstrumentSymbolTypes.Contains(shapeTypeName))
                {
                    Instrument instrument = new Instrument();
                    instrument.ID = shape.ID.ToString();
                    instrument.ClassName = shapeTypeName.ToLower();
                    CreateInstrumentProperties(plantModel, instrument, shape);
                }
                else if (PipeLineTypes.Contains(shapeTypeName))
                {
                    CreatePipeLineProperties(plantModel, shape, shapeTypeName);
                }
                else if (InstrumentLineTypes.Contains(shapeTypeName))
                {
                    CreateSignalLineProperties(plantModel, shape, shapeTypeName);
                }
                else
                {
                    Console.WriteLine(shape.NameU);
                }
            }
        }

        private void CreateEquipmentProperties(PlantModel plantModel, Equipment equipment, Shape shape)
        {
            equipment.Extent = ExtractObjectBoxInformationSize(shape);
            equipment.Angle = ExtractObjectBoxInformationAngle(shape);

            ExtractObjectConnectionInformation(shape, plantModel, equipment.Extent, equipment.Angle, equipment.ConnectionPoints);

            plantModel.Equipments.Add(equipment);
        }

        private void CreatePipingComponentProperties(PlantModel plantModel, PipingComponent pipingComponent, Shape shape)
        {
            pipingComponent.Extent = ExtractObjectBoxInformationSize(shape);
            pipingComponent.Angle = ExtractObjectBoxInformationAngle(shape);

            ExtractObjectConnectionInformation(shape, plantModel, pipingComponent.Extent, pipingComponent.Angle, pipingComponent.ConnectionPoints);

            plantModel.PipingComponents.Add(pipingComponent);
        }

        private void CreateInstrumentProperties(PlantModel plantModel, Instrument instrument, Shape shape)
        {
            instrument.Extent = ExtractObjectBoxInformationSize(shape);
            instrument.Angle = ExtractObjectBoxInformationAngle(shape);

            ExtractObjectConnectionInformation(shape, plantModel, instrument.Extent, instrument.Angle, instrument.ConnectionPoints);

            plantModel.Instruments.Add(instrument);
        }

        private void CreatePipeLineProperties(PlantModel plantModel, Shape shape, string shapeName)
        {
            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (i < lineItem.X.Count - 1)
                {
                    PipeLine pipeLine = new PipeLine();
                    ConnectionLine connectionLine = new ConnectionLine();
                    pipeLine.ID = shape.ID.ToString() + "." + i;
                    pipeLine.Type = "piping_line";

                    if (PipeLineTypes.Contains(shapeName))
                    {
                        pipeLine.ClassName = "primary";
                    }

                    pipeLine.Extent = ExtractObjectBoxInformationSize(shape);

                    pipeLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    pipeLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    pipeLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    pipeLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    plantModel.PipeLines.Add(pipeLine);

                    connectionLine.ID = pipeLine.ID;

                    connectionLine.LineEndPoints.BeginPoints.BeginX = pipeLine.LineEndPoints.BeginPoints.BeginX;
                    connectionLine.LineEndPoints.BeginPoints.BeginY = pipeLine.LineEndPoints.BeginPoints.BeginY;
                    connectionLine.LineEndPoints.EndPoints.EndX = pipeLine.LineEndPoints.EndPoints.EndX;
                    connectionLine.LineEndPoints.EndPoints.EndY = pipeLine.LineEndPoints.EndPoints.EndY;

                    connectionLine.ObjCenterX = pipeLine.Extent.Center.X;
                    connectionLine.ObjCenterY = pipeLine.Extent.Center.Y;

                    plantModel.ConnectionLines.Add(connectionLine);
                }
            }
        }

        private void CreateSignalLineProperties(PlantModel plantModel, Shape shape, string shapeName)
        {
            LineItem lineItem = new LineItem();

            ExtractLineInformation(shape, lineItem);

            for (int i = 0; i < lineItem.X.Count; i++)
            {
                if (i < lineItem.X.Count - 1)
                {
                    SignalLine signalLine = new SignalLine();
                    ConnectionLine connectionLine = new ConnectionLine();
                    signalLine.ID = shape.ID.ToString() + "." + i;
                    signalLine.Type = "signal_line";

                    if (InstrumentLineTypes.Contains(shapeName))
                    {
                        if (shapeName == "Data")
                        {
                            signalLine.ClassName = "data";
                        }
                        else
                            signalLine.ClassName = "dashed";
                    }

                    signalLine.Extent = ExtractObjectBoxInformationSize(shape);

                    signalLine.LineEndPoints.BeginPoints.BeginX = lineItem.X[i];
                    signalLine.LineEndPoints.BeginPoints.BeginY = lineItem.Y[i];
                    signalLine.LineEndPoints.EndPoints.EndX = lineItem.X[i + 1];
                    signalLine.LineEndPoints.EndPoints.EndY = lineItem.Y[i + 1];

                    plantModel.SignalLines.Add(signalLine);

                    connectionLine.ID = signalLine.ID;

                    connectionLine.LineEndPoints.BeginPoints.BeginX = signalLine.LineEndPoints.BeginPoints.BeginX;
                    connectionLine.LineEndPoints.BeginPoints.BeginY = signalLine.LineEndPoints.BeginPoints.BeginY;
                    connectionLine.LineEndPoints.EndPoints.EndX = signalLine.LineEndPoints.EndPoints.EndX;
                    connectionLine.LineEndPoints.EndPoints.EndY = signalLine.LineEndPoints.EndPoints.EndY;

                    connectionLine.ObjCenterX = signalLine.Extent.Center.X;
                    connectionLine.ObjCenterY = signalLine.Extent.Center.Y;

                    plantModel.ConnectionLines.Add(connectionLine);
                }
            }
        }

        private void CreateTextProperties(PlantModel plantModel, Text text, Shape shape)
        {
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

            var minX = shapePinX - shapeWidth * 0.5;
            var minY = shapePinY - shapeHeight * 0.5;

            int textcenterX = (int)minX + pinX;
            int textcenterY = (int)minY + pinY;

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

            text.Extent = CreateBox(textcenterX, textcenterY, width, height);
            text.Angle = angle;

            //ConnectionLine connectionLine = new ConnectionLine();
            //connectionLine.ID = shape.ID.ToString() + 'T';
            //connectionLine.LineEndPoints.BeginPoints.BeginX = text.Extent.Center.X;
            //connectionLine.LineEndPoints.BeginPoints.BeginY = text.Extent.Center.Y;
            //connectionLine.LineEndPoints.EndPoints.EndX = shapePinX;
            //connectionLine.LineEndPoints.EndPoints.EndY = shapePinY;

            //plantModel.ConnectionLines.Add(connectionLine);
        }

        private Obb2 ExtractObjectBoxInformationSize(Shape shape)
        {
            short iRow = (short)VisRowIndices.visRowXFormOut;

            var strPinX = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormPinX
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinX = RemoveUnits(strPinX);

            string strPinY = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormPinY
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            int pinY = RemoveUnits(strPinY);

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

            var extent = CreateBox(pinX, pinY, width, height);
            return extent;
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

        private void ExtractObjectConnectionInformation(Shape shape, PlantModel plantModel, Obb2 extent, double angle, List<ConnectionPoint> connectionPoints)
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

                if (angle != 0) // 아래 각도별 인식으로 바꾸기(임시로 생성)
                {
                    //double x2 = Math.Cos(radian) * cnnPinX - Math.Sin(radian) * cnnPinX;
                    //double y2 = Math.Sin(radian) * cnnPinY + Math.Cos(radian) * cnnPinY;

                    double pinX = extent.GlobalMin.X + cnnPinY;
                    double pinY = extent.GlobalMin.Y + cnnPinX;

                    connectionPoint.ConnetionX = pinX;
                    connectionPoint.ConnetionY = pinY;
                }
                else
                {
                    connectionPoint.ConnetionX = extent.GlobalMin.X + cnnPinX;
                    connectionPoint.ConnetionY = extent.GlobalMin.Y + cnnPinY;
                }

                connectionPoint.ID = shape.ID.ToString();

                connectionPoint.ObjCenterX = extent.Center.X;
                connectionPoint.ObjCenterY = extent.Center.Y;

                connectionPoints.Add(connectionPoint);
                plantModel.ConnectionPoints.Add(connectionPoint);

                iRowCnn++;
            }
        }

        private double ExtractObjectBoxInformationAngle(Shape shape)
        {
            short iRow = (short)VisRowIndices.visRowXFormOut;

            string strAngle = shape.get_CellsSRC(
                    (short)VisSectionIndices.visSectionObject,
                    iRow,
                    (short)VisCellIndices.visXFormAngle
                    ).get_ResultStr(VisUnitCodes.visNoCast);
            double angle = RemoveUnits(strAngle);

            return angle;
        }

        private Obb2 CreateBox(int pinX, int pinY, int width, int height)
        {
            var minX = pinX - width * 0.5;
            var minY = pinY - height * 0.5;
            var maxX = pinX + width * 0.5;
            var maxY = pinY + height * 0.5;

            Position2 startmin = new Position2(minX, minY);
            Position2 endmax = new Position2(maxX, maxY);
            Obb2 obb2 = Obb2.Create(startmin, endmax);

            return obb2;
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
    }
}
