//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2025 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeneralAssembly.Windows.SaveWindows
{
    public class SaveAsWindowDialog : WindowMessage
    {
        public enum ActionSave
        {
            UserQuery,
            File,
            NotSave,
            Continue
        }

        public ActionSave Action { set; get; }

        private const int WIDTH_BUTTON = 120;

        public SaveAsWindowDialog(string nameQuery)
        {
            Background = new SolidColorBrush(SystemColors.ControlColor);
            WindowStartupLocation = WindowStartupLocation.Manual;
            SizeToContent = SizeToContent.WidthAndHeight;

            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Title = "Save Dialog";

            Action = ActionSave.NotSave;

            var border = new Border { BorderThickness = new Thickness(0), BorderBrush = Brushes.Gray };

            var root = new StackPanel { Margin = new Thickness(10) };
            border.Child = root;
            Content = border;

            var message = new TextBlock
            {
                Text = "Save changes to the [" + nameQuery + "]?",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var bottomStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var buttonSaveFile = new Button { Width = WIDTH_BUTTON, Content = "Save to file..." };
            buttonSaveFile.Click += delegate
            {
                Action = ActionSave.File;
                Close();
            };

            var buttonSaveUserQuery = new Button
            {
                Width = WIDTH_BUTTON,
                Content = "Save as User Query",
                Margin = new Thickness(5, 0, 5, 0)
            };
            buttonSaveUserQuery.Click += delegate
            {
                Action = ActionSave.UserQuery;
                Close();
            };

            var buttonNotSave = new Button
            {
                Width = WIDTH_BUTTON,
                Content = "Don't save",
                Margin = new Thickness(0, 0, 5, 0)
            };
            buttonNotSave.Click += delegate
            {
                Action = ActionSave.NotSave;
                Close();
            };


            var buttonCancel = new Button { Width = WIDTH_BUTTON, Content = "Cancel" };
            buttonCancel.Click += delegate
            {
                Action = ActionSave.Continue;
                Close();
            };

            root.Children.Add(message);
            root.Children.Add(bottomStack);

            bottomStack.Children.Add(buttonSaveFile);
            bottomStack.Children.Add(buttonSaveUserQuery);
            bottomStack.Children.Add(buttonNotSave);
            bottomStack.Children.Add(buttonCancel);
        }
    }
}
