//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2018 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View;
using ActiveQueryBuilder.View.PropertiesEditors;
using ActiveQueryBuilder.View.WPF;
using ActiveQueryBuilder.View.WPF.ExpressionEditor;
using ActiveQueryBuilder.View.WPF.QueryView;

namespace FullFeaturedMdiDemo.PropertiesForm
{
    /// <summary>
    /// Interaction logic for QueryPropertiesWindow.xaml
    /// </summary>
    public partial class QueryPropertiesWindow
    {
        private readonly Dictionary<TextBlock, Grid> _linkToPageGeneral = new Dictionary<TextBlock, Grid>();
        private readonly Dictionary<TextBlock, UserControl> _linkToPageFormatting = new Dictionary<TextBlock, UserControl>();

        private readonly UserControl _sqlGenerationControl;
        private readonly TextEditorOptions _textEditorOptions = new TextEditorOptions();
        private readonly SqlTextEditorOptions _textEditorSqlOptions = new SqlTextEditorOptions();
        private readonly ChildWindow _childWindow;

        private TextBlock _currentGeneralSelectedLink;
        private TextBlock _currentFormattingSelectedLink;

        public QueryPropertiesWindow()
        {
            InitializeComponent();
        }
        
        public QueryPropertiesWindow(ChildWindow childWindow, DatabaseSchemaViewOptions schemaViewOptions)
        {
            InitializeComponent();

            _childWindow = childWindow;

            linkAddObject.Visibility = Visibility.Collapsed;

            _linkToPageFormatting.Add(linkGeneral, new GeneralPage(childWindow.SqlFormattingOptions));
            _linkToPageFormatting.Add(linkMainQuery, new SqlFormattingPage(SqlBuilderOptionsPages.MainQuery, childWindow.SqlFormattingOptions));
            _linkToPageFormatting.Add(linkDerievedQueries, new SqlFormattingPage(SqlBuilderOptionsPages.DerievedQueries, childWindow.SqlFormattingOptions));
            _linkToPageFormatting.Add(linkExpressionSubqueries, new SqlFormattingPage(SqlBuilderOptionsPages.ExpressionSubqueries, childWindow.SqlFormattingOptions));
            
            _sqlGenerationControl = new SqlGenerationPage(childWindow.SqlGenerationOptions, childWindow.SqlFormattingOptions);
            _linkToPageGeneral.Add(linkBehavior, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.BehaviorOptions)));
            _linkToPageGeneral.Add(linkSchemaView, GetPropertyPage(new ObjectProperties(schemaViewOptions)));
            _linkToPageGeneral.Add(linkDesignPane, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.DesignPaneOptions)));
            _linkToPageGeneral.Add(linkVisual, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.VisualOptions)));
            //_linkToPageGeneral.Add(linkAddObject, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.)));
            _linkToPageGeneral.Add(linkDatasource, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.DataSourceOptions)));
            _linkToPageGeneral.Add(linkMetadataLoading, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.MetadataLoadingOptions)));
            _linkToPageGeneral.Add(linkMetadataStructure, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.MetadataStructureOptions)));
            _linkToPageGeneral.Add(linkQueryColumnList, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.QueryColumnListOptions)));
            _linkToPageGeneral.Add(linkQueryNavBar, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.QueryNavBarOptions)));
            _linkToPageGeneral.Add(linkUserInterface, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.UserInterfaceOptions)));
            _linkToPageGeneral.Add(linkExpressionEditor, GetPropertyPage(new ObjectProperties(childWindow.ContentControl.ExpressionEditorOptions)));

            _textEditorOptions.Assign(childWindow.ContentControl.TextEditorOptions);
            _textEditorOptions.Updated += TextEditorOptionsOnUpdated;
            _linkToPageGeneral.Add(linkTextEditor, GetPropertyPage(new ObjectProperties(_textEditorOptions)));

            _textEditorSqlOptions.Assign(childWindow.ContentControl.TextEditorSqlOptions);
            _textEditorSqlOptions.Updated += TextEditorOptionsOnUpdated;
            _linkToPageGeneral.Add(linkTextEditorSql, GetPropertyPage(new ObjectProperties(_textEditorSqlOptions)));

            GeneralLinkClick(linkGeneration, null);
            FormattingLinkClick(linkGeneral, null);
        }

        private void TextEditorOptionsOnUpdated(object sender, EventArgs eventArgs)
        {
            _childWindow.ContentControl.TextEditorOptions = _textEditorOptions;
            _childWindow.ContentControl.TextEditorSqlOptions = _textEditorSqlOptions;
        }

        private Grid GetPropertyPage(ObjectProperties propertiesObject)
        {
            var propertiesContainer = PropertiesFactory.GetPropertiesContainer(propertiesObject);

            // create property page control
            var propertyPage = new PropertiesBar();

            // set properties to property page
            var propertiesControl = (IPropertiesControl)propertyPage;
            propertiesControl.SetProperties(propertiesContainer);

            return propertyPage;
        }

        private void GeneralLinkClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentGeneralSelectedLink != null)
                _currentGeneralSelectedLink.Foreground = Brushes.Black;

            _currentGeneralSelectedLink = (TextBlock)sender;
            _currentGeneralSelectedLink.Foreground = Brushes.Blue;

            if (Equals(_currentGeneralSelectedLink, linkGeneration))
            {
                gridGeneral.Children.Clear();
                _sqlGenerationControl.Margin = new Thickness(10, 10, 0, 0);
                gridGeneral.Children.Add(_sqlGenerationControl);
                return;
            }

            SwitchGeneralPage(_linkToPageGeneral[_currentGeneralSelectedLink]);
        }

        private void SwitchGeneralPage(Grid page)
        {
            gridGeneral.Children.Clear();
            page.Margin = new Thickness(10, 10, 0, 0);
            gridGeneral.Children.Add(page);
        }

        private void SwitchFormattingPage(UserControl page)
        {
            gridFormatting.Children.Clear();
            page.Margin = new Thickness(10, 10, 0, 0);
            gridFormatting.Children.Add(page);
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {            
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void FormattingLinkClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentFormattingSelectedLink != null)
                _currentFormattingSelectedLink.Foreground = Brushes.Black;

            _currentFormattingSelectedLink = (TextBlock)sender;
            _currentFormattingSelectedLink.Foreground = Brushes.Blue;
            SwitchFormattingPage(_linkToPageFormatting[_currentFormattingSelectedLink]);
        }
    }
}
