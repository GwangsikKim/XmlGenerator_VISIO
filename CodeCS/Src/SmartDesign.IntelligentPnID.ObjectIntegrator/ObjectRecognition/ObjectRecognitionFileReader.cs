using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class ObjectRecognitionFileReader
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string[] UnspecifiedSymbolTypes =
        {
            "none",
            "diaphragm_with_positioner", "packaged_system_by_plc_of_single_controller",
            "unspecified_interface", "indirect_drain", "typical_detail"
        };

        private readonly string[] EquipmentSymbolTypes =
        {
            "none",
            "vessel_with_dished_ends", "general_column", "m_shape_heat_exchanger", "straight_tubes_heat_exchanger", "plate_heat_exchanger",
            "general_liquid_filter", "suction_filter",
            "pump_liquid_type_general", "pump_reciprocating_piston_type",
            "general_compressor", "centrifugal_compressor", "reciprocating_compressor",
            "general_blower", "general_turbine", "general_electric_motor",
            
            //VisioTypes
            "Vessel", "Selectablecompressor", "Selectablecompressor1", "Heatexchanger1" ,"Fluidcontacting", "Barrel"
        };

        private readonly string[] PipeSymbolTypes =
        {
            "none",
            "gate_valve", "check_valve", "globe_valve", "butterfly_valve", "ball_valve", "three_way_valve",
            "y_type_strainer", "t_type_strainer",
            "steam_trap",
            "in_line_silencer", "vent_silencer",
            "ejector", "desuperheater", "removable_spool", "flexible_hose", "expansion_joint", "damper",
            "breather", "flange", "cap", "concentric_reducer", "eccentric_reducer",
            "hose_connection", "spacer", "blank", "open_figure_8_blind", "closed_figure_8_blind",
            "plug", "blind_flange", "off_page_connector", "utility_connector", "tie_in_symbol",
            
            //VisioTypes
            "Flangedvalve","Flanged/bolted", "Gatevalve", "Relief", "Junction", "Globevalve", "Checkvalve", "Poweredvalve", "Reducer",
            "Diaphragmvalve", "Endcaps", "Endcaps2", "Relief(angle)", "Off-SheetLabel3", "Butterflyvalve", "Callout3", "Ballvalve",
            "Screw-downvalve","CapillaryTube", "Sleevejoint"


        };

        private readonly string[] ValveSymbolTypes =
        {
            "gate_valve", "check_valve", "globe_valve", "butterfly_valve", "ball_valve", "three_way_valve",

            //VisioTypes
            "Gatevalve", "Relief", "Diaphragmvalve", "Globevalve", "Checkvalve", "Poweredvalve", "Butterflyvalve", "Ballvalve"
        };

        private readonly string[] InstrumentSymbolTypes =
        {
            "none",
            "general_symbol_in_line_element", "in_line_flow_element_with_integral_transmitter", "in_line_flow_element_with_separate_transmitter",
            "field_mounted_discrete_instruments", "field_mounted_dcs", "field_mounted_plc", "field_mounted_interlock", "primary_location_normally_accessible_discrete_instruments",
            "primary_location_normally_accessible_dcs", "primary_location_normally_accessible_plc", "primary_location_normally_inaccessible_discrete_instruments",
            "primary_location_normally_inaccessible_dcs", "primary_location_normally_inaccessible_plc", "auxiliary_location_normally_accessible_discrete_instruments",
            "auxiliary_location_normally_accessible_dcs", "auxiliary_location_normally_accessible_plc", "auxiliary_location_normally_inaccessible_discrete_instruments",
            "auxiliary_location_normally_inaccessible_dcs", "auxiliary_location_normally_inaccessible_plc", "orifice", "manual_operator", "diaphragm", "cylinder",
            "motor_operated", "single_solenoid", "pressure_relief_valve", "vacuum_relief_valve", "pressure_and_vacuum_relief_valve",
            
            //VisioTypes
            "Flangedaccesspoint", "Indicator", "CRT", "Diamond", "GenericUtility", "Generaljoint", "Filter2"
        };

        private readonly string[] UnspecifiedLineTypes =
        {
            "none", "solid", "dashed", "connectLine"
        };

        private readonly string[] PipeLineTypes =
        {
            "none", "primary", "secondary",

            //VisioTypes
            "MajorPipeline", "MinorPipeline", "MajorPipelineR", "MajorPipelineL", "MinorPipelineR", "MinorPipelineL"
        };

        private readonly string[] InstrumentLineTypes =
        {
            "none", "instrument_supply", "pneumatic_signal", "electric_signal",

            //VisioTypes
            "Electric", "Data", "Electric3"
        };

        public ObjectRecognitionFileReader()
        {

        }

        public Annotation Read(string path)
        {
            Annotation annotation = new Annotation();

            XDocument document = XDocument.Load(path, LoadOptions.SetLineInfo);
            XElement annotationElement = document.Root;
            if (annotationElement.Name != "annotation")
                return null;

            XElement basicDrawingInformationElement = annotationElement.Element("basic_drawing_information");
            if (basicDrawingInformationElement == null)
            {
                string position = GetXmlPosition(annotationElement);
                string message = string.Format("<annotation/> 요소가 <basic_drawing_information/> 요소를 가지고 있지 않습니다: 위치 {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            ReadBasicDrawingInformation(basicDrawingInformationElement, annotation.BasicDrawingInformation);

            var symbolObjectList = annotationElement.Elements("symbol_object");
            foreach (var symbolObject in symbolObjectList)
            {
                ReadSymbolObject(symbolObject, annotation.SymbolObjectList);
            }

            var lineObjectList = annotationElement.Elements("line_object");
            foreach (var lineObject in lineObjectList)
            {
                ReadLineObjectList(lineObject, annotation.LineObjectList);
            }

            var connectionObjectList = annotationElement.Elements("connection_object");
            foreach (var connectionObject in connectionObjectList)
            {
                ReadConnectionObjectList(connectionObject, annotation.ConnectionObjectList);
            }

            return annotation;
        }

        private XElement CheckChildElement(XElement parentElement, string childElementName)
        {
            XElement childElement = parentElement.Element(childElementName);
            if (childElement == null)
            {
                //string position = GetXmlPosition(parentElement);
                //string message = string.Format("<{0}/> 요소가 <{1}/> 요소를 가지고 있지 않습니다: 위치 {2}", parentElement.Name, childElementName, position);

                //if (log.IsErrorEnabled)
                //    log.Error(message);

                //throw new ArgumentNullException(message);
            }

            return childElement;
        }

        private string CheckNonEmptyValue(XElement element)
        {
            Debug.Assert(element != null);

            string typeValue = element.Value.Trim().ToLower();
            if (string.IsNullOrEmpty(typeValue))
            {
                string position = GetXmlPosition(element);
                string message = string.Format("<{0}/> 요소가 비어 있습니다: {1}", element.Name, position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            return typeValue;
        }

        //basicDrawingInformation의 값을 읽기 및 저장

        private void ReadBasicDrawingInformation(XElement basicDrawingInformationElement, BasicDrawingInformation basicDrawingInformation)
        {
            XElement filenameElement = CheckChildElement(basicDrawingInformationElement, "filename");
            string filename = filenameElement.Value;
            basicDrawingInformation.FileName = filename;

            XElement pathElement = CheckChildElement(basicDrawingInformationElement, "path");
            string path = pathElement.Value;
            basicDrawingInformation.Path = path;

            XElement sizeElement = CheckChildElement(basicDrawingInformationElement, "size");
            ReadSize(sizeElement, basicDrawingInformation.Size);

            XElement externalBorderLineElement = CheckChildElement(basicDrawingInformationElement, "external_border_line");
            XElement externalBorderLineBoundingBoxElement = CheckChildElement(externalBorderLineElement, "bndbox");
            ReadBoundingBoxElement(externalBorderLineBoundingBoxElement, basicDrawingInformation.ExternalBorderLine);

            XElement pureDrawingAreaElement = CheckChildElement(basicDrawingInformationElement, "pure_drawing_area");
            XElement pureDrawingAreaBoundingBoxElement = CheckChildElement(pureDrawingAreaElement, "bndbox");
            ReadBoundingBoxElement(pureDrawingAreaBoundingBoxElement, basicDrawingInformation.PureDrawingArea);

            XElement noteAreaElement = CheckChildElement(basicDrawingInformationElement, "note_area");
            XElement noteAreaBoundingBoxElement = CheckChildElement(noteAreaElement, "bndbox");
            ReadBoundingBoxElement(noteAreaBoundingBoxElement, basicDrawingInformation.NoteArea);

            XElement titleAreaElement = CheckChildElement(basicDrawingInformationElement, "title_area");
            XElement titleAreaBoundingBoxElement = CheckChildElement(titleAreaElement, "bndbox");
            ReadBoundingBoxElement(titleAreaBoundingBoxElement, basicDrawingInformation.TitleArea);

            XElement drawingAreaSeparatorElement = CheckChildElement(basicDrawingInformationElement, "drawing_area_separator");
            XElement edgeElement = CheckChildElement(drawingAreaSeparatorElement, "edge");
            ReadEdgeElement(edgeElement, basicDrawingInformation.DrawingAreaSeparator);
        }

        //SymbolObject 값을 읽기 및 저장
        private void ReadSymbolObject(XElement symbolObjectElement, List<SymbolObject> symbolObjectList)
        {
            SymbolObject symbolObject = new SymbolObject();

            XElement idElement = symbolObjectElement.Element("id");
            if (idElement != null)
            {
                string id = idElement.Value.Trim();
                symbolObject.Id = id;
            }

            // Type
            XElement typeElement = CheckChildElement(symbolObjectElement, "type");
            string typeValue = CheckNonEmptyValue(typeElement);

            if (ParseSymbolObjectType(typeValue, out SymbolObjectType symbolObjectType))
            {
                symbolObject.Type = symbolObjectType;
            }
            else
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<type/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }
            //

            //Class           
            XElement classElement = CheckChildElement(symbolObjectElement, "class");
            string className = classElement.Value.Trim();

            if ((symbolObjectType != SymbolObjectType.UnspecifiedSymbol && symbolObjectType != SymbolObjectType.Text) && string.IsNullOrEmpty(className))
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<class/> 요소가 비어 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            if (CheckSymbolClassName(symbolObjectType, className))
            {
                if (symbolObjectType != SymbolObjectType.Text)
                    symbolObject.ClassName = className.ToLower();
                else
                    symbolObject.ClassName = className;
            }
            else
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<class/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }
            //

            //Degree
            XElement degreeElement = CheckChildElement(symbolObjectElement, "degree");
            string strDegree = degreeElement.Value.Trim();
            double degree = Convert.ToDouble(strDegree);
            symbolObject.Rotation = Angle.FromDegree(degree);
            //

            //Flip
            XElement flipElement = CheckChildElement(symbolObjectElement, "flip");
            string strFlip = CheckNonEmptyValue(flipElement);

            if (strFlip == "y")
                symbolObject.Flip = true;
            else if (strFlip == "n")
                symbolObject.Flip = false;
            else
            {
                string message = "<flip/> 요소의 값은 y 또는 n이어야 합니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new Exception(message);
            }
            //

            //Bndbox
            XElement boundingBoxElement = CheckChildElement(symbolObjectElement, "bndbox");
            ReadBoundingBoxElement(boundingBoxElement, symbolObject.BoundingBox);
            //

            // 불필요한 심볼은 추가하지 않는다.
            if (symbolObject.Type == SymbolObjectType.Text && string.IsNullOrEmpty(symbolObject.ClassName))
            {
                string position = GetXmlPosition(symbolObjectElement);
                string message = string.Format("텍스트에 값이 비어있어 추가하지 않습니다: {0}", position);

                if (log.IsInfoEnabled)
                    log.Info(message);

                return;
            }

            symbolObjectList.Add(symbolObject);
        }

        //LineObject값을 읽기 및 저장
        private void ReadLineObjectList(XElement lineObjectElement, List<LineObject> lineObjectList)
        {
            LineObject lineObject = new LineObject();

            XElement idElement = lineObjectElement.Element("id");
            if (idElement != null)
            {
                string id = idElement.Value.Trim();
                lineObject.Id = id;
            }

            // Type
            XElement typeElement = CheckChildElement(lineObjectElement, "type");
            string typeValue = CheckNonEmptyValue(typeElement);

            if (ParseLineObjectType(typeValue, out LineObjectType lineObjectType))
            {
                lineObject.Type = lineObjectType;
            }
            else
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<type/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                //if (log.IsErrorEnabled)
                //    log.Error(message);

                //throw new ArgumentNullException(message);
            }
            //

            //Class
            XElement classElement = CheckChildElement(lineObjectElement, "class");
            string className = classElement.Value.Trim();

            if (lineObjectType != LineObjectType.UnspecifiedLine && string.IsNullOrEmpty(className))
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<class/> 요소가 비어 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            if (CheckLineClassName(lineObjectType, className))
            {
                lineObject.ClassName = className;
            }
            else if (lineObjectType == LineObjectType.UnspecifiedLine && string.IsNullOrEmpty(className))
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<class/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                if (log.IsInfoEnabled)
                    log.Info(message);

                lineObject.ClassName = LineObject.ClassNameDefault;
            }
            else
            {
                string position = GetXmlPosition(typeElement);
                string message = string.Format("<class/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                //if (log.IsErrorEnabled)
                //    log.Error(message);

                //throw new ArgumentNullException(message);
            }

            //Edge
            if (className == "connection")
            {
                XElement edgeElement = CheckChildElement(lineObjectElement, "edge");
                if (edgeElement != null)
                {
                    ReadCnnEdgeElement(edgeElement, lineObject.Edge);
                }
            }
            else
            {
                XElement edgeElement = CheckChildElement(lineObjectElement, "edge");
                ReadEdgeElement(edgeElement, lineObject.Edge);
            }

            //EndType
            XElement endTypeElement = CheckChildElement(lineObjectElement, "endtype");

            XElement startEndTypeElement = CheckChildElement(endTypeElement, "start");
            string startEndTypeValue = CheckNonEmptyValue(startEndTypeElement);

            if (ParseLineEndType(startEndTypeValue, out LineEndShape startLineEndShape))
            {
                lineObject.StartEndType = startLineEndShape;
            }
            else
            {
                string position = GetXmlPosition(startEndTypeElement);
                string message = string.Format("<start/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            XElement endEndTypeElement = CheckChildElement(endTypeElement, "end");
            string endEndTypeValue = CheckNonEmptyValue(endEndTypeElement);

            if (ParseLineEndType(endEndTypeValue, out LineEndShape endLineEndShape))
            {
                lineObject.EndEndType = endLineEndShape;
            }
            else
            {
                string position = GetXmlPosition(endEndTypeElement);
                string message = string.Format("<end/> 요소에 허용되지 않는 값이 있습니다: {0}", position);

                if (log.IsErrorEnabled)
                    log.Error(message);

                throw new ArgumentNullException(message);
            }

            if (!ExistsLine(lineObjectList, lineObject))
                lineObjectList.Add(lineObject);
        }

        private void ReadConnectionObjectList(XElement connectionObjectElement, List<ConnectionObject> connectionObjectList)
        {
            ConnectionObject connectionObject = new ConnectionObject();

            XElement idElement = connectionObjectElement.Element("id");
            if (idElement != null)
            {
                string id = idElement.Value.Trim();
                connectionObject.Id = id;
            }

            //ConnectionPoint
            XElement connectionPointElement = CheckChildElement(connectionObjectElement, "connection_point");
            XElement pointXElement = CheckChildElement(connectionPointElement, "x");
            string pointXValue = CheckNonEmptyValue(pointXElement);
            connectionObject.X = pointXValue;

            XElement pointYElement = CheckChildElement(connectionPointElement, "y");
            string pointYElementValue = CheckNonEmptyValue(pointYElement);
            connectionObject.Y = pointYElementValue;

            //From To informaiton
            XElement connectionElement = connectionObjectElement.Element("connection");
            if (connectionElement != null)
            {
                XAttribute fromAttribute = connectionElement.Attribute("From");
                connectionObject.From = fromAttribute.Value.ToString();

                XAttribute toAttribute = connectionElement.Attribute("To");
                connectionObject.To = toAttribute.Value.ToString();
            }

            connectionObjectList.Add(connectionObject);
        }



        private bool ExistsLine(List<LineObject> lineObjectList, LineObject lineObject)
        {
            foreach (var existingLine in lineObjectList)
            {
                if (existingLine.Edge.XStart == lineObject.Edge.XStart &&
                    existingLine.Edge.YStart == lineObject.Edge.YStart &&
                    existingLine.Edge.XEnd == lineObject.Edge.XEnd &&
                    existingLine.Edge.YEnd == lineObject.Edge.YEnd)
                {
                    return true;
                }
                else if (existingLine.Edge.XStart == lineObject.Edge.XEnd &&
                    existingLine.Edge.YStart == lineObject.Edge.YEnd &&
                    existingLine.Edge.XEnd == lineObject.Edge.XStart &&
                    existingLine.Edge.YEnd == lineObject.Edge.YStart)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ParseSymbolObjectType(string typeValue, out SymbolObjectType objectType)
        {
            bool result = true;

            if (typeValue == "equipment_symbol")
                objectType = SymbolObjectType.EquipmentSymbol;
            else if (typeValue == "pipe_symbol")
                objectType = SymbolObjectType.PipeSymbol;
            else if (typeValue == "instrument_symbol")
                objectType = SymbolObjectType.InstrumentSymbol;
            else if (typeValue == "text")
                objectType = SymbolObjectType.Text;
            else if (typeValue == "unspecified_symbol")
                objectType = SymbolObjectType.UnspecifiedSymbol;
            else
            {
                objectType = SymbolObjectType.UnspecifiedSymbol;
                result = false;
            }

            return result;
        }

        private bool CheckSymbolClassName(SymbolObjectType symbolType, string className)
        {
            if (symbolType == SymbolObjectType.UnspecifiedSymbol)
            {
                if (string.IsNullOrEmpty(className))
                    return true;
                else
                    return UnspecifiedSymbolTypes.Contains(className);
            }
            else if (symbolType == SymbolObjectType.EquipmentSymbol)
            {
                return EquipmentSymbolTypes.Contains(className);
            }
            else if (symbolType == SymbolObjectType.InstrumentSymbol)
            {
                return InstrumentSymbolTypes.Contains(className);
            }
            else if (symbolType == SymbolObjectType.PipeSymbol)
            {
                return PipeSymbolTypes.Contains(className);
            }
            else if (symbolType == SymbolObjectType.Text)
                return true;

            return false;
        }

        private bool ParseLineObjectType(string typeValue, out LineObjectType objectType)
        {
            bool result = true;

            if (typeValue == "piping_line")
                objectType = LineObjectType.PipeLine;
            else if (typeValue == "signal_line")
                objectType = LineObjectType.InstrumentLine;
            else if (typeValue == "unspecified_line")
                objectType = LineObjectType.UnspecifiedLine;
            else
            {
                objectType = LineObjectType.UnspecifiedLine;
                result = false;
            }

            return result;
        }

        private bool CheckLineClassName(LineObjectType symbolType, string className)
        {
            if (symbolType == LineObjectType.UnspecifiedLine)
            {
                return UnspecifiedLineTypes.Contains(className);
            }
            else if (symbolType == LineObjectType.PipeLine)
            {
                return PipeLineTypes.Contains(className);
            }
            else if (symbolType == LineObjectType.InstrumentLine)
            {
                return InstrumentLineTypes.Contains(className);
            }

            return false;
        }

        private bool ParseLineEndType(string strEndType, out LineEndShape endShape)
        {
            bool result = true;
            endShape = LineEndShape.None;

            if (strEndType == "none")
                endShape = LineEndShape.None;
            else if (strEndType == "arrow")
                endShape = LineEndShape.Arrow;
            else
                result = false;

            return result;
        }

        //ReadSize
        private void ReadSize(XElement sizeElement, Size size)
        {
            XElement widthElement = CheckChildElement(sizeElement, "width");
            string strSizeWidth = widthElement.Value;
            int sizeWidth = Convert.ToInt32(strSizeWidth);
            size.Width = sizeWidth;

            XElement heightElement = CheckChildElement(sizeElement, "height");
            string strSizeHeight = heightElement.Value;
            int sizeHeight = Convert.ToInt32(strSizeHeight);
            size.Height = sizeHeight;

            XElement depthElement = CheckChildElement(sizeElement, "depth");
            string strSizeDepth = depthElement.Value;
            int sizeDepth = Convert.ToInt32(strSizeDepth);
            size.Depth = sizeDepth;
        }

        //ReadBoundingBox
        private void ReadBoundingBoxElement(XElement boundingBoxElement, BoundingBox boundingBox)
        {
            XElement xminElement = CheckChildElement(boundingBoxElement, "xmin");
            string strXMin = xminElement.Value;
            int xMin = Convert.ToInt32(strXMin);
            boundingBox.XMin = xMin;

            XElement yminElement = CheckChildElement(boundingBoxElement, "ymin");
            string strYMin = yminElement.Value;
            int yMin = Convert.ToInt32(strYMin);
            boundingBox.YMin = yMin;

            XElement xmaxElement = CheckChildElement(boundingBoxElement, "xmax");
            string strXMax = xmaxElement.Value;
            int xMax = Convert.ToInt32(strXMax);
            boundingBox.XMax = xMax;

            XElement ymaxElement = CheckChildElement(boundingBoxElement, "ymax");
            string strYMax = ymaxElement.Value;
            int yMax = Convert.ToInt32(strYMax);
            boundingBox.YMax = yMax;
        }

        //ReadEdge
        private void ReadEdgeElement(XElement edgeElement, Edge edge)
        {
            XElement xstartElement = CheckChildElement(edgeElement, "xstart");
            string strXStart = xstartElement.Value;
            int xStart = Convert.ToInt32(strXStart);
            edge.XStart = xStart;

            XElement ystartElement = CheckChildElement(edgeElement, "ystart");
            string strYStart = ystartElement.Value;
            int yStart = Convert.ToInt32(strYStart);
            edge.YStart = yStart;

            XElement xendElement = CheckChildElement(edgeElement, "xend");
            string strXEnd = xendElement.Value;
            int xEnd = Convert.ToInt32(strXEnd);
            edge.XEnd = xEnd;

            XElement yendElement = CheckChildElement(edgeElement, "yend");
            string strYEnd = yendElement.Value;
            int yEnd = Convert.ToInt32(strYEnd);
            edge.YEnd = yEnd;
        }

        private void ReadCnnEdgeElement(XElement edgeElement, Edge edge)
        {
            XElement xstartElement = CheckChildElement(edgeElement, "xstart");
            string strXStart = xstartElement.Value;
            int xStart = Convert.ToInt32(strXStart);
            edge.XStart = xStart;




            XElement ystartElement = CheckChildElement(edgeElement, "ystart");
            string strYStart = ystartElement.Value;
            int yStart = Convert.ToInt32(strYStart);
            edge.YStart = yStart;

            XElement xendElement = CheckChildElement(edgeElement, "xend");
            string strXEnd = xendElement.Value;
            int xEnd = Convert.ToInt32(strXEnd);
            edge.XEnd = xEnd;

            XElement yendElement = CheckChildElement(edgeElement, "yend");
            string strYEnd = yendElement.Value;
            int yEnd = Convert.ToInt32(strYEnd);
            edge.YEnd = yEnd;
        }

        private static string GetXmlPosition(IXmlLineInfo lineInfo)
        {
            if (lineInfo.HasLineInfo())
            {
                string position = string.Format("{0}줄 {0}번째 위치", lineInfo.LineNumber, lineInfo.LinePosition);
                return position;
            }

            return string.Empty;
        }
    }
}
