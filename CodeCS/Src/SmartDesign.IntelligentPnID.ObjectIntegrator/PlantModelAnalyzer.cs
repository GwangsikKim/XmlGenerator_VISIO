using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator
{
    public class PlantModelAnalyzer
    {
        class AnalysisData
        {
            public int NumberOfEquipment { get; set; }
            public int NumberOfNozzle { get; set; }
            public int NumberOfInstrument { get; set; }
            public int NumberOfPipeConnectorSymbol { get; set; }
            public int NumberOfPipeTee { get; set; }
            public int NumberOfPipeCross { get; set; }
            public int NumberOfPipingComponent { get; set; }
            public int NumberOfPipingNetworkSystem { get; set; }
            public int NumberOfPipingNetworkSegment { get; set; }
            public int NumberOfSignalConnectorSymbol { get; set; }
            public int NumberOfSignalLine { get; set; }
            public int NumberOfText { get; set; }
            public int NumberOfSignalBranch { get; set; }
            public int NumberOfUnknownSymbol { get; set; }
            public int NumberOfUnknownLine { get; set; }
        }

        public void Analyze(PlantModel plantModel, StreamWriter sw)
        {
            AnalysisData data = new AnalysisData();

            CountChildren(plantModel, data);

            Write(data, sw);
        }        

        private void CountChildren(PlantEntity plantEntity, AnalysisData data)
        {
            CountEntity(plantEntity, data);

            foreach(var childEntity in plantEntity.Children)
            {
                CountChildren(childEntity, data);
            }
        }

        private void CountEntity(PlantEntity plantEntity, AnalysisData data)
        {
            if(plantEntity is PlantModel)
            {

            }
            else if (plantEntity is Equipment equipment)
            {
                ++data.NumberOfEquipment;
            }
            else if (plantEntity is Nozzle nozzle)
            {
                ++data.NumberOfNozzle;
            }
            else if (plantEntity is Instrument instrument)
            {
                ++data.NumberOfInstrument;
            }
            else if (plantEntity is PipeConnectorSymbol pipeConnectorSymbol)
            {
                ++data.NumberOfPipeConnectorSymbol;
            }
            else if (plantEntity is PipeTee pipeTee)
            {
                ++data.NumberOfPipeTee;
            }
            else if (plantEntity is PipeCross pipeCross)
            {
                ++data.NumberOfPipeCross;
            }
            else if (plantEntity is PipingComponent pipingComponent)
            {
                ++data.NumberOfPipingComponent;
            }
            else if (plantEntity is PipingNetworkSegment pipingNetworkSegment)
            {
                ++data.NumberOfPipingNetworkSegment;
            }
            else if (plantEntity is SignalConnectorSymbol signalConnectorSymbol)
            {
                ++data.NumberOfSignalConnectorSymbol;
            }
            else if (plantEntity is SignalLine signalLine)
            {
                ++data.NumberOfSignalLine;
            }
            else if (plantEntity is Text text)
            {
                ++data.NumberOfText;
            }
            else if (plantEntity is SignalBranch signalBranch)
            {
                ++data.NumberOfSignalBranch;
            }
            else if (plantEntity is UnknownSymbol unknownSymbol)
            {
                ++data.NumberOfUnknownSymbol;
            }
            else if (plantEntity is UnknownLine unknownLine)
            {
                ++data.NumberOfUnknownLine;
            }
            else
                throw new ArgumentException("지원하지 않는 클래스 타입입니다.", nameof(plantEntity));
        }

        private void Write(AnalysisData data, StreamWriter sw)
        {
            sw.WriteLine("Equipment " + data.NumberOfEquipment);
            sw.WriteLine("Nozzle " + data.NumberOfNozzle);
            sw.WriteLine("Instrument " + data.NumberOfInstrument);
            sw.WriteLine("PipeConnectorSymbol " + data.NumberOfPipeConnectorSymbol);
            sw.WriteLine("PipeTee " + data.NumberOfPipeTee);
            sw.WriteLine("PipeCross " + data.NumberOfPipeCross);
            sw.WriteLine("PipingComponent " + data.NumberOfPipingComponent);
            sw.WriteLine("PipingNetworkSystem " + data.NumberOfPipingNetworkSystem);
            sw.WriteLine("PipingNetworkSegment " + data.NumberOfPipingNetworkSegment);
            sw.WriteLine("SignalConnectorSymbol " + data.NumberOfSignalConnectorSymbol);
            sw.WriteLine("SignalLine " + data.NumberOfSignalLine);
            sw.WriteLine("Text " + data.NumberOfText);
            sw.WriteLine("SignalBranch " + data.NumberOfSignalBranch);
            sw.WriteLine("UnknownSymbol " + data.NumberOfUnknownSymbol);
            sw.WriteLine("UnknownLine " + data.NumberOfUnknownLine);
        }
    }
}
