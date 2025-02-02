//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.CriteriaBuilder.CustomEditors;
using ActiveQueryBuilder.View.WPF;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace CriteriaBuilderCustomize
{
    public partial class CustomEditor : ICriteriaBuilderCustomEditor
    {
        public event EventHandler CommitChanges;
        public new event CKeyEventHandler KeyDown;

        bool ICriteriaBuilderCustomEditor.IsVisible => Visibility == Visibility.Visible;
        public string Value
        {
            get { return BoxValue.Text; }
            set { BoxValue.Text = value; }
        }
        public CustomEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            var focusProperty = DependencyPropertyDescriptor.FromProperty(IsKeyboardFocusWithinProperty, GetType());
            focusProperty.AddValueChanged(this, Handler);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            BoxValue.Focus();
        }

        private void Handler(object sender, EventArgs e)
        {
            if (IsKeyboardFocusWithin) return;
            OnCommitChanges();
        }

        public void Dispose()
        {
            var focusProperty = DependencyPropertyDescriptor.FromProperty(IsKeyboardFocusWithinProperty, GetType());
            focusProperty.RemoveValueChanged(this, Handler);

            Loaded -= OnLoaded;
        }

        private CRectangle _bounds;
        public CRectangle Bounds
        {
            get
            {
                return new CRectangle(new CPoint(Margin.Left, Margin.Top), new CSize(ActualWidth, ActualHeight));
            }
            set
            {
                Margin = new Thickness(value.Left, value.Top, Margin.Right, Margin.Bottom);
                _bounds = value;

                MinWidth = value.Width;
                MinHeight = value.Height;

                Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Arrange(new Rect(DesiredSize));

                Margin = new Thickness(value.X, value.Top + (_bounds.Height - ActualHeight) / 2, Margin.Right, Margin.Bottom);
            }
        }

        protected virtual void OnCommitChanges()
        {
            CommitChanges?.Invoke(this, EventArgs.Empty);
        }

        private void BoxValue_OnKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(new CKeyEventArgs(Helpers.GetKey(e.Key), Helpers.GetCharFromKey(e.Key)));
        }

        protected virtual void OnKeyDown(CKeyEventArgs e)
        {
            KeyDown?.Invoke(this, e);
        }
    }
}
