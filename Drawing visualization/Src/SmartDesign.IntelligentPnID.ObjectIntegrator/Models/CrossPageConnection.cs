using SmartDesign.MathUtil;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class CrossPageConnection : PlantEntity
    {
        public CrossPageConnection()
        {
            LinkedId = string.Empty;
        }

        protected CrossPageConnection(CrossPageConnection other) : base(other)
        {
        }

        public string LinkedId
        {
            get;
            set;
        }

        public override PlantEntity Clone()
        {
            return new CrossPageConnection(this);
        }

        public override void CopyPropertiesTo(PlantEntity plantEntity)
        {
            base.CopyPropertiesTo(plantEntity);

            if (plantEntity is CrossPageConnection crossPageConnection)
            {
                crossPageConnection.LinkedId = LinkedId;
            }
        }
    }
}
