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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View.PropertiesEditors;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ActiveQueryBuilder.View.WPF.PropertiesEditors;

namespace MetadataEditorDemo.Common
{
    internal class CollapsingControl : Expander
    {
        private const double SpaceRow = 5;
        private static Thickness DefaultMarginNextColumn = new Thickness(10, 0, 0, 0);

        public event EventHandler<EventArgs> OnCollapse;
        public event EventHandler<EventArgs> OnExpand;

        private ICollapsingDivider _collapsingDivider;
        private bool _canCollapse = true;
        private bool _isExpanded;
        private bool _showDescription;

        protected readonly Grid PrimaryPanel;
        protected readonly CustomGrid SecondaryPanel;
        protected readonly CustomStackPanel DividerPanel;

        internal bool ShowDescription
        {
            get { return _showDescription; }
            set
            {
                _showDescription = value;
                if (SecondaryPanel != null)
                {
                    SecondaryPanel.ColumnDefinitions[1].Width = value ? new GridLength(80, GridUnitType.Star) : new GridLength(1, GridUnitType.Star);
                    SecondaryPanel.ColumnDefinitions[2].Width = value ? new GridLength(20, GridUnitType.Star) : GridLength.Auto;
                }

                if (PrimaryPanel != null)
                {
                    PrimaryPanel.ColumnDefinitions[1].Width = value ? new GridLength(80, GridUnitType.Star) : new GridLength(1, GridUnitType.Star);
                    PrimaryPanel.ColumnDefinitions[2].Width = value ? new GridLength(20, GridUnitType.Star) : GridLength.Auto;
                }
            }
        }

        protected int SecondaryControlsCount => SecondaryPanel.Children.Count;

        public bool CanCollapse
        {
            get
            {
                return _canCollapse;
            }
            set
            {
                if (_canCollapse == value)
                {
                    return;
                }
                _canCollapse = value;

                if (value)
                    Style = (Style)Resources["CustomExpanderSyle"];
                else
                    Style = (Style)Resources["ExpanderNonButton"];

                if (_collapsingDivider != null)
                {
                    _collapsingDivider.CanCollapse = _canCollapse;
                }

                if (value || _isExpanded) return;

                IsExpanded = true;
                SetValue(IsExpandedProperty, true);
            }
        }

        protected ICollapsingDivider CollapsingDivider
        {
            get
            {
                return _collapsingDivider;
            }
            set
            {
                var divider = _collapsingDivider as UserControl;
                if (divider != null)
                {
                    DividerPanel.Children.Remove(divider);
                }
                _collapsingDivider = value;
                if (_collapsingDivider == null)
                {
                    return;
                }
                _collapsingDivider = value;

                divider = _collapsingDivider as UserControl;
                if (divider != null)
                {
                    divider.Padding = new Thickness(Padding.Left, 3, Padding.Right, 3);
                    DividerPanel.Children.Add(divider);
                }
                _collapsingDivider.CanCollapse = CanCollapse;
                if (_collapsingDivider.CanCollapse)
                {
                    _collapsingDivider.ButtonClick += CollapsingDividerButtonClick;
                }
            }
        }

        private void CollapsingDividerButtonClick(object sender, EventArgs e)
        {
            IsExpanded = !IsExpanded;
        }

        public Brush DividerColor
        {
            get
            {
                return DividerPanel.Background;
            }
            set
            {
                DividerPanel.Background = value;
            }
        }

        [DefaultValue(true)]
        public new bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded == value || (!value && !CanCollapse))
                {
                    return;
                }

                _isExpanded = value;

