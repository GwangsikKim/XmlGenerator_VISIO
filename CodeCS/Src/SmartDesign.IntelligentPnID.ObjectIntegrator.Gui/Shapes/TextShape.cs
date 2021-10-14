using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class TextShape : ShapeItem
    {
        public TextShape(Text text) : base(text)
        {

        }

        protected Text Text { get { return (Text)PlantEntity; } }

        public Color TextColor { get; set; }
        public Color IsolatedColor { get; set; }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);

            Color textColor;
            Color borderColor;

            if(IsSelected)
            {
                textColor = ShapeColors.SelectionColor;
                borderColor = ShapeColors.SelectionColor;
            }
            else
            {
                if(Text.Parent == Text.PlantModel)
                {
                    textColor = IsolatedColor;
                    borderColor = IsolatedColor;
                }
                else
                {
                    textColor = TextColor;
                    borderColor = DrawingColor;
                }
            }

            if(IsSelected)
            {
                ShapeDrawer.DrawFilledBox(drawingContext, Text.DebugInfo.ExpandedExtent, ShapeColors.SelectedExpandedExtent);
            }

            ShapeDrawer.DrawBox(drawingContext, Text.Extent, borderColor, DrawingThickness);
            ShapeDrawer.DrawText(drawingContext, Text.Extent, Text.String, textColor);
        }

        public override bool HitTest(double x, double y)
        {
            Obb2 extent = Text.Extent;
            return extent.Contains(x, y);
        }
    }
}
