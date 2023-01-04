//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2023 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ActiveQueryBuilder.Core;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for MetadataFilterItemControl.xaml
    /// </summary>
    internal partial class MetadataFilterItemControl
    {
        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;

        private readonly MetadataFilterItem _filterItem;
        private MetadataFilterItem _originalFilterItem;

        private List<string> _databaseList = new List<string>();
        private List<string> _schemaList = new List<string>();

        private bool _showServer;
        private bool _showDatabase = true;
        private bool _showSchema = true;
        private bool _showPackage;

        public event EventHandler ItemUpdated;

        private void DoItemUpdated()
        {
            ItemUpdated?.Invoke(this, EventArgs.Empty);
        }

        public MetadataFilterItem FilterItem
        {
            get { return _filterItem; }
            set
            {
                _originalFilterItem = value;
                _filterItem.Assign(value);
                UpdateControls();
            }
        }

        public List<string> DatabaseList
        {
            get { return _databaseList; }
            set
            {
                _databaseList = value;

                if (ComboBoxDatabase == null) return;

                ComboBoxDatabase.RemoveHandler(TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(ComboBoxDatabase_TextChanged));

                ComboBoxDatabase.Items.Clear();
                ComboBoxDatabase.Items.Add("");

                if (_databaseList != null)
                {
                    foreach (var item in _databaseList)
                        ComboBoxDatabase.Items.Add(item);
                }

                ComboBoxDatabase.AddHandler(TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(ComboBoxDatabase_TextChanged));
            }
        }

        public List<string> SchemaList
        {
            get { return _schemaList; }
            set
            {
                _schemaList = value;

                if (ComboBoxSchema == null) return;
                ComboBoxSchema.RemoveHandler(TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(ComboBoxSchema_TextChanged));

                var text = ComboBoxSchema.Text;

                ComboBoxSchema.Items.Clear();
                ComboBoxSchema.Items.Add("");

                if (_schemaList != null)
                {
                    foreach (var item in _schemaList)
                        ComboBoxSchema.Items.Add(item);
                }

                ComboBoxSchema.Text = text;

                ComboBoxSchema.AddHandler(TextBoxBase.TextChangedEvent,
                    new TextChangedEventHandler(ComboBoxSchema_TextChanged));
            }
        }

        public bool ShowServer
        {
            get { return _showServer; }
            set
            {
                _showServer = value;

                TextBlockServer.Visibility = _showServer ? Visibility.Visible : Visibility.Collapsed;
                TextBlockServer.Visibility = _showServer ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ShowDatabase
        {
            get { return _showDatabase; }
            set
            {
                _showDatabase = value;

                TextBlockDatabase.Visibility = _showDatabase ? Visibility.Visible : Visibility.Collapsed;
                TextBlockDatabase.Visibility = _showDatabase ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ShowSchema
        {
            get { return _showSchema; }
            set
            {
                _showSchema = value;

                TextBlockSchema.Visibility = _showSchema ? Visibility.Visible : Visibility.Collapsed;
                TextBlockSchema.Visibility = _showSchema ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool ShowPackage
        {
            get { return _showPackage; }
            set
            {
                _showPackage = value;

                TextBlockPackage.Visibility = _showPackage ? Visibility.Visible : Visibility.Collapsed;
                TextBlockPackage.Visibility = _showPackage ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public MetadataFilterItemControl()
        {
            InitializeComponent();

            var filter = new MetadataFilter();
            _filterItem = new MetadataFilterItem(filter);

            TextBoxServer.TextChanged += TextBoxServer_TextChanged;

            ComboBoxDatabase.AddHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxDatabase_TextChanged));
            ComboBoxSchema.AddHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxSchema_TextChanged));

            TextBoxPackage.TextChanged += TextBoxPackage_TextChanged;
            TextBoxObjectName.TextChanged += TextBlockObjectName_TextChanged;

            CheckBoxUserObject.Checked += CheckBoxUserObject_CheckedChanged;
            CheckBoxUserObject.Unchecked += CheckBoxUserObject_CheckedChanged;

            CheckBoxSystemObject.Checked += CheckBoxSystemObject_CheckedChanged;
            CheckBoxSystemObject.Unchecked += CheckBoxSystemObject_CheckedChanged;

            CheckBoxTable.Checked += CheckBoxTable_CheckedChanged;
            CheckBoxTable.Unchecked += CheckBoxTable_CheckedChanged;

            CheckBoxView.Checked += CheckBoxView_CheckedChanged;
            CheckBoxView.Unchecked += CheckBoxView_CheckedChanged;

            CheckBoxProcedure.Checked += CheckBoxProcedure_CheckedChanged;
            CheckBoxProcedure.Unchecked += CheckBoxProcedure_CheckedChanged;

            CheckBoxSynonym.Checked += CheckBoxSynonym_CheckedChanged;
            CheckBoxSynonym.Unchecked += CheckBoxSynonym_CheckedChanged;

            TextBoxField.TextChanged += TextBoxField_TextChanged;

            BindLocalization();
        }

        private void BindLocalization()
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Server.Subscribe(x => TextBlockServer.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Database.Subscribe(x => TextBlockDatabase.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Schema.Subscribe(x => TextBlockSchema.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Package.Subscribe(x => TextBlockPackage.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Fields.Subscribe(x => TextBlockFields.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Object.Subscribe(x => TextBlockObjectName.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.UserObject.Subscribe(x => CheckBoxUserObject.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.SystemObject.Subscribe(x => CheckBoxSystemObject.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Table.Subscribe(x => CheckBoxTable.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.View.Subscribe(x => CheckBoxView.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Procedure.Subscribe(x => CheckBoxProcedure.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Metadata.Synonym.Subscribe(x => CheckBoxSynonym.Content = x));

            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.ObjectType.Subscribe(x => TextBlockObjectType.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.ApplyTo.Subscribe(x => TextBlockApplyTo.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.FilterByNamespace.Subscribe(x => TextBlockFilterNamespace.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.FilterByObjects.Subscribe(x => TextBlockFilterObject.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.FilterByFileds.Subscribe(x => TextBlockFilterFields.Text = x));

            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Common.ButtonOk.Subscribe(x => ButtonOk.Content = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.Common.ButtonCancel.Subscribe(x => ButtonCancel.Content = x));
        }

        private string GetFilterText(string text)
        {
            return text == "%" ? "" : text;
        }

        private string SetFilterText(string text)
        {
            return String.IsNullOrEmpty(text) ? "%" : text;
        }

        private void UpdateControls()
        {
            TextBoxServer.TextChanged -= TextBoxServer_TextChanged;

            ComboBoxDatabase.RemoveHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxDatabase_TextChanged));
            ComboBoxSchema.RemoveHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxSchema_TextChanged));

            TextBoxPackage.TextChanged -= TextBoxPackage_TextChanged;
            TextBoxObjectName.TextChanged -= TextBlockObjectName_TextChanged;

            CheckBoxTable.Checked -= CheckBoxTable_CheckedChanged;
            CheckBoxTable.Unchecked -= CheckBoxTable_CheckedChanged;

            CheckBoxView.Checked -= CheckBoxView_CheckedChanged;
            CheckBoxView.Unchecked -= CheckBoxView_CheckedChanged;

            CheckBoxProcedure.Checked -= CheckBoxProcedure_CheckedChanged;
            CheckBoxProcedure.Unchecked -= CheckBoxProcedure_CheckedChanged;

            CheckBoxSynonym.Checked -= CheckBoxSynonym_CheckedChanged;
            CheckBoxSynonym.Unchecked -= CheckBoxSynonym_CheckedChanged;

            TextBoxField.TextChanged -= TextBoxField_TextChanged;

            if (_filterItem == null)
            {
                TextBoxServer.Text = "";
                TextBoxServer.IsEnabled = false;

                ComboBoxDatabase.Items.Clear();
                ComboBoxDatabase.IsEnabled = false;

                ComboBoxSchema.Items.Clear();
                ComboBoxSchema.IsEnabled = false;

                TextBoxPackage.Text = "";
                TextBoxPackage.IsEnabled = false;

                TextBlockObjectName.Text = "";
                TextBlockObjectName.IsEnabled = false;

                CheckBoxUserObject.IsChecked = true;
                CheckBoxUserObject.IsEnabled = false;

                CheckBoxSystemObject.IsChecked = true;
                CheckBoxSystemObject.IsEnabled = false;

                CheckBoxTable.IsChecked = false;
                CheckBoxTable.IsEnabled = false;

                CheckBoxView.IsChecked = false;
                CheckBoxView.IsEnabled = false;

                CheckBoxProcedure.IsChecked = false;
                CheckBoxProcedure.IsEnabled = false;

                CheckBoxSynonym.IsChecked = false;
                CheckBoxSynonym.IsEnabled = false;
            }
            else
            {
                TextBoxServer.Text = GetFilterText(_filterItem.Server);
                TextBoxServer.IsEnabled = true;

                ComboBoxDatabase.Text = GetFilterText(_filterItem.Database);
                ComboBoxDatabase.IsEnabled = true;

                ComboBoxSchema.Text = GetFilterText(_filterItem.Schema);
                ComboBoxSchema.IsEnabled = true;

                TextBoxPackage.Text = GetFilterText(_filterItem.Package);
                TextBoxPackage.IsEnabled = true;

                TextBoxObjectName.Text = GetFilterText(_filterItem.Object);
                TextBoxObjectName.IsEnabled = true;

                CheckBoxUserObject.IsChecked = _filterItem.FlagUser;
                CheckBoxUserObject.IsEnabled = true;

                CheckBoxSystemObject.IsChecked = _filterItem.FlagSystem;
                CheckBoxSystemObject.IsEnabled = true;

                CheckBoxTable.IsChecked = (_filterItem.ObjectTypes & MetadataType.Table) == MetadataType.Table;
                CheckBoxTable.IsEnabled = true;

                CheckBoxView.IsChecked = (_filterItem.ObjectTypes & MetadataType.View) == MetadataType.View;
                CheckBoxView.IsEnabled = true;

                CheckBoxProcedure.IsChecked = (_filterItem.ObjectTypes & MetadataType.Procedure) == MetadataType.Procedure;
                CheckBoxProcedure.IsEnabled = true;

                CheckBoxSynonym.IsChecked = (_filterItem.ObjectTypes & MetadataType.Synonym) == MetadataType.Synonym;
                CheckBoxSynonym.IsEnabled = true;

                TextBoxField.Text = GetFilterText(_filterItem.Field);
                TextBoxField.IsEnabled = true;
            }

            TextBoxServer.TextChanged += TextBoxServer_TextChanged;
            ComboBoxDatabase.AddHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxDatabase_TextChanged));
            ComboBoxSchema.AddHandler(TextBoxBase.TextChangedEvent,
                new TextChangedEventHandler(ComboBoxSchema_TextChanged));
            TextBoxPackage.TextChanged += TextBoxPackage_TextChanged;
            TextBoxObjectName.TextChanged += TextBlockObjectName_TextChanged;

            CheckBoxUserObject.Checked += CheckBoxUserObject_CheckedChanged;
            CheckBoxUserObject.Unchecked += CheckBoxUserObject_CheckedChanged;

            CheckBoxSystemObject.Checked += CheckBoxSystemObject_CheckedChanged;
            CheckBoxSystemObject.Unchecked += CheckBoxSystemObject_CheckedChanged;

            CheckBoxTable.Checked += CheckBoxTable_CheckedChanged;
            CheckBoxTable.Unchecked += CheckBoxTable_CheckedChanged;

            CheckBoxView.Checked += CheckBoxView_CheckedChanged;
            CheckBoxView.Unchecked += CheckBoxView_CheckedChanged;

            CheckBoxProcedure.Checked += CheckBoxProcedure_CheckedChanged;
            CheckBoxProcedure.Unchecked += CheckBoxProcedure_CheckedChanged;

            CheckBoxSynonym.Checked += CheckBoxSynonym_CheckedChanged;
            CheckBoxSynonym.Unchecked += CheckBoxSynonym_CheckedChanged;

            TextBoxField.TextChanged += TextBoxField_TextChanged;
        }

        private bool ValidateUserSystemFlags()
        {
            if (!CheckBoxSystemObject.IsChecked != true || !CheckBoxUserObject.IsChecked != true) return true;

            CheckBoxUserObject.Checked -= CheckBoxUserObject_CheckedChanged;
            CheckBoxUserObject.Unchecked -= CheckBoxUserObject_CheckedChanged;

            CheckBoxSystemObject.Checked -= CheckBoxSystemObject_CheckedChanged;
            CheckBoxSystemObject.Unchecked -= CheckBoxSystemObject_CheckedChanged;

            try
            {
                CheckBoxSystemObject.IsChecked = true;
                CheckBoxUserObject.IsChecked = true;
                if (_filterItem != null)
                {
                    _filterItem.FlagUser = true;
                    _filterItem.FlagSystem = true;
                    DoItemUpdated();
                }
            }
            finally
            {
                CheckBoxUserObject.Checked += CheckBoxUserObject_CheckedChanged;
                CheckBoxUserObject.Unchecked += CheckBoxUserObject_CheckedChanged;

                CheckBoxSystemObject.Checked += CheckBoxSystemObject_CheckedChanged;
                CheckBoxSystemObject.Unchecked += CheckBoxSystemObject_CheckedChanged;
            }

            return false;

        }

        private bool ValidateItemTypeFlags()
        {
            if (!CheckBoxProcedure.IsChecked != true || !CheckBoxSynonym.IsChecked != true ||
                !CheckBoxView.IsChecked != true || !CheckBoxTable.IsChecked != true) return true;

            CheckBoxTable.Checked -= CheckBoxTable_CheckedChanged;
            CheckBoxTable.Unchecked -= CheckBoxTable_CheckedChanged;

            CheckBoxView.Checked -= CheckBoxView_CheckedChanged;
            CheckBoxView.Unchecked -= CheckBoxView_CheckedChanged;

            CheckBoxProcedure.Checked -= CheckBoxProcedure_CheckedChanged;
            CheckBoxProcedure.Unchecked -= CheckBoxProcedure_CheckedChanged;

            CheckBoxSynonym.Checked -= CheckBoxSynonym_CheckedChanged;
            CheckBoxSynonym.Unchecked -= CheckBoxSynonym_CheckedChanged;

            try
            {
                CheckBoxTable.IsChecked = true;
                CheckBoxView.IsChecked = true;
                CheckBoxProcedure.IsChecked = true;
                CheckBoxSynonym.IsChecked = true;

                if (_filterItem != null)
                {
                    _filterItem.ObjectTypes = MetadataType.Objects;
                    DoItemUpdated();
                }
            }
            finally
            {
                CheckBoxTable.Checked += CheckBoxTable_CheckedChanged;
                CheckBoxTable.Unchecked += CheckBoxTable_CheckedChanged;

                CheckBoxView.Checked += CheckBoxView_CheckedChanged;
                CheckBoxView.Unchecked += CheckBoxView_CheckedChanged;

                CheckBoxProcedure.Checked += CheckBoxProcedure_CheckedChanged;
                CheckBoxProcedure.Unchecked += CheckBoxProcedure_CheckedChanged;

                CheckBoxSynonym.Checked += CheckBoxSynonym_CheckedChanged;
                CheckBoxSynonym.Unchecked += CheckBoxSynonym_CheckedChanged;
            }

            return false;

        }

        private void TextBoxServer_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Server = SetFilterText(TextBoxServer.Text);

            DoItemUpdated();
        }

        private void ComboBoxDatabase_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Database = SetFilterText(ComboBoxDatabase.Text);

            DoItemUpdated();
        }

        private void ComboBoxSchema_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Schema = SetFilterText(ComboBoxSchema.Text);

            DoItemUpdated();
        }

        private void TextBoxPackage_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Package = SetFilterText(TextBoxPackage.Text);

            DoItemUpdated();
        }

        private void TextBlockObjectName_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Object = SetFilterText(TextBlockObjectName.Text);

            DoItemUpdated();
        }

        private void CheckBoxUserObject_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateUserSystemFlags())
                return;

            if (_filterItem == null) return;
            _filterItem.FlagUser = CheckBoxUserObject.IsChecked == true;

            DoItemUpdated();
        }

        private void CheckBoxSystemObject_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateUserSystemFlags())
                return;

            if (_filterItem == null) return;

            _filterItem.FlagSystem = CheckBoxSystemObject.IsChecked == true;

            DoItemUpdated();
        }

        private void CheckBoxTable_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateItemTypeFlags())
                return;

            if (_filterItem == null) return;

            if (CheckBoxTable.IsChecked == true)
            {
                _filterItem.ObjectTypes |= MetadataType.Table;
            }
            else
            {
                _filterItem.ObjectTypes &= ~MetadataType.Table;
            }

            DoItemUpdated();
        }

        private void CheckBoxView_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateItemTypeFlags())
                return;

            if (_filterItem == null) return;

            if (CheckBoxView.IsChecked == true)
            {
                _filterItem.ObjectTypes |= MetadataType.View;
            }
            else
            {
                _filterItem.ObjectTypes &= ~MetadataType.View;
            }

            DoItemUpdated();
        }

        private void CheckBoxProcedure_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateItemTypeFlags())
                return;

            if (_filterItem == null) return;

            if (CheckBoxProcedure.IsChecked == true)
            {
                _filterItem.ObjectTypes |= MetadataType.Procedure;
            }
            else
            {
                _filterItem.ObjectTypes &= ~MetadataType.Procedure;
            }

            DoItemUpdated();
        }

        private void CheckBoxSynonym_CheckedChanged(object sender, EventArgs e)
        {
            if (!ValidateItemTypeFlags())
                return;

            if (_filterItem == null) return;

            if (CheckBoxSynonym.IsChecked == true)
            {
                _filterItem.ObjectTypes |= MetadataType.Synonym;
            }
            else
            {
                _filterItem.ObjectTypes &= ~MetadataType.Synonym;
            }

            DoItemUpdated();
        }

        private void TextBoxField_TextChanged(object sender, EventArgs e)
        {
            if (_filterItem == null) return;

            _filterItem.Field = SetFilterText(TextBoxField.Text);

            DoItemUpdated();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges();
            OkClicked?.Invoke(this, e);
        }

        public void ApplyChanges()
        {
            _originalFilterItem?.Assign(_filterItem);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelChanges();
            CancelClicked?.Invoke(this, e);
        }

        public void CancelChanges()
        {
            if (_originalFilterItem != null)
                _filterItem.Assign(_originalFilterItem);
        }
    }
}
