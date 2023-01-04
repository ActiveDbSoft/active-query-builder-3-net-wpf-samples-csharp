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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.View.QueryView;
using GeneralAssembly.Common;

namespace GeneralAssembly.Windows
{
    /// <summary>
    /// Interaction logic for EditUserExpressionWindow.xaml
    /// </summary>
    public partial class EditUserExpressionWindow
    {
        private IQueryView _queryView;
        private UserConditionVisualItem _selectedPredefinedCondition;

        private readonly ObservableCollection<UserConditionVisualItem> _source =
            new ObservableCollection<UserConditionVisualItem>();

        public EditUserExpressionWindow()
        {
            InitializeComponent();

            ListBoxUserConditions.ItemsSource = _source;

            foreach (var name in Enum.GetNames(typeof(DbType)))
                ComboboxDbTypes.Items.Add(new SelectableItem(name));
        }

        public void Load(IQueryView queryView)
        {
            _source.Clear();
            _selectedPredefinedCondition = null;

            if (_queryView == null)
                _queryView = queryView;

            if (_queryView == null) return;

            foreach (var expression in _queryView.UserPredefinedConditions)
            {
                _source.Add(new UserConditionVisualItem(expression));
            }
        }

        private void ButtonSaveForm_OnClick(object sender, RoutedEventArgs e)
        {
            SaveUserExpression();
        }

