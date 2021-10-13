using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator
{
    public class ConnectionObject
    {
        public static string ClassNameDefault = "Connection";

        public ConnectionObject()
        {
            Id = string.Empty;
            ClassName = ClassNameDefault;

            X = string.Empty;
            Y = string.Empty;

            From = string.Empty;
            To = string.Empty;
        }

        public string Id { get; set; }

        public string ClassName { get; set; }

        public string X { get; set; }

        public string Y { get; set; }

        public string From
        {
            get;
            set;
        }

        public string To
        {
            get;
            set;
        }
    }
}
