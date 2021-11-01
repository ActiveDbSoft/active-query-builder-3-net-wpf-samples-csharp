//*******************************************************************//
//       Active Query Builder Component Suite                        //
//                                                                   //
//       Copyright © 2006-2021 Active Database Software              //
//       ALL RIGHTS RESERVED                                         //
//                                                                   //
//       CONSULT THE LICENSE AGREEMENT FOR INFORMATION ON            //
//       RESTRICTIONS.                                               //
//*******************************************************************//

using System.Windows;
using System.Windows.Controls;

namespace GeneralAssembly.Windows.SaveWindows
{
    public class SaveExistQueryDialog :WindowMessage
    {
        public bool? Result;

        private const int WIDTH_BUTTON = 90;

        public SaveExistQueryDialog()
        {
            Title = "Save dialog";
            Text = "Save changes?";
            ContentAlignment = HorizontalAlignment.Center;
            Result = null;

            var saveButton = new Button
            {
                Content = "Save",
                Width = WIDTH_BUTTON,
                IsDefault = true
            };
            saveButton.Click += delegate
            {
                Result = true;
                Close();
            };

            var notSaveButton = new Button
            {
                Margin = new Thickness(5, 0, 5, 0),
                Content = "Don't save",
                Width = WIDTH_BUTTON,
                IsCancel = true
            };
            notSaveButton.Click += delegate
            {
                Result = false;
                Close();
            };

            var continueButton = new Button
            {
                Content = "Continue edit",
                Width = WIDTH_BUTTON
            };
            continueButton.Click += delegate { Close(); };

            Buttons.Add(saveButton);
            Buttons.Add(notSaveButton);
            Buttons.Add(continueButton);
        }
    }
}
