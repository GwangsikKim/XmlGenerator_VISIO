using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes
{
    class PlantModelShape : ShapeItem
    {
        public PlantModelShape(PlantModel plantModel) : base(plantModel)
        {
            GridX = 100.0;
            GridY = 100.0;
        }

        public double GridX
        {
            get;
            set;
        }

        public double GridY
        {
            get;
            set;
        }

        public override void Draw(DrawingContext drawingContext)
        {
            base.Draw(drawingContext);

            PlantModel plantModel = (PlantModel)PlantEntity;

            Color borderColor = IsSelected ? ShapeColors.SelectionColor : ShapeColors.PaperBorder;
            double thickness = IsSelected ? 2.0 : 1.0;

            Pen borderPen = new Pen();
            borderPen.Thickness = thickness;
            borderPen.Brush = new SolidColorBrush(borderColor);

            Brush backgroundBrush = new SolidColorBrush(ShapeColors.PaperBackground);

            Rect rect = new Rect(plantModel.Extent.GlobalMin.X, plantModel.Extent.GlobalMin.Y, plantModel.Extent.GlobalMax.X, plantModel.Extent.GlobalMax.Y);

            drawingContext.DrawRectangle(backgroundBrush, borderPen, rect);

            DrawGrid(drawingContext);
        }

        private void DrawGrid(DrawingContext drawingContext)
        {
            PlantModel plantModel = (PlantModel)PlantEntity;

            for(double x = plantModel.Extent.GlobalMin.X + GridX; x < plantModel.Extent.GlobalMax.X; x+= GridX)
            {
                Pen pen = new Pen();
                pen.Thickness = 1.0;
                pen.Brush = new SolidColorBrush(ShapeColors.Grid);

                Point p0 = new Point(x, plantModel.Extent.GlobalMin.Y);
                Point p1 = new Point(x, plantModel.Extent.GlobalMax.Y);

                drawingContext.DrawLine(pen, p0, p1);
            }

            for (double y = plantModel.Extent.GlobalMin.Y + GridY; y < plantModel.Extent.GlobalMax.Y; y += GridY)
            {
                Pen pen = new Pen();
                pen.Thickness = 1.0;
                pen.Brush = new SolidColorBrush(ShapeColors.Grid);

                Point p0 = new Point(plantModel.Extent.GlobalMin.X, y);
                Point p1 = new Point(plantModel.Extent.GlobalMax.X, y);

                drawingContext.DrawLine(pen, p0, p1);
            }
        }
    }
}
