//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2019 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Documents;

namespace CustomExpressionBuilderDemo.Common
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow
    {
        public string Message
        {
            get { return BlockMessage.Text; }
            set { BlockMessage.Text = value; }
        }

        public string StackTrace
        {
            get { return new TextRange(BoxStackTrace.Document.ContentStart, BoxStackTrace.Document.ContentEnd).Text; }
            set
            {
                BoxStackTrace.Document.Blocks.Clear();
                BoxStackTrace.Document.Blocks.Add(new Paragraph(new Run(value)));
            }
        }

        public ExceptionWindow()
        {
            InitializeComponent();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
