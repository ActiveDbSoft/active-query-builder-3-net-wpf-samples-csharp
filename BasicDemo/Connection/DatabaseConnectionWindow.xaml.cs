//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using ActiveQueryBuilder.Core;
using GeneralAssembly;
using GeneralAssembly.Connection;

namespace BasicDemo.Connection
{
    /// <summary>
    /// Interaction logic for DatabaseConnectionWindow.xaml
    /// </summary>
    public partial class DatabaseConnectionWindow
    {
        public ConnectionInfo SelectedConnection
        {
            get
            {
                if (TabControl1.SelectedIndex == 0)
                {
                    if (LvConnections.SelectedItems.Count > 0)
                    {
                        var item = (ConnectionListItem) LvConnections.SelectedItem;
                        return (ConnectionInfo) item.Tag;
                    }

                    return null;
                }

                if (LvXmlFiles.SelectedItems.Count > 0)
                {
                    var item = (ConnectionListItem) LvXmlFiles.SelectedItem;
                    return (ConnectionInfo) item.Tag;
                }

                return null;
            }
        }

        public DatabaseConnectionWindow(bool showHint = false)
        {
            InitializeComponent();

            GridHint.Visibility = showHint ? Visibility.Visible : Visibility.Collapsed;

            var sourcelvConnection = new ObservableCollection<ConnectionListItem>();

            // fill connection list
            for (var i = 0; i < App.Connections.Count; i++)
            {
                sourcelvConnection.Add(new ConnectionListItem
                {
                    Name = App.Connections[i].Name,
                    Type = App.Connections[i].Type.ToString(),
                    Tag = App.Connections[i]
                });
            }

            LvConnections.ItemsSource = sourcelvConnection;

            if (LvConnections.Items.Count > 0)
            {
                LvConnections.SelectedItem = LvConnections.Items[0];
            }

            // add preset
            var found = false;
            var northwind = new ConnectionInfo(ConnectionTypes.MSSQL, "Northwind.xml", "Northwind.xml", true, "")
                {SyntaxProvider = new MSSQLSyntaxProvider()};

            for (var i = 0; i < App.XmlFiles.Count; i++)
            {
                if (App.XmlFiles[i].Equals(northwind))
                {
                    found = true;
                }
            }

            if (!found)
            {
                App.XmlFiles.Insert(0, northwind);
            }

            var sourceXmlfiles = new ObservableCollection<ConnectionListItem>();

            // fill XML files list
            for (var i = 0; i < App.XmlFiles.Count; i++)
            {
                sourceXmlfiles.Add(new ConnectionListItem
                {
                    Name = App.XmlFiles[i].Name,
                    Type = App.XmlFiles[i].Type.ToString(),
                    Tag = App.XmlFiles[i],
                    UserQueries = App.XmlFiles[i].UserQueries
                });
            }

            LvXmlFiles.ItemsSource = sourceXmlfiles;

            if (LvXmlFiles.Items.Count > 0)
            {
                LvXmlFiles.SelectedItem = LvXmlFiles.Items[0];
            }
            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += Hooks_DispatcherInactive;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive -= Hooks_DispatcherInactive;
        }

        private void Hooks_DispatcherInactive(object sender, EventArgs e)
        {
            ButtonRemoveConnection.IsEnabled = (LvConnections.SelectedItems.Count > 0);
            ButtonConfigureConnection.IsEnabled = (LvConnections.SelectedItems.Count > 0);
            ButtonConfigureXml.IsEnabled = (LvXmlFiles.SelectedItems.Count > 0);
            ButtonRemoveXml.IsEnabled = (LvXmlFiles.SelectedItems.Count > 0);

            if (TabControl1.SelectedIndex == 0)
            {
                BtnOk.IsEnabled = (LvConnections.SelectedItems.Count > 0);
            }
            else
            {
                BtnOk.IsEnabled = (LvXmlFiles.SelectedItems.Count > 0);
            }
        }

        private static string GetNewConnectionEntryName()
        {
            var x = 0;
            bool found;
            string name;

            do
            {
                x++;
                found = false;
                name = string.Format("Connection {0}", x);

                for (var i = 0; i < App.Connections.Count; i++)
                {
                    if (App.Connections[i].Name != name) continue;

                    found = true;
                    break;
                }
            } while (found);

            return name;
        }

        private static string GetNewXmlFileEntryName()
        {
            var x = 0;
            bool found;
            string name;

            do
            {
                x++;
                found = false;
                name = string.Format("XML File {0}", x);

                for (var i = 0; i < App.XmlFiles.Count; i++)
                {
                    if (App.XmlFiles[i].Name != name) continue;
                    found = true;
                    break;
                }
            } while (found);

            return name;
        }

        private void ButtonAddConnection_OnClick(object sender, RoutedEventArgs e)
        {
            var ci = new ConnectionInfo(ConnectionTypes.MSSQL, GetNewConnectionEntryName(), null, false,  "");

            var cef = new AddConnectionWindow(ci) {Owner = this};

            if (cef.ShowDialog() == true)
            {
                var item = new ConnectionListItem()
                {
                    Name = ci.Name,
                    Type = ci.Type.ToString(),
                    Tag = ci
                };

                var source = LvConnections.ItemsSource as ObservableCollection<ConnectionListItem>;
                if (source != null) source.Add(item);

                App.Connections.Add(ci);
                LvConnections.SelectedItem = item;
            }

            LvConnections.Focus();
            Properties.Settings.Default.XmlFiles = App.XmlFiles;
            
            Properties.Settings.Default.Connections = App.Connections;
            Properties.Settings.Default.Save();
        }

