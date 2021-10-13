using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class BasicDrawingInformation
    {
        public BasicDrawingInformation()
        {
            Size = new Size();
            ExternalBorderLine = new BoundingBox();
            PureDrawingArea = new BoundingBox();
            NoteArea = new BoundingBox();
            TitleArea = new BoundingBox();
            DrawingAreaSeparator = new Edge();
        }

        public string FileName { get; set; }

        public string Path { get; set; }

        public Size Size { get; set; }

        public BoundingBox ExternalBorderLine { get; set; }

        public BoundingBox PureDrawingArea { get; set; }

        public BoundingBox NoteArea { get; set; }

        public BoundingBox TitleArea { get; set; }

        public Edge DrawingAreaSeparator { get; set; }

    }
}
