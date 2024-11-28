using Npgsql;
using System.Windows;

namespace AuthApp
{
    public partial class MainPage : Window
    {
        private int _userId;
        private DatabaseHelper _dbHelper;
        private User _currentUser;
        private string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=docudb";

        public MainPage(int userId)
        {
            InitializeComponent();
            _userId = userId; // Сохраняем идентификатор пользователя
            _dbHelper = new DatabaseHelper(); // Инициализируем DatabaseHelper

            _currentUser = GetUserById(_userId);

            if (_currentUser != null && _currentUser.IsAdmin)
            {
                CreateTaskButton.Visibility = Visibility.Visible;
            }

            var userProfileView = new UserProfileView(_currentUser);
            

        }

        private User GetUserById(int userId)
        {
            // Метод для получения пользователя из базы данных по userId
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, Username, Password, IsAdmin FROM users WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", userId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            IsAdmin = reader.GetBoolean(3)
                        };
                    }
                }
            }
            return null;
        }

        private void LoadTasksView()
        {
            // Создаем экземпляр TasksView, передавая userId для фильтрации задач
            var tasksView = new TasksView(_userId);
            ContentArea.Content = tasksView; // ContentArea — это элемент, где отображается TasksView
        }

        private void OpenCreateTaskWindow()
        {
            if (_currentUser.IsAdmin)
            {
                var createTaskWindow = new CreateTaskWindow(_currentUser.Id);
                createTaskWindow.ShowDialog();
                LoadTasksView(); 
            }
            else
            {
                MessageBox.Show("У вас нет прав для создания задачи.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            OpenCreateTaskWindow();
        }

        private void TasksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTasksView(); 
        }

        private void DocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            var documentsView = new DocumentsView(_userId); // Передаем ID пользователя
            ContentArea.Content = documentsView; // Отображаем список документов
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            UserProfileView userProfileView = new UserProfileView(_currentUser);

            ContentArea.Content = userProfileView;
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportWindow();
            ContentArea.Content = reportWindow;
        }
    }
}
