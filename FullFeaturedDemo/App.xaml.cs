//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows.Threading;
using FullFeaturedDemo.Properties;
using GeneralAssembly;
using GeneralAssembly.Windows;

namespace FullFeaturedDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static ConnectionList Connections = new ConnectionList();
        public static ConnectionList XmlFiles = new ConnectionList();

        public App()
        {
            //if new version, import upgrade from previous version
            if (Settings.Default.CallUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CallUpgrade = false;
            }

            if (Settings.Default.Connections != null)
                Connections = Settings.Default.Connections;

            if (Settings.Default.XmlFiles != null)
                XmlFiles = Settings.Default.XmlFiles;

            Connections.RestoreData();
            XmlFiles.RestoreData();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var errorWindow = new ExceptionWindow
            {
                Owner = Current.MainWindow,
                Message = e.Exception.Message,
                StackTrace = e.Exception.StackTrace
            };

            errorWindow.ShowDialog();
        }
    }
}
