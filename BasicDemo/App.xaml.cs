//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace BasicDemo
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
	    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	    {
	        using (var writer = new StreamWriter("log.txt", true,Encoding.UTF8))
	        {
	            writer.WriteLine("========================\\\\=[{0}]=\\\\========================", DateTime.Now);
	            writer.WriteLine("Message: " + e.Exception.Message);
                writer.WriteLine("Stack: \n" + e.Exception.StackTrace);
	            if (e.Exception.InnerException != null)
	            {
	                writer.WriteLine("Inner Message: " + e.Exception.InnerException.Message);
	                writer.WriteLine("inner Stack: \n" + e.Exception.InnerException.StackTrace);
                }
	            writer.WriteLine("===============================================================");

	            MessageBox.Show(string.Format("Message: {0}\nStack: \n{1}", e.Exception.Message, e.Exception.StackTrace));
	        }   
	    }
	}
}
