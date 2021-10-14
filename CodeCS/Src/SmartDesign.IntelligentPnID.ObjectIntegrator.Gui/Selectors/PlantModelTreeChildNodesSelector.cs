using DevExpress.Xpf.Grid;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Selectors
{
    public class PlantModelTreeChildNodesSelector : IChildNodesSelector
    {
        public IEnumerable SelectChildren(object item)
        {
            if (item is PlantEntity plantEntity)
                return plantEntity.Children;

            return null;
        }
    }
}
