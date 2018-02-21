//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FullFeaturedMdiDemo.MdiControl.ButtonsIcon
{
    public abstract class BaseButtonIcon: Grid, IBaseButtonIcon
    {
        public event RoutedEventHandler Click;

        public static new readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof (Brush), typeof (BaseButtonIcon), new PropertyMetadata(Brushes.Transparent));

        public new Brush Background
        {
            get { return (Brush) GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof (Brush), typeof (BaseButtonIcon),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
            "IsMaximized", typeof(bool), typeof(BaseButtonIcon),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, CallbackIsMaximized));

        private static void CallbackIsMaximized(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as BaseButtonIcon;
            var value = (bool)e.NewValue;
            if (sender != null)
                sender.SizeContent = value ? new Size(8, 8) : new Size(11, 9);
        }

        protected Size SizeContent { set; get; }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public bool IsMaximized
        {
            get { return (bool) GetValue(IsMaximizedProperty); }
            set { SetValue(IsMaximizedProperty, value); }
        }

        protected BaseButtonIcon()
        {
            SnapsToDevicePixels = true;
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            SizeContent = new Size(11, 9);
            
            
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            InvalidateVisual();
            OnClick(e);
        }


        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            InvalidateVisual();
            //base.OnPreviewMouseDown(e);
        }
        protected virtual void OnClick(RoutedEventArgs e)
        {
            var handler = Click;
            if (handler != null) handler(this, e);
        }
    }
}
