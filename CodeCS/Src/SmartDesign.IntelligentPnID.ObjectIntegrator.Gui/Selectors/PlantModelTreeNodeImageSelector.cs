using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Grid;
using SmartDesign.IntelligentPnID.ObjectIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui.Selectors
{
    public class PlantModelTreeNodeImageSelector : TreeListNodeImageSelector
    {
        //이미지 파일
        public ImageSource PlantModelImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/PlantModel.svg", UriKind.Absolute));
        public BitmapImage PipingNetworkSegmentimage = new BitmapImage(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/PipingNetworkSegment.ico", UriKind.Absolute));
        public ImageSource TeeImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/Tee.svg", UriKind.Absolute));
        public ImageSource CrossImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/Cross.svg", UriKind.Absolute));
        public ImageSource SignalLineImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/SignalLine.svg", UriKind.Absolute));
        public ImageSource SignalBranchImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/SignalBranch.svg", UriKind.Absolute));
        public ImageSource TextImage = WpfSvgRenderer.CreateImageSource(DXImageHelper.GetImageUri("SvgImages/Spreadsheet/Text2.svg"));
        public ImageSource PipingComponentImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/PipingComponent.svg", UriKind.Absolute));
        public ImageSource NozzleImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/Nozzle.svg", UriKind.Absolute));
        public ImageSource PipeConnectorSymbolImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/PipeConnectorSymbol.svg", UriKind.Absolute));
        public ImageSource EquipmentImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/Equipment.svg", UriKind.Absolute));
        public ImageSource InstrumentImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/Instrument.svg", UriKind.Absolute));
        public ImageSource SignalConnectorSymbolImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/SignalConnectorSymbol.svg", UriKind.Absolute));
        public BitmapImage PipingNetworkSystemImage = new BitmapImage(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/PipingNetworkSystem.ico", UriKind.Absolute));
        public ImageSource UnknownSymbolImage = WpfSvgRenderer.CreateImageSource(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/UnknownSymbol.svg", UriKind.Absolute));
        public BitmapImage UnknownLineImage = new BitmapImage(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/UnknownLine.ico", UriKind.Absolute));
        public BitmapImage ConnectionImage = new BitmapImage(new Uri("pack://application:,,,/SmartDesign.IntelligentPnID.ObjectIntegrator.Gui;component/Resources/Images/connect_icon_161112.png", UriKind.Absolute));


        //이미지 파일 적용
        public override ImageSource Select(DevExpress.Xpf.Grid.TreeList.TreeListRowData rowData)
        {
            if (rowData.Row is PlantModel)
                return PlantModelImage;

            else if (rowData.Row is Equipment)
                return EquipmentImage;

            else if (rowData.Row is Instrument)
                return InstrumentImage;

            else if (rowData.Row is Nozzle)
                return NozzleImage;

            else if (rowData.Row is PipeConnectorSymbol)
                return PipeConnectorSymbolImage;

            else if (rowData.Row is PipeTee)
                return TeeImage;

            else if (rowData.Row is PipeCross)
                return CrossImage;

            else if (rowData.Row is PipingComponent)
                return PipingComponentImage;

            else if (rowData.Row is PipingNetworkSegment)
                return PipingNetworkSegmentimage;

            else if (rowData.Row is SignalConnectorSymbol)
                return SignalConnectorSymbolImage;

            else if (rowData.Row is SignalLine)
                return SignalLineImage;

            else if (rowData.Row is SignalBranch)
                return SignalBranchImage;

            else if (rowData.Row is Text)
                return TextImage;

            else if (rowData.Row is UnknownSymbol)
                return UnknownSymbolImage;

            else if (rowData.Row is UnknownLine)
                return UnknownLineImage;

            else if (rowData.Row is Connection)
                return ConnectionImage;

            else
                throw new ArgumentException("지원하지 않는 클래스 타입입니다.", nameof(rowData));

        }
    }
}
