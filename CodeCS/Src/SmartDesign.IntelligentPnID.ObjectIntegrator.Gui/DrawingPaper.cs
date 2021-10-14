using DevExpress.XtraPrinting.Shape;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Shapes;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    class DrawingPaper : FrameworkElement
    {
        public DrawingPaper()
        {
            plantModel = null;
            ZoomFactor = 1.0;
            IsAxesVisible = false;
        }

        private PlantModel plantModel; //필드 생성

        public PlantModel PlantModel
        {
            get { return plantModel; }
            set
            {
                plantModel = value;
                Width = ZoomFactor * plantModel.Width;
                Height = ZoomFactor * plantModel.Height;
                CreateShapeFromPlantModel(plantModel);
            }
        }

        public List<ShapeItem> Shapes
        {
            get;
            set;
        }

        public double ZoomFactor { get; set; }

        public bool IsAxesVisible { get; set; }

        public void ResetSize()
        {
            if (plantModel == null)
                return;

            Width = ZoomFactor * plantModel.Width;
            Height = ZoomFactor * plantModel.Height;
        }
        
        private void CreateShapeFromPlantModel(PlantModel plantModel)
        {
            Shapes = new List<ShapeItem>();

            InternalCreateShapeFromPlantModel(plantModel, Shapes);
        }

        private void InternalCreateShapeFromPlantModel(PlantEntity plantEntity, List<ShapeItem> shapes)
        {
            ShapeItem shapeItem = ShapeItemCreator.Create(plantEntity);
            Debug.Assert(shapeItem != null);
            shapes.Add(shapeItem);

            foreach (var childEntity in plantEntity.Children)
            {
                InternalCreateShapeFromPlantModel(childEntity, shapes);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (PlantModel == null)
                return;

            if (Shapes.Count == 0)
                return;

            //MatrixTransform transformDrawingToImage = new MatrixTransform(1.0, 0.0, 0.0, -1.0, 0.0, Height);
            MatrixTransform transformDrawingToImage = new MatrixTransform(ZoomFactor, 0.0, 0.0, -ZoomFactor, 0.0, Height);

            drawingContext.PushTransform(transformDrawingToImage);

            foreach(ShapeItem shapeItem in Shapes)
            {
                shapeItem.IsAxesVisible = IsAxesVisible;
                shapeItem.Draw(drawingContext);
            }

            drawingContext.Pop();

        }

        public ShapeItem FindByPlantEntity(PlantEntity plantEntity)
        {
            return Shapes.FirstOrDefault(shape => shape.PlantEntity == plantEntity);
        }

        public void DeselectAll()
        {
            Shapes.ForEach(shape => shape.IsSelected = false);
        }

        private void TransformToDrawingSpace(double mouseX, double mouseY, out double x, out double y)
        {
            x = mouseX / ZoomFactor;
            y = (Height - mouseY) / ZoomFactor;
        }
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Image 공간에서 Drawing 공간으로 변환
            Point mousePosition = e.GetPosition(this);
            TransformToDrawingSpace(mousePosition.X, mousePosition.Y, out double x, out double y);

            Shapes.ForEach(shape => shape.IsSelected = false);

            foreach(var shape in Shapes)
            {
                if(shape.HitTest(x, y))
                {
                    shape.IsSelected = true;
                    break;
                }
            }

            InvalidateVisual();
        }
    }
}
