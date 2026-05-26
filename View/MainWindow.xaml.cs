using sberbank.Pages;
using sberbank.Services;
using System.Windows;

namespace sberbank.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ProfilePage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ServicesPage());
        }

        private void ApplicationsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApplicationsPage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из аккаунта?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SessionService.SignOut();
            new AuthorizationWindow().Show();
            Close();
        }
    }
}
