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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View.PropertiesEditors;

namespace MetadataEditorDemo.Common
{
    internal class GroupContainer : CollapsingControl, IGroupContainer
    {
        private readonly List<IPropertyItem> _items = new List<IPropertyItem>();

        public bool IsPrimary { get; set; }

        public IGroupContainer[] GroupContainers => _items.OfType<IGroupContainer>().ToArray();
        public ObjectProperties ObjectProperties { get; set; }
        public void SetEditorsOptions(PropertiesEditorsOptions options)
        {
            foreach (var propertiesEditor in PropertiesEditors)
            {
                propertiesEditor.SetEditorsOptions(options);
            }
        }

        public new bool CanCollapse
        {
            get
            {
                return base.CanCollapse;
            }
            set
            {
                if (base.CanCollapse == value) return;
                
                base.CanCollapse = value;

                if (!value)
                {
                    Style = (Style)Resources["ExpanderNonButtonGroup"];
                }
            }
        }

        public void InsertHeader(object control)
        {
            var element = (FrameworkElement) control;
            element.SetValue(Grid.ColumnSpanProperty, 2);
            PrimaryPanel.Children.Insert(0, element);
        }

        public GroupContainer()
        {
            DividerColor = Brushes.Azure;
            Loaded += GroupContainer_Loaded;
        }

        void GroupContainer_Loaded(object sender, RoutedEventArgs e)
        {
            AlignmentFirstPanel();
            AlignmentSecondaryPanel();
        }

        public string Text
        {
            get { return Header as string; }
            set { Header = value; }
        }

        public void Add(IPropertyItem editor)
        {
            var control = editor as Control;
            if (control == null)
            {
                return;
            }
            _items.Add(editor);

            AddSecondaryControl(control);
        }

        public IPropertyEditor[] PropertiesEditors
        {
            get
            {
                return _items.SelectMany(item => item.FindPropertiesEditors()).ToArray();
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}

