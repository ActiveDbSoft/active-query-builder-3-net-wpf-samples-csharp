//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows.Threading;
using GeneralAssembly.Windows;

namespace SubQueryTextEditingDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
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
