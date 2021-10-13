using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition
{
    public class LineObject
    {
        public static string ClassNameDefault = "none";

        public LineObject()
        {
            Id = string.Empty;
            ClassName = ClassNameDefault;
            Edge = new Edge();
        }

        public string Id { get; set; }

        public LineObjectType Type { get; set; }

        public string ClassName { get; set; }

        public Edge Edge { get; set; }

        public LineEndShape StartEndType { get; set; }
        public LineEndShape EndEndType { get; set; }
    }
}
