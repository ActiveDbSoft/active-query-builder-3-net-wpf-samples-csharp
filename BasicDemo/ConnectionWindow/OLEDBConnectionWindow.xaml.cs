//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Input;

namespace BasicDemo.ConnectionWindow
{
    /// <summary>
    /// Interaction logic for OLEDBConnectionWindow.xaml
    /// </summary>
    public partial class OLEDBConnectionWindow
    {
        public string ConnectionString = "";

        public OLEDBConnectionWindow()
        {
            InitializeComponent();
        }

        private void ButtonConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var builder = new OleDbConnectionStringBuilder();

            try
            {
                builder.ConnectionString = textBoxConnectionString.Text;

                Mouse.OverrideCursor = Cursors.Wait;

                using (var connection = new OleDbConnection(builder.ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        ConnectionString = builder.ConnectionString;
						DialogResult = true;
						Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to connect.");
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message, "Invalid OLE DB connection string.");
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
