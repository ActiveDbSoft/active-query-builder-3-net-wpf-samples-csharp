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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using FullFeaturedMdiDemo.Annotations;

namespace FullFeaturedMdiDemo.Common
{
    /// <summary>
    /// Interaction logic for WindowMessage.xaml
    /// </summary>
    public partial class WindowMessage : INotifyPropertyChanged
    {
        public HorizontalAlignment ContetnAlignment
        {
            get { return TextBlockContent.HorizontalAlignment; }
            set { TextBlockContent.HorizontalAlignment = value; }
        }
        public string Text { get { return TextBlockContent.Text; } set { TextBlockContent.Text = value; } }
        public ObservableCollection<Button> Buttons { private set; get; }

        public WindowMessage()
        {
            Buttons = new ObservableCollection<Button>();
            InitializeComponent();
            Buttons.CollectionChanged += Buttons_CollectionChanged;
        }

        private void Buttons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    foreach (Button button in e.NewItems)
                        PlaceButtons.Children.Add(button);

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Button button in e.OldItems)
                        PlaceButtons.Children.Remove(button);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    PlaceButtons.Children.Clear();
                    foreach (var button in Buttons)
                        PlaceButtons.Children.Add(button);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnPropertyChanged("Buttons");
        }

        public new bool Show()
        {
            return ShowDialog() == true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
