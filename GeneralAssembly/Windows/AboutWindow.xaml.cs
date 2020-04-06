//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Diagnostics;
using System.Reflection;
using System.Windows.Navigation;
using ActiveQueryBuilder.View.WPF;

namespace GeneralAssembly.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            LblQueryBuilderVersion.Text = "v" + typeof(QueryBuilder).Assembly.GetName().Version;
            LblDemoVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void Hyperlink1_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
