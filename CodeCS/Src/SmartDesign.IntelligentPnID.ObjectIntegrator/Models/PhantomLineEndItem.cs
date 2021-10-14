using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    /// <summary>
    /// 클래스 PhantomLineEndItem은 텍스트 통합을 위해서만 임시적으로 사용하는 클래스이다.
    /// </summary>
    public class PhantomLineEndItem : PlantItem
    {
        public PhantomLineEndItem()
        {
            LineItem = null;
        }

        protected PhantomLineEndItem(PhantomLineEndItem other)
        {
            LineItem = other.LineItem;
        }

        public LineItem LineItem { get; set; }

        public override PlantEntity Clone()
        {
            return new PhantomLineEndItem(this);
        }
    }
}
