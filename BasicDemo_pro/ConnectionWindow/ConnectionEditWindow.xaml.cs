//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2024 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.PropertiesEditors;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Images;
using GeneralAssembly;
using Helpers = ActiveQueryBuilder.Core.Helpers;

namespace BasicDemo.ConnectionWindow
{
    /// <summary>
    /// Interaction logic for ConnectionEditWindow.xaml
    /// </summary>
    public partial class ConnectionEditWindow
    {
        private class ListViewItem
        {
            public string Name { get; set; }
            public CImage Icon { get; set; }
        }

        private BaseConnectionDescriptor _connection;

        public BaseConnectionDescriptor Connection => _connection;

        public ConnectionEditWindow()
        {
            InitializeComponent();
            _connection = new MSSQLConnectionDescriptor();

            FillConnectionTypes();
            FillSyntaxTypes();

            cbConnectionType.SelectedItem = _connection.GetDescription();
            cbLoadFromDefaultDatabase.Visibility = _connection.SyntaxProvider.IsSupportDatabases()
                ? Visibility.Visible
                : Visibility.Collapsed;
            cbLoadFromDefaultDatabase.IsChecked =
                _connection.MetadataLoadingOptions.LoadDefaultDatabaseOnly;

            UpdateConnectionPropertiesFrames();
        }

        private void FillConnectionTypes()
        {
            foreach (var name in Misc.ConnectionDescriptorNames)
            {
                cbConnectionType.Items.Add(name);
            }
        }

        private void FillSyntaxTypes()
        {
            foreach (Type syntax in Helpers.SyntaxProviderList)
            {
                var instance = Activator.CreateInstance(syntax) as BaseSyntaxProvider;
                cbSyntax.Items.Add(instance.Description);
            }
        }

        private void liFilter_Selected(object sender, RoutedEventArgs e)
        {
            if (tpFilter != null)
                InitializeFilterPage();
        }

        private void liConnection_Selected(object sender, RoutedEventArgs e)
        {
            if (tpConnection != null)
                tpConnection.IsSelected = true;
        }

        private bool _isFilterPageInitialized;

        private void InitializeFilterPage()
        {
            if (!_isFilterPageInitialized)
            {
                Cursor = Cursors.Wait;
                try
                {
                    databaseSchemaView1.SQLContext = _connection.GetSqlContext();
                    ClearFilters(databaseSchemaView1.SQLContext.LoadingOptions);
                    databaseSchemaView1.InitializeDatabaseSchemaTree();
                    LoadFilters();
                    _isFilterPageInitialized = true;
                }
                catch
                {
                    _isFilterPageInitialized = false;
                }
                finally
                {
                    Cursor = null;
                }
            }

            tpFilter.IsSelected = true;
        }

        private void ClearFilters(MetadataLoadingOptions options)
        {
            options.ExcludeFilter.Objects.Clear();
            options.IncludeFilter.Objects.Clear();
            options.ExcludeFilter.Schemas.Clear();
            options.IncludeFilter.Schemas.Clear();
        }

        private void LoadFilters()
        {
            LoadIncludeFilters();
            LoadExcludeFilters();
        }

        private void LoadIncludeFilters()
        {
            var filter = _connection.MetadataLoadingOptions.IncludeFilter;
            LoadFilterTo(filter, lvInclude);
        }

        private void LoadExcludeFilters()
        {
            var filter = _connection.MetadataLoadingOptions.ExcludeFilter;
            LoadFilterTo(filter, lvExclude);
        }

        private void LoadFilterTo(MetadataSimpleFilter filter, ListView listBox)
        {
            foreach (var filterObject in filter.Objects)
            {
                var item = FindItemByName(filterObject);
                listBox.Items.Add(new ListViewItem {Name = filterObject, Icon = GetImage(item)});
            }

            foreach (var filterSchema in filter.Schemas)
            {
                var item = FindItemByName(filterSchema);
                listBox.Items.Add(new ListViewItem {Name = filterSchema, Icon = GetImage(item)});
            }
        }

        private MetadataItem FindItemByName(string name)
        {
            return databaseSchemaView1.MetadataStructure.MetadataItem.FindItem<MetadataItem>(name);
        }

        private void CbConnectionType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var descriptorType = GetSelectedDescriptorType();
            if (_connection != null && _connection.GetType() == descriptorType)
            {
                return;
            }

            _connection = CreateConnectionDescriptor(descriptorType);

            if (_connection == null)
            {
                LockUI();
                return;
            }
            else
                UnlockUI();

            UpdateConnectionPropertiesFrames();
        }

