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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ActiveQueryBuilder.Core;

namespace GeneralAssembly.Common
{
    public partial class SqlErrorBox
    {
        private bool _allowChangedSyntax = true;

        public event SelectionChangedEventHandler SyntaxProviderChanged;
        public event EventHandler GoToErrorPosition;
        public event EventHandler RevertValidText;

        public static readonly DependencyProperty VisibilityCheckSyntaxBlockProperty = DependencyProperty.Register(
            "VisibilityCheckSyntaxBlock", typeof(Visibility), typeof(SqlErrorBox), new PropertyMetadata(Visibility.Collapsed));

        public Visibility VisibilityCheckSyntaxBlock
        {
            get { return (Visibility) GetValue(VisibilityCheckSyntaxBlockProperty); }
            set { SetValue(VisibilityCheckSyntaxBlockProperty, value); }
        }
        
        public SqlErrorBox()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            var collection = new ObservableCollection<ComboBoxItem>();
            foreach (Type syntax in ActiveQueryBuilder.Core.Helpers.SyntaxProviderList)
            {
                var instance = Activator.CreateInstance(syntax) as BaseSyntaxProvider;
                collection.Add(new ComboBoxItem(instance));
            }

            ComboBoxSyntaxProvider.ItemsSource = collection;
        }

        public void Show(string message, BaseSyntaxProvider currentSyntaxProvider)
        {
            if (string.IsNullOrEmpty(message))
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            _allowChangedSyntax = false;
            TextBlockErrorPrompt.Text = message;
            ComboBoxSyntaxProvider.Text = currentSyntaxProvider.ToString();
            _allowChangedSyntax = true;
            Visibility = Visibility.Visible;
        }

        private void ComboBoxSyntaxProvider_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_allowChangedSyntax) return;

            var syntaxProvider = ((ComboBoxItem)ComboBoxSyntaxProvider.SelectedItem).SyntaxProvider;

            OnSyntaxProviderChanged(new SelectionChangedEventArgs(e.RoutedEvent,
                new List<BaseSyntaxProvider>(), new List<BaseSyntaxProvider> { syntaxProvider }));
        }

        protected virtual void OnSyntaxProviderChanged(SelectionChangedEventArgs e)
        {
            SyntaxProviderChanged?.Invoke(this, e);
            Visibility = Visibility.Collapsed;
        }

        private void HyperlinkGoToPosition_OnClick(object sender, RoutedEventArgs e)
        {
            OnGoToErrorPositionEvent();
            Visibility = Visibility.Collapsed;
        }

        private void HyperlinkPreviousValidText_OnClick(object sender, RoutedEventArgs e)
        {
            OnRevertValidTextEvent();
            Visibility = Visibility.Collapsed;
        }

        protected virtual void OnGoToErrorPositionEvent()
        {
            GoToErrorPosition?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRevertValidTextEvent()
        {
            RevertValidText?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ComboBoxItem
    {
        public BaseSyntaxProvider SyntaxProvider { get; }
        public string DisplayString => SyntaxProvider.ToString();
        public ComboBoxItem() { }

        public ComboBoxItem(BaseSyntaxProvider provider)
        {
            SyntaxProvider = provider;
        }
    }
}
