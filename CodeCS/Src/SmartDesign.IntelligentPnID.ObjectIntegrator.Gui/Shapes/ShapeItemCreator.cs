using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class ShapeItemCreator
    {
        public static ShapeItem Create(PlantEntity plantEntity)
        {
            if (plantEntity is PlantModel plantModel)
                return new PlantModelShape(plantModel);

            else if (plantEntity is Equipment equipment)
                return new SymbolShape(equipment) { Color = ShapeColors.EquipmentBorder };

            else if (plantEntity is Instrument instrument)
                return new SymbolShape(instrument) { Color = ShapeColors.InstrumentBorder };

            else if (plantEntity is Nozzle nozzle)
                return new NozzleShape(nozzle) { Color = ShapeColors.NozzleBorder };

            else if (plantEntity is PipeConnectorSymbol pipeConnectorSymbol)
                return new SymbolShape(pipeConnectorSymbol) { Color = ShapeColors.PipingConnectorSymbolBorder };

            else if (plantEntity is PipeTee pipeTee)
                return new PipeTeeShape(pipeTee) { Color = ShapeColors.PipeTee };

            else if (plantEntity is PipeCross pipeCross)
                return new PipeCrossShape(pipeCross) { Color = ShapeColors.PipeCross };

            else if (plantEntity is PipingComponent pipingComponent)
                return new SymbolShape(pipingComponent) { Color = ShapeColors.PipingComponentBorder };

            else if (plantEntity is PipingNetworkSegment pipingNetworkSegment)
                return new LineShape(pipingNetworkSegment) { Color = ShapeColors.PipingNetworkSegment, LineStyle = GetLineStyle(pipingNetworkSegment) };

            else if (plantEntity is SignalConnectorSymbol signalConnectorSymbol)
                return new SymbolShape(signalConnectorSymbol) { Color = ShapeColors.SignalConnectorSymbolBorder };

            else if (plantEntity is SignalLine signalLine)
                return new LineShape(signalLine) { Color = ShapeColors.SignalLine, LineStyle = GetLineStyle(signalLine) };

            else if (plantEntity is SignalBranch signalBranch)
                return new SignalBranchShape(signalBranch) { Color = ShapeColors.SignalBranch };

            else if (plantEntity is Text text)
                return new TextShape(text) { Color = ShapeColors.TextBorder, TextColor = ShapeColors.Text, IsolatedColor = ShapeColors.IsolatedColor };

            else if (plantEntity is UnknownSymbol unknownSymbol)
                return new SymbolShape(unknownSymbol) { Color = ShapeColors.UnknownSymbolBorder };

            else if (plantEntity is UnknownLine unknownLine)
                return new LineShape(unknownLine) { Color = ShapeColors.UnknownLine, LineStyle = GetLineStyle(unknownLine) };

            else if (plantEntity is Connection connection)
                return new SymbolShape(connection) { Color = ShapeColors.ConnectionLine };

            else
                throw new ArgumentException("알 수 없는 형식입니다.");

        }

        private static DashStyle GetLineStyle(LineItem lineItem)
        {
            DashStyle lineStyle = DashStyles.Solid;
            if (lineItem.ComponentClass == "solid" || lineItem.ComponentClass == "none")
                lineStyle = DashStyles.Solid;
            else if (lineItem.ComponentClass == "dashed")
                lineStyle = DashStyles.Dash;
            else if (lineItem.ComponentClass == "Data")
                lineStyle = DashStyles.Dot;
            return lineStyle;
        }
    }
}
