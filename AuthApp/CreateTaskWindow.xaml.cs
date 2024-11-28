using System;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace AuthApp
{
    public partial class CreateTaskWindow : Window
    {
        private DatabaseHelper _dbHelper;
        private int _adminId;
        private Task _task;

        // Конструктор для создания новой задачи
        public CreateTaskWindow(int adminId)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _adminId = adminId;

            // Очистить поля для новой задачи
            ClearFields();
        }

        // Конструктор для редактирования существующей задачи
        public CreateTaskWindow(int adminId, Task task) : this(adminId)
        {
            _task = task; // Сохраняем ссылку на задачу
            PopulateFields(task); // Заполняем поля данными задачи
        }

        private void ClearFields()
        {
            TitleTextBox.Clear();
            DescriptionTextBox.Clear();
            ExecutorTextBox.Clear();
            StatusComboBox.SelectedIndex = 0; // Установить статус по умолчанию
        }

        private void PopulateFields(Task task)
        {
            TitleTextBox.Text = task.Title;
            DescriptionTextBox.Text = task.Description;
            ExecutorTextBox.Text = task.Executor;
            StatusComboBox.SelectedItem = StatusComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == task.Status);
        }

        private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string description = DescriptionTextBox.Text;
            string executor = ExecutorTextBox.Text;
            string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "В процессе";

            if (_task == null) // Если задача новая
            {
                _task = new Task
                {
                    Title = title,
                    Description = description,
                    Executor = executor,
                    StartDate = DateTime.Now,
                    Status = status,
                    UserId = _adminId // Связываем задачу с администратором, который ее создал
                };

                _dbHelper.AddTask(_task); // Метод для добавления задачи в базу данных
                MessageBox.Show("Задача успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else // Если задача редактируется
            {
                _task.Title = title;
                _task.Description = description;
                _task.Executor = executor;
                _task.Status = status;

                // Если статус изменился на "Completed", устанавливаем EndDate
                DateTime? endDate = null;
                if (_task.Status == "Закрыта" && _task.EndDate == null)
                {
                    endDate = DateTime.Now; // Устанавливаем текущую дату и время
                }

                // Обновляем задачу в базе данных
                _dbHelper.UpdateTask(_task.Id, _task.Title, _task.Description, _task.Executor, _task.Status, endDate);
                MessageBox.Show("Задача успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.Close(); // Закрываем окно
        }
    }
}