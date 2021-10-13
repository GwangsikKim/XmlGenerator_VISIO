using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class Text : PlantEntity, IHasExtent, ICheckingDuplication
    {
        public Text()
        {
            Extent = new Obb2();
            SymbolFlip = Flip.None;
            String = string.Empty;
        }

        protected Text(Text other) : base(other)
        {
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [ReadOnly(true)]
        public Obb2 Extent
        {
            get;
            set;
        }        

        public string String
        {
            get;
            set;
        }

        public Flip SymbolFlip
        {
            get;
            set;
        }

        public override PlantEntity Clone()
        {
            return new Text(this);
        }

        public override void CopyPropertiesTo(PlantEntity plantEntity)
        {
            base.CopyPropertiesTo(plantEntity);

            if (plantEntity is Text text)
            {
                text.Extent = Extent;
                text.SymbolFlip = SymbolFlip;
                text.String = String;
            }
        }

        public virtual bool IsDuplicated(PlantEntity other, double iouThreshold, bool checkComponentClass)
        {
            Text otherText = other as Text;
            if (otherText == null)
                return false;

            double intersectingArea = Obb2.IntersectingArea(this.Extent, otherText.Extent);
            double overlapRatio1 = intersectingArea / this.Extent.Area();
            double overlapRatio2 = intersectingArea / otherText.Extent.Area();

            if (!(overlapRatio1 > iouThreshold || overlapRatio2 > iouThreshold))
                return false;

            if (checkComponentClass && !this.String.Equals(otherText))
                return false;

            return true;
        }
    }
}
