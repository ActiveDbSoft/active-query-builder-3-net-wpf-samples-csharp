//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using ActiveQueryBuilder.Core;
using FullFeaturedMdiDemo.Properties;

namespace FullFeaturedMdiDemo
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Name = "Active Query Builder Demo";

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
            {
                Connections = Settings.Default.Connections;
            }

            if (Settings.Default.XmlFiles != null)
            {
                XmlFiles = Settings.Default.XmlFiles;
            }

            Settings.Default.Connections = Connections;
            Settings.Default.XmlFiles = XmlFiles;
            Settings.Default.Save();
        }
    }
}
