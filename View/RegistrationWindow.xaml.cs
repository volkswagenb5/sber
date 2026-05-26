using sberbank.Services;
using System;
using System.Linq;
using System.Windows;

namespace sberbank.View
{
    public partial class RegistrationWindow : Window
    {
        private readonly DatabaseService _database = new DatabaseService();

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль обязательны.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string passwordError = ValidatePassword(password);
            if (!string.IsNullOrEmpty(passwordError))
            {
                MessageBox.Show(passwordError, "Проверка пароля", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_database.LoginExists(login))
                {
                    MessageBox.Show("Этот логин уже занят.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _database.RegisterUser(login, password, FullNameTextBox.Text.Trim(), PhoneTextBox.Text.Trim(), EmailTextBox.Text.Trim());
                MessageBox.Show("Пользователь зарегистрирован. Теперь можно войти.", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                BackToAuthorization();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ValidatePassword(string password)
        {
            if (password.Length <= 4)
            {
                return "Пароль должен быть длиннее 4 символов.";
            }
            if (!password.Any(char.IsDigit))
            {
                return "Пароль должен содержать минимум 1 цифру.";
            }
            if (!password.Any(char.IsUpper))
            {
                return "Пароль должен содержать минимум 1 заглавную букву.";
            }
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return "Пароль должен содержать минимум 1 специальный символ.";
            }
            return null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackToAuthorization();
        }

        private void BackToAuthorization()
        {
            new AuthorizationWindow().Show();
            Close();
        }
    }
}
