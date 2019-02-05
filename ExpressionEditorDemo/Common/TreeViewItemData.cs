//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.Annotations;

namespace ExpressionEditorDemo.Common
{
    internal class TreeViewItemData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private variables
        private CImage _imageSource;
        private string _text;
        private int _level;
        private string _tooltip;
        private bool _isExpanded;
        private bool _isEditing;
        private string _textBeforeEditing;
        private bool _isSelected;
        private string _stringToUnderscore;
        #endregion

        public string StringToUnderscore
        {
            set
            {
                _stringToUnderscore = value;
                OnPropertyChanged(nameof(StringToUnderscore));
            }
            get { return _stringToUnderscore; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsEditing
        {
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
            }
            get { return _isEditing; }
        }

        public string TextBeforeEdit => _textBeforeEditing;

        public IComparer ItemSorter
        {
            get
            {
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
                return view.CustomSort;
            }
            set
            {
                var view = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
                view.CustomSort = value;

                foreach (var treeViewItemData in Items)
                    treeViewItemData.ItemSorter = value;

                OnPropertyChanged(nameof(ItemSorter));
            }
        }

        public TreeViewItemData Parent { private set; get; }

        public object Tag { set; get; }

        public CImage ImageSource
        {
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
            get { return _imageSource; }
        }



        public string Text
        {
            set
            {
                if (!_isEditing)
                    _textBeforeEditing = value;

                _text = value;
                OnPropertyChanged(nameof(Text));
            }
            get { return _text; }
        }

        public string ToolTip
        {
            set
            {
                _tooltip = value;
                OnPropertyChanged(nameof(ToolTip));
            }
            get { return _tooltip; }
        }

        public bool HasChildren { set; get; }

        public bool IsExpanded
        {
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
            get { return _isExpanded; }
        }

        public int Level
        {
            set
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
            }
            get { return _level; }
        }

        public ObservableCollection<TreeViewItemData> Items { get; }

        public TreeViewItemData()
        {
            Items = new ObservableCollection<TreeViewItemData>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Items));
            if (e.Action != NotifyCollectionChangedAction.Add) return;

            var childItems = e.NewItems;

            foreach (TreeViewItemData childItem in childItems)
            {
                childItem.Parent = this;
                childItem.ItemSorter = ItemSorter;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"[{Text}]";
        }
    }
}
