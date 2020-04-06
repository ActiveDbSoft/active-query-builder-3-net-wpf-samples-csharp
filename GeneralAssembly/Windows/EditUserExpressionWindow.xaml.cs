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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
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
        private UserExpressionVisualItem _editingUserExpression;

        public EditUserExpressionWindow()
        {
            InitializeComponent();

            foreach (var name in Enum.GetNames(typeof(DbType)))
                ComboboxDbTypes.Items.Add(new SelectableItem(name));
        }

        public void Load(IQueryView queryView)
        {
            ListBoxUserExpressions.Items.Clear();
            _editingUserExpression = null;

            if (_queryView == null)
                _queryView = queryView;

            if (_queryView == null) return;

            foreach (var expression in _queryView.UserPredefinedConditions)
            {
                ListBoxUserExpressions.Items.Add(new UserExpressionVisualItem(expression));
            }
        }

        private void ClearFormButton_OnClick(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void AddFormButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveUserExpression();
        }

        private void RemoveSelectedButton_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedUserExpression();
        }

        private void SaveUserExpression()
        {
            if (_queryView == null) return;

            var listTypes = ComboboxDbTypes.Items.Where(x => x.IsChecked).Select(selectableItem =>
                (DbType) Enum.Parse(typeof(DbType), selectableItem.Content.ToString(), true)).ToList();

            var userExpression = new PredefinedCondition(
                TextBoxCaption.Text,
                listTypes,
                TextBoxExpression.Text, CheckBoxIsNeedEdit.IsChecked == true
            );

            if (_editingUserExpression != null)
                _queryView.UserPredefinedConditions.Remove(_editingUserExpression.ConditionExpression);

            _queryView.UserPredefinedConditions.Add(userExpression);

            Load(null);

            ResetForm();
        }

        private void RemoveSelectedUserExpression()
        {
            var itemForRemove = ListBoxUserExpressions.SelectedItems.OfType<UserExpressionVisualItem>().ToList();

            foreach (var item in itemForRemove)
            {
                _queryView.UserPredefinedConditions.Remove(item.ConditionExpression);
            }

            Load(null);
        }

        private void ResetForm()
        {
            _editingUserExpression = null;
            TextBoxCaption.Text = string.Empty;
            TextBoxExpression.Text = string.Empty;
            CheckBoxIsNeedEdit.IsChecked = false;
            foreach (var selectableItem in ComboboxDbTypes.Items)
                selectableItem.IsChecked = false;
        }

        private void EditExpressionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListBoxUserExpressions.SelectedItems.Count != 1) return;

            _editingUserExpression = ListBoxUserExpressions.SelectedItem as UserExpressionVisualItem;

            if (_editingUserExpression == null) return;

            TextBoxCaption.Text = _editingUserExpression.Caption;
            TextBoxExpression.Text = _editingUserExpression.Expression;
            CheckBoxIsNeedEdit.IsChecked = _editingUserExpression.IsNeedEdit;

            foreach (var item in _editingUserExpression.ShowOnlyForDbTypes.Select(type => ComboboxDbTypes.Items.First(x =>
                string.Equals(x.Content.ToString(), type.ToString(), StringComparison.InvariantCultureIgnoreCase))))
            {
                item.IsChecked = true;
            }
        }
    }

    public class UserExpressionVisualItem 
    {
        public List<DbType> ShowOnlyForDbTypes { get; set; }

        public string Caption { get; set; }
        public string Expression { get; set; }
        public bool IsNeedEdit { get; set; }

        public PredefinedCondition ConditionExpression { get; private set; }

        public UserExpressionVisualItem(PredefinedCondition conditionExpression)
        {
            ConditionExpression = conditionExpression;
            ShowOnlyForDbTypes = new List<DbType>();

            Caption = conditionExpression.Caption;
            Expression = conditionExpression.Expression;
            IsNeedEdit = conditionExpression.IsNeedEdit;

            ShowOnlyForDbTypes.AddRange(conditionExpression.ShowOnlyFor);
        }

        public override string ToString()
        {
            var types = ShowOnlyForDbTypes.Count == 0 ? "For all types": "For types: ";
            foreach (var dbType in ShowOnlyForDbTypes)
            {
                if (ShowOnlyForDbTypes.IndexOf(dbType) != 0)
                    types += ", ";

                types += dbType;
            }

            return $"{Caption}, [{Expression}], Is need edit: {IsNeedEdit}, {types}";
        }
    }
}
