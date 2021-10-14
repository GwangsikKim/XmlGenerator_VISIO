using SmartDesign.MathUtil;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public abstract class LineItem : PlantItem
    {
        public LineItem()
        {
            CenterLine = new CenterLine();
            StartShape = LineEndShape.None;
            EndShape = LineEndShape.None;
        }

        protected LineItem(LineItem other) : base(other)
        {            
        }

        [ReadOnly(true)]
        public CenterLine CenterLine
        {
            get;
            set;
        }

        public LineEndShape StartShape { get; set; }
        
        public LineEndShape EndShape { get; set; }

        public override void CopyPropertiesTo(PlantEntity plantEntity)
        {
            base.CopyPropertiesTo(plantEntity);

            if (plantEntity is LineItem lineItem)
            {
                lineItem.CenterLine = CenterLine.Clone();
                lineItem.StartShape = StartShape;
                lineItem.EndShape = EndShape;
            }
        }
    }
}
