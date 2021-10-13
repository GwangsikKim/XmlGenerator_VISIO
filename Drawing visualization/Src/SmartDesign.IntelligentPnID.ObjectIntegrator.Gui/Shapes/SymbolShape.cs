using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class SymbolShape : ShapeItem
    {
        public SymbolShape(PlantItem plantItem) : base(plantItem)
        {
        }

        protected PlantItem PlantItem
        {
            get { return (PlantItem)PlantEntity; }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);            

            ShapeDrawer.DrawBox(drawingContext, PlantItem.Extent, DrawingColor, DrawingThickness);

            if (IsSelected)
            {
                IHasNodes hasNodes = PlantItem as IHasNodes;
                if (hasNodes != null)
                {
                    foreach (Node node in hasNodes.Nodes)
                    {
                        ShapeDrawer.DrawNode(drawingContext, node, true);
                    }
                }
            }
        }

        public override bool HitTest(double x, double y)
        {
            return PlantItem.Extent.Contains(x, y);
        }
    }
}