        private void LockUI()
        {            
            lbMenu.IsEnabled = false;
            ButtonOk.IsEnabled = false;
            cbLoadFromDefaultDatabase.Visibility = Visibility.Collapsed;

            RemoveConnectionPropertiesFrame();
            RemoveSyntaxFrame();
        }

        private void UnlockUI()
        {
            lbMenu.IsEnabled = true;
            ButtonOk.IsEnabled = true;
            cbLoadFromDefaultDatabase.Visibility = Visibility.Visible;
        }

        private void UpdateConnectionPropertiesFrames()
        {
            SetupSyntaxCombobox();
            RecreateConnectionFrame();
            RecreateSyntaxFrame();
        }

        private void SetupSyntaxCombobox()
        {
            if (IsGenericConnection())
            {
                rowSyntax.Height = new GridLength(25);
                cbSyntax.SelectedItem = _connection.SyntaxProvider.Description;
            }
            else
            {
                rowSyntax.Height = new GridLength(0);
            }
        }

        private bool IsGenericConnection()
        {
            return _connection is ODBCConnectionDescriptor || _connection is OLEDBConnectionDescriptor;
        }

        private Type GetSelectedDescriptorType()
        {
            return Misc.ConnectionDescriptorList[cbConnectionType.SelectedIndex];
        }

