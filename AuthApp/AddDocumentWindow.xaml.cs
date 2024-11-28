using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace AuthApp
{
    public partial class AddDocumentWindow : Window
    {
        private int _uploadedByUserId; // Поле для хранения ID пользователя
        private byte[] _fileContent; // Поле для хранения содержимого файла

        public AddDocumentWindow(int userId)
        {
            InitializeComponent();
            _uploadedByUserId = userId; // Сохраняем ID пользователя
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _fileContent = File.ReadAllBytes(openFileDialog.FileName); // Читаем содержимое файла
                SelectedFileTextBlock.Text = openFileDialog.FileName; // Отображаем имя файла
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DocumentNameTextBox.Text))
            {
                MessageBox.Show("Введите название документа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var document = new Document
            {
                DocumentName = DocumentNameTextBox.Text,
                UploadedBy = _uploadedByUserId, // Используем ID пользователя
                UploadDate = DateTime.Now, // Добавляем дату загрузки
                Content = _fileContent // Используем содержимое файла
            };

            var dbHelper = new DatabaseHelper();
            dbHelper.AddDocument(document); // Вызов метода для добавления документа

            DialogResult = true; // Закрываем окно с результатом
            Close();
        }
    }
}