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
using System.Windows.Input;

namespace FullFeaturedMdiDemo.Common
{
    /// <summary>
    /// Interaction logic for PaginationPanel.xaml
    /// </summary>
    public partial class PaginationPanel
    {
        private const string TitleCheckBoxWithOffset = "Enable pagination";
        private const string TitleCheckBoxWithoutOffset = "Enable limitations";
        public static readonly DependencyProperty IsSupportLimitCountProperty = DependencyProperty.Register(
            "IsSupportLimitCount", typeof(bool), typeof(PaginationPanel), new PropertyMetadata(true,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs args)
                {

                    var obj = o as PaginationPanel;
                    if (obj != null)
                        obj.PanelLimit.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Collapsed;
                }));

        public bool IsSupportLimitCount
        {
            get { return (bool)GetValue(IsSupportLimitCountProperty); }
            set { SetValue(IsSupportLimitCountProperty, value); }
        }

        public static readonly DependencyProperty IsSupportLimitOffsetProperty = DependencyProperty.Register(
            "IsSupportLimitOffset", typeof(bool), typeof(PaginationPanel), new PropertyMetadata(true,
                delegate (DependencyObject o, DependencyPropertyChangedEventArgs args)
                {
                    var value = (bool)args.NewValue;
                    var obj = o as PaginationPanel;

                    if (obj == null) return;

                    obj.PanelOffset.Visibility = (bool)args.NewValue ? Visibility.Visible : Visibility.Collapsed;
                    obj.CheckBoxEnabled.Content = value ? TitleCheckBoxWithOffset : TitleCheckBoxWithoutOffset;
                }));

        public bool IsSupportLimitOffset
        {
            get { return (bool)GetValue(IsSupportLimitOffsetProperty); }
            set { SetValue(IsSupportLimitOffsetProperty, value); }
        }

        public event RoutedEventHandler EnabledPaginationChanged;
        public event RoutedEventHandler CurrentPageChanged;
        public event RoutedEventHandler PageSizeChanged;

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
            "PageSize", typeof(int), typeof(PaginationPanel), new PropertyMetadata(10, (o, args) =>
            {
                var obj = o as PaginationPanel;
                if (obj != null) obj.TextBoxPageSize.Text = args.NewValue.ToString();
            }));

        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set
            {
                SetValue(PageSizeProperty, value);
                _maxPageCount = _countRows / PageSize + (_maxPageCount * PageSize < _countRows ? 1 : 0);
            }
        }

        private int _maxPageCount;

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            "CurrentPage", typeof(int), typeof(PaginationPanel), new PropertyMetadata(1, (o, args) =>
            {
                var obj = o as PaginationPanel;
                if (obj != null) obj.TextBoxCurrentPage.Text = args.NewValue.ToString();
            }));

        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        private int _countRows;

        public int CountRows
        {
            set
            {
                _countRows = value;
                _maxPageCount = _countRows / PageSize;
                if (_maxPageCount * PageSize < _countRows) _maxPageCount++;
            }
            get
            {
                return _countRows;
            }
        }

        public new bool IsEnabled { get { return CheckBoxEnabled.IsChecked == true; } }

        public PaginationPanel()
        {
            InitializeComponent();

            TextBoxCurrentPage.Text = CurrentPage.ToString();
            TextBoxPageSize.Text = PageSize.ToString();
        }

        public void Reset()
        {
            ToggleEnabled(false);

            CurrentPage = 1;
            PageSize = 10;
            _maxPageCount = 1;
        }

        private void CheckBoxEnabled_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ToggleEnabled(CheckBoxEnabled.IsChecked == true);

            OnEnabledPaginationChanged(e);
        }

        private void ButtonPreviewPage_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage - 1 == 0) return;

            CurrentPage--;
            OnCurrentPageChanged(e);

        }

        private void ButtonNextPage_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage + 1 > _maxPageCount)
                return;

            CurrentPage++;
            OnCurrentPageChanged(e);
        }

        private void TextBoxCurrentPage_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            int value;

            var success = int.TryParse(TextBoxCurrentPage.Text, out value);
            if (success)
            {
                CurrentPage = value;
                OnCurrentPageChanged(new RoutedEventArgs(e.RoutedEvent));
            }
            else
                TextBoxCurrentPage.Text = CurrentPage.ToString();
        }

        private void TextBoxPageSize_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            int value;

            var success = int.TryParse(TextBoxPageSize.Text, out value);
            if (success)
            {
                PageSize = value;
                OnPageSizeChanged(new RoutedEventArgs(e.RoutedEvent));
            }
            else
                TextBoxPageSize.Text = PageSize.ToString();
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            Keyboard.ClearFocus();
        }

        private void ToggleEnabled(bool value)
        {
            CheckBoxEnabled.IsChecked = value;
            TextBoxCurrentPage.IsEnabled = value;
            TextBoxPageSize.IsEnabled = value;
            ButtonPreviewPage.IsEnabled = value;
            ButtonNextPage.IsEnabled = value;
        }

        private void TextBoxCurrentPage_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private void TextBoxPageSize_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string str)
        {
            var reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        #region Invokators

        protected virtual void OnEnabledPaginationChanged(RoutedEventArgs e)
        {
            if (EnabledPaginationChanged != null) EnabledPaginationChanged(this, e);
        }

        protected virtual void OnCurrentPageChanged(RoutedEventArgs e)
        {
            if (CheckBoxEnabled.IsChecked != true) return;

            var handler = CurrentPageChanged;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnPageSizeChanged(RoutedEventArgs e)
        {
            if (CheckBoxEnabled.IsChecked != true) return;

            var handler = PageSizeChanged;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}