        private BaseConnectionDescriptor CreateConnectionDescriptor(Type type)
        {
            try
            {
                return Activator.CreateInstance(type) as BaseConnectionDescriptor;
            }
            catch (Exception e)
            {
                var message = e.InnerException != null ? e.InnerException.Message : e.Message;
                MessageBox.Show(message + "\r\n \r\n" +
                                "To fix this error you may need to install the appropriate database client software or \r\n re-compile the project from sources and add the needed assemblies to the References section.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return null;
            }
        }

        private void RecreateConnectionFrame()
        {
            RemoveConnectionPropertiesFrame();

            pbConnection.ClearProperties();
            var container =
                PropertiesFactory.GetPropertiesContainer(_connection.MetadataProperties);
            (pbConnection as IPropertiesControl).SetProperties(container);
        }

        private void RemoveConnectionPropertiesFrame()
        {            
            ClearProperties(_connection.MetadataProperties);
        }

        private void ClearProperties(ObjectProperties properties)
        {
            properties.GroupProperties.Clear();
            properties.PropertiesEditors.Clear();
        }

        private void RecreateSyntaxFrame()
        {
            RemoveSyntaxFrame();
            var syntxProps = _connection.SyntaxProperties;
            if (syntxProps == null)
                return;
            
            var container = PropertiesFactory.GetPropertiesContainer(syntxProps);
            (pbSyntax as IPropertiesControl).SetProperties(container);

            cbLoadFromDefaultDatabase.Visibility = _connection.SyntaxProvider.IsSupportDatabases()
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void RemoveSyntaxFrame()
        {
            pbSyntax.ClearProperties();
            var syntxProps = _connection.SyntaxProperties;
            if (syntxProps == null)
                return;

            ClearProperties(syntxProps);
        }

        private void CbSyntax_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsGenericConnection())
            {
                return;
            }

            var syntaxType = GetSelectedSyntaxType();
            if (_connection.SyntaxProvider.GetType() == syntaxType)
            {
                return;
            }

            _connection.SyntaxProvider = CreateSyntaxProvider(syntaxType);

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

        private void CbLoadFromDefaultDatabase_OnChecked(object sender, RoutedEventArgs e)
        {
            _connection.MetadataLoadingOptions.LoadDefaultDatabaseOnly =
                cbLoadFromDefaultDatabase.IsChecked ?? default(bool);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            if (tcFilter.SelectedItem == tpInclude)
            {
                AddIncludeFilter(databaseSchemaView1.SelectedItems);
            }
            else if (tcFilter.SelectedItem == tpExclude)
            {
                AddExcludeFilter(databaseSchemaView1.SelectedItems);
            }
        }

        private void AddIncludeFilter(MetadataStructureItem[] items)
        {
            var filter = _connection.MetadataLoadingOptions.IncludeFilter;
            foreach (var structureItem in items)
            {
                var metadataItem = structureItem.MetadataItem;
                if (metadataItem == null)
                {
                    continue;
                }

                if (metadataItem.Type.IsNamespace())
                {
                    filter.Schemas.Add(metadataItem.NameFull);
                    lvInclude.Items.Add(new ListViewItem {Name = metadataItem.NameFull, Icon = GetImage(metadataItem)});
                }
                else if (metadataItem.Type.IsObject())
                {
                    filter.Objects.Add(metadataItem.NameFull);
                    lvInclude.Items.Add(new ListViewItem {Name = metadataItem.NameFull, Icon = GetImage(metadataItem)});
                }
            }
        }

        private void AddExcludeFilter(MetadataStructureItem[] items)
        {
            var filter = _connection.MetadataLoadingOptions.ExcludeFilter;
            foreach (var structureItem in items)
            {
                var metadataItem = structureItem.MetadataItem;
                if (metadataItem == null)
                {
                    continue;
                }

                if (metadataItem.Type.IsNamespace())
                {
                    filter.Schemas.Add(metadataItem.NameFull);
                    lvExclude.Items.Add(new ListViewItem {Name = metadataItem.NameFull, Icon = GetImage(metadataItem)});
                }
                else if (metadataItem.Type.IsObject())
                {
                    filter.Objects.Add(metadataItem.NameFull);
                    lvExclude.Items.Add(new ListViewItem {Name = metadataItem.NameFull, Icon = GetImage(metadataItem)});
                }
            }
        }

        private void DeleteFilter(string itemName)
        {
            MetadataSimpleFilter filter = null;
            if (tcFilter.SelectedItem == tpInclude)
            {
                filter = _connection.MetadataLoadingOptions.IncludeFilter;
            }
            else if (tcFilter.SelectedItem == tpExclude)
            {
                filter = _connection.MetadataLoadingOptions.ExcludeFilter;
            }

            if (filter != null)
            {
                filter.Objects.Remove(itemName);
                filter.Schemas.Remove(itemName);
            }

            if (tcFilter.SelectedItem == tpInclude)
            {
                var item = lvInclude.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Name == itemName);
                if (item != null)
                {
                    lvInclude.Items.Remove(item);
                }
            }
            else if (tcFilter.SelectedItem == tpExclude)
            {
                var item = lvExclude.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Name == itemName);
                if (item != null)
                {
                    lvExclude.Items.Remove(item);
                }
            }
        }

        private CImage GetImage(MetadataItem item)
        {
            if (item == null)
            {
                return null;
            }

            switch (item.Type)
            {
                case MetadataType.Server:
                    return Metadata.Server.Value;
                case MetadataType.Database:
                    return Metadata.Database.Value;
                case MetadataType.Schema:
                    return Metadata.Schema.Value;
                case MetadataType.Package:
                    return Metadata.Package.Value;
                case MetadataType.Table:
                    return Metadata.UserTable.Value;
                case MetadataType.View:
                    return Metadata.UserView.Value;
                case MetadataType.Procedure:
                    return Metadata.UserProcedure.Value;
                case MetadataType.Synonym:
                    return Metadata.UserSynonym.Value;
                default:
                    return null;
            }
        }

        private void BtnRemove_OnClick(object sender, RoutedEventArgs e)
        {
            if (tcFilter.SelectedItem == tpInclude)
            {
                var itemsToDelete = new List<ListViewItem>();
                foreach (ListViewItem selectedItem in lvInclude.SelectedItems)
                {
                    itemsToDelete.Add(selectedItem);
                }

                foreach (var item in itemsToDelete)
                {
                    DeleteFilter(item.Name);
                }
            }
            else if (tcFilter.SelectedItem == tpExclude)
            {
                var itemsToDelete = new List<ListViewItem>();
                foreach (ListViewItem selectedItem in lvExclude.SelectedItems)
                {
                    itemsToDelete.Add(selectedItem);
                }

                foreach (var item in itemsToDelete)
                {
                    DeleteFilter(item.Name);
                }
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

        private void LvExclude_OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }

        private void LvInclude_OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
        }

        private void DropItems(DragEventArgs e, bool toInclude)
        {
            var dragObject = e.Data.GetData(e.Data.GetFormats()[0]) as MetadataDragObject;
            if (dragObject != null)
            {
                if (toInclude)
                {
                    AddIncludeFilter(dragObject.MetadataStructureItems.ToArray());
                }
                else
                {
                    AddExcludeFilter(dragObject.MetadataStructureItems.ToArray());
                }
            }
        }

        private void LvInclude_OnDrop(object sender, DragEventArgs e)
        {
            DropItems(e, true);
        }

        private void LvExclude_OnDrop(object sender, DragEventArgs e)
        {
            DropItems(e, false);
        }

        private void DatabaseSchemaView1_OnItemDoubleClick(object sender, MetadataStructureItem item)
        {
            BtnAdd_OnClick(this, null);
        }
    }
}
