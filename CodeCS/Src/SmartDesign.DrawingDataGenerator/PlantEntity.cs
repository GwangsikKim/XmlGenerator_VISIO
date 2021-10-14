using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartDesign.DrawingDataGenerator;

namespace SmartDesign.DrawingDataGenerator
{
    class PlantEntity
    {
        public PlantEntity()
        {
             Equipments = new List<Equipment>();
            Instruments = new List<Instrument>();
            PipingComponents = new List<PipingComponent>();
            Texts = new List<Text>();

            PipeLines = new List<PipeLine>();
            SignalLines = new List<SignalLine>();
            ConnectionPoints = new List<ConnectionPoint>();
        }

        public List<Equipment> Equipments
        {
            get;
            set;
        }

        public List<Instrument> Instruments
        {
            get;
            set;
        }

        public List<PipingComponent> PipingComponents
        {
            get;
            set;
        }

        public List<Text> Texts
        {
            get;
            set;
        }

        public List<PipeLine> PipeLines
        {
            get;
            set;
        }

        public List<SignalLine> SignalLines
        {
            get;
            set;
        }

        public List<ConnectionPoint> ConnectionPoints
        {
            get;
            set;
        }
    }
}