                if (_collapsingDivider != null)
                {
                    _collapsingDivider.IsExpanded = value;
                }
                if (value)
                {
                    Expand();
                }
                else
                {
                    Collapse();
                }
            }
        }

        protected CollapsingControl()
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"/ActiveQueryBuilder.View.WPF;component/Common/GlobalStyleDictionary.xaml", UriKind.Relative) });

            var resource = new ResourceDictionary
            {
                Source =
                    new Uri(
                        "/ActiveQueryBuilder.View.WPF;component/PropertiesEditors/PropEditorResources.xaml",
                        UriKind.RelativeOrAbsolute)
            };

            Resources.MergedDictionaries.Add(resource);
            Style = (Style)Resources["CustomExpanderSyle"];

            var root = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = GridLength.Auto},
                    new RowDefinition {Height = GridLength.Auto},
                    new RowDefinition {Height = GridLength.Auto}
                },

                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 0, 0)
            };

            Content = root;

            PrimaryPanel = new Grid { Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

            PrimaryPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            PrimaryPanel.ColumnDefinitions.Add(new ColumnDefinition());
            PrimaryPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            PrimaryPanel.SetValue(Grid.RowProperty, 0);
            root.Children.Add(PrimaryPanel);

            DividerPanel = new CustomStackPanel { Margin = new Thickness(0, 6, 0, 0) };
            DividerPanel.ChildrenCollectionChanged += DividerPanel_ChildrenCollectionChanged;
            DividerPanel.Visibility = Visibility.Collapsed;
            DividerPanel.SetValue(Grid.RowProperty, 1);
            root.Children.Add(DividerPanel);

            SecondaryPanel = new CustomGrid { Margin = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };

            SecondaryPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            SecondaryPanel.ColumnDefinitions.Add(new ColumnDefinition());
            SecondaryPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            SecondaryPanel.SetValue(Grid.RowProperty, 2);
            SecondaryPanel.Visibility = Visibility.Collapsed;
            root.Children.Add(SecondaryPanel);

            SecondaryPanel.ChildrenCollectionChanged += SecondaryPanel_ChildrenCollectionChanged;

            SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
        }

        private void DividerPanel_ChildrenCollectionChanged(object sender, EventArgs e)
        {
            var stack = (CustomStackPanel)sender;
            stack.Visibility = stack.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SecondaryPanel_ChildrenCollectionChanged(object sender, EventArgs e)
        {
            var grid = (Grid)sender;
            grid.Visibility = grid.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AddControlAtGrid(Grid panel, FrameworkElement control)
        {
            var compositeEditor = control as ICompositeEditor;
            if (panel == null) return;

            var indexRow = panel.RowDefinitions.Count;
            panel.RowDefinitions.Add(new RowDefinition());

            if (compositeEditor != null)
            {
                var verticalGrid = new Grid()
                {
                    RowDefinitions =
                    {
                        new RowDefinition
                        {
                            Height = new GridLength(1, GridUnitType.Auto)
                        },
                        new RowDefinition
                        {
                            Height = new GridLength(1, GridUnitType.Auto)
                        },
                        new RowDefinition
                        {
                            Height = new GridLength(1, GridUnitType.Auto)
                        }
                    }
                };
                verticalGrid.SetValue(Grid.RowProperty, indexRow);

                if (compositeEditor.LabelElement != null)
                {
                    if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Horizontal)
                    {
                        compositeEditor.LabelElement.SetValue(Grid.RowProperty, indexRow);
                        compositeEditor.LabelElement.SetValue(Grid.ColumnProperty, 0);
                        panel.Children.Add(compositeEditor.LabelElement);

                        if (compositeEditor.ContentElement == null)
                            compositeEditor.LabelElement.SetValue(Grid.ColumnSpanProperty, 2);
                    }

                    if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Vertical)
                    {
                        compositeEditor.LabelElement.SetValue(Grid.RowProperty, 0);
                        var m = compositeEditor.LabelElement.Margin;
                        compositeEditor.LabelElement.Margin = new Thickness(
                            m.Left,
                            m.Top,
                            m.Right,
                            5
                            );
                        verticalGrid.Children.Add(compositeEditor.LabelElement);
                        panel.Children.Add(verticalGrid);

                        if (compositeEditor.ContentElement == null)
                            verticalGrid.SetValue(Grid.ColumnSpanProperty, 2);
                    }
                }

                if (compositeEditor.ContentElement != null)
                {
                    if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Horizontal)
                    {
                        compositeEditor.ContentElement.SetValue(Grid.RowProperty, indexRow);

                        if (compositeEditor.LabelElement != null &&
                            string.IsNullOrEmpty(
                                compositeEditor.LabelElement.GetValue(LabelTextBlock.TextProperty) as string) ||
                            compositeEditor.LabelElement == null)
                        {
                            compositeEditor.ContentElement.SetValue(Grid.ColumnProperty, 0);
                            compositeEditor.ContentElement.SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        else
                        {
                            compositeEditor.ContentElement.SetValue(Grid.ColumnProperty, 1);
                        }

                        panel.Children.Add(compositeEditor.ContentElement);
                    }

                    if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Vertical)
                    {

                        verticalGrid.SetValue(Grid.ColumnProperty, 0);
                        verticalGrid.SetValue(Grid.ColumnSpanProperty, 2);
                        compositeEditor.ContentElement.SetValue(Grid.RowProperty, 1);
                        verticalGrid.Children.Add(compositeEditor.ContentElement);

                        if (verticalGrid.Parent == null)
                            panel.Children.Add(verticalGrid);
                    }
                }

                if (compositeEditor.LabelDescription == null) return;

                compositeEditor.LabelDescription.SetValue(Grid.RowProperty, indexRow);
                compositeEditor.LabelDescription.SetValue(Grid.ColumnProperty, 2);
                panel.Children.Add(compositeEditor.LabelDescription);
            }
            else
            {
                control.SetValue(Grid.ColumnProperty, 0);
                control.SetValue(Grid.ColumnSpanProperty, 2);
                control.SetValue(Grid.RowProperty, indexRow);
                PrimaryPanel.Children.Add(control);
            }
        }

        protected void AddPrimaryControl(FrameworkElement control)
        {
            if (control == null) return;

            AddControlAtGrid(PrimaryPanel, control);
  
            SecondaryPanel.Margin = new Thickness(0, 3, 0, 0);
            AlignmentFirstPanel();
        }

        public void AddSecondaryControl(FrameworkElement control)
        {
            if (control == null) return;

            AddControlAtGrid(SecondaryPanel, control);
          
            AlignmentSecondaryPanel();
        }

        /// <summary>
        /// Expand panel content
        /// </summary>
        protected void Expand()
        {
            OnExpand?.Invoke(this, EventArgs.Empty);
            _collapsingDivider?.OnExpand();
        }

        /// <summary>
        /// Collapse panel content
        /// </summary>
        protected void Collapse()
        {
            if (!CanCollapse) return;

            OnCollapse?.Invoke(this, EventArgs.Empty);

            _collapsingDivider?.OnCollapse();
        }

        public static void AddControlAtGrid(Grid panel, UIElement control)
        {
            var compositeEditor = control as ICompositeEditor;
            if (panel == null) return;

            //add empty space row
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SpaceRow) });

            //add current row
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var indexRow = panel.RowDefinitions.Count - 1;

            if (compositeEditor == null)
            {
                control.SetValue(Grid.RowProperty, indexRow);
                control.SetValue(Grid.ColumnProperty, 0);
                control.SetValue(Grid.ColumnSpanProperty, panel.ColumnDefinitions.Count - 1);

                panel.Add(control);
                return;
            }
            var editor = control as IPropertyEditor;

            var labelElement = editor != null && !string.IsNullOrEmpty(editor.Label)
                ? compositeEditor.LabelElement
                : null;
            var contentElement = compositeEditor.ContentElement;
            var additionalElement = compositeEditor.AdditionalElement;

            if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Vertical)
            {
                var stackPanel = new StackPanel { Name = "StackPanelVerticalOrientation" };

                stackPanel.SetValue(Grid.ColumnProperty, 0);
                stackPanel.SetValue(Grid.RowProperty, indexRow);
                stackPanel.SetValue(Grid.ColumnSpanProperty, 3);

                if (labelElement != null)
                {
                    stackPanel.Add(labelElement);
                    if (contentElement != null)
                        stackPanel.AddSpace(SpaceRow);
                }

                if (contentElement != null)
                {
                    stackPanel.Add(contentElement);
                    if (additionalElement != null)
                        stackPanel.AddSpace(SpaceRow);
                }

                if (additionalElement != null)
                    stackPanel.Add(additionalElement);

                panel.Add(stackPanel);
                return;
            }

            if (compositeEditor.DirectionLabelAndContent == DirectionLabelAndContent.Horizontal)
            {
                if (labelElement != null)
                {
                    labelElement.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                    labelElement.SetValue(Grid.RowProperty, indexRow);
                    labelElement.SetValue(Grid.ColumnProperty, 0);

                    if (contentElement == null && additionalElement == null)
                        labelElement.SetValue(Grid.ColumnSpanProperty, 3);

                    if (contentElement == null && additionalElement != null)
                        labelElement.SetValue(Grid.ColumnSpanProperty, 2);

                    panel.Add(labelElement);
                }

                if (contentElement != null)
                {
                    contentElement.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                    contentElement.SetValue(Grid.RowProperty, indexRow);
                    contentElement.SetValue(Grid.ColumnProperty, labelElement == null ? 0 : 1);

                    if (labelElement == null && additionalElement == null)
                        contentElement.SetValue(Grid.ColumnSpanProperty, 3);

                    if (labelElement != null && additionalElement == null)
                        contentElement.SetValue(Grid.ColumnSpanProperty, 2);

                    panel.Add(contentElement);
                }

                if (additionalElement != null)
                {
                    additionalElement.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                    additionalElement.SetValue(Grid.RowProperty, indexRow);

                    if (labelElement == null && contentElement == null)
                    {
                        additionalElement.SetValue(Grid.ColumnProperty, 0);
                        additionalElement.SetValue(Grid.ColumnSpanProperty, 3);
                    }

                    if (labelElement != null && contentElement == null)
                    {
                        additionalElement.SetValue(Grid.ColumnProperty, 1);
                        additionalElement.SetValue(Grid.ColumnSpanProperty, 2);
                    }

                    if (labelElement != null && contentElement != null)
                    {
                        additionalElement.SetValue(Grid.ColumnProperty, 2);
                    }

                    panel.Add(additionalElement);
                }

                if (contentElement != null && (int)contentElement.GetValue(Grid.ColumnProperty) > 0)
                    contentElement.Margin = DefaultMarginNextColumn;

                if (additionalElement != null && (int)additionalElement.GetValue(Grid.ColumnProperty) > 0)
                    additionalElement.Margin = DefaultMarginNextColumn;

                return;
            }
        }

        protected void AlignmentFirstPanel()
        {
            for (var rowId = 0; rowId < PrimaryPanel.RowDefinitions.Count; rowId++)
            {
                var collectionItem =
                    PrimaryPanel.Children.Cast<FrameworkElement>().Where(item => Grid.GetRow(item) == rowId).ToList();

                foreach (var element in collectionItem)
                {
                    if (collectionItem.Count == 1)
                    {
                        element.Margin = new Thickness(6, 6, 6, 0);
                        if (element is GroupContainer)
                        {
                            var gr = element as GroupContainer;
                            gr.AlignmentFirstPanel();
                        }
                        break;
                    }

                    if (element is CheckBox)
                    {
                        element.Margin = new Thickness(6, 6, 0, 0);
                    }

                    if (element is LabelTextBlock)
                    {
                        var tb = element as LabelTextBlock;
                        tb.VerticalAlignment = VerticalAlignment.Center;
                        if (!string.IsNullOrEmpty(tb.Text))
                            tb.Margin = new Thickness(6, 6, tb.Margin.Right, tb.Margin.Bottom);
                    }

                    if (element is Grid)
                    {
                        var tBlock = collectionItem.FirstOrDefault(x => x is LabelTextBlock);

                        var flag = (tBlock as LabelTextBlock) != null && string.IsNullOrEmpty((tBlock as LabelTextBlock).Text);

                        element.Margin = new Thickness(flag ? 6 : 10, 6, 6, 0);
                    }

                    if (element is TextBox || element is PasswordBox || element is SingleLineSqlTextEditor)
                    {
                        var tBlock = collectionItem.FirstOrDefault(x => x is LabelTextBlock);

                        var flag = (tBlock as LabelTextBlock) != null && string.IsNullOrEmpty((tBlock as LabelTextBlock).Text);

                        element.Margin = new Thickness(flag ? 6 : 10, 6, 6, 0);
                    }

                    if (element is ComboBox)
                    {
                        element.Margin = new Thickness(10, 6, 6, 0);
                    }
                }
            }
        }

        protected void AlignmentSecondaryPanel()
        {
            for (var rowId = 0; rowId < SecondaryPanel.RowDefinitions.Count; rowId++)
            {
                var collectionItem =
                    SecondaryPanel.Children.Cast<FrameworkElement>().Where(item => Grid.GetRow(item) == rowId).ToList();

                foreach (var element in collectionItem)
                {
                    if (collectionItem.Count == 1)
                    {
                        element.Margin = new Thickness(6, 6, 6, 0);
                        if (element is GroupContainer)
                        {
                            var gr = element as GroupContainer;
                            gr.AlignmentFirstPanel();
                        }
                        break;
                    }

                    if (element is CheckBox)
                    {
                        element.Margin = new Thickness(6, 6, 0, 0);
                    }

                    if (element is LabelTextBlock)
                    {
                        var tb = element as LabelTextBlock;

                        tb.VerticalAlignment = VerticalAlignment.Center;
                        if (!string.IsNullOrEmpty(tb.Text))
                            tb.Margin = new Thickness(6, 6, tb.Margin.Right, tb.Margin.Bottom);
                    }

                    if (element is TextBox || element is PasswordBox || element is SingleLineSqlTextEditor)
                    {
                        var tBlock = collectionItem.FirstOrDefault(x => x is LabelTextBlock);

                        var flag = (tBlock as LabelTextBlock) != null && string.IsNullOrEmpty((tBlock as LabelTextBlock).Text);
                        
                        element.Margin = new Thickness(flag ? 6 : 10, 6, 6, 0);
                    }

                    if (element is ComboBox)
                    {
                        var box = element as ComboBox;
                        box.Margin = new Thickness(10, 6, 6, 0);
                    }
                }
            }
        }
    }

    [DesignTimeVisible(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [ToolboxItem(false)]
    internal class CustomGrid : Grid
    {
        public delegate void ChildrenCollectionHandler(object sender, EventArgs e);

        public event ChildrenCollectionHandler ChildrenCollectionChanged;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            OnChildrenCollectionChanged(EventArgs.Empty);
        }

        protected virtual void OnChildrenCollectionChanged(EventArgs e)
        {
            var handler = ChildrenCollectionChanged;
            handler?.Invoke(this, e);
        }
    }

    [DesignTimeVisible(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    [ToolboxItem(false)]
    internal class CustomStackPanel : StackPanel
    {
        public delegate void ChildrenCollectionHandler(object sender, EventArgs e);

        public event ChildrenCollectionHandler ChildrenCollectionChanged;

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            OnChildrenCollectionChnged(EventArgs.Empty);
        }

        protected virtual void OnChildrenCollectionChnged(EventArgs e)
        {
            var handler = ChildrenCollectionChanged;
            if (handler != null) handler(this, e);
        }
    }

    internal static class CollectionChildrenHelpers
    {
        public static void Add(this Grid grid, UIElement child)
        {
            grid.Children.Add(child);
        }
        public static void Add(this StackPanel panel, UIElement child)
        {
            panel.Children.Add(child);
        }

        public static void AddSpace(this StackPanel panel, double space)
        {
            panel.Children.Add(new Border { Height = space, Opacity = 0, Visibility = Visibility.Hidden });
        }
    }
}
