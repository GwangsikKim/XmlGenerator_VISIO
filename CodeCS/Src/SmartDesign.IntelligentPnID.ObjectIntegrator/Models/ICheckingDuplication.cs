using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public interface ICheckingDuplication
    {
        bool IsDuplicated(PlantEntity other, double iouThreshold, bool checkComponentClass);
    }
}
