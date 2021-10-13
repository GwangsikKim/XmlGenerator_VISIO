using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class NozzleShape : ShapeItem
    {
        public NozzleShape(Nozzle nozzle) : base(nozzle)
        {
        }

        protected Nozzle Nozzle
        {
            get { return (Nozzle)PlantEntity; }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);

            Obb2 extent = Nozzle.Extent;
            double shrinkingSize = extent.Height * 0.25;

            extent.Expand(0, -shrinkingSize, 0, -shrinkingSize);

            ShapeDrawer.DrawBox(drawingContext, extent, DrawingColor, DrawingThickness);

            if (IsSelected)
            {
                IHasNodes hasNodes = Nozzle as IHasNodes;
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
            return Nozzle.Extent.Contains(x, y);
        }
    }
}
