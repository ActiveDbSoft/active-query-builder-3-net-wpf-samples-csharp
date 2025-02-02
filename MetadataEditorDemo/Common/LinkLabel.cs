//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MetadataEditorDemo.Common
{
    internal class LinkLabel:UserControl, IDisposable
    {
        public event RoutedEventHandler LinkClicked;

        public Uri Link
        {
            set { _link.NavigateUri = value; }
            get { return _link.NavigateUri; }
        }

        private Hyperlink _link;
        private  string _uri;
        private  string _text;
        private TextBlock _textBlock;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(LinkLabel), new PropertyMetadata(default(string), TextChangeCallback));

        private static void TextChangeCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as LinkLabel;

            if(obj == null) return;

            obj._link.Inlines.Clear();
            obj._link.Inlines.Add(new Run(obj.Text));
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public LinkLabel()
        {
            Init();
        }

        public LinkLabel(string text)
        {
            _text = text;
            Init();
            SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            SnapsToDevicePixels = true;
        }

        public LinkLabel(string text, string url)
        {
            _text = text;
            _uri = url;
            Init();
        }

        private void Init()
        {
            _textBlock = new TextBlock();

            AddChild(_textBlock);

            if (_link != null)
            {
                _link.Click -= _link_Click;
                _link = null;
            }

            _link = new Hyperlink(new Run(_text));
            
            if (!string.IsNullOrEmpty(_uri))
                _link.NavigateUri = new Uri(_uri, UriKind.Absolute);
            _textBlock.Inlines.Add(_link);
            _link.Click += _link_Click;
        }

        protected virtual void OnLinkClicked(RoutedEventArgs e)
        {
            var handler = LinkClicked;
            handler?.Invoke(this, e);
        }

        private void _link_Click(object sender, RoutedEventArgs e)
        {
            OnLinkClicked(e);
        }

        public void Dispose()
        {
            _link.Click -= _link_Click;
        }
    }
}
