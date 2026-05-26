using sberbank.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace sberbank.Pages
{
    public partial class ServicesPage : Page
    {
        private readonly DatabaseService _database = new DatabaseService();

        public ServicesPage()
        {
            InitializeComponent();
            LoadTypes();
            LoadProducts();
        }

        private void LoadTypes()
        {
            TypeComboBox.Items.Clear();
            TypeComboBox.Items.Add("Все типы");
            foreach (var type in _database.GetProductTypes())
            {
                TypeComboBox.Items.Add(type);
            }
            TypeComboBox.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            var type = TypeComboBox.SelectedItem as string;
            if (type == "Все типы")
            {
                type = null;
            }

            ProductsItemsControl.ItemsSource = _database.GetBankProducts(type, SortCheckBox.IsChecked == true);
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (ProductsItemsControl != null)
            {
                LoadProducts();
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                return;
            }

            var button = (Button)sender;
            var productId = Convert.ToInt32(button.Tag);
            var result = MessageBox.Show("Создать заявку на выбранную услугу?", "Заявка", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _database.CreateApplication(SessionService.CurrentUser.UserId, productId, "Заявка создана из личного кабинета");
                MessageBox.Show("Заявка создана.", "Заявка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка заявки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
