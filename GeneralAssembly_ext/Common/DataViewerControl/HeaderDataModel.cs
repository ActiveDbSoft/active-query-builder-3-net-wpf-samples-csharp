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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GeneralAssembly.Common.DataViewerControl
{
    public class HeaderDataModel : INotifyPropertyChanged
    {
        private Sorting _sorting;
        private string _title;
        private int _counter;

        private Visibility _upArrowVisible;
        private Visibility _downArrowVisible;
        private Visibility _showSortBlock;

        public Visibility UpArrowVisible
        {
            private set
            {
                _upArrowVisible = value;
                OnPropertyChanged("UpArrowVisible");
            }
            get { return _upArrowVisible; }
        }

        public Visibility DownArrowVisible
        {
            private set
            {
                _downArrowVisible = value;
                OnPropertyChanged("DownArrowVisible");
            }
            get { return _downArrowVisible; }
        }

        public Visibility ShowSortBlock
        {
            private set
            {
                _showSortBlock = value;
                OnPropertyChanged("ShowSortBlock");
            }
            get { return _showSortBlock; }
        }

        public Sorting Sorting
        {
            set
            {
                _sorting = value;

                switch (value)
                {
                    case Sorting.Asc:
                        ShowSortBlock = Visibility.Visible;
                        UpArrowVisible = Visibility.Visible;
                        DownArrowVisible = Visibility.Hidden;
                        break;
                    case Sorting.Desc:
                        ShowSortBlock = Visibility.Visible;
                        UpArrowVisible = Visibility.Hidden;
                        DownArrowVisible = Visibility.Visible;
                        break;
                    case Sorting.None:
                        ShowSortBlock = Visibility.Hidden;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value", value, null);
                }

                OnPropertyChanged("Sorting");
            }
            get { return _sorting; }
        }

        public string Title
        {
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
            get { return _title; }
        }

        public int Counter
        {
            set
            {
                _counter = value;
                OnPropertyChanged("Counter");
            }
            get { return _counter; }
        }

        public HeaderDataModel()
        {
            Sorting = Sorting.None;
            Title = "Empty";
            Counter = 0;
            ShowSortBlock = Visibility.Hidden;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum Sorting { Asc, Desc, None }
}
