using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class PlantModel : PlantEntity
    {
        public PlantModel()
        {
            FullPath = string.Empty;
            Extent = Obb2.Create(new Position2(0.0, 0.0), new Position2(1024, 768));
        }

        protected PlantModel(PlantModel other) : base(other)
        {
        }

        [Browsable(false)]

        public double Width
        {
            get { return Extent.GlobalMax.X; }
        }

        public double Height
        {
            get { return Extent.GlobalMax.Y; }
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [ReadOnly(true)]
        public Obb2 Extent
        {
            get;
            set;
        }
        
        private string fullPath;
        
        [ReadOnly(true)]
        public string FullPath
        {
            get { return fullPath; }
            set
            {
                fullPath = value;
                DisplayName = FileName;
            }
        }

        public string FileName
        {
            get { return Path.GetFileName(FullPath); }
        }

        public List<Equipment> Equipments
        {
            get
            {
                return Children.OfType<Equipment>().ToList();
            }
        }

        public List<Instrument> Instruments
        {
            get
            {
                return Children.OfType<Instrument>().ToList();
            }
        }

        public List<PipingComponent> PipingComponents
        {
            get
            {
                return Children.OfType<PipingComponent>().ToList();
            }
        }

        public List<PipeConnectorSymbol> PipeConnectorSymbols
        {
            get
            {
                return Children.OfType<PipeConnectorSymbol>().ToList();
            }
        }

        public List<SignalConnectorSymbol> SignalConnectorSymbols
        {
            get
            {
                return Children.OfType<SignalConnectorSymbol>().ToList();
            }
        }

        public List<PipingNetworkSegment> PipingNetworkSegments
        {
            get
            {
                return Children.OfType<PipingNetworkSegment>().ToList();
            }
        }

        public List<SignalLine> SignalLines
        {
            get
            {
                return Children.OfType<SignalLine>().ToList();
            }
        }

        public List<Text> Texts
        {
            get
            {
                return Children.OfType<Text>().ToList();
            }
        }

        public List<Connection> Connections
        {
            get
            {
                return Children.OfType<Connection>().ToList();
            }
        }

        public List<UnknownSymbol> UnknownSymbols
        {
            get
            {
                return Children.OfType<UnknownSymbol>().ToList();
            }
        }

        public List<UnknownLine> UnknownLines
        {
            get
            {
                return Children.OfType<UnknownLine>().ToList();
            }
        }

        public override PlantEntity Clone()
        {
            return new PlantModel(this);
        }

        public override void CopyPropertiesTo(PlantEntity plantEntity)
        {
            base.CopyPropertiesTo(plantEntity);

            if(plantEntity is PlantModel plantModel)
            {
                plantModel.FullPath = FullPath;
                plantModel.Extent = Extent;
            }            
        }

        public PlantEntity Find(PlantEntity startEntity, string property, string content, bool caseSensitive)
        {
            if (!caseSensitive)
                content = content.ToLower();

            List<PlantEntity> entities = new List<PlantEntity>();
            TraverseDepthFirst(entities);

            int startIndex = -1;

            if(startEntity != null)
            {
                startIndex = entities.IndexOf(startEntity);
            }

            var targetEntities = entities.Skip(startIndex + 1);

            foreach(var targetEntity in targetEntities)
            {
                string targetValue = string.Empty;

                if(property == "ID")
                {
                    targetValue = targetEntity.ID;
                }
                else if(property == "DisplayName")
                {
                    targetValue = targetEntity.DisplayName;
                }
                else if (property == "OriginalID")
                {
                    targetValue = targetEntity.OriginalID;
                }
                else if (property == "TypeName")
                {
                    targetValue = targetEntity.TypeName;
                }
                else if (property == "ComponentClass")
                {
                    if(targetEntity is PlantItem plantItem)
                    {
                        targetValue = plantItem.ComponentClass;
                    }
                }
                else if(property == "String")
                {
                    if (targetEntity is Text text)
                    {
                        targetValue = text.String;
                    }
                }

                if (targetValue != null && !caseSensitive)
                    targetValue = targetValue.ToLower();

                if (targetValue.Contains(content))
                    return targetEntity;
            }

            return null;
        }
    }
}
