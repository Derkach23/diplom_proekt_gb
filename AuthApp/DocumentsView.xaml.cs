using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace AuthApp
{
    public partial class DocumentsView : UserControl
    {
        private DatabaseHelper _dbHelper;
        private int _userId; // Поле для хранения ID пользователя
        public ObservableCollection<Document> Documents { get; set; }

        public DocumentsView(int userId)
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _userId = userId; // Сохраняем ID пользователя
            Documents = new ObservableCollection<Document>();
            DocumentsListView.ItemsSource = Documents;
            LoadDocuments(); // Загружаем документы из базы
        }

        private void LoadDocuments()
        {
            var dbHelper = new DatabaseHelper();
            var documents = dbHelper.GetDocuments(); // Получаем список документов
            Documents.Clear();
            foreach (var document in documents)
            {
                Documents.Add(document); // Добавляем документы в коллекцию
            }
        }

        private void AddDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var addDocumentWindow = new AddDocumentWindow(_userId); // Передаем ID пользователя
            if (addDocumentWindow.ShowDialog() == true)
            {
                LoadDocuments(); // Перезагрузка документов
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int documentId = (int)button.Tag;

            // Получаем содержимое документа
            byte[] content = _dbHelper.GetDocumentContent(documentId);

            if (content != null)
            {
                // Создаем диалог выбора файла для сохранения
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "document", // Имя файла по умолчанию
                    DefaultExt = ".pdf", // Расширение файла по умолчанию
                    Filter = "Documents (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt" // Разрешенные форматы файлов
                };

                // Показываем диалог и проверяем, было ли нажато "OK"
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    File.WriteAllBytes(filePath, content); // Сохраняем содержимое документа в выбранный файл
                    MessageBox.Show("Документ успешно загружен по адресу: " + filePath);
                }
            }
            else
            {
                MessageBox.Show("Не удалось скачать документ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int documentId = (int)button.Tag;

            // Подтверждение удаления
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот документ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _dbHelper.DeleteDocument(documentId);
                MessageBox.Show("Документ удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDocuments(); // Перезагрузите список документов
            }
        }
    }
}
