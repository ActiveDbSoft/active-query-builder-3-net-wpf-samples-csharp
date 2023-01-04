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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GeneralAssembly.Windows.SaveWindows
{
    /// <summary>
    /// Interaction logic for WindowMessage.xaml
    /// </summary>
    public partial class WindowMessage : INotifyPropertyChanged
    {
        public HorizontalAlignment ContentAlignment
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
            Icon = GetImageSource(Properties.Resources.disk);
            Buttons.CollectionChanged += Buttons_CollectionChanged;
        }

        public static BitmapImage GetImageSource(Icon image)
        {
            if (image == null) return null;

            using (var memory = new MemoryStream())
            {
                image.Save(memory);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
