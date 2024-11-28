using System;
using System.Windows;
using System.Windows.Controls;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Xceed.Words.NET;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace AuthApp
{
    public partial class ReportWindow : UserControl
    {
        private DatabaseHelper _databaseHelper;

        public ReportWindow()
        {
            InitializeComponent();
            _databaseHelper = new DatabaseHelper();
            LoadStatistics(1);
        }

        private void LoadStatistics(int userId)
        {
            // Получаем статистику по документам
            var documentStats = _databaseHelper.GetDocumentStatistics(userId);
            TotalDocumentsText.Text = $"Общее количество документов: {documentStats.totalDocuments}";
            RecentDocumentsText.Text = $"Загружено за последние 30 дней: {documentStats.documentsLast30Days}";
            AccessibleDocumentsText.Text = $"Доступные документы: {documentStats.accessibleDocuments}";

            // Получаем статистику по задачам
            var taskStats = _databaseHelper.GetTaskStatistics(userId);
            CurrentTasksText.Text = $"Текущие задачи: {taskStats.currentTasks}";
            CompletedTasksText.Text = $"Завершенные задачи: {taskStats.completedTasks}";
            AverageCompletionTimeText.Text = $"Среднее время выполнения задач: {taskStats.avgCompletionTime:F0} дней";

            // Получаем общую статистику
            var generalStats = _databaseHelper.GetGeneralStatistics();
            ActiveUsersText.Text = $"Активные пользователи: {generalStats.activeUsers}";
            DocumentUpdatesText.Text = $"Загрузки или обновления за последний месяц: {generalStats.documentUploadsLastMonth}";
        }
        private string GetReportContent()
        {
            // Формируем текст отчета из статистики
            var documentStats = _databaseHelper.GetDocumentStatistics(1);
            var taskStats = _databaseHelper.GetTaskStatistics(1);
            var generalStats = _databaseHelper.GetGeneralStatistics();

            return $"Общее количество документов: {documentStats.totalDocuments}\n" +
                   $"Загружено за последние 30 дней: {documentStats.documentsLast30Days}\n" +
                   $"Доступные документы: {documentStats.accessibleDocuments}\n" +
                   $"Текущие задачи: {taskStats.currentTasks}\n" +
                   $"Завершенные задачи: {taskStats.completedTasks}\n" +
                   $"Среднее время выполнения задач: {taskStats.avgCompletionTime:F0} дней\n" +
                   $"Активные пользователи: {generalStats.activeUsers}\n" +
                   $"Загрузки за последний месяц: {generalStats.documentUploadsLastMonth}";
        }

        private string GetSaveFilePath(string fileType)
        {
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = $"Сохранить отчет как {fileType}";
            saveFileDialog.Filter = $"{fileType} Files|*.{fileType.ToLower()}"; 

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName; 
            }

            return null; 
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            string content = GetReportContent();
            string filePath = GetSaveFilePath("PDF");

            if (!string.IsNullOrEmpty(filePath))
            {
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var font = new XFont("Verdana", 12);

                    // Разделяем контент на строки и рисуем каждую строку
                    var lines = content.Split(new[] { '\n' }, StringSplitOptions.None);
                    double yPos = 0;
                    foreach (var line in lines)
                    {
                        gfx.DrawString(line, font, XBrushes.Black, new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopLeft);
                        yPos += font.GetHeight(); // Инкрементируем Y-координату для следующей строки
                    }

                    document.Save(filePath);
                }
                MessageBox.Show($"Отчет успешно сохранен как PDF: {filePath}");
            }
        }

        private void ExportToDocx_Click(object sender, RoutedEventArgs e)
        {
            string content = GetReportContent();
            string filePath = GetSaveFilePath("DOCX");

            if (!string.IsNullOrEmpty(filePath))
            {
                using (var doc = DocX.Create(filePath))
                {
                    // Разделяем контент на строки и вставляем параграфы в документ
                    var lines = content.Split(new[] { '\n' }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        doc.InsertParagraph(line);
                    }
                    doc.Save();
                }
                MessageBox.Show($"Отчет успешно сохранен как DOCX: {filePath}");
            }
        }

        private void ExportToXlsx_Click(object sender, RoutedEventArgs e)
        {
            string content = GetReportContent();
            string filePath = GetSaveFilePath("XLSX");

            if (!string.IsNullOrEmpty(filePath))
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчет");

                    // Разделяем контент на строки и вставляем каждую строку в новую ячейку
                    var lines = content.Split(new[] { '\n' }, StringSplitOptions.None);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        worksheet.Cell(i + 1, 1).Value = lines[i]; // Вставляем каждую строку в ячейку
                    }

                    workbook.SaveAs(filePath);
                }
                MessageBox.Show($"Отчет успешно сохранен как XLSX: {filePath}");
            }
        }
    }
}
