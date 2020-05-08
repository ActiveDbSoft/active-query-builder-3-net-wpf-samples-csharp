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
using System.Windows;
using System.Windows.Controls;

namespace MetadataEditorDemo.Common
{
    internal sealed class SplitContainer : Grid
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(SplitContainer), new FrameworkPropertyMetadata(Orientation.Horizontal, PropertyOrientationChangedCallback));

        private static void PropertyOrientationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (Orientation)e.NewValue;

            var self = (SplitContainer)d;

            self.Reset();

            switch (value)
            {
                case Orientation.Horizontal:
                    self.ColumnDefinitions.Add(new ColumnDefinition());
                    self.ColumnDefinitions.Add(new ColumnDefinition());

                    self._panel1.SetValue(ColumnProperty, 0);
                    self._panel2.SetValue(ColumnProperty, 1);
                    self._splitter.SetValue(ColumnProperty, 0);
                    self._splitter.HorizontalAlignment = HorizontalAlignment.Right;
                    self._splitter.Width = 2;
                    break;
                case Orientation.Vertical:
                    self.RowDefinitions.Add(new RowDefinition());
                    self.RowDefinitions.Add(new RowDefinition());

                    self._panel1.SetValue(RowProperty, 0);
                    self._panel2.SetValue(RowProperty, 1);
                    self._splitter.SetValue(RowProperty, 0);
                    self._splitter.VerticalAlignment = VerticalAlignment.Bottom;
                    self._splitter.Height = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private readonly Grid _panel1 = new Grid();
        private readonly Grid _panel2 = new Grid();
        private readonly GridSplitter _splitter = new GridSplitter { Width = 2 };
        private GridLength _splitterDistance = new GridLength(1, GridUnitType.Auto);

        public UIElementCollection Panel1 => _panel1.Children;
        public UIElementCollection Panel2 => _panel2.Children;

        public GridLength SplitterDistance
        {
            get { return _splitterDistance; }
            set
            {
                _splitterDistance = value;

                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        ColumnDefinitions[0].Width = value;
                        break;
                    case Orientation.Vertical:
                        RowDefinitions[0].Height = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public SplitContainer()
        {
            _panel1.SetValue(ColumnProperty, 0);
            _panel2.SetValue(ColumnProperty, 1);
            _splitter.SetValue(ColumnProperty, 0);
            _splitter.HorizontalAlignment = HorizontalAlignment.Right;

            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition());

            Children.Add(_panel1);
            Children.Add(_panel2);
            Children.Add(_splitter);
        }

        private void Reset()
        {
            ColumnDefinitions.Clear();
            RowDefinitions.Clear();

            _panel1.SetValue(ColumnProperty, 0);
            _panel2.SetValue(ColumnProperty, 0);
            _splitter.SetValue(ColumnProperty, 0);

            _panel1.SetValue(RowProperty, 0);
            _panel2.SetValue(RowProperty, 0);
            _splitter.SetValue(RowProperty, 0);

            _splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            _splitter.VerticalAlignment = VerticalAlignment.Stretch;

            _splitter.Height = double.NaN;
            _splitter.Width = double.NaN;
        }
    }
}
