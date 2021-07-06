//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.CriteriaBuilder.CustomEditors;
using ActiveQueryBuilder.View.WPF;

namespace CriteriaBuilderCustomize
{
    internal partial class CustomLookupControl : ICriteriaBuilderCustomLookupControl
    {
        public event EventHandler ItemSelected;

        public event EventHandler LoadMoreClick
        {
            add { LinkShowMore.MouseLeftButtonUp += new MouseButtonEventHandler(value); }
            remove { LinkShowMore.MouseLeftButtonUp -= new MouseButtonEventHandler(value); }
        }

        public event EventHandler LoadAllClick
        {
            add { LinkShowAll.MouseLeftButtonUp += new MouseButtonEventHandler(value); }
            remove { LinkShowAll.MouseLeftButtonUp -= new MouseButtonEventHandler(value); }
        }

        public event EventHandler Closing;

        private DataTable _tableSource;

        public bool IsDisposed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            IsDisposed = true;
            Closing?.Invoke(this, EventArgs.Empty);
            base.OnClosed(e);
        }

        public int SelectedIndex => DataGridView.SelectedIndex;

        public string SelectedItem
        {
            get
            {
                if (DataGridView.SelectedItem == null) return "";
                var row = (DataRowView)DataGridView.SelectedItem;
                return row.Row.ItemArray[KeyColumnIndex].ToString();
            }
        }

        public CPoint Location
        {
            get { return new CPoint((int)HorizontalOffset, (int)VerticalOffset); }
            set
            {
                HorizontalOffset = value.X;
                VerticalOffset = value.Y;
            }
        }

        public bool MoreLinkLabelVisible
        {
            get { return LinkShowMore.Visibility == Visibility.Visible; }
            set { LinkShowMore.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool AllLinkLabelVisible
        {
            get { return LinkShowAll.Visibility == Visibility.Visible; }
            set { LinkShowAll.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public int KeyColumnIndex { set; get; }

        public object DataSource
        {
            get { return _tableSource; }
            set
            {
                _tableSource = (DataTable)value;
                DataGridView.ItemsSource = _tableSource.DefaultView;

                if (DataGridView.IsLoaded)
                    ApplyTemplateGrid();
            }
        }

        public int RowCount => DataGridView.Items.Count;

        public int FirstDisplayedScrollingRowIndex
        {
            get { return DataGridView.SelectedIndex; }
            set
            {
                DataGridView.UpdateLayout();
                DataGridView.ScrollIntoView(DataGridView.Items[value]);
                DataGridView.InvalidateVisual();
            }
        }

        public object Owner
        {
            set { PlacementTarget = (UIElement)value; }
            get { return PlacementTarget; }
        }

        public bool Visible
        {
            get { return IsOpen; }
            set
            {
                if (value)
                {
                    IsDisposed = false;
                    if (IsOpen)
                    {
                        IsOpen = true;
                        return;
                    }

                    IsOpen = true;
                }
                else
                {
                    if (IsDisposed) return;
                    Hide();
                }
            }
        }

        public CustomLookupControl()
        {
            InitializeComponent();

            IsDisposed = false;
            DataGridView.Loaded += DataGridView_Loaded;
        }

        private void ApplyTemplateGrid()
        {
            var keyColumn = DataGridView.Columns[KeyColumnIndex];

            var style = new Style(typeof(DataGridCell)) { BasedOn = keyColumn.CellStyle };

            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(SystemColors.InfoColor)));

            var trigger = new Trigger { Property = DataGridCell.IsSelectedProperty, Value = true };

            trigger.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Color.FromRgb(51, 153, 255))));

            style.Triggers.Add(trigger);
            keyColumn.CellStyle = style;

            DataGridView.InvalidateVisual();
            DataGridView.UpdateLayout();
            Keyboard.Focus(DataGridView);

            if (SelectedIndex < 0) return;

            DataGridView.SelectedIndex = SelectedIndex;
            DataGridView.SelectedIndex = SelectedIndex + 1;
            DataGridView.SelectedIndex = SelectedIndex - 1;

            var row = (DataGridRow)DataGridView.ItemContainerGenerator.ContainerFromIndex(SelectedIndex);
            row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));


            DataGridView.CurrentCell = new DataGridCellInfo(DataGridView.SelectedItem,
                DataGridView.Columns[KeyColumnIndex]);

            DataGridView.ScrollIntoView(DataGridView.Items[SelectedIndex], null);
        }

        private void DataGridView_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTemplateGrid();
        }

        public int DefaultCountOfLookupValues { get; set; }

        public void Hide()
        {
            IsOpen = false;
        }

        public void UpdateSize() { }

        public void SelectRow(int rowIndex)
        {
            DataGridView.SelectedItem = null;
            DataGridView.SelectedItem = DataGridView.Items[rowIndex];
        }

        public void ForwardKeyDownToGrid(CKeyEventArgs keyEventArgs)
        {
            var source = PresentationSource.FromVisual(DataGridView);
            if (source != null) DataGridView_OnPreviewKeyDown(null, new KeyEventArgs(null, source, 0, Key.Down));
        }

        public void ForwardMouseWheelToGrid(CMouseEventArgs mouseEventArgs)
        {
            var source = PresentationSource.FromVisual(DataGridView);

            if (source != null)
                DataGridView_OnPreviewKeyDown(null, new KeyEventArgs(null, source, 0, mouseEventArgs.Delta > 0 ? Key.Up : Key.Down));
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsOpen = false;
        }

        public void FireItemSelectedEvent(EventArgs e)
        {
            ItemSelected?.Invoke(this, e);
        }

        private void DataGridView_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Hide();
                    break;
                case Key.Tab:
                case Key.Enter:
                    //    if (!e.Shift && !e.Control && !e.Alt)
                    //    {
                    FireItemSelectedEvent(EventArgs.Empty);
                    e.Handled = true;
                    //    }
                    break;
                    /*case System.Windows.Forms.Keys.Up:
						if (SelectedIndex == 0)
						{
							SelectedIndex = Items.Count - 1;
							e.SuppressKeyPress = true;
							return;
						}
						break;
					case System.Windows.Forms.Keys.Down:
						if (SelectedIndex == Items.Count - 1)
						{
							SelectedIndex = 0;
							e.SuppressKeyPress = true;
							return;
						}
						break;*/
            }

            OnKeyDown(e);
        }

        private void DataGridView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FireItemSelectedEvent(EventArgs.Empty);
        }

        private void DataGridView_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Scroller.ScrollToVerticalOffset(Scroller.ContentVerticalOffset - e.Delta);
        }

        private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if ((Height + e.VerticalChange) > 150)
                Height += (int)e.VerticalChange;

            if ((Width + e.HorizontalChange) > 300)
                Width += (int)e.HorizontalChange;
        }
    }
}
