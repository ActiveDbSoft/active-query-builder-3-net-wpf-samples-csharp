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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.View;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for InformationPanel.xaml
    /// </summary>
    [ToolboxItem(false), DesignTimeVisible(false)]
    public partial class InformationPanel: IInformationPanel
    {
        private InfoIconLocation _iconLocation;

        public static readonly DependencyProperty InfoTextProperty = DependencyProperty.Register(
            "InfoText", typeof(string), typeof(InformationPanel), new PropertyMetadata(default(string)));

        public string InfoText
        {
            get { return (string) GetValue(InfoTextProperty); }
            set { SetValue(InfoTextProperty, value); }
        }

        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
            "Tooltip", typeof(string), typeof(InformationPanel), new PropertyMetadata(default(string)));

        public string Tooltip
        {
            get { return (string) GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly DependencyProperty IconTooltipProperty = DependencyProperty.Register(
            "IconTooltip", typeof(string), typeof(InformationPanel), new PropertyMetadata(default(string)));

        public string IconTooltip
        {
            get { return (string) GetValue(IconTooltipProperty); }
            set { SetValue(IconTooltipProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
            "ShowIcon", typeof(bool), typeof(InformationPanel), new PropertyMetadata(true));

        public bool ShowIcon
        {
            get { return (bool) GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public InfoIconLocation IconLocation
        {
            get { return _iconLocation; }
            set
            {
                _iconLocation = value;
                switch (value)
                {
                    case InfoIconLocation.Left:
                        Icon.SetValue(Grid.ColumnProperty, 0);
                        Icon.Margin = new Thickness(0, 0, 5, 0);

                        TextBlockBorder.SetValue(Grid.ColumnProperty, 1);

                        GridRoot.ColumnDefinitions[0].Width = GridLength.Auto;
                        GridRoot.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                        break;
                    case InfoIconLocation.Right:
                        Icon.SetValue(Grid.ColumnProperty, 1);
                        Icon.Margin = new Thickness(5, 0, 0, 0);

                        TextBlockBorder.SetValue(Grid.ColumnProperty, 0);

                        GridRoot.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                        GridRoot.ColumnDefinitions[1].Width = GridLength.Auto;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public InformationPanel()
        {
            InitializeComponent();
        }
    }
}
