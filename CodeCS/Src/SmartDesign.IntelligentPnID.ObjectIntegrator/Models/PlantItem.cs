using SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PlantItem : PlantEntity, IHasExtent, ICheckingDuplication
    {
        public static string ValveStatusDefault = "None";
        public static string ComponentClassDefault = "unknown";

        public PlantItem()
        {
            TagName = string.Empty;
            ComponentClass = ComponentClassDefault;

            Extent = new Obb2();
            ExpandedExtent = Extent;
            SymbolFlip = Flip.None;
            ValveStatus = ValveStatusDefault;

            Attributes = new List<Attribute>();
        }

        protected PlantItem(PlantItem other) : base(other)
        {
        }

        public string TagName
        {
            get;
            set;
        }

        public string ComponentClass
        {
            get;
            set;
        }

        [ReadOnly(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Obb2 Extent
        {
            get;
            set;
        }

        /// <summary>
        /// 디버깅 목적으로 사용
        /// </summary>
        [ReadOnly(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Obb2 ExpandedExtent
        {
            get;
            set;
        }

        public Flip SymbolFlip
        {
            get;
            set;
        }

        public string ValveStatus
        {
            get;
            set;
        }

        [ReadOnly(true)]
        public List<Attribute> Attributes
        {
            get;
            set;
        }

        [ReadOnly(true)]
        public List<Text> Texts
        {
            get { return Children.OfType<Text>().ToList(); }
        }

        public override PlantEntity Clone()
        {
            return new PlantItem(this);
        }

        public override void CopyPropertiesTo(PlantEntity plantEntity)
        {
            base.CopyPropertiesTo(plantEntity);

            if (plantEntity is PlantItem plantItem)
            {
                plantItem.TagName = TagName;
                plantItem.ComponentClass = ComponentClass;

                plantItem.Extent = Extent;
                plantItem.ExpandedExtent = ExpandedExtent;
                plantItem.SymbolFlip = SymbolFlip;
                plantItem.ValveStatus = ValveStatus;

                plantItem.Attributes = Attributes.Select(a => a.Clone()).ToList();
            }
        }

        public virtual bool IsDuplicated(PlantEntity other, double iouThreshold, bool checkComponentClass)
        {
            PlantItem otherItem = other as PlantItem;
            if (otherItem == null)
                return false;

            double intersectingArea = Obb2.IntersectingArea(this.Extent, otherItem.Extent);
            double overlapRatio1 = intersectingArea / this.Extent.Area();
            double overlapRatio2 = intersectingArea / otherItem.Extent.Area();

            if (!(overlapRatio1 > iouThreshold || overlapRatio2 > iouThreshold))
                return false;

            if (checkComponentClass && !this.ComponentClass.Equals(otherItem.ComponentClass))
                return false;

            return true;
        }
    }
}
