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
using System.Windows.Controls;
using System.Windows.Documents;

namespace FullFeaturedDemo.Common
{
    /// <summary>
    /// Interaction logic for WindowDispatcherUnhandledException.xaml
    /// </summary>
    public  partial class WindowDispatcherUnhandledException
    {
        private static WindowDispatcherUnhandledException _internal;

        public WindowDispatcherUnhandledException()
        {
            InitializeComponent();
        }

        public static void Show(Exception ex)
        {
            if (_internal == null)
            {
                _internal = new WindowDispatcherUnhandledException();
            }

            _internal.TextBlockMessageError.Text = ex.Message;

            SetTextRichTextBox(ex.StackTrace, _internal.StackTraceBox);
            _internal.ShowDialog();
        }

        private static void SetTextRichTextBox(string text, RichTextBox editor)
        {
            editor.Document.Blocks.Clear();
            editor.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
    }
}
