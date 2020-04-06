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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace GeneralAssembly.Common
{
    /// <summary>
    /// Interaction logic for CheckableCombobox.xaml
    /// </summary>
    public partial class CheckableCombobox
    {
        public event EventHandler ItemCheckStateChanged;
        public new ObservableCollection<SelectableItem> Items { get; private set; }

        public CheckableCombobox()
        {
            Items = new ObservableCollection<SelectableItem>();
            
            InitializeComponent();
            ItemsSource = Items;

            Items.CollectionChanged += Items_CollectionChanged;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectedIndex = -1;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    foreach (SelectableItem eNewItem in e.NewItems)
                        eNewItem.PropertyChanged += ItemPropertyChanged;

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SelectableItem eNewItem in e.OldItems)
                        eNewItem.PropertyChanged -= ItemPropertyChanged;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (SelectableItem eNewItem in e.NewItems)
                        eNewItem.PropertyChanged += ItemPropertyChanged;
                    foreach (SelectableItem eNewItem in e.OldItems)
                        eNewItem.PropertyChanged -= ItemPropertyChanged;
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Text = string.Empty;

            foreach (var item in Items)
            {
                if(!item.IsChecked) continue;

                if (!string.IsNullOrEmpty(Text))
                    Text += ", ";
                Text += item.Content;
            }
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(SelectableItem.IsChecked)) return;
            UpdateText();

            OnItemCheckStateChanged();
        }

        protected virtual void OnItemCheckStateChanged()
        {
            ItemCheckStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsItemChecked(int i)
        {
            return Items[i].IsChecked;
        }

        public void ClearCheckedItems()
        {
            foreach (var checkableComboboxItem in Items)
                checkableComboboxItem.IsChecked = false;
        }

        public void SetItemChecked(int i, bool b)
        {
            Items[i].IsChecked = b;
        }

        private void UpdateText()
        {
            var list = Items.Where(x => x.IsChecked).ToList();

            Text = string.Empty;

            for (var i = 0; i < list.Count; i++)
                Text += (i == 0 ? "" : ", ") + list[i].Content;

            ToolTip = string.IsNullOrEmpty(Text) ? null : Text;
        }
    }

    public class SelectableItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isChecked;
        private object _content;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public object Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public SelectableItem()
        {
        }

        public SelectableItem(object content)
        {
            Content = content;
        }

        public SelectableItem(object content, bool isChecked) : this(content)
        {
            IsChecked = isChecked;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
