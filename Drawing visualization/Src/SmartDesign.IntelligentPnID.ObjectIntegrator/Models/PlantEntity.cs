using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public abstract class PlantEntity
    {
        public PlantEntity()
        {
            Parent = null;
            ID = Guid.NewGuid().ToString("N").ToUpper();
            DisplayName = ID;
            OriginalID = string.Empty;
            Children = new List<PlantEntity>();

            DebugInfo = new DebugInfo();

        }

        protected PlantEntity(PlantEntity other)
        {
            other.CopyPropertiesTo(this);
            
            Parent = other.Parent;

            Children = other.Children.Select(c => c.Clone()).ToList();
        }

        [Display(Order = 1)]
        [ReadOnly(true)]
        public string TypeName
        {
            get
            {
                return GetType().Name;
            }
        }

        [Display(Order = 2)]
        [ReadOnly(true)]
        public string ID
        {
            get;
            set;
        }

        [ReadOnly(false)]
        [Display(Order = 3)]
        public string OriginalID { get; set; }


        [Display(Order = 4)]
        public string DisplayName
        {
            get;
            set;
        }

        //[Browsable(false)]
        public PlantModel PlantModel 
        { 
            get
            {
                if (this is PlantModel plantModel)
                    return plantModel;
                else
                {
                    if (Parent == null)
                        return null;
                    else
                        return Parent.PlantModel;
                }
            }
        }

        //[Browsable(false)]
        public PlantEntity Parent { get; set; }

        //[Browsable(false)]
        public List<PlantEntity> Children { get; private set; }
                
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DebugInfo DebugInfo { get; private set; }

        public void Add(PlantEntity entity)
        {
            Children.Add(entity);
            entity.Parent = this;
        }

        public void Remove(PlantEntity entity)
        {
            Children.Remove(entity);
            entity.Parent = null;
        }

        public List<PlantEntity> GetAllDecendants()
        {
            List<PlantEntity> decendants = new List<PlantEntity>();

            foreach(var child in Children)
            {
                decendants.Add(child);
                decendants.AddRange(child.GetAllDecendants());
            }

            return decendants;
        }

        public abstract PlantEntity Clone();

        public virtual PlantEntity CloneNew()
        {
            PlantEntity clone = Clone();
            clone.ID = Guid.NewGuid().ToString("N").ToUpper();
            clone.DisplayName = clone.ID;
            return clone;
        }

        public virtual void CopyPropertiesTo(PlantEntity plantEntity)
        {
            plantEntity.ID = ID;
            plantEntity.OriginalID = OriginalID;
            plantEntity.DisplayName = DisplayName;            

            plantEntity.DebugInfo = DebugInfo.Clone();
        }

        public void TraverseDepthFirst(List<PlantEntity> entities)
        {
            entities.Add(this);

            foreach (var child in Children)
            {
                child.TraverseDepthFirst(entities);
            }
        }
    }
}