        private void ButtonRemoveConnection_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (ConnectionListItem) LvConnections.SelectedItem;

            if (item == null) return;

            var source = LvConnections.ItemsSource as ObservableCollection<ConnectionListItem>;
            if (source != null) source.Remove(item);
            App.Connections.Remove((ConnectionInfo) item.Tag);

            LvConnections.Focus();
        }

        private void ButtonConfigureConnection_OnClick(object sender, RoutedEventArgs e)
        {
            if (LvConnections.SelectedItem == null) return;
            var item = (ConnectionListItem) LvConnections.SelectedItem;

            var ci = (ConnectionInfo) item.Tag;

            var cef = new AddConnectionWindow(ci);

            if (cef.ShowDialog() == true)
            {
                item.Name = ci.Name;
                item.Type = ci.Type.ToString();
            }

            LvConnections.Focus();
        }

        private void ButtonAddXml_OnClick(object sender, RoutedEventArgs e)
        {
            var ci = new ConnectionInfo(ConnectionTypes.MSSQL, GetNewXmlFileEntryName(), null, true, "");

            var cef = new AddConnectionWindow(ci) {Owner = this};

            if (cef.ShowDialog() == true)
            {
                var item = new ConnectionListItem()
                {
                    Name = ci.Type.ToString(),
                    Type = ci.Type.ToString(),
                    Tag = ci
                };

                var source = LvXmlFiles.ItemsSource as ObservableCollection<ConnectionListItem>;
                if (source != null) source.Add(item);

                App.XmlFiles.Add(ci);
                LvXmlFiles.SelectedItem = item;
            }

            LvXmlFiles.Focus();

            Properties.Settings.Default.XmlFiles = App.XmlFiles;

            Properties.Settings.Default.Connections = App.Connections;
            Properties.Settings.Default.Save();
        }

        private void ButtonRemoveXml_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (ConnectionListItem) LvXmlFiles.SelectedItem;
            if (item == null) return;

            var source = LvXmlFiles.ItemsSource as ObservableCollection<ConnectionListItem>;
            if (source == null) return;

            source.Remove(item);

            App.XmlFiles.Remove((ConnectionInfo) item.Tag);

            LvXmlFiles.Focus();

            Properties.Settings.Default.XmlFiles = App.XmlFiles;

            Properties.Settings.Default.Connections = App.Connections;
            Properties.Settings.Default.Save();
        }

        private void ButtonConfigureXml_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (ConnectionListItem) LvXmlFiles.SelectedItem;
            if (item == null) return;

            var ci = (ConnectionInfo) item.Tag;

            var cef = new AddConnectionWindow(ci);

            if (cef.ShowDialog() == true)
            {
                item.Name = ci.Name;
                item.Type = ci.Type.ToString();
            }

            LvXmlFiles.Focus();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedConnection.SyntaxProvider == null)
            {
                switch (SelectedConnection.SyntaxProviderName)
                {
                    case "ANSI SQL-2003":
                        SelectedConnection.SyntaxProvider = new SQL2003SyntaxProvider();
                        break;
                    case "ANSI SQL-92":
                        SelectedConnection.SyntaxProvider = new SQL92SyntaxProvider();
                        break;
                    case "ANSI SQL-89":
                        SelectedConnection.SyntaxProvider = new SQL89SyntaxProvider();
                        break;
                    case "Firebird":
                        SelectedConnection.SyntaxProvider = new FirebirdSyntaxProvider();
                        break;
                    case "IBM DB2":
                        SelectedConnection.SyntaxProvider = new DB2SyntaxProvider();
                        break;
                    case "IBM Informix":
                        SelectedConnection.SyntaxProvider = new InformixSyntaxProvider();
                        break;
                    case "Microsoft Access":
                        SelectedConnection.SyntaxProvider = new MSAccessSyntaxProvider();
                        break;
                    case "Microsoft SQL Server":
                        SelectedConnection.SyntaxProvider = new MSSQLSyntaxProvider();
                        break;
                    case "MySQL":
                        SelectedConnection.SyntaxProvider = new MySQLSyntaxProvider();
                        break;
                    case "Oracle":
                        SelectedConnection.SyntaxProvider = new OracleSyntaxProvider();
                        break;
                    case "PostgreSQL":
                        SelectedConnection.SyntaxProvider = new PostgreSQLSyntaxProvider();
                        break;
                    case "SQLite":
                        SelectedConnection.SyntaxProvider = new SQLiteSyntaxProvider();
                        break;
                    case "Sybase":
                        SelectedConnection.SyntaxProvider = new SybaseSyntaxProvider();
                        break;
                    case "VistaDB":
                        SelectedConnection.SyntaxProvider = new VistaDBSyntaxProvider();
                        break;
                    case "Universal":
                        SelectedConnection.SyntaxProvider = new GenericSyntaxProvider();
                        break;
                }
            }
            DialogResult = true;
            Close();
        }

        private void ButtonBaseClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
