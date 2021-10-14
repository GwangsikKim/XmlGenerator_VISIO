using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartDesign.MathUtil;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Models
{
    public class DebugInfo : INotifyPropertyChanged
    {
        public DebugInfo()
        {
            ExpandedExtent = new Obb2();
            TextIntegrationCategory = string.Empty;
            TextIntegrationState = 0; // -1: Failed, 0: Unknown, 1: Sucessful
        }

        protected DebugInfo(DebugInfo other)
        {
            ExpandedExtent = other.ExpandedExtent;
            TextIntegrationCategory = other.TextIntegrationCategory;
            TextIntegrationState = other.TextIntegrationState;
        }

        /// <summary>
        /// 디버깅 목적으로 사용
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [ReadOnly(true)]
        public Obb2 ExpandedExtent
        {
            get;
            set;
        }

        [ReadOnly(true)]
        public string TextIntegrationCategory { get; set; }

        private int textIntegrationState;

        [ReadOnly(true)]
        public int TextIntegrationState
        {
            get { return textIntegrationState; }
            set
            {
                textIntegrationState = value;
                OnPropertyChanged(nameof(TextIntegrationState));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public virtual DebugInfo Clone()
        {
            return new DebugInfo(this);
        }
    }
}
