using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class TasksView : UserControl
    {
        private int _userId; // Идентификатор текущего пользователя
        private DatabaseHelper _dbHelper; // Вспомогательный класс для работы с базой данных

        public TasksView(int userId)
        {
            InitializeComponent();
            _userId = userId; // Сохраняем userId
            _dbHelper = new DatabaseHelper(); // Инициализируем DatabaseHelper
            LoadTasks(); // Загружаем задачи
        }

        // Метод для загрузки задач
        private void LoadTasks()
        {
            List<Task> tasks = _dbHelper.GetTasks(_userId); // Получаем задачи для текущего пользователя
            TasksItemsControl.ItemsSource = tasks; // Привязываем задачи к элементу управления
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var task = (sender as Button)?.Tag as Task; // Получаем задачу из Tag кнопки
            if (task != null)
            {
                var editTaskWindow = new CreateTaskWindow(_userId, task); // Передаем userId и объект Task
                if (editTaskWindow.ShowDialog() == true)
                {
                    LoadTasks(); // Обновляем список задач после редактирования
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int taskId && taskId != 0) // Проверяем, что Tag является int
            {
                _dbHelper.DeleteTask(taskId); // Удаляем задачу из базы
                LoadTasks(); // Обновляем список задач
            }
        }
    }
}

