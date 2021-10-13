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
    class SignalBranchShape : ShapeItem
    {
        public SignalBranchShape(SignalBranch signalBranch) : base(signalBranch)
        {

        }

        protected SignalBranch SignalBranch
        {
            get { return (SignalBranch)PlantEntity; }
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);

            Obb2 newExtent = SignalBranch.Extent;
            newExtent.Expand(5.0);

            ShapeDrawer.DrawBox(drawingContext, newExtent, DrawingColor, DrawingThickness);

            if (IsSelected)
            {
                IHasNodes hasNodes = SignalBranch;
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
            Obb2 newExtent = SignalBranch.Extent;
            newExtent.Expand(5.0);

            return newExtent.Contains(x, y);
        }
    }
}
