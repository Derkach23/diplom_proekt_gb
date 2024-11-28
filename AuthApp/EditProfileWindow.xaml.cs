using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class EditProfileWindow : Window
    {
        public User UpdatedUser { get; private set; }

        public EditProfileWindow(User user)
        {
            InitializeComponent();
            UsernameTextBox.Text = user.Username;
            UpdatedUser = user;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatedUser.Username = UsernameTextBox.Text;
            string newPassword = PasswordBox.Password;

            // Логика для обновления профиля в базе данных
            var dbHelper = new DatabaseHelper();
            dbHelper.UpdateUserProfile(UpdatedUser.Id, UpdatedUser.Username, newPassword);

            MessageBox.Show("Профиль успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
    }
}
