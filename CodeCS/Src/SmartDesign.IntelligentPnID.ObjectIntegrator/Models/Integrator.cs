using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Graph;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class Integrator
    {
        public const double DefaultMaximumDistance = 50.0;
        public const double DefaultDistanceTolerance = 2.0;
        public const double FakeLineEndSize = 6.0;

        public Integrator(PlantModel plantModel)
        {
            PlantModel = plantModel;
            MaximumDistance = DefaultMaximumDistance;
            DistanceTolerance = DefaultDistanceTolerance;
        }

        private PlantModel PlantModel { get; set; }

        public PlantGraph PlantGraph { get; private set; }

        public double MaximumDistance { get; set; }
        public double DistanceTolerance { get; set; }

        public void Integrate()
        {
            LineItemSplitter lineItemSplitter = new LineItemSplitter(PlantModel)
            {
                DistanceTolerance = this.DistanceTolerance
            };

            lineItemSplitter.Split();
            
            TextIntegrator textIntegrator = new TextIntegrator(PlantModel)
            {
                MaximumDistance = this.MaximumDistance,
                DistanceTolerance = this.DistanceTolerance
            };

            textIntegrator.Integrate();
            /*
            GraphCreator graphCreator = new GraphCreator(PlantModel)
            {
                MaximumDistance = this.MaximumDistance,
                DistanceTolerance = this.DistanceTolerance
            };

            PlantGraph = graphCreator.Create();
            */
        }
    }
}
