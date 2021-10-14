using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    /// <summary>
    /// FindWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FindWindow : ThemedWindow
    {
        public FindWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.DataContext = Items;

        }

        public List<string> Items = new List<string>()
            {
                "ID", "DisplayName", "OriginalID", "TypeName", "ComponentClass", "String"
            };

        private MainWindow mainWindow;

        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
            bool caseSensitive = (bool)caseSensitiveCheckEdit.EditValue;
            int itemIndex = itemEdit.SelectedIndex;

            mainWindow.FindNextItem(Items[itemIndex], contentTextEdit.EditValue.ToString(), caseSensitive);
        }

        private void ThemedWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
