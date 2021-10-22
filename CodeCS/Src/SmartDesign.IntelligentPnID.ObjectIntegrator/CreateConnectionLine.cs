using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator
{
    public class CreateConnectionLine
    {
        public CreateConnectionLine()
        {
        }

        public PlantModel Creator(PlantModel plantModel)
        {
            PlantEntity plantEntity = plantModel;
            List<PipingNetworkSegment> pipingNetworkSegments = new List<PipingNetworkSegment>();

            foreach (var connection in plantEntity.PlantModel.Connections)
            {
                List<PlantEntity> plantEntities = new List<PlantEntity>();

                for (int i = 0; i < plantEntity.Children.Count; i++)
                {
                    if (connection.From == plantEntity.Children[i].OriginalID)
                    {
                        //var a = plantEntity.Children[i].OriginalID;
                        plantEntities.Add(plantEntity.Children[i]);
                        break;
                    }
                }

                for (int i = 0; i < plantEntity.Children.Count; i++)
                {
                    if (connection.To == plantEntity.Children[i].OriginalID)
                    {
                        //var b = plantEntity.Children[i].OriginalID;
                        plantEntities.Add(plantEntity.Children[i]);
                        break;
                    }
                }

                if (plantEntities.Count > 1)
                {
                   // Console.WriteLine("From : {0}  To : {1}", a, b);
                    PipingNetworkSegment connectLine = ConnectionLineCreat(plantEntities, plantModel);
                    pipingNetworkSegments.Add(connectLine);
                }
            }

            foreach (var item in pipingNetworkSegments)
            {
                plantModel.Add(item);
            }

            return plantModel;
        }

        private PipingNetworkSegment ConnectionLineCreat(List<PlantEntity> plantEntities, PlantModel plantModel)
        {
            Position2 startPosition = new Position2();
            Position2 endPosition = new Position2();

            switch (plantEntities[0].TypeName)
            {
                case "Equipment":
                    foreach (var equipment in plantModel.Equipments)
                    {
                        if (plantEntities[0].ID == equipment.ID)
                        {
                            startPosition.X = equipment.Extent.Center.X;
                            startPosition.Y = equipment.Extent.Center.Y;
                        }
                    };
                    break;
                case "Instrument":
                    foreach (var instrument in plantModel.Instruments)
                    {
                        if (plantEntities[0].ID == instrument.ID)
                        {
                            startPosition.X = instrument.Extent.Center.X;
                            startPosition.Y = instrument.Extent.Center.Y;
                        }
                    };
                    break;
                case "PipingComponent":
                    foreach (var pipingComponent in plantModel.PipingComponents)
                    {
                        if (plantEntities[0].ID == pipingComponent.ID)
                        {
                            startPosition.X = pipingComponent.Extent.Center.X;
                            startPosition.Y = pipingComponent.Extent.Center.Y;
                        }
                    };
                    break;
                case "PipingNetworkSegment":
                    foreach (var pipingNetworkSegment in plantModel.PipingNetworkSegments)
                    {
                        if (plantEntities[0].ID == pipingNetworkSegment.ID)
                        {
                            startPosition.X = pipingNetworkSegment.Extent.Center.X;
                            startPosition.Y = pipingNetworkSegment.Extent.Center.Y;
                        }
                    };
                    break;
                case "SignalLine":
                    foreach (var signalLine in plantModel.SignalLines)
                    {
                        if (plantEntities[0].ID == signalLine.ID)
                        {
                            startPosition.X = signalLine.Extent.Center.X;
                            startPosition.Y = signalLine.Extent.Center.Y;
                        }
                    };
                    break;
                case "UnknownSymbol":
                    foreach (var unknownSymbol in plantModel.UnknownSymbols)
                    {
                        if (plantEntities[0].ID == unknownSymbol.ID)
                        {
                            startPosition.X = unknownSymbol.Extent.Center.X;
                            startPosition.Y = unknownSymbol.Extent.Center.Y;
                        }
                    };
                    break;
                case "UnknownLine":
                    foreach (var unknownLine in plantModel.UnknownLines)
                    {
                        if (plantEntities[0].ID == unknownLine.ID)
                        {
                            startPosition.X = unknownLine.Extent.Center.X;
                            startPosition.Y = unknownLine.Extent.Center.Y;
                        }
                    };
                    break;
            }

            switch (plantEntities[1].TypeName)
            {
                case "Equipment":
                    foreach (var equipment in plantModel.Equipments)
                    {
                        if (plantEntities[1].ID == equipment.ID)
                        {
                            endPosition.X = equipment.Extent.Center.X;
                            endPosition.Y = equipment.Extent.Center.Y;
                        }
                    };
                    break;
                case "Instrument":
                    foreach (var instrument in plantModel.Instruments)
                    {
                        if (plantEntities[1].ID == instrument.ID)
                        {
                            endPosition.X = instrument.Extent.Center.X;
                            endPosition.Y = instrument.Extent.Center.Y;
                        }
                    };
                    break;
                case "PipingComponent":
                    foreach (var pipingComponent in plantModel.PipingComponents)
                    {
                        if (plantEntities[1].ID == pipingComponent.ID)
                        {
                            endPosition.X = pipingComponent.Extent.Center.X;
                            endPosition.Y = pipingComponent.Extent.Center.Y;
                        }
                    };
                    break;
                case "PipingNetworkSegment":
                    foreach (var pipingNetworkSegment in plantModel.PipingNetworkSegments)
                    {
                        if (plantEntities[1].ID == pipingNetworkSegment.ID)
                        {
                            endPosition.X = pipingNetworkSegment.Extent.Center.X;
                            endPosition.Y = pipingNetworkSegment.Extent.Center.Y;
                        }
                    };
                    break;
                case "SignalLine":
                    foreach (var signalLine in plantModel.SignalLines)
                    {
                        if (plantEntities[1].ID == signalLine.ID)
                        {
                            endPosition.X = signalLine.Extent.Center.X;
                            endPosition.Y = signalLine.Extent.Center.Y;
                        }
                    };
                    break;
                case "UnknownSymbol":
                    foreach (var unknownSymbol in plantModel.UnknownSymbols)
                    {
                        if (plantEntities[1].ID == unknownSymbol.ID)
                        {
                            endPosition.X = unknownSymbol.Extent.Center.X;
                            endPosition.Y = unknownSymbol.Extent.Center.Y;
                        }
                    };
                    break;
                case "UnknownLine":
                    foreach (var unknownLine in plantModel.UnknownLines)
                    {
                        if (plantEntities[1].ID == unknownLine.ID)
                        {
                            endPosition.X = unknownLine.Extent.Center.X;
                            endPosition.Y = unknownLine.Extent.Center.Y;
                        }
                    };
                    break;
            }

            if (startPosition.X == 0)
            {
                Console.WriteLine(plantEntities[0].TypeName);
            }

            PipingNetworkSegment connectLine = new PipingNetworkSegment();
            connectLine.CenterLine.Start = startPosition;
            connectLine.CenterLine.End = endPosition;
            connectLine.Extent = Obb2.Create(startPosition, endPosition);
            connectLine.ExpandedExtent = Obb2.Create(startPosition, endPosition);
            connectLine.StartShape = LineEndShape.Arrow;
            connectLine.EndShape = LineEndShape.Arrow;

            return connectLine;
        }
    }
}