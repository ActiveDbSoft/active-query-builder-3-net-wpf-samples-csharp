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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.Commands;
using ActiveQueryBuilder.View.WPF.Annotations;
using ActiveQueryBuilder.View.WPF.Commands;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for MetadataFilterControl.xaml
    /// </summary>
    [DesignTimeVisible(false)]
    internal partial class MetadataFilterControl
    {
        private DataItem _dataItemNeedShowEditor = null;
        private MetadataFilter _metadataFilter;
        private Orientation _orientation = Orientation.Horizontal;
        private readonly MetadataFilterItemControl _itemControl;
        private Popup _dropDown;
        private string _inclompleteItemCaption;

        public MetadataFilter MetadataFilter
        {
            get { return _metadataFilter; }
            set
            {
                if (_metadataFilter == value) return;

                _metadataFilter = value;

                FillListBox();
            }
        }

        public List<string> DatabaseList
        {
            get { return _itemControl.DatabaseList; }
            set { _itemControl.DatabaseList = value; }
        }

        public List<string> SchemaList
        {
            get { return _itemControl.SchemaList; }
            set { _itemControl.SchemaList = value; }
        }

        public bool ShowServer
        {
            get { return _itemControl.ShowServer; }
            set { _itemControl.ShowServer = value; }
        }

        public bool ShowDatabase
        {
            get { return _itemControl.ShowDatabase; }
            set { _itemControl.ShowDatabase = value; }
        }

        public bool ShowSchema
        {
            get { return _itemControl.ShowSchema; }
            set { _itemControl.ShowSchema = value; }
        }

        public bool ShowPackage
        {
            get { return _itemControl.ShowPackage; }
            set { _itemControl.ShowPackage = value; }
        }

        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;

                splitContainer1.Orientation = _orientation;
            }
        }

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private Command _addIncludeCommand;
        private Command _addExcludeCommand;
        private Command _deleteIncludeCommand;
        private Command _deleteExcludeCommand;
        private Command _editIncludeCommand;
        private Command _editExcludeCommand;

        public MetadataFilterControl()
        {
            InitializeComponent();

            _inclompleteItemCaption =
                $"<{ActiveQueryBuilder.View.Helpers.Localizer.GetString("strIncompleteFilterItem", LocalizableConstantsUI.strIncompleteFilterItem)}>";

            InitializeComponent();
            _itemControl = new MetadataFilterItemControl();
            _itemControl.OkClicked += ItemControlOkClicked;
            _itemControl.CancelClicked += ItemControlCancelClicked;

            BindCommands();
            BindLocalization();

            var languageProperty = DependencyPropertyDescriptor.FromProperty(LanguageProperty, GetType());
            languageProperty.AddValueChanged(this, OnLanguageChange);
        }

        private void ItemControlCancelClicked(object sender, EventArgs e)
        {
            _dropDown.IsOpen = false;
        }

        private void ItemControlOkClicked(object sender, EventArgs e)
        {
            _dropDown.IsOpen = false;
        }
        
        private void EditFilterItem(ListBox list)
        {
            if (list.SelectedIndex == -1)
                return;

            _itemControl.FilterItem = ((DataItem)list.SelectedItem).MetadataFilterItem;

            list.ScrollIntoView(list.Items[list.SelectedIndex]);

            var container = list.ItemContainerGenerator.ContainerFromIndex(list.SelectedIndex);

            if (container == null)
            {
                _dataItemNeedShowEditor = list.Items[list.SelectedIndex] as DataItem;
                return;
            }

            var element = container as FrameworkElement;
            ShowItemDropDown(element);
        }

        private void DeleteSelectedItem(ListBox list)
        {
            var item = list.SelectedItem as DataItem;
            if (item == null)
            {
                return;
            }

            var idx = list.SelectedIndex;
            list.Items.Remove(item);
            _metadataFilter.Remove(item.MetadataFilterItem);

            if (list.Items.Count == 0)
            {
                return;
            }

            if (idx == list.Items.Count)
            {
                list.SelectedIndex = idx - 1;
            }
            else if (idx < list.Items.Count)
            {
                list.SelectedIndex = idx;
            }
        }

        private void ShowItemDropDown(FrameworkElement element)
        {
            var oldVisualParent = VisualTreeHelper.GetParent(_itemControl) as Popup;
            var oldLogicParent = LogicalTreeHelper.GetParent(_itemControl) as Popup;

            if (oldVisualParent != null)
                oldVisualParent.Child = null;

            if (oldLogicParent != null)
                oldLogicParent.Child = null;

            _dropDown = new Popup
            {
                Child = _itemControl,
                Placement = PlacementMode.Bottom,
                PlacementTarget = element,
                IsOpen = true,
                StaysOpen = false
            };
            _dropDown.Closed += DropDownOnClosed;
        }

        private void DropDownOnClosed(object sender, EventArgs e)
        {
            _itemControl.ApplyChanges();
        }

        private void BindLocalization()
        {
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.IncludeRange.Subscribe(x => TextBlockInclude.Text = x));
            _subscriptions.Add(ActiveQueryBuilder.Core.Localization.MetadataEditor.ExcludeRange.Subscribe(x => TextBlockExclude.Text = x));
        }

        private void BindCommands()
        {
            _addIncludeCommand = new Command(tsbAdd_Click);
            _addExcludeCommand = new Command(tsbAddExclude_Click);
            _deleteIncludeCommand = new Command(tsbDelete_Click);
            _deleteExcludeCommand = new Command(tsbDeleteExclude_Click);
            _editIncludeCommand = new Command(tsbEditInclude_Click);
            _editExcludeCommand = new Command(tsbEditExclude_Click);

            _subscriptions.Add(CommandBinder.Bind(MenuItemIncludeAdd, _addIncludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Add, x => MenuItemIncludeAdd.IsEnabled = x, false));
            _subscriptions.Add(CommandBinder.Bind(MenuItemExcludeAdd, _addExcludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Add, x => MenuItemExcludeAdd.IsEnabled = x, false));
            _subscriptions.Add(CommandBinder.Bind(MenuItemIncludeRemove, _deleteIncludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Delete, x => MenuItemIncludeRemove.IsEnabled = x, false));
            _subscriptions.Add(CommandBinder.Bind(MenuItemExcludeRemove, _deleteExcludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Delete, x => MenuItemExcludeRemove.IsEnabled = x, false));
            _subscriptions.Add(CommandBinder.Bind(MenuItemIncludeEdit, _editIncludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Edit, x => MenuItemIncludeEdit.IsEnabled = x, false));
            _subscriptions.Add(CommandBinder.Bind(MenuItemExcludeEdit, _editExcludeCommand, ActiveQueryBuilder.View.WPF.Commands.Descriptions.MetadataEditor.Edit, x => MenuItemExcludeEdit.IsEnabled = x, false));
        }

        private void FillListBox()
        {

                ListBoxInclude.Items.Clear();

                if (_metadataFilter != null)
                {
                    foreach (MetadataFilterItem filterItem in _metadataFilter)
                    {
                        var item = new DataItem
                        {
                            MetadataFilterItem = filterItem, 
                            InvalidTextMessage = _inclompleteItemCaption
                        };

                        if (filterItem.Exclude)
                            ListBoxExclude.Items.Add(item);
                        else
                            ListBoxInclude.Items.Add(item);
                    }
                }

            if (ListBoxInclude.Items.Count > 0)
                ListBoxInclude.SelectedIndex = 0;

            if (ListBoxExclude.Items.Count > 0)
                ListBoxExclude.SelectedIndex = 0;
        }

        private void OnLanguageChange(object sender, EventArgs e)
        {
            _inclompleteItemCaption =
                $"<{ActiveQueryBuilder.View.Helpers.Localizer.GetString("strIncompleteFilterItem", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsUI.strIncompleteFilterItem)}>";
        }

        private void tsbAdd_Click()
        {
            if (_metadataFilter == null) return;

            var item = new DataItem { MetadataFilterItem = _metadataFilter.Add(), InvalidTextMessage = _inclompleteItemCaption};

            var index = ListBoxInclude.Items.Add(item);

            ListBoxInclude.SelectedIndex = index;

            EditFilterItem(ListBoxInclude);
        }

        private void tsbDelete_Click()
        {
            DeleteSelectedItem(ListBoxInclude);
        }

        private void tsbAddExclude_Click()
        {
            if (_metadataFilter == null) return;

            var md = _metadataFilter.Add();
            md.Exclude = true;

            var item = new DataItem { MetadataFilterItem = md, InvalidTextMessage = _inclompleteItemCaption };

            var index = ListBoxExclude.Items.Add(item);

            ListBoxExclude.SelectedIndex = index;

            EditFilterItem(ListBoxExclude);
        }

        private void tsbDeleteExclude_Click()
        {
            DeleteSelectedItem(ListBoxExclude);
        }

        private void tsbEditInclude_Click()
        {
            EditFilterItem(ListBoxInclude);
        }

        private void tsbEditExclude_Click()
        {
            EditFilterItem(ListBoxExclude);
        }

        private void ListBoxInclude_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    tsbDelete_Click();
                    break;
                case Key.Insert:
                    tsbAdd_Click();
                    break;
            }
        }

        private void ListBoxExclude_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    tsbDeleteExclude_Click();
                    break;
                case Key.Insert:
                    tsbAddExclude_Click();
                    break;
            }
        }

        private void ListBoxExclude_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditFilterItem((ListBox) sender);
        }

        private void ListBoxInclude_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditFilterItem((ListBox) sender);
        }

        private void ListBoxItemOnLoaded(object sender, RoutedEventArgs e)
        {
            if(_dataItemNeedShowEditor == null) return;

            var source = sender as FrameworkElement;

            if(source == null || source.DataContext != _dataItemNeedShowEditor) return;

            var item = Helpers.FindVisualParent<ListBoxItem>(source);

            if(item == null) return;

            _dataItemNeedShowEditor = null;

            item.Focus();

            ShowItemDropDown(item);
        }
    }

    public class DataItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MetadataFilterItem _metadataFilterItem;
        private string _invalidDisplayString = String.Empty;

        public string InvalidTextMessage
        {
            get { return _invalidDisplayString; }
            set
            {
                _invalidDisplayString = value;
                OnPropertyChanged(nameof(InvalidTextMessage));
                OnPropertyChanged(nameof(Value));
            }
        }

        public bool IsValid
        {
            get
            {
                if (_metadataFilterItem == null) return false;

                return !_metadataFilterItem.IsEmpty;
            }
        }

        public string Value => IsValid ? MetadataFilterItem?.ToString() : InvalidTextMessage;

        public MetadataFilterItem MetadataFilterItem
        {
            get { return _metadataFilterItem; }
            set
            {
                _metadataFilterItem = value;

                _metadataFilterItem.Updated += delegate
                {
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(IsValid));
                };

                OnPropertyChanged(nameof(MetadataFilterItem));
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}
