using Microsoft.Win32;
using sberbank.Services;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace sberbank.View
{
    public partial class AdminWindow : Window
    {
        private readonly DatabaseService _database = new DatabaseService();
        private DataTable _usersTable;
        private DataTable _clientsTable;
        private DataTable _productsTable;
        private DataTable _applicationsTable;

        public AdminWindow()
        {
            InitializeComponent();
            LoadStatuses();
            LoadActiveTab();
        }

        private void LoadStatuses()
        {
            StatusComboBox.ItemsSource = _database.GetStatuses().DefaultView;
        }

        private void LoadActiveTab()
        {
            try
            {
                var search = SearchTextBox.Text.Trim();
                switch (AdminTabs.SelectedIndex)
                {
                    case 0:
                        _usersTable = _database.GetTable("Users", search);
                        UsersDataGrid.ItemsSource = _usersTable.DefaultView;
                        break;
                    case 1:
                        _clientsTable = _database.GetTable("Clients", search);
                        ClientsDataGrid.ItemsSource = _clientsTable.DefaultView;
                        break;
                    case 2:
                        _productsTable = _database.GetTable("BankProducts", search);
                        ProductsDataGrid.ItemsSource = _productsTable.DefaultView;
                        break;
                    case 3:
                        _applicationsTable = _database.GetTable("Applications", search);
                        ApplicationsDataGrid.ItemsSource = _applicationsTable.DefaultView;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataGrid CurrentGrid()
        {
            switch (AdminTabs.SelectedIndex)
            {
                case 0: return UsersDataGrid;
                case 1: return ClientsDataGrid;
                case 2: return ProductsDataGrid;
                case 3: return ApplicationsDataGrid;
                default: return UsersDataGrid;
            }
        }

        private DataTable CurrentTable()
        {
            switch (AdminTabs.SelectedIndex)
            {
                case 0: return _usersTable;
                case 1: return _clientsTable;
                case 2: return _productsTable;
                case 3: return _applicationsTable;
                default: return null;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActiveTab();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            LoadActiveTab();
        }

        private void AdminTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && e.Source is TabControl)
            {
                LoadActiveTab();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdminTabs.SelectedIndex == 3)
            {
                MessageBox.Show("Заявки создаются клиентами из личного кабинета. Администратор меняет статус.", "Заявки", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CurrentTable()?.Rows.Add(CurrentTable().NewRow());
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var view = CurrentGrid().SelectedItem as DataRowView;
            if (view == null)
            {
                MessageBox.Show("Выберите строку для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            view.Delete();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (AdminTabs.SelectedIndex)
                {
                    case 0:
                        _database.SaveUsers(_usersTable);
                        break;
                    case 1:
                        _database.SaveClients(_clientsTable);
                        break;
                    case 2:
                        _database.SaveProducts(_productsTable);
                        break;
                    case 3:
                        MessageBox.Show("Для заявок используйте кнопку изменения статуса.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                }

                MessageBox.Show("Изменения сохранены.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadActiveTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var row = ApplicationsDataGrid.SelectedItem as DataRowView;
            if (row == null || StatusComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите заявку и статус.", "Заявки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _database.UpdateApplicationStatus(Convert.ToInt32(row["ApplicationId"]), Convert.ToInt32(StatusComboBox.SelectedValue));
                LoadActiveTab();
                MessageBox.Show("Статус заявки изменен.", "Заявки", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка статуса", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var table = CurrentTable();
            if (table == null)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV файл (*.csv)|*.csv",
                FileName = table.TableName + ".csv"
            };

            if (dialog.ShowDialog() == true)
            {
                CsvExportService.ExportDataTable(table, dialog.FileName);
                MessageBox.Show("CSV-файл сохранен.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из админ-панели?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SessionService.SignOut();
                new AuthorizationWindow().Show();
                Close();
            }
        }
    }
}
