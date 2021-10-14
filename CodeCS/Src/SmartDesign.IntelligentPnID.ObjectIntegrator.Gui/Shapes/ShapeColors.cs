using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    static class ShapeColors
    {
        public static readonly Color Default = Color.FromRgb(0, 0, 0);

        public static readonly Color PaperBackground = Color.FromRgb(255, 255, 255);
        public static readonly Color PaperBorder = Color.FromRgb(0, 0, 0);
        public static readonly Color Grid = Color.FromRgb(230, 230, 230);

        public static readonly Color EquipmentBorder = Color.FromRgb(0, 0, 255);
        public static readonly Color InstrumentBorder = Color.FromRgb(0, 255, 0);
        public static readonly Color NozzleBorder = Color.FromRgb(128, 128, 0);
        public static readonly Color PipingConnectorSymbolBorder = Color.FromRgb(125, 125, 125);
        public static readonly Color PipingComponentBorder = Color.FromRgb(0, 255, 255);
        public static readonly Color PipeTee = Color.FromRgb(0, 128, 128);
        public static readonly Color PipeCross = Color.FromRgb(0, 128, 128);
        public static readonly Color SignalConnectorSymbolBorder = Color.FromRgb(125, 125, 0);
        public static readonly Color TextBorder = Color.FromRgb(100, 100, 100);
        public static readonly Color UnknownSymbolBorder = Color.FromRgb(100, 100, 100);

        public static readonly Color PipingNetworkSegment = Color.FromRgb(255, 165, 0);
        public static readonly Color SignalLine = Color.FromRgb(138, 0, 230);
        public static readonly Color SignalBranch = Color.FromRgb(138, 0, 138);
        public static readonly Color UnknownLine = Color.FromRgb(100, 100, 100);
        public static readonly Color ConnectionLine = Color.FromRgb(250, 0, 255);

        public static readonly Color Text = Color.FromRgb(100, 100, 100);
        public static readonly Color IsolatedColor = Color.FromRgb(200, 200, 200);

        public static readonly Color SelectionColor = Color.FromRgb(255, 0, 0);
        public static readonly Color AdjacentNodeColor = Color.FromRgb(255, 255, 0);
        
        public static readonly Color SelectedExpandedExtent = Color.FromArgb(100, 155, 155, 155);

        public static readonly Color GraphEdge = Color.FromRgb(0, 0, 255);
    }
}
