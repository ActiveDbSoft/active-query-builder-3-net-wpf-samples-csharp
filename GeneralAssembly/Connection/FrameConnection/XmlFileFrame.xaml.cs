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
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace GeneralAssembly.Connection.FrameConnection
{
    /// <summary>
    /// Interaction logic for XmlFileFrame.xaml
    /// </summary>
    public partial class XmlFileFrame : IConnectionFrame
    {
        private OpenFileDialog _openFileDialog1;
        public XmlFileFrame()
        {
            InitializeComponent();
        }

        public string ConnectionString
        {
            get { return tbXmlFile.Text; }
            set { tbXmlFile.Text = value; }
        }

        public event SyntaxProviderDetected OnSyntaxProviderDetected;

        public void SetServerType(string serverType)
        {

        }

        public XmlFileFrame(string xmlFilePath)
        {
            InitializeComponent();

            tbXmlFile.Text = xmlFilePath;
        }

        public bool TestConnection()
        {
            if (!string.IsNullOrEmpty(ConnectionString)) return true;

            MessageBox.Show("Invalid Xml file path.", Assembly.GetEntryAssembly().GetName().Name);

            return false;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            _openFileDialog1 = new OpenFileDialog {FileName = ConnectionString};

            if (_openFileDialog1.ShowDialog() == true)
            {
                ConnectionString = _openFileDialog1.FileName;
            }
        }
    }
}
