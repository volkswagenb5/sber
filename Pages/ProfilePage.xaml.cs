using sberbank.Model;
using sberbank.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace sberbank.Pages
{
    public partial class ProfilePage : Page
    {
        private readonly DatabaseService _database = new DatabaseService();

        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            User user = SessionService.CurrentUser;
            if (user == null)
            {
                return;
            }

            FullNameTextBox.Text = user.FullName;
            PhoneTextBox.Text = user.Phone;
            EmailTextBox.Text = user.Email;
            LoginTextBox.Text = user.Login;
            PasswordBox.Password = user.Password;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionService.CurrentUser == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Логин и пароль не должны быть пустыми.", "Профиль", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _database.UpdateProfile(SessionService.CurrentUser.UserId, LoginTextBox.Text.Trim(), PasswordBox.Password,
                    FullNameTextBox.Text.Trim(), PhoneTextBox.Text.Trim(), EmailTextBox.Text.Trim());
                SessionService.SignIn(_database.GetUserById(SessionService.CurrentUser.UserId));
                MessageBox.Show("Профиль обновлен.", "Профиль", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
