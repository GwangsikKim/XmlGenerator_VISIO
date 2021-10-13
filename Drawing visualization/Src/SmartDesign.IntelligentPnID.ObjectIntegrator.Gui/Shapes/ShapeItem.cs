using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class ShapeItem
    {
        public ShapeItem(PlantEntity plantEntity)
        {
            PlantEntity = plantEntity;
            IsSelected = false;
            DrawingThickness = 1.0;
            Color = ShapeColors.Default;
            IsAxesVisible = true;
        }

        public PlantEntity PlantEntity { get; set; }

        public bool IsSelected { get; set; }
        public Color Color { get; set; }
        public bool IsAxesVisible { get; set; }

        protected double DrawingThickness { get; set; }
        protected Color DrawingColor { get; set; }

        public virtual void Draw(DrawingContext drawingContext)
        {
            DrawingThickness = IsSelected ? 2.0 : 1.0;
            DrawingColor = IsSelected ? ShapeColors.SelectionColor : Color;

            if (IsSelected && IsAxesVisible)
            {
                if(PlantEntity is IHasExtent extent)
                    ShapeDrawer.DrawAxes(drawingContext, extent.Extent.CoordinateSystem);
            }                
        }

        public virtual bool HitTest(double x, double y)
        {
            return false;
        }
    }
}
