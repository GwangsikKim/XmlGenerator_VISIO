using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    public class WaitCursor : IDisposable
    {
        private Cursor oldCursor = null;

        public WaitCursor()
        {
            oldCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = oldCursor;
        }
    }
}
