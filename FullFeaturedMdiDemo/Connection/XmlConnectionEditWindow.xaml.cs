//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.PropertiesEditors;
using GeneralAssembly;
using Microsoft.Win32;
using Helpers = ActiveQueryBuilder.Core.Helpers;

namespace FullFeaturedMdiDemo.Connection
{
    /// <summary>
    /// Interaction logic for XmlConnectionEditWindow.xaml
    /// </summary>
    public partial class XmlConnectionEditWindow
    {
        private readonly ConnectionInfo _connection;
        public XmlConnectionEditWindow()
        {
            InitializeComponent();
        }

        public XmlConnectionEditWindow(ConnectionInfo connection)
            : this()
        {
            _connection = connection;
            FillSyntaxTypes();

            tbConnectionName.Text = _connection.Name;
            tbXmlPath.Text = _connection.XMLPath;
            cbSyntax.SelectedItem = _connection.ConnectionDescriptor.SyntaxProvider.Description;

            RecreateSyntaxFrame();
        }

        private Type GetSelectedSyntaxType()
        {
            return Helpers.SyntaxProviderList[cbSyntax.SelectedIndex];
        }

        private BaseSyntaxProvider CreateSyntaxProvider(Type type)
        {
            return Activator.CreateInstance(type) as BaseSyntaxProvider;
        }

        private void RecreateSyntaxFrame()
        {
            pbSyntax.ClearProperties();
            var syntxProps = _connection.ConnectionDescriptor.SyntaxProperties;
            if (syntxProps == null)
            {
                pbSyntax.Visibility = Visibility.Collapsed;
                return;
            }

            pbSyntax.Visibility = Visibility.Visible;
            ClearProperties(syntxProps);
            var container = PropertiesFactory.GetPropertiesContainer(syntxProps);
            (pbSyntax as IPropertiesControl).SetProperties(container);            
        }

        private void ClearProperties(ObjectProperties properties)
        {
            properties.GroupProperties.Clear();
            properties.PropertiesEditors.Clear();
        }

        private void FillSyntaxTypes()
        {
            foreach (Type syntax in Helpers.SyntaxProviderList)
            {
                var instance = Activator.CreateInstance(syntax) as BaseSyntaxProvider;
                cbSyntax.Items.Add(instance.Description);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CbSyntax_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var syntaxType = GetSelectedSyntaxType();
            if (_connection.ConnectionDescriptor.SyntaxProvider.GetType() == syntaxType)
            {
                return;
            }

            _connection.ConnectionDescriptor.SyntaxProvider = CreateSyntaxProvider(syntaxType);
            _connection.SyntaxProviderName = syntaxType.ToString();
            RecreateSyntaxFrame();
        }

        private void TbConnectionName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _connection.Name = tbConnectionName.Text;
        }

        private void TbXmlPath_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _connection.XMLPath = tbXmlPath.Text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = @"XML files|*xml|All files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                tbXmlPath.Text = openFileDialog.FileName;
            }
        }
    }
}
