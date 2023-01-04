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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl
{
    /// <summary>
    /// Interaction logic for MdiContainer.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class MdiContainer
    {
        public delegate void ActiveWindowChangedHandler(object sender, EventArgs args);

        public event ActiveWindowChangedHandler ActiveWindowChanged;

        private static int _zindex;

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            "Children", typeof(ObservableCollection<MdiChildWindow>), typeof(MdiContainer), new PropertyMetadata(default(ObservableCollection<MdiChildWindow>)));

        public static readonly DependencyProperty FocusedWindowProperty = DependencyProperty.Register(
            "FocusedWindow", typeof(MdiChildWindow), typeof(MdiContainer), new PropertyMetadata(null, CallbackFocusedWindow));

        public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(MdiContainer), new PropertyMetadata(SystemColors.ControlBrush, CallBackBackground));

        public static readonly DependencyProperty ActiveChildProperty = DependencyProperty.Register(
            "ActiveChild", typeof(MdiChildWindow), typeof(MdiContainer), new PropertyMetadata(default(MdiChildWindow)));

        public MdiChildWindow ActiveChild
        {
            get { return (MdiChildWindow)GetValue(ActiveChildProperty); }
            set
            {
                SetValue(ActiveChildProperty, value);
                FocusedWindow = value;
                OnActiveWindowChanged();
            }
        }

        public event DependencyPropertyChangedEventHandler ActivatedChild;

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private static void CallBackBackground(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as MdiContainer;
            if (obj != null) obj.ScrollViewerRoot.Background = (Brush)e.NewValue;
        }

        private static void CallbackFocusedWindow(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldWindow = e.OldValue as MdiChildWindow;
            var newWindow = e.NewValue as MdiChildWindow;

            if (oldWindow != null)
                oldWindow.IsActive = false;

            if (newWindow == null) return;

            newWindow.IsActive = true;

            if(newWindow.State == StateWindow.Minimized)
                newWindow.State = StateWindow.Normal;

            var cont = d as MdiContainer;

            if (cont == null) return;

            cont.PromoteChildToFront(newWindow);
            cont.OnActivatedChild(e);
        }

        public void PromoteChildToFront(MdiChildWindow child)
        {
            var currentZindex = Panel.GetZIndex(child);

            foreach (var localChild in Children)
            {
                localChild.IsActive = localChild.Equals(child);

                var localZindex = Panel.GetZIndex(localChild);

                Panel.SetZIndex(localChild, localChild.Equals(child) ? _zindex - 1 : localZindex < currentZindex ? localZindex : localZindex - 1);
            }
            child.Focus();
            ActiveChild = child;
        }

        public MdiChildWindow FocusedWindow
        {
            get { return (MdiChildWindow)GetValue(FocusedWindowProperty); }
            set { SetValue(FocusedWindowProperty, value); }
        }

        public ObservableCollection<MdiChildWindow> Children
        {
            get { return (ObservableCollection<MdiChildWindow>)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }

        public MdiContainer()
        {
            Children = new ObservableCollection<MdiChildWindow>();
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            InitializeComponent();
            Loaded += MdiContainer_Loaded;
        }

        public void LayoutMdi(MdiLayout mdiLayout)
        {
            var point = new Point(0, 0);
            
            switch (mdiLayout)
            {
                case MdiLayout.Cascade:
                    foreach (var mdiChildWindow in Children)
                    {
                        mdiChildWindow.State = StateWindow.Normal;
                        mdiChildWindow.IsMaximized = false;
                        mdiChildWindow.SetLocation(point);

                        point.X += 20;
                        point.Y += 20;
                    }
                    break;
                case MdiLayout.TileHorizontal:
                    var widthWindow = ScrollViewerRoot.ViewportWidth/Children.Count;
                    foreach (var mdiChildWindow in Children)
                    {
                        mdiChildWindow.State = StateWindow.Normal;
                        mdiChildWindow.IsMaximized = false;
                        mdiChildWindow.SetLocation(point);

                        mdiChildWindow.Width = widthWindow;
                        mdiChildWindow.Height = ScrollViewerRoot.ViewportHeight;

                        point.X += widthWindow;
                    }
                    break;
                case MdiLayout.TileVertical:
                    var windowHeight = ScrollViewerRoot.ViewportHeight/Children.Count;
                    foreach (var mdiChildWindow in Children)
                    {
                        mdiChildWindow.State = StateWindow.Normal;
                        mdiChildWindow.IsMaximized = false;

                        mdiChildWindow.Width = ScrollViewerRoot.ViewportWidth;
                        mdiChildWindow.Height = windowHeight;

                        mdiChildWindow.SetLocation(point);
                        point.Y += windowHeight;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mdiLayout", mdiLayout, null);
            }

           PerformLayout();
        }

        private void PerformLayout()
        {
            var rect = new Rect();

            foreach (var mdiChildWindow in Children)
                rect.Union(mdiChildWindow.Bounds);

            GridRoot.Width = rect.Right > ActualWidth ? rect.Right : ScrollViewerRoot.ViewportWidth;

            GridRoot.Height = rect.Bottom > ActualHeight ? rect.Bottom : ScrollViewerRoot.ViewportHeight;
        }

        private void MdiContainer_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MdiContainer_Loaded;
            CheckVisibilityScrollbar();
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var objAdd = args.NewItems[0] as MdiChildWindow;
                    if (objAdd == null)
                        throw new ArgumentException("Error argument value");
                    _zindex++;

                    objAdd.Closing += ObjAddClosing;
                    objAdd.Minimize += objAdd_Minimize;
                    objAdd.Maximize += objAdd_Maximize;
                    objAdd.Resize += objAdd_Resize;

                    GridRoot.Children.Add(objAdd);
                    PromoteChildToFront(objAdd);
                    if (objAdd.State == StateWindow.Maximized)
                        objAdd.Width = ScrollViewerRoot.ViewportWidth;

                    objAdd.Height = ScrollViewerRoot.ViewportHeight;

                    break;
                case NotifyCollectionChangedAction.Remove:
                    var objRemove = args.OldItems[0] as MdiChildWindow;
                    if (objRemove == null)
                        throw new ArgumentException("Error argument value");
                    GridRoot.Children.Remove(objRemove);
                    break;
            }
        }

        private void CheckVisibilityScrollbar()
        {
            var visibleBar = Children.Any(x => x.State == StateWindow.Maximized);

            ScrollViewerRoot.VerticalScrollBarVisibility = visibleBar ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            ScrollViewerRoot.HorizontalScrollBarVisibility = visibleBar ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
        }

        private void objAdd_Resize(object sender, EventArgs e)
        {
            PerformLayout();
        }

        private void objAdd_Maximize(object sender, EventArgs e)
        {
            var mdiChild = (MdiChildWindow)sender;

            if (mdiChild.State == StateWindow.Maximized)
            {
                ScrollViewerRoot.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                ScrollViewerRoot.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

                var width = ScrollViewerRoot.ViewportWidth + (GridRoot.ActualWidth > ScrollViewerRoot.ViewportWidth ? SystemParameters.VerticalScrollBarWidth : 0);

                var height = ScrollViewerRoot.ViewportHeight + (GridRoot.ActualHeight > ScrollViewerRoot.ViewportHeight ? SystemParameters.HorizontalScrollBarHeight : 0);

                mdiChild.SetLocation(new Point(0, 0));
                mdiChild.SetSize(new Size(width, height));
            }
            else
            {
                ScrollViewerRoot.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                ScrollViewerRoot.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private void objAdd_Minimize(object sender, EventArgs e)
        {
            var mdiChild = (MdiChildWindow)sender;

            var collectionMinimizedWindow = Children.Where(x => x.State == StateWindow.Minimized && Math.Abs(x.Height - SystemParameters.MinimizedWindowHeight) < 0.2 && !x.Equals(mdiChild)).ToList();

            var startY = ScrollViewerRoot.ViewportHeight - SystemParameters.MinimizedWindowHeight;

            var startX = collectionMinimizedWindow.Sum(mdiChildWindow => mdiChildWindow.ActualWidth + 2);

            if (mdiChild.State == StateWindow.Minimized)
                mdiChild.SetLocation(new Point(startX, startY));

            if (Equals(FocusedWindow, mdiChild))
                FocusedWindow = null;
        }

        private void ObjAddClosing(object sender, EventArgs e)
        {
            var mdiChild = (MdiChildWindow)sender;

            if (Equals(ActiveChild, mdiChild)) ActiveChild = null;

            mdiChild.Closing -= ObjAddClosing;
            mdiChild.Minimize -= objAdd_Minimize;
            mdiChild.Maximize -= objAdd_Maximize;
            mdiChild.Resize -= objAdd_Resize;

            Children.Remove(mdiChild);
        }

        private void GridRoot_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);
            var isFound = false;

            foreach (var mdiChildWindow
                in from mdiChildWindow in Children
                    let isContains = mdiChildWindow.IsContainsPoint(point)
                    let mouseOver = mdiChildWindow.IsMouseOver
                    where mouseOver && isContains
                    select mdiChildWindow)
            {
                isFound = true;
                FocusedWindow = mdiChildWindow;
                break;
            }

            if (isFound) return;

            FocusedWindow = null;
        }

        private void ScrollViewerRoot_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var mdiChild = Children.FirstOrDefault(child => child.State == StateWindow.Maximized);

            if (mdiChild == null) return;

            mdiChild.SetSize(new Size(ScrollViewerRoot.ActualWidth, ScrollViewerRoot.ActualHeight));
        }

        protected virtual void OnActivatedChild(DependencyPropertyChangedEventArgs e)
        {
            var handler = ActivatedChild;
            if (handler != null) handler(this, e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var rect = new Rect();

            foreach (var mdiChildWindow in Children)
                rect.Union(mdiChildWindow.Bounds);

            GridRoot.Width = rect.Right > ActualWidth ? rect.Right : ScrollViewerRoot.ViewportWidth;
            GridRoot.Height = rect.Bottom > ActualHeight ? rect.Bottom : ScrollViewerRoot.ViewportHeight;

            ScrollViewerRoot.VerticalScrollBarVisibility = rect.Bottom > ActualHeight
                ? ScrollBarVisibility.Visible
                : ScrollBarVisibility.Disabled;
            ScrollViewerRoot.HorizontalScrollBarVisibility = rect.Right > ActualWidth
                ? ScrollBarVisibility.Visible
                : ScrollBarVisibility.Disabled;

            base.OnRenderSizeChanged(sizeInfo);

            CheckVisibilityScrollbar();
        }

        protected virtual void OnActiveWindowChanged()
        {
            if (ActiveWindowChanged != null) ActiveWindowChanged(this, EventArgs.Empty);
        }
    }

    public enum MdiLayout
    {
        Cascade,
        TileHorizontal,
        TileVertical
    }
}
