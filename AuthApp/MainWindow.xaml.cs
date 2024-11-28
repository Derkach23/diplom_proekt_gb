using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем введенные имя пользователя и пароль
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Создаем экземпляр AuthService для аутентификации
            var authService = new AuthService();
            User currentUser = authService.Authenticate(username, password);

            // Проверяем, был ли найден пользователь
            if (currentUser != null)
            {
                // Если аутентификация успешна, открываем главное окно с задачами
                var mainPage = new MainPage(currentUser.Id); // Передаем идентификатор пользователя
                mainPage.Show();

                // Закрываем окно входа
                this.Close();
            }
            else
            {
                // Если аутентификация неудачна, выводим сообщение
                MessageBox.Show("Неправильное имя пользователя или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}