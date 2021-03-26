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
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.PropertiesEditors;
using ActiveQueryBuilder.View.WPF;
using Helpers = ActiveQueryBuilder.View.WPF.Helpers;

namespace MetadataEditorDemo.Common
{
    /// <summary>
    /// Interaction logic for InformationMessageControl.xaml
    /// </summary>
    internal partial class InformationMessageControl
    {
        public static readonly DependencyProperty IsShowCloseButtonProperty = DependencyProperty.Register(
            "IsShowCloseButton", typeof(bool), typeof(InformationMessageControl), new PropertyMetadata(true));

        public bool IsShowCloseButton
        {
            get { return (bool) GetValue(IsShowCloseButtonProperty); }
            set { SetValue(IsShowCloseButtonProperty, value); }
        }

        public event EventHandler FixIssueEvent;
        public event EventHandler Closing;

        public object Owner { set; get; }
        private PropertyErrorDescription _errorDescription;
        public PropertyErrorDescription ErrorDescription { get { return _errorDescription; } }
        public int MaxLengthValue { set; get; }
        private static readonly Regex Regex = new Regex("\"([^\"]+)\"");

        public static readonly DependencyProperty ImageIconProperty = DependencyProperty.Register(
            "ImageIcon", typeof(CImage), typeof(InformationMessageControl), new PropertyMetadata(default(CImage)));

        public CImage ImageIcon
        {
            get { return (CImage) GetValue(ImageIconProperty); }
            set { SetValue(ImageIconProperty, value); }
        }

        public InformationMessageControl()
        {
            InitializeComponent();

            SetValue(Panel.ZIndexProperty, int.MaxValue);
            MaxLengthValue = 20;
            Hide();

            ActiveQueryBuilder.View.WPF.Images.Common.Exclamation.Value.DefaultSize = new Size(24,24);
            ActiveQueryBuilder.View.WPF.Images.Common.AlertExclamation.Value.DefaultSize = new Size(24, 24);
        }

        public void Show(PropertyErrorDescription description)
        {
            _errorDescription = description;  

            var message = description.Message;
            var match = Regex.Match(message);

            while (match.Success)
            {
                var value = match.Groups[1].ToString();

                if (value.Length > MaxLengthValue)
                    message = message.Replace(value, value.Remove(MaxLengthValue) + "...");

                match = match.NextMatch();
            }

            Show(message, description.IsError);
        }

        public void Show(string message, bool isError)
        {
            if (string.IsNullOrEmpty(message))
            {
                Hide();
                return;
            }

            ImageIcon = isError ? ActiveQueryBuilder.View.WPF.Images.Common.Exclamation.Value : ActiveQueryBuilder.View.WPF.Images.Common.AlertExclamation.Value;
            BoxMessage.Text = message;

            if (isError)
            {
                var link = new Hyperlink(
                    new Run(Helpers.Localizer.GetString("StrFixIssue", Helpers.ConvertLanguageFromNative(Language), LocalizableConstantsUI.strFixIssue)));

                link.Click += delegate
                {
                    OnFixIssue();
                };

                BoxMessage.Inlines.Add(" ");
                BoxMessage.Inlines.Add(link);
            }

            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            BoxMessage.Text = string.Empty;
            ImageIcon = null;
        }

        private void LinkLabelClose_OnLinkClicked(object sender, RoutedEventArgs e)
        {
            Closing?.Invoke(this, EventArgs.Empty);

            Hide();
        }

        protected virtual void OnFixIssue()
        {
            var handler = FixIssueEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
