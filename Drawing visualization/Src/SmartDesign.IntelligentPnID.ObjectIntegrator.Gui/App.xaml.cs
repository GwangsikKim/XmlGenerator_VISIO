using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

// log4net configuration을 app.config에서 읽는다. watch = true는 app.config 변경 시 이를 반영하는 옵션
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace SmartDesign.IntelligentPnID.ObjectIntegrator.Gui
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (log.IsInfoEnabled)
                log.Info("프로그램 시작");

            MainWindow mainWindow = new MainWindow();

            if (e.Args.Length > 0)
            {
                bool result = mainWindow.SetCommandLineArguments(e.Args);
                if(!result)
                {
                    if (log.IsErrorEnabled)
                        log.Error("명령행 인자 오류로 프로그램을 종료.");

                    Current.Shutdown(1);
                    return;
                }
            }

            Current.MainWindow = mainWindow;
            Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (log.IsInfoEnabled)
                log.InfoFormat("프로그램 종료(Exit code: {0})", e.ApplicationExitCode);
        }
    }
}
