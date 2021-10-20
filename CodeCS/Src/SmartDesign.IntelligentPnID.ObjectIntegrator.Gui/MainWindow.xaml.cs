using DevExpress.Xpf.Core;
using Microsoft.Win32;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using SmartDesign.IntelligentPnID.ObjectIntegrator.ObjectRecognition;
using SmartDesign.MathUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string DefaultDocumentName = "Noname.xml";

        public MainWindow()
        {
            InitializeComponent();
            IsServerMode = false;
            IsDirty = false;
            DocumentFilePath = string.Empty;
            EnableIntegration = false;
        }        

        private bool IsServerMode { get; set; }
        private InterfaceConfiguration InterfaceConfiguration { get; set; }

        private bool EnableIntegration { get; set; }

        private bool IsDirty { get; set; }
        private string DocumentFilePath { get; set; }

        private PlantModel PlantModel { get; set; }
        private List<PlantEntity> ModelTreeRoot { get; set; }

        //private FindWindow findWindow = null;

        public bool SetCommandLineArguments(string[] args)
        {
            if(log.IsInfoEnabled)
            {
                log.Info($"명령행 인자 개수: {args.Length}");
                foreach (var arg in args)
                    log.Info($"명령행 인자: {arg}");
            }

            if (args.Length == 0 || args == null)
            {
                IsServerMode = false;
                return true;
            }

            if (args.Length != 1)
            {
                string message = "전달받은 명령행 인자의 개수가 1개가 아닙니다. 명령행 인자를 확인해 주세요.";
                
                if (log.IsErrorEnabled)
                    log.Error(message);

                ThemedMessageBox.Show("오류", message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(args[0]))
            {
                string message = $"인터페이스 파일을 찾을 수 없습니다. 경로: {args[0]}";
                
                if (log.IsErrorEnabled)
                    log.Error(message);

                ThemedMessageBox.Show("오류", message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            try
            {
                ReadInterfaceFile(args[0]);
            }
            catch (Exception e)
            {
                string message = $"인터페이스 파일을 읽는데 실패했습니다. 오류: {e.Message}";

                if (log.IsErrorEnabled)
                    log.Error(message);

                ThemedMessageBox.Show("오류", message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            IsServerMode = true;

            return true;
        }

        private void ReadInterfaceFile(string path)
        {
            InterfaceFileReader reader = new InterfaceFileReader();
            InterfaceConfiguration = reader.Read(path);
        }

        private void Clear()
        {
            PlantModel = new PlantModel();
            PlantModel.FullPath = DefaultDocumentName;
            DocumentFilePath = string.Empty;
            IsDirty = false;
            Zoom(1.0);
        }

        private void NewBarButtonItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            using (new WaitCursor())
            {
                Clear();

                Refresh();
                UpdateTitle();
            }
        }

        private void ImportBarButtonItem_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false; //중복금지

            openFileDialog.Filter = "xml 파일 (*.xml)|*.xml"; //확장자 선택
            openFileDialog.Title = "파일 가져오기";

            if (openFileDialog.ShowDialog() == true)
            {
                using (new WaitCursor())
                {
                    string path = openFileDialog.FileName;
                    ImportFile(path);                    
                    UpdateTitle();
                }
            }
        }

        private void XmlFile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ImportFile(files[0]);                    
            }
        }

        //xml파일 변환
        private void ImportFile(string fileName)
        {
            ObjectRecognitionFileReader reader = new ObjectRecognitionFileReader();
            Annotation annotation = reader.Read(fileName);

            if (annotation != null)
            {
                AnnotationToPlantModelConverter converter = new AnnotationToPlantModelConverter();
                PlantModel = converter.Convert(annotation);
                PlantModel.FullPath = fileName;

                EnableIntegration = true;
                IsDirty = true;
                Refresh();
            }
            else
            {
                string message = "불러온 파일이 형식에 맞지 않습니다.";

                if (log.IsErrorEnabled)
                    log.Error(message);

                ThemedMessageBox.Show("오류", message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //tree생성 및 속성 
        private void InitializeModelTree()
        {
            ModelTreeRoot = new List<PlantEntity>();
            ModelTreeRoot.Add(PlantModel);

            plantModelTreeListControl.ItemsSource = ModelTreeRoot;
            plantModelTreeListControl.SelectedItem = null;
        }

        private void Redraw()
        {
            drawingPaper.InvalidateVisual();
            plantModelScrollViewer.UpdateLayout();
        }

        private void Refresh()
        {
            drawingPaper.PlantModel = PlantModel;
            drawingPaper.InvalidateVisual();

            plantModelScrollViewer.UpdateLayout();            

            // 순서 중요함
            InitializeModelTree();
        }

        private void PlantModelTreeListControl_SelectedItemChanged(object sender, DevExpress.Xpf.Grid.SelectedItemChangedEventArgs e)
        {
            if (e.OldItem != null)
            {
                PlantEntity plantEntity = e.OldItem as PlantEntity;
                Debug.Assert(plantEntity != null);

                List<PlantEntity> selectedItems = new List<PlantEntity>();
                selectedItems.Add(plantEntity);
                selectedItems.AddRange(plantEntity.GetAllDecendants());

                foreach (var selectedItem in selectedItems)
                {
                    var shape = drawingPaper.FindByPlantEntity(selectedItem);
                    if (shape != null)
                    {
                        shape.IsSelected = false;
                    }
                }
            }

            if (e.NewItem != null)
            {
                PlantEntity plantEntity = e.NewItem as PlantEntity;
                Debug.Assert(plantEntity != null);

                List<PlantEntity> selectedItems = new List<PlantEntity>();
                selectedItems.Add(plantEntity);
                selectedItems.AddRange(plantEntity.GetAllDecendants());

                foreach (var selectedItem in selectedItems)
                {
                    var shape = drawingPaper.FindByPlantEntity(selectedItem);
                    if (shape != null)
                    {
                        shape.IsSelected = true;
                    }
                }

                plantModelPropertyGridControl.SelectedObject = plantEntity;
            }

            drawingPaper.InvalidateVisual();
        }

        private void ShowConnection_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            CreateConnectionLine createConnectionLine = new CreateConnectionLine();
            createConnectionLine.Creator(PlantModel);

            Refresh();
            Redraw();
        }

        private void FocusSelected_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var selectedItem = plantModelTreeListControl.SelectedItem;
            if (selectedItem == null)
                return;

            IHasExtent hasExtent = selectedItem as IHasExtent;
            if (hasExtent == null)
                return;

            double zoomFactor = drawingPaper.ZoomFactor;

            Position2 center = hasExtent.Extent.Center;
            double displacementX = center.X * zoomFactor - plantModelScrollViewer.ActualWidth * 0.5;
            double displacementY = (PlantModel.Height - center.Y) * zoomFactor - plantModelScrollViewer.ActualHeight * 0.5;

            plantModelScrollViewer.ScrollToHorizontalOffset(displacementX);
            plantModelScrollViewer.ScrollToVerticalOffset(displacementY);   
        }

        private void UpdateTitle()
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            string title = fileVersionInfo.ProductName;
            title += " - ";

            if (string.IsNullOrEmpty(DocumentFilePath))
                title += DefaultDocumentName;
            else
                title += DocumentFilePath;

            if(IsDirty)
            {
                title += "*";
            }

            if(IsServerMode)
            {
                title += " [통합모드]";
            }

            Title = title;
        }

        private void ThemedWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(IsDirty)
            {
                var result = ThemedMessageBox.Show("확인", "변경된 내용이 있습니다. 파일을 저장 하시겠습니까?", MessageBoxButton.YesNoCancel);

                if(result == MessageBoxResult.Yes)
                {

                }
                else if(result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void Zoom(double factor)
        {
            if (factor < 0.1 || factor > 4.0)
                throw new ArgumentOutOfRangeException("범위는 0.1에서 4.0 사이어야 합니다.", nameof(factor));            

            if (factor == 1.0)
                zoomEditItem.EditValue = 0.0;
            else if (factor < 1.0)
            {
                double value = 100.0 / 0.9 * (factor - 0.1) - 100.0;
                zoomEditItem.EditValue = value;
            }
            else if(factor > 1.0)
            {
                double value = 100.0 / 3.0 * (factor - 1.0);
                zoomEditItem.EditValue = value;
            }            
        }

        private void ZoomEditItem_EditValueChanged(object sender, RoutedEventArgs e)
        {
            double value = (double)zoomEditItem.EditValue;
            double factor = 1.0;

            if (value == 0.0)
                factor = 1.0;
            else if (value < 0.0)
            {
                factor = (0.9 * value + 100.0) / 100.0;
            }
            else if (value > 1.0)
            {
                factor = 3.0 * value / 100.0 + 1.0;
            }

            drawingPaper.ZoomFactor = factor;

            zoomEditItem.Content = string.Format("{0}%", (int)(factor * 100));

            drawingPaper.ResetSize();
            Redraw();
        }

        private void ZoomFitButton_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            const double Margin = 20.0;

            double zoomToWidth = (plantModelScrollViewer.ActualWidth - Margin) / PlantModel.Width;
            double zoomToHeight = (plantModelScrollViewer.ActualHeight - Margin) / PlantModel.Height;
            double zoomFactor = System.Math.Min(zoomToWidth, zoomToHeight);

            Zoom(zoomFactor);
        }

        private void AxesVisibleEditItem_EditValueChanged(object sender, RoutedEventArgs e)
        {
            drawingPaper.IsAxesVisible = (bool)this.axesVisibleEditItem.EditValue;
            Redraw();
        }

        public void FindNextItem(string property, string content, bool caseSensitive)
        {
            PlantEntity selectedEntity = plantModelTreeListControl.SelectedItem as PlantEntity;
            PlantEntity foundEntity = PlantModel.Find(selectedEntity, property, content, caseSensitive);

            if(foundEntity != null)
            {
                plantModelTreeListControl.SelectedItem = foundEntity;
            }
        }
    }
}
