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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl
{
    /// <summary>
    /// Interaction logic for MdiChildWindow.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class MdiChildWindow
    {
        private const double WidthMinimized = 173;
        private Size _oldSize = Size.Empty;
        private Point _oldPoint = new Point(0, 0);
        private new Grid Content { set; get; }

        public event EventHandler Closing;
        public event EventHandler Minimize;
        public event EventHandler Maximize;
        public event EventHandler Resize;

#region Dependency
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(
            "Left", typeof (double), typeof (MdiChildWindow), new PropertyMetadata(0.0, CallBackLeft));

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof (double), typeof (MdiChildWindow), new PropertyMetadata(0.0, CallBackTop));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof (bool), typeof (MdiChildWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof (string), typeof (MdiChildWindow), new PropertyMetadata("Window"));

        public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof (Brush), typeof (MdiChildWindow), new PropertyMetadata(Brushes.White, CallBackBackgound));

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State", typeof (StateWindow), typeof (MdiChildWindow), new PropertyMetadata(StateWindow.Normal, CallbackStateChange));

        private static void CallbackStateChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var state = (StateWindow) e.NewValue;

            var obj = d as MdiChildWindow;
            if (obj != null)
            {
                switch (state)
                {
                    case StateWindow.Normal:
                        obj.SetSize(obj._oldSize == Size.Empty ? new Size(300, 200) : obj._oldSize);
                        obj.SetLocation(obj._oldPoint);
                        break;
                    case StateWindow.Minimized:
                        obj.State = StateWindow.Minimized;
                        obj._oldSize = new Size(obj.ActualWidth, obj.ActualHeight);
                        obj._oldPoint = new Point(obj.Left, obj.Top);
                        obj.Height = SystemParameters.MinimizedWindowHeight;
                        obj.Width = WidthMinimized;
                        break;
                    case StateWindow.Maximized:
                        obj.IsMaximized = true;
                        obj.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        obj.Arrange(new Rect(obj.DesiredSize));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
            }
        }

        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
            "IsMaximized", typeof (bool), typeof (MdiChildWindow), new PropertyMetadata(false));
#endregion

        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        public StateWindow State
        {
            get { return (StateWindow) GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public new Brush Background
        {
            get { return (Brush) GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public ObservableCollection<Visual> Children { set; get; }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public double Top
        {
            get { return (double) GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public double Left
        {
            get { return (double) GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public virtual void Close()
        {
            OnClose();
        }

        public Rect Bounds
        {
            get { return new Rect(Left, Top, ActualWidth, ActualHeight); }
        }

        public MdiChildWindow()
        {
            Children = new ObservableCollection<Visual>();
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            InitializeComponent();

            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);

            DataContext = this;

            State = StateWindow.Normal;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Content = (Grid)Template.FindName("GridRoot", this);
        }

        public bool IsContainsPoint(Point point)
        {
            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);

            if (double.IsNaN(left))
                left = TranslatePoint(new Point(0, 0), (UIElement)Parent).X;

            if (double.IsNaN(top))
                top = TranslatePoint(new Point(0, 0), (UIElement)Parent).Y;

            var rect = new Rect(left, top, ActualWidth, ActualHeight);
            return rect.Contains(point);
        }

        public void SetLocation(Point point)
        {
            Left = point.X;
            Top = point.Y;
        }

        public void SetSize(Size size)
        {
            Width = size.Width;
            Height = size.Height;
        }

        private static void CallBackLeft(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as MdiChildWindow;

            if (obj != null)
                obj.SetValue(Canvas.LeftProperty, (double) e.NewValue);
        }

        private static void CallBackTop(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as MdiChildWindow;

            if (obj != null)
                obj.SetValue(Canvas.TopProperty, (double) e.NewValue);
        }

        private static void CallBackBackgound(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as MdiChildWindow;
            if (obj != null)
            {
                if (obj.Content == null) obj.ApplyTemplate();
                obj.Content.Background = (Brush) e.NewValue;
            }
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (Content == null) ApplyTemplate();

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Content.Children.Add((FrameworkElement) args.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Content.Children.Remove((FrameworkElement) args.NewItems[0]);
                    break;
            }
        }

        protected virtual void OnClose()
        {
            var handler = Closing;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnMinimize()
        {
            var handler = Minimize;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnMaximize()
        {
            var handler = Maximize;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected virtual void OnResize()
        {
            var handler = Resize;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void ThumbHeight_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (IsMaximized || State == StateWindow.Minimized) return;
            if (ActualHeight + e.VerticalChange < 30) return;
            var newSize = new Size(ActualWidth, ActualHeight + e.VerticalChange);

            Height = newSize.Height;

            OnResize();
        }

        private void ThumbWidth_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (IsMaximized || State == StateWindow.Minimized) return;

            if (ActualWidth + e.HorizontalChange < 140) return;
            var newSize = new Size(ActualWidth + e.HorizontalChange, ActualHeight);

            Width = newSize.Width;
            OnResize();
        }

        private void ThumbHW_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (IsMaximized || State == StateWindow.Minimized) return;
            var nWidth = ActualWidth;
            var nHeight = ActualHeight;

            if (ActualHeight + e.VerticalChange > 30)
                nHeight += e.VerticalChange;

            if (ActualWidth + e.HorizontalChange > 140)
                nWidth += e.HorizontalChange;

            Width = nWidth;
            Height = nHeight;
            OnResize();
        }

        private void ThumbMove_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (IsMaximized || State == StateWindow.Minimized) return;

            Top += e.VerticalChange;
            Left += e.HorizontalChange;
            OnResize();
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMaximize_OnClick(object sender, RoutedEventArgs e)
        {
            if (State == StateWindow.Maximized)
            {
                IsMaximized = false;
                State = StateWindow.Normal;
                SetSize(_oldSize == Size.Empty ? new Size(300,200) : _oldSize );
                SetLocation(_oldPoint);
            }
            else
            {
                IsMaximized = true;
                
                if (State != StateWindow.Minimized)
                {
                    _oldSize = new Size(ActualWidth, ActualHeight);
                    _oldPoint = new Point(Left, Top);
                }
                State = StateWindow.Maximized;
            }

            OnMaximize();
        }

        private void ButtonMinimize_OnClick(object sender, RoutedEventArgs e)
        {
            if (State == StateWindow.Minimized)
            {
                State = IsMaximized ? StateWindow.Maximized : StateWindow.Normal;
                Width = _oldSize.Width;
                Height = _oldSize.Height;
                Left = _oldPoint.X;
                Top = _oldPoint.Y;
            }
            else
            {
                State = StateWindow.Minimized;
                _oldSize = new Size(ActualWidth, ActualHeight);
                _oldPoint = new Point(Left, Top);
                Height = SystemParameters.MinimizedWindowHeight;
                Width = WidthMinimized;
            }

            OnMinimize();
        }

        private void Header_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonMaximize_OnClick(sender, e);
        }
    }

    public enum StateWindow
    {
        Normal,
        Minimized,
        Maximized
    }
}
