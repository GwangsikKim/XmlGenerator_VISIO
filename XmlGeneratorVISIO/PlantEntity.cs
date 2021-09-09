using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLGeneratorVISIO;

namespace XmlGeneratorVISIO
{
    class PlantEntity
    {
        public PlantEntity()
        {
            plantModels = new List<PlantModel>();
        }

        public List<PlantModel> plantModels
        {
            get;
            set;
        }
    }
}
