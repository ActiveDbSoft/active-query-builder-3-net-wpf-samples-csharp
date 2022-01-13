//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2022 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.CriteriaBuilder.CustomEditors;

namespace CriteriaBuilderCustomize
{
    public partial class CustomLookupButton : ICriteriaBuilderCustomLookupButton
    {
        event EventHandler ICriteriaBuilderCustomLookupButton.Click
        {
            add { Click += new RoutedEventHandler(value); }
            remove { Click -= new RoutedEventHandler(value); }
        }

        private CRectangle _bounds;
        public CRectangle Bounds
        {
            get { return new CRectangle(new CPoint(Margin.Left, Margin.Top), new CSize(ActualWidth, ActualWidth)); }
            set
            {
                _bounds = value;

                Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Arrange(new Rect(DesiredSize));

                Margin = new Thickness(value.X +  2, value.Top + (_bounds.Height - ActualHeight) / 2, Margin.Right, Margin.Bottom);
            }
        }

        public CustomLookupButton()
        {
            InitializeComponent();

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;

        }

        public void Dispose()
        {
        }
    }
}