        private bool SaveUserExpression()
        {
            if (_queryView == null) return false;
            
            try
            {
                var result = _queryView.Query.SQLContext.ParseLogicalExpression(TextBoxExpression.Text, false, false, out var token);
                if (result == null && token != null)
                {
                    throw new SQLParsingException(
                        string.Format(
                            ActiveQueryBuilder.Core.Helpers.Localizer.GetString(nameof(LocalizableConstantsUI.strInvalidCondition),
                                ActiveQueryBuilder.View.WPF.Helpers.ConvertLanguageFromNative(Language),
                                LocalizableConstantsUI.strInvalidCondition), TextBoxExpression.Text),
                        token);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Invalid SQL", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK, MessageBoxOptions.None);

                TextBoxExpression.Focus();
                Keyboard.Focus(TextBoxExpression);
                return false;
            }

            var listTypes = ComboboxDbTypes.Items.Where(x => x.IsChecked).Select(selectableItem =>
                (DbType)Enum.Parse(typeof(DbType), selectableItem.Content.ToString(), true)).ToList();

            var userExpression = new PredefinedCondition(
                TextBoxCaption.Text,
                listTypes,
                TextBoxExpression.Text, CheckBoxIsNeedEdit.IsChecked == true
            );

            var index = -1;
            if (_selectedPredefinedCondition != null)
            {
                index = _queryView.UserPredefinedConditions.IndexOf(_selectedPredefinedCondition.PredefinedCondition);
                _queryView.UserPredefinedConditions.Remove(_selectedPredefinedCondition.PredefinedCondition);
            }

            if (_queryView.UserPredefinedConditions.Any(x =>
                string.Compare(x.Caption, TextBoxCaption.Text, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                MessageBox.Show($"Condition with caption \"{TextBoxCaption.Text}\" already exist", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Keyboard.Focus(TextBoxCaption);

                return false;
            }

            try
            {
                if (index != -1)
                    _queryView.UserPredefinedConditions.Insert(index, userExpression);
                else
                    _queryView.UserPredefinedConditions.Add(userExpression);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Load(null);

            ResetForm();

            if (index >= 0)
                ListBoxUserConditions.SelectedIndex = index;

            return true;
        }

        private void RemoveSelectedUserConditions()
        {
            var itemForRemove = ListBoxUserConditions.SelectedItems.OfType<UserConditionVisualItem>().ToList();

            foreach (var item in itemForRemove)
                _queryView.UserPredefinedConditions.Remove(item.PredefinedCondition);

            Load(null);

            ResetForm();
        }

        private void ResetForm()
        {
            _selectedPredefinedCondition = null;
            TextBoxCaption.Text = string.Empty;
            TextBoxExpression.Text = string.Empty;
            CheckBoxIsNeedEdit.IsChecked = false;

            foreach (var selectableItem in ComboboxDbTypes.Items)
                selectableItem.IsChecked = false;
        }

        private void ListBoxUserExpressions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateForm();
        }

        private void UpdateForm()
        {
            var enable = ListBoxUserConditions.SelectedItem is UserConditionVisualItem;

            ButtonCopyCurrent.IsEnabled = enable;
            ButtonDelete.IsEnabled = enable;
            ButtonMoveUp.IsEnabled = enable;
            ButtonMoveDown.IsEnabled = enable;

            if (!enable) return;

            _selectedPredefinedCondition = (UserConditionVisualItem)ListBoxUserConditions.SelectedItem;

            TextBoxCaption.Text = _selectedPredefinedCondition.Caption;
            TextBoxExpression.Text = _selectedPredefinedCondition.Condition;
            CheckBoxIsNeedEdit.IsChecked = _selectedPredefinedCondition.IsNeedEdit;

            foreach (var item in _selectedPredefinedCondition.ShowOnlyForDbTypes.Select(type => ComboboxDbTypes.Items.First(x =>
                string.Equals(x.Content.ToString(), type.ToString(), StringComparison.InvariantCultureIgnoreCase))))
                item.IsChecked = true;

            ButtonSave.IsEnabled = false;
        }

        private void ButtonAddNew_OnClick(object sender, RoutedEventArgs e)
        {
            ResetForm();
            ListBoxUserConditions.SelectedItem = null;
            Keyboard.Focus(TextBoxCaption);
        }

        private void ButtonCopyCurrent_OnClick(object sender, RoutedEventArgs e)
        {
            _selectedPredefinedCondition = ListBoxUserConditions.SelectedItem as UserConditionVisualItem;

            if (_selectedPredefinedCondition == null) return;

            var name = _selectedPredefinedCondition.Caption;

            var newName = "";

            if (_source.All(x => string.Compare(x.Caption, $"{name} Copy", StringComparison.InvariantCultureIgnoreCase) != 0))
            {
                newName = $"{name} Copy";
            }
            else
            {
                for (var i = 1; i < 1000; i++)
                {
                    if (_source.Any(x => string.Compare(x.Caption, $"{name} Copy ({i})", StringComparison.InvariantCultureIgnoreCase) == 0)) continue;

                    newName = $"{name} Copy ({i})";
                    break;
                }
            }

            var newCopy = _selectedPredefinedCondition.Copy(newName);
            var index = _source.IndexOf(_selectedPredefinedCondition);

            _queryView.UserPredefinedConditions.Insert(index + 1, newCopy.PredefinedCondition);

            Load(null);
            ListBoxUserConditions.SelectedIndex = index + 1;
        }


        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedUserConditions();
        }

        private void ButtonMoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = ListBoxUserConditions.SelectedItem as UserConditionVisualItem;

            if (selectedItem == null) return;

            var index = _source.IndexOf(selectedItem);

            if (index - 1 < 0) return;

            ActiveQueryBuilder.Core.Helpers.IListMove(_queryView.UserPredefinedConditions, index, index - 1);

            Load(null);

            ListBoxUserConditions.SelectedIndex = index - 1;
        }

        private void ButtonMoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = ListBoxUserConditions.SelectedItem as UserConditionVisualItem;

            if (selectedItem == null) return;

            var index = _source.IndexOf(selectedItem);

            if (index + 1 >= _source.Count) return;

            ActiveQueryBuilder.Core.Helpers.IListMove(_queryView.UserPredefinedConditions, index, index + 1);

            Load(null);

            ListBoxUserConditions.SelectedIndex = index + 1;
        }

        private void CheckChangingItem()
        {
            if (_selectedPredefinedCondition == null)
            {
                ButtonSave.IsEnabled =
                    !string.IsNullOrEmpty(TextBoxCaption.Text) && !string.IsNullOrEmpty(TextBoxExpression.Text);

                return;
            }

            var dbTypes = ComboboxDbTypes.Items.Where(x => x.Content != null && x.IsChecked ).Select(x=> (DbType)Enum.Parse(typeof(DbType), x.Content.ToString(), true)).ToList();

            var changed = string.Compare(TextBoxCaption.Text, _selectedPredefinedCondition.Caption,
                              StringComparison.InvariantCulture) != 0 ||
                          string.Compare(TextBoxExpression.Text, _selectedPredefinedCondition.Condition,
                              StringComparison.InvariantCulture) != 0 ||
                          CheckBoxIsNeedEdit.IsChecked != _selectedPredefinedCondition.IsNeedEdit ||
                              !((_selectedPredefinedCondition.ShowOnlyForDbTypes.Count == dbTypes.Count) && !_selectedPredefinedCondition.ShowOnlyForDbTypes.Except(dbTypes).Any());

            ButtonSave.IsEnabled = changed;
        }

        private void TextBoxExpression_OnTextChanged(object sender, EventArgs e)
        {
            CheckChangingItem();
        }

        private void ComboboxDbTypes_OnItemCheckStateChanged(object sender, EventArgs e)
        {
            CheckChangingItem();
        }

        private void CheckBoxIsNeedEdit_OnCheckChanged(object sender, RoutedEventArgs e)
        {
            CheckChangingItem();
        }

        private void TextBoxCaption_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckChangingItem();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ButtonSave.IsEnabled)
            {
                var result =
                    MessageBox.Show(@"Save changes to the current condition?",
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes && !SaveUserExpression())
                    e.Cancel = true;
            }

            base.OnClosing(e);
        }

        private void ButtonRevert_OnClick(object sender, RoutedEventArgs e)
        {
            ResetForm();
            UpdateForm();
        }
    }

    public class UserConditionVisualItem
    {
        public List<DbType> ShowOnlyForDbTypes { get; set; }

        public string Caption { get; }
        public string Condition { get; }
        public bool IsNeedEdit { get; }

        public PredefinedCondition PredefinedCondition { get; }


        public UserConditionVisualItem(PredefinedCondition predefinedCondition)
        {
            PredefinedCondition = predefinedCondition;
            ShowOnlyForDbTypes = new List<DbType>();

            Caption = predefinedCondition.Caption;
            Condition = predefinedCondition.Expression;
            IsNeedEdit = predefinedCondition.IsNeedEdit;

            ShowOnlyForDbTypes.AddRange(predefinedCondition.ShowOnlyFor);
        }

        public override string ToString()
        {
            return $"{Caption}";
        }

        public UserConditionVisualItem Copy(string newName)
        {
            return new UserConditionVisualItem(new PredefinedCondition(newName, PredefinedCondition.ShowOnlyFor,
                PredefinedCondition.Expression, PredefinedCondition.IsNeedEdit));
        }
    }
}
