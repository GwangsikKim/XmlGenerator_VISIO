using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class XmlFileReader
    {
        private readonly Dictionary<PipingComponent, string> AssociationPairs = new Dictionary<PipingComponent, string>();

        public PlantModel Read(string path)
        {
            AssociationPairs.Clear();

            XDocument document = XDocument.Load(path, LoadOptions.SetLineInfo);
            XElement plantModelElement = document.Root;
            if (plantModelElement.Name != "PlantModel")
                throw new Exception("파일은 <PlantModel>로 시작해야 합니다.");

            PlantModel plantModel = new PlantModel();
            
            Obb2 extent = ReadObb2Element(plantModelElement.Element("Extent"));
            plantModel.Extent = extent;

            XAttribute idAttribute = plantModelElement.Attribute("ID");
            if (idAttribute != null)
            {
                plantModel.ID = idAttribute.Value;
            }            

            List<string> childElementNames = new List<string>() 
            { 
                "Equipment", "Instrument", "PipingComponent", "PipeConnectorSymbol", "SignalConnectorSymbol", "PipingNetworkSystem", "PipingNetworkSegment",
                "SignalLine", "Text", "PipeTee", "PipeCross", "SignalBranch", "UnknownSymbol", "UnknownLine"
            };

            var plantEntityElements = plantModelElement.Elements().Where(x => childElementNames.Contains(x.Name.ToString()));

            foreach(var plantEntityElement in plantEntityElements)
            {
                PlantEntity plantEntity = ReadPlantEntityElement(plantEntityElement);
                if(plantEntity != null)
                    plantModel.Add(plantEntity);
            }

            MakeUpAssociations(plantModel);
            //MakeUpLineConnections(plantModel);

            return plantModel;
        }        

        private PlantEntity ReadPlantEntityElement(XElement plantEntityElement)
        {
            PlantEntity plantEntity = null;

            switch(plantEntityElement.Name.ToString())
            {
                case "Text":
                    plantEntity = ReadTextElement(plantEntityElement);
                    break;
                case "Equipment":
                    plantEntity = ReadEquipmentElement(plantEntityElement);
                    break;
                case "Instrument":
                    plantEntity = ReadInstrumentElement(plantEntityElement);
                    break;
                case "PipingComponent":
                    plantEntity = ReadPipingComponentElement(plantEntityElement);
                    break;
                case "PipeConnectorSymbol":
                    plantEntity = ReadPipeConnectorSymbolElement(plantEntityElement);
                    break;
                case "SignalConnectorSymbol":
                    plantEntity = ReadSignalConnectorSymbolElement(plantEntityElement);
                    break;
                case "SignalLine":
                    plantEntity = ReadSignalLineElement(plantEntityElement);
                    break;
                case "PipingNetworkSegment":
                    plantEntity = ReadPipingNetworkSegmentElement(plantEntityElement);
                    break;            
                case "PipeTee":
                    plantEntity = ReadPipeTeeElement(plantEntityElement);
                    break;
                case "PipeCross":
                    plantEntity = ReadPipeCrossElement(plantEntityElement);
                    break;
                case "SignalBranch":
                    plantEntity = ReadSignalBranchElement(plantEntityElement);
                    break;
                case "UnknownSymbol":
                    plantEntity = ReadUnknownSymbolElement(plantEntityElement);
                    break;
                case "UnknownLine":
                    plantEntity = ReadUnknownLineElement(plantEntityElement);
                    break;
                default:
                    {
                        string message = string.Format("<{0}>는 지원하지 않는 요소 타입니다.", plantEntityElement.Name);
                        throw new Exception(message);
                    }                    
            }

            return plantEntity;
        }

        private PlantEntity ReadPipeTeeElement(XElement pipeTeeElement)
        {
            Debug.Assert(pipeTeeElement.Name.ToString() == "PipeTee");

            PipeTee pipeTee = new PipeTee();
            FillPlantItem(pipeTee, pipeTeeElement);
            FillNodes(pipeTee, pipeTeeElement);
            if (pipeTee.Nodes.Count != 3)
            {
                string position = GetXmlPosition(pipeTeeElement);
                string message = string.Format("<{0}> 요소는 <Node> 요소를 3개 가져야 합니다. 위치: {1}", pipeTeeElement.Name, position);
                throw new Exception(message);
            }

            return pipeTee;
        }

        private PlantEntity ReadPipeCrossElement(XElement pipeCrossElement)
        {
            Debug.Assert(pipeCrossElement.Name.ToString() == "PipeCross");

            PipeCross pipeCross = new PipeCross();
            FillPlantItem(pipeCross, pipeCrossElement);
            FillNodes(pipeCross, pipeCrossElement);
            if (pipeCross.Nodes.Count != 4)
            {
                string position = GetXmlPosition(pipeCrossElement);
                string message = string.Format("<{0}> 요소는 <Node> 요소를 4개 가져야 합니다. 위치: {1}", pipeCrossElement.Name, position);
                throw new Exception(message);
            }

            return pipeCross;
        }

        private PlantEntity ReadSignalBranchElement(XElement signalBranchElement)
        {
            Debug.Assert(signalBranchElement.Name.ToString() == "SignalBranch");

            SignalBranch signalBranch = new SignalBranch();
            FillPlantItem(signalBranch, signalBranchElement);
            FillNodes(signalBranch, signalBranchElement);
            if (signalBranch.Nodes.Count <= 2)
            {
                string position = GetXmlPosition(signalBranchElement);
                string message = string.Format("<{0}> 요소는 <Node> 요소를 3개 이상 가져야 합니다. 위치: {1}", signalBranchElement.Name, position);
                throw new Exception(message);
            }

            return signalBranch;
        }

        private Nozzle ReadNozzleElement(XElement nozzleElement)
        {
            Debug.Assert(nozzleElement.Name.ToString() == "Nozzle");

            Nozzle nozzle = new Nozzle();
            FillPlantItem(nozzle, nozzleElement);
            FillNodes(nozzle, nozzleElement);

            return nozzle;
        }

        private PlantEntity ReadEquipmentElement(XElement equipmentElement)
        {
            Debug.Assert(equipmentElement.Name.ToString() == "Equipment");

            Equipment equipment = new Equipment();
            FillPlantItem(equipment, equipmentElement);
            FillNozzleElements(equipment, equipmentElement);

            return equipment;
        }

        private void FillNozzleElements(Equipment equipment, XElement equipmentElement)
        {
            var nozzleElements = equipmentElement.Elements("Nozzle");

            foreach (var nozzleElement in nozzleElements)
            {
                var nozzle = ReadNozzleElement(nozzleElement);
                equipment.Add(nozzle);
            }
        }

        private PlantEntity ReadUnknownLineElement(XElement unknownLineElement)
        {
            Debug.Assert(unknownLineElement.Name.ToString() == "UnknownLine");

            UnknownLine unknownLine = new UnknownLine();
            FillLineItem(unknownLine, unknownLineElement);

            return unknownLine;
        }

        private PlantEntity ReadPipingNetworkSegmentElement(XElement pipingNetworkSegmentElement)
        {
            Debug.Assert(pipingNetworkSegmentElement.Name.ToString() == "PipingNetworkSegment");

            PipingNetworkSegment pipingNetworkSegment = new PipingNetworkSegment();
            FillLineItem(pipingNetworkSegment, pipingNetworkSegmentElement);

            return pipingNetworkSegment;
        }

        private PlantEntity ReadSignalLineElement(XElement signalLineElement)
        {
            Debug.Assert(signalLineElement.Name.ToString() == "SignalLine");

            SignalLine signalLine = new SignalLine();
            FillLineItem(signalLine, signalLineElement);

            return signalLine;
        }

        private void FillLineItem(LineItem lineItem, XElement lineItemElement)
        {
            XAttribute idAttribute = lineItemElement.Attribute("ID");
            if (idAttribute == null)
            {
                string position = GetXmlPosition(lineItemElement);
                string message = string.Format("<{0}> 요소는 ID 속성을 가져야 합니다. 위치: {1}", lineItemElement.Name, position);
                throw new Exception(message);
            }
            lineItem.ID = idAttribute.Value;
            lineItem.DisplayName = idAttribute.Value;

            XAttribute componentClassAttribute = lineItemElement.Attribute("ComponentClass");
            if (componentClassAttribute != null)
                lineItem.ComponentClass = componentClassAttribute.Value;

            XAttribute tagNameAttribute = lineItemElement.Attribute("TagName");
            if (tagNameAttribute != null)
                lineItem.TagName = tagNameAttribute.Value;

            XElement centerLineElement = lineItemElement.Element("CenterLine");
            if(centerLineElement == null)
            {
                string position = GetXmlPosition(lineItemElement);
                string message = string.Format("<{0}> 요소는 <CenterLine> 요소를 가져야 합니다. 위치: {1}", lineItemElement.Name, position);
                throw new Exception(message);
            }

            CenterLine centerLine = ReadCenterLineElement(centerLineElement);
            lineItem.CenterLine = centerLine;

            XElement connectionElement = lineItemElement.Element("Connection");
            if (connectionElement == null)
            {
                string position = GetXmlPosition(lineItemElement);
                string message = string.Format("<{0}> 요소는 <Connection> 요소를 가져야 합니다. 위치: {1}", lineItemElement.Name, position);
                throw new Exception(message);
            }

            //Connection connection = ReadConnectionElement(connectionElement);
            //lineItem.Connection = connection;

            Obb2 extent = Obb2.Create(centerLine.Start, centerLine.End);
            lineItem.Extent = extent;
            lineItem.ExpandedExtent = extent;

            XElement startShapeElement = lineItemElement.Element("StartShape");
            if (startShapeElement != null)
            {
                string stringStartShape = startShapeElement.Value;
                LineEndShape startShape = (LineEndShape)Enum.Parse(typeof(LineEndShape), stringStartShape);
                lineItem.StartShape = startShape;
            }

            XElement endShapeElement = lineItemElement.Element("EndShape");
            if (endShapeElement != null)
            {
                string stringEndShape = endShapeElement.Value;
                LineEndShape endShape = (LineEndShape)Enum.Parse(typeof(LineEndShape), stringEndShape);
                lineItem.EndShape = endShape;
            }

            var textElements = lineItemElement.Elements("Text");
            foreach(var textElement in textElements)
            {
                var text = ReadTextElement(textElement);
                lineItem.Add(text);
            }
        }

        private CenterLine ReadCenterLineElement(XElement centerLineElement)
        {
            CenterLine centerLine = new CenterLine();
            centerLine.Coordinates.Clear();

            var coordinateElements = centerLineElement.Elements("Coordinate");
            if(coordinateElements.Count() < 2)
            {
                string position = GetXmlPosition(centerLineElement);
                string message = string.Format("<{0}> 요소는 2개 이상의 <Coordinate> 요소를 가져야 합니다. 위치: {1}", centerLineElement.Name, position);
                throw new Exception(message);
            }

            foreach(var coordinateElement in coordinateElements)
            {
                Position2 coordinate = ReadPosition2ElementFromAttributes(coordinateElement);
                centerLine.Coordinates.Add(coordinate);
            }

            return centerLine;
        }

        private PlantEntity ReadInstrumentElement(XElement instrumentElement)
        {
            Debug.Assert(instrumentElement.Name.ToString() == "Instrument");

            Instrument instrument = new Instrument();
            FillPlantItem(instrument, instrumentElement);
            FillNodes(instrument, instrumentElement);            

            return instrument;
        }

        private PlantEntity ReadPipeConnectorSymbolElement(XElement pipeConnectorSymbolElement)
        {
            Debug.Assert(pipeConnectorSymbolElement.Name.ToString() == "PipeConnectorSymbol");

            PipeConnectorSymbol pipeConnectorSymbol = new PipeConnectorSymbol();
            FillPlantItem(pipeConnectorSymbol, pipeConnectorSymbolElement);
            FillNodes(pipeConnectorSymbol, pipeConnectorSymbolElement);

            XElement crossPageConnectionElement = pipeConnectorSymbolElement.Element("CrossPageConnection");
            if (crossPageConnectionElement != null)
            {
                pipeConnectorSymbol.CrossPageConnection = ReadCrossPageConnectionElement(crossPageConnectionElement);
            }

            return pipeConnectorSymbol;
        }

        private CrossPageConnection ReadCrossPageConnectionElement(XElement crossPageConnectionElement)
        {
            XAttribute linkedIdAttribute = crossPageConnectionElement.Attribute("LinkedId");
            if (linkedIdAttribute == null)
            {
                string position = GetXmlPosition(crossPageConnectionElement);
                string message = string.Format("<{0}> 요소는 LinkedId 속성을 가져야 합니다. 위치: {1}", crossPageConnectionElement.Name, position);
                throw new Exception(message);
            }

            CrossPageConnection crossPage = new CrossPageConnection();
            crossPage.LinkedId = linkedIdAttribute.Value;

            return crossPage;
        }

        private PlantEntity ReadSignalConnectorSymbolElement(XElement signalConnectorSymbolElement)
        {
            Debug.Assert(signalConnectorSymbolElement.Name.ToString() == "PipeConnectorSymbol");

            SignalConnectorSymbol signalConnectorSymbol = new SignalConnectorSymbol();
            FillPlantItem(signalConnectorSymbol, signalConnectorSymbolElement);
            FillNodes(signalConnectorSymbol, signalConnectorSymbolElement);

            XElement crossPageConnectionElement = signalConnectorSymbolElement.Element("CrossPageConnection");
            if (crossPageConnectionElement != null)
            {
                signalConnectorSymbol.CrossPageConnection = ReadCrossPageConnectionElement(crossPageConnectionElement);
            }

            return signalConnectorSymbol;
        }

        private PlantEntity ReadPipingComponentElement(XElement pipingComponentElement)
        {
            Debug.Assert(pipingComponentElement.Name.ToString() == "PipingComponent");

            PipingComponent pipingComponent = new PipingComponent();
            FillPlantItem(pipingComponent, pipingComponentElement);
            FillNodes(pipingComponent, pipingComponentElement);

            XElement associationElement = pipingComponentElement.Element("Association");
            if(associationElement != null)
            {
                XAttribute idAttribute = associationElement.Attribute("ID");
                if (idAttribute == null)
                {
                    string position = GetXmlPosition(associationElement);
                    string message = string.Format("<{0}> 요소는 ID 속성을 가져야 합니다. 위치: {1}", associationElement.Name, position);
                    throw new Exception(message);
                }

                string associatedId = idAttribute.Value;
                AssociationPairs.Add(pipingComponent, associatedId);                
            }

            return pipingComponent;
        }

        private void FillNodes(IHasNodes hasNodes, XElement element)
        {
            hasNodes.Nodes.Clear();
            var nodeElements = element.Elements("Node");            

            foreach (var nodeElement in nodeElements)
            {
                Node node = ReadNodeElement(nodeElement);
                hasNodes.Nodes.Add(node);
            }
        }

        private void FillAttributes(PlantItem plantItem, XElement element)
        {
            plantItem.Attributes.Clear();
            var attributeElements = element.Elements("Attribute");

            foreach (var attributeElement in attributeElements)
            {
                Attribute attribute = ReadAttributeElement(attributeElement);
                plantItem.Attributes.Add(attribute);
            }
        }

        private Attribute ReadAttributeElement(XElement attributeElement)
        {
            Attribute attribute = new Attribute();

            XAttribute nameAttribute = attributeElement.Attribute("Name");
            if (nameAttribute == null)
            {
                string position = GetXmlPosition(attributeElement);
                string message = string.Format("<{0}> 요소는 Name 속성을 가져야 합니다. 위치: {1}", attributeElement.Name, position);
                throw new Exception(message);
            }
                
            attribute.Name = nameAttribute.Value;

            XAttribute valueAttribute = attributeElement.Attribute("Value");
            if (valueAttribute != null)
                attribute.Value = valueAttribute.Value;

            return attribute;
        }

        private void FillTexts(PlantItem plantItem, XElement element)
        {
            var textElements = element.Elements("Text");

            foreach (var textElement in textElements)
            {
                Text text = ReadTextElement(textElement);
                plantItem.Add(text);
            }
        }

        private Node ReadNodeElement(XElement nodeElement)
        {
            Node node = new Node();

            XAttribute idAttribute = nodeElement.Attribute("ID");
            if (idAttribute == null || string.IsNullOrEmpty(idAttribute.Value))
            {
                /*
                string position = GetXmlPosition(nodeElement);
                string message = string.Format("<{0}> 요소는 ID 속성을 가져야 합니다. 위치: {1}", nodeElement.Name, position);
                throw new Exception(message);
                */
                return null; // 노드 ID가 없는 경우 빈 노드로 처리함
            }

            node.ID = idAttribute.Value;            

            XElement coordinateElement = nodeElement.Element("Coordinate");
            Position2 coordinate = ReadPosition2ElementFromAttributes(coordinateElement);
            node.Coordinate = coordinate;

            return node;
        }

        private Text ReadTextElement(XElement textElement)
        {
            Text text = new Text();

            XAttribute idAttribute = textElement.Attribute("ID");
            if (idAttribute != null)
            {
                text.ID = idAttribute.Value;
                text.DisplayName = idAttribute.Value;
            }

            XAttribute stringAttribute = textElement.Attribute("String");
            if (stringAttribute != null)
                text.String = stringAttribute.Value;

            XElement extentElement = textElement.Element("Extent");
            if (extentElement == null)
            {
                string position = GetXmlPosition(textElement);
                string message = string.Format("<{0}> 요소는 <Extent> 요소를 가져야 합니다. 위치: {1}", textElement.Name, position);
                throw new Exception(message);
            }

            Obb2 extent = ReadObb2Element(extentElement);
            text.Extent = extent;
            text.DebugInfo.ExpandedExtent = extent;

            XElement symbolFlipElement = textElement.Element("SymbolFlip");
            if (symbolFlipElement != null)
            {
                if (Enum.TryParse(symbolFlipElement.Value, out Flip symbolFlip))
                {
                    text.SymbolFlip = symbolFlip;
                }
                else
                {
                    string position = GetXmlPosition(symbolFlipElement);
                    string message = string.Format("SymbolFlip 요소에 허용되지 않는 값 '{0}'이 있습니다: 위치 {1}", symbolFlipElement.Value, position);
                    throw new ArgumentNullException(message);
                }
            }

            return text;
        }

        private void FillPlantItem(PlantItem plantItem, XElement plantItemElement)
        {
            XAttribute idAttribute = plantItemElement.Attribute("ID");
            if (idAttribute == null)
            {
                string position = GetXmlPosition(plantItemElement);
                string message = string.Format("<{0}> 요소는 ID 속성을 가져야 합니다. 위치: {1}", plantItemElement.Name, position);
                throw new Exception(message);
            }

            plantItem.ID = idAttribute.Value;
            plantItem.DisplayName = idAttribute.Value;

            XAttribute componentClassAttribute = plantItemElement.Attribute("ComponentClass");
            if(componentClassAttribute != null)
                plantItem.ComponentClass = componentClassAttribute.Value;

            XAttribute tagNameAttribute = plantItemElement.Attribute("TagName");
            if (tagNameAttribute != null)
                plantItem.TagName = tagNameAttribute.Value;

            XElement extentElement = plantItemElement.Element("Extent");
            if(extentElement == null)
            {
                string position = GetXmlPosition(plantItemElement);
                string message = string.Format("Extent 요소가 존재하지 않습니다: 위치 {0}", position);
                throw new Exception(message);
            }

            Obb2 extent = ReadObb2Element(extentElement);
            plantItem.Extent = extent;
            plantItem.ExpandedExtent = extent;

            XElement symbolFlipElement = plantItemElement.Element("SymbolFlip");
            if (symbolFlipElement != null)
            {
                if (Enum.TryParse(symbolFlipElement.Value, out Flip symbolFlip))
                {
                    plantItem.SymbolFlip = symbolFlip;
                }
                else
                {
                    string position = GetXmlPosition(symbolFlipElement);
                    string message = string.Format("SymbolFlip 요소에 허용되지 않는 값 '{0}'이 있습니다: 위치 {1}", symbolFlipElement.Value, position);
                    throw new ArgumentNullException(message);
                }
            }

            XElement valveStatusElement = plantItemElement.Element("ValveStatus");
            if (valveStatusElement != null)
            {
                if (CheckValveStatus(valveStatusElement.Value))
                {
                    plantItem.ValveStatus = valveStatusElement.Value;
                }
                else
                {
                    string position = GetXmlPosition(symbolFlipElement);
                    string message = string.Format("SymbolFlip 요소에 허용되지 않는 값 '{0}'이 있습니다: 위치 {1}", symbolFlipElement.Value, position);
                    throw new ArgumentNullException(message);
                }
            }

            FillAttributes(plantItem, plantItemElement);
            FillTexts(plantItem, plantItemElement);
        }

        private bool CheckValveStatus(string valveStatusValue)
        {
            if (string.IsNullOrEmpty(valveStatusValue))
                return false;

            string trimmedValue = valveStatusValue.Trim();

            if (trimmedValue == "None" || trimmedValue == "ValveOpen" || trimmedValue == "ValveClose")
                return true;
            else
                return false;
        }

        private PlantEntity ReadUnknownSymbolElement(XElement unknownElement)
        {
            Debug.Assert(unknownElement.Name.ToString() == "UnknownSymbol");

            UnknownSymbol unknownSymbol = new UnknownSymbol();
            FillPlantItem(unknownSymbol, unknownElement);

            return unknownSymbol;
        }

        private Obb2 ReadObb2Element(XElement extentElement)
        {
            Axis22 transform = ReadAxis22Element(extentElement.Element("Transform"));
            Position2 min = ReadPosition2ElementFromAttributes(extentElement.Element("Min"));
            Position2 max = ReadPosition2ElementFromAttributes(extentElement.Element("Max"));

            Obb2 obb2 = new Obb2()
            {
                CoordinateSystem = transform,
                LocalMin = min,
                LocalMax = max
            };

            return obb2;
        }        

        private Position2 ReadPosition2ElementFromAttributes(XElement position2Element)
        {
            string stringX = position2Element.Attribute("X").Value;
            string stringY = position2Element.Attribute("Y").Value;

            double x = ConvertToDouble(position2Element, stringX);
            double y = ConvertToDouble(position2Element, stringY);

            return new Position2(x, y);
        }

        private Axis22 ReadAxis22Element(XElement transformElement)
        {
            Position2 origin = ReadPosition2Element(transformElement.Element("Origin"));
            Vector2 xDirection = ReadVector2Element(transformElement.Element("XDirection"));
            Vector2 yDirection = ReadVector2Element(transformElement.Element("YDirection"));

            Axis22 transform = new Axis22()
            {
                Location = origin,
                XDirection = xDirection,
                YDirection = yDirection
            };

            return transform;
        }

        private Vector2 ReadVector2Element(XElement vector2Element)
        {
            string stringX = vector2Element.Element("X").Value;
            string stringY = vector2Element.Element("Y").Value;

            double x = ConvertToDouble(vector2Element, stringX);
            double y = ConvertToDouble(vector2Element, stringY);

            return new Vector2(x, y);
        }

        private Position2 ReadPosition2Element(XElement position2Element)
        {
            string stringX = position2Element.Element("X").Value;
            string stringY = position2Element.Element("Y").Value;

            double x = ConvertToDouble(position2Element, stringX);
            double y = ConvertToDouble(position2Element, stringY);

            return new Position2(x, y);
        }

        private void MakeUpAssociations(PlantModel plantModel)
        {
            var duplications = AssociationPairs.Values
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            foreach (var duplication in duplications)
            {
                string message = $"ID가 {duplication}인 Instrument가 두 개 이상의 PipingComponent에 연관되어 있습니다.";
                Trace.WriteLine(message);
            }

            foreach (var associationPair in AssociationPairs)
            {
                string associatedId = associationPair.Value;

                var associatedInstrument = plantModel.GetAllDecendants().OfType<Instrument>().Where(x => x.ID == associatedId).FirstOrDefault();
                if(associatedInstrument == null)
                {
                    string message = $"ID가 {associatedId}인 Instrument가 PipingComponent에 연관되어 있지만, 해당 Instrument가 존재하지 않습니다.";
                    Trace.WriteLine(message);
                }
                else
                {
                    PipingComponent pipingComponent = associationPair.Key;
                    pipingComponent.Association = associatedInstrument;
                }
            }
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

        private double ConvertToDouble(XElement element, string str)
        {
            if (!double.TryParse(str, out double d))
            {
                string strPosition = GetXmlPosition(element);
                string message = string.Format("읽을 수 없는 값 '{0}' 입니다: 위치 {1}", str, strPosition);
                throw new Exception(message);
            }

            return d;
        }
    }
}
