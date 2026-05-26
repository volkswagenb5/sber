using sberbank.Model;
using sberbank.Services;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace sberbank.View
{
    public partial class AuthorizationWindow : Window
    {
        private readonly DatabaseService _database = new DatabaseService();

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User user = _database.GetUserByLogin(login);
                if (user == null)
                {
                    MessageBox.Show("Пользователь с таким логином не найден.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (user.Password != password)
                {
                    MessageBox.Show("Неверный пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SessionService.SignIn(user);
                Window nextWindow = user.IsAdmin ? (Window)new AdminWindow() : new MainWindow();
                nextWindow.Show();
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось подключиться к SQL Server. Проверьте SberbankDB и строку подключения.\n\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            new RegistrationWindow().Show();
            Close();
        }
    }
}
