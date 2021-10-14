using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class PipeTeeShape : ShapeItem
    {
        public PipeTeeShape(PipeTee pipeTee) : base(pipeTee)
        {

        }

        protected PipeTee PipeTee
        {
            get { return (PipeTee)PlantEntity; }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);
            
            Obb2 newExtent = PipeTee.Extent;
            newExtent.Expand(5.0);

            ShapeDrawer.DrawBox(drawingContext, newExtent, DrawingColor, DrawingThickness);

            if (IsSelected)
            {
                IHasNodes hasNodes = PipeTee;
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
            Obb2 newExtent = PipeTee.Extent;
            newExtent.Expand(5.0);

            return newExtent.Contains(x, y);
        }
    }
}
