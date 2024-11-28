using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AuthApp
{
    public partial class UserProfileView : UserControl
    {
        private User _currentUser;
        private DatabaseHelper _dbHelper;

        public UserProfileView(User user)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _currentUser = user;

            DisplayUserInfo();
        }

        private void DisplayUserInfo()
        {
            UsernameTextBlock.Text = _currentUser.Username;
            RoleTextBlock.Text = _currentUser.IsAdmin ? "Администратор" : "Пользователь";

            // Загружаем изображение профиля, если путь задан
            if (!string.IsNullOrEmpty(_currentUser.AvatarPath))
            {
                AvatarImage.Source = new BitmapImage(new Uri(_currentUser.AvatarPath, UriKind.RelativeOrAbsolute));
            }
            else
            {
                AvatarImage.Source = new BitmapImage(new Uri("C:\\Users\\Admin\\Pictures\\i-_2_.png")); // Укажите путь к изображению по умолчанию
            }
        }

            private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика для редактирования профиля
            var editProfileWindow = new EditProfileWindow(_currentUser);
            if (editProfileWindow.ShowDialog() == true)
            {
                _currentUser = editProfileWindow.UpdatedUser;
                DisplayUserInfo();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Обновляем информацию о пользователе из базы данных
            _currentUser = _dbHelper.GetUserById(_currentUser.Id);
            DisplayUserInfo();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Логика для выхода из системы
            Application.Current.Shutdown();
        }
    }
}
