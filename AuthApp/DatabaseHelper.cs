using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

public class DatabaseHelper
{
    private string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=docudb";

    public User GetUserById(int userId)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Id, Username, Password, IsAdmin FROM Users WHERE Id = @Id", connection);
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

    // Метод для обновления профиля пользователя
    public void UpdateUserProfile(int userId, string newUsername, string newPassword)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("UPDATE Users SET Username = @Username, Password = @Password WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Username", newUsername);
            command.Parameters.AddWithValue("@Password", newPassword);
            command.Parameters.AddWithValue("@Id", userId);
            command.ExecuteNonQuery();
        }
    }

    public void DeleteTask(int taskId)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("DELETE FROM tasks WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", taskId);
            command.ExecuteNonQuery();
        }
    }

    public void UpdateTask(int taskId, string title, string description, string executor, string status, DateTime? endDate)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            // Создаем строку запроса с добавленным полем EndDate
            var commandText = "UPDATE tasks SET Title = @Title, Description = @Description, Executor = @Executor, Status = @Status, EndDate = @EndDate WHERE Id = @Id";

            var command = new NpgsqlCommand(commandText, connection);

            // Добавляем параметры для всех полей, включая EndDate
            command.Parameters.AddWithValue("@Id", taskId);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@Executor", executor);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value); // Если EndDate null, передаем DBNull

            command.ExecuteNonQuery();
        }
    }

    public List<Task> GetTasks(int userId)
    {
        var tasks = new List<Task>();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand("SELECT * FROM Tasks WHERE UserId = @userId", connection))
            {
                command.Parameters.AddWithValue("userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Task
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            Executor = reader.IsDBNull(3) ? null : reader.GetString(3),
                            StartDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            EndDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            UserId = reader.GetInt32(6),
                            Status = reader.IsDBNull(7) ? "In Progress" : reader.GetString(7) // Получаем статус или устанавливаем значение по умолчанию
                        });
                    }
                }
            }
        }

        return tasks;
    }

    public void AddTask(Task task)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("INSERT INTO tasks (Title, Description, Executor, StartDate, Status, UserId) VALUES (@Title, @Description, @Executor, @StartDate, @Status, @UserId)", connection);
            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@Description", task.Description);
            command.Parameters.AddWithValue("@Executor", task.Executor);
            command.Parameters.AddWithValue("@StartDate", task.StartDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Status", task.Status);
            command.Parameters.AddWithValue("@UserId", task.UserId);
            command.ExecuteNonQuery();
        }
    }

    public List<Document> GetDocuments()
    {
        var documents = new List<Document>();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Id, DocumentName, UploadDate FROM Documents", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    documents.Add(new Document
                    {
                        Id = reader.GetInt32(0),
                        DocumentName = reader.GetString(1),
                        UploadDate = reader.GetDateTime(2)
                    });
                }
            }
        }

        return documents;
    }

    public byte[] GetDocumentContent(int documentId)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT Content FROM Documents WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", documentId);
            return command.ExecuteScalar() as byte[]; // Возвращаем содержимое документа
        }
    }

    public void DeleteDocument(int documentId)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("DELETE FROM Documents WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", documentId);
            command.ExecuteNonQuery();
        }
    }

    public void AddDocument(Document document)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("INSERT INTO Documents (DocumentName, Content, UploadedBy, UploadDate) VALUES (@DocumentName, @Content, @UploadedBy, @UploadDate)", connection);
            command.Parameters.AddWithValue("@DocumentName", document.DocumentName);
            command.Parameters.AddWithValue("@Content", document.Content ?? (object)DBNull.Value); // Добавляем содержимое документа
            command.Parameters.AddWithValue("@UploadedBy", document.UploadedBy);
            command.Parameters.AddWithValue("@UploadDate", document.UploadDate);
            command.ExecuteNonQuery();
        }
    }

    // Метод для получения статистики по документам
    public (int totalDocuments, int documentsLast30Days, int accessibleDocuments) GetDocumentStatistics(int userId)
    {
        int totalDocuments = 0;
        int documentsLast30Days = 0;
        int accessibleDocuments = 0;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            // Получаем общее количество документов
            var totalCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Documents", connection);
            totalDocuments = Convert.ToInt32(totalCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0

            // Получаем количество документов, загруженных за последние 30 дней
            var last30DaysCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Documents WHERE UploadDate > @Date", connection);
            last30DaysCommand.Parameters.AddWithValue("@Date", DateTime.Now.AddDays(-30));
            documentsLast30Days = Convert.ToInt32(last30DaysCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0

            // Получаем количество документов, доступных пользователю
            var accessibleCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Documents WHERE UploadedBy = @UserId", connection);
            accessibleCommand.Parameters.AddWithValue("@UserId", userId);
            accessibleDocuments = Convert.ToInt32(accessibleCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0
        }

        return (totalDocuments, documentsLast30Days, accessibleDocuments);
    }

    // Метод для получения статистики по задачам
    public (int currentTasks, int completedTasks, double avgCompletionTime) GetTaskStatistics(int userId)
    {
        int currentTasks = 0;
        int completedTasks = 0;
        double avgCompletionTime = 0;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            // Получаем количество текущих задач
            var currentTasksCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Tasks WHERE UserId = @UserId AND Status = 'В процессе'", connection);
            currentTasksCommand.Parameters.AddWithValue("@UserId", userId);
            currentTasks = Convert.ToInt32(currentTasksCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0
            Console.WriteLine($"Current Tasks: {currentTasks}");  // Логирование

            // Получаем количество завершенных задач
            var completedTasksCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Tasks WHERE UserId = @UserId AND Status = 'Закрыта'", connection);
            completedTasksCommand.Parameters.AddWithValue("@UserId", userId);
            completedTasks = Convert.ToInt32(completedTasksCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0
            Console.WriteLine($"Completed Tasks: {completedTasks}");  // Логирование

            // Получаем среднее время завершения задач
            var avgCompletionTimeCommand = new NpgsqlCommand("SELECT AVG(EXTRACT(EPOCH FROM (enddate - startdate)) / 86400) FROM Tasks WHERE UserId = @UserId AND Status = 'Закрыта' AND enddate IS NOT NULL AND startdate IS NOT NULL", connection);
            avgCompletionTimeCommand.Parameters.AddWithValue("@UserId", userId);
            var result = avgCompletionTimeCommand.ExecuteScalar();
            avgCompletionTime = result != DBNull.Value ? Convert.ToDouble(result) : 0; // Если результат DBNull, присваиваем 0
            Console.WriteLine($"Average Completion Time: {avgCompletionTime}");  // Логирование
        }

        return (currentTasks, completedTasks, avgCompletionTime);
    }

    // Метод для получения общего состояния (количество активных пользователей, количество загрузок документов за последний месяц)
    public (int activeUsers, int documentUploadsLastMonth) GetGeneralStatistics()
    {
        int activeUsers = 0;
        int documentUploadsLastMonth = 0;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            // Получаем количество активных пользователей
            var activeUsersCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Users WHERE IsAdmin = FALSE", connection);
            activeUsers = Convert.ToInt32(activeUsersCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0

            // Получаем количество загрузок документов за последний месяц
            var documentUploadsCommand = new NpgsqlCommand("SELECT COUNT(*) FROM Documents WHERE UploadDate > @Date", connection);
            documentUploadsCommand.Parameters.AddWithValue("@Date", DateTime.Now.AddMonths(-1));
            documentUploadsLastMonth = Convert.ToInt32(documentUploadsCommand.ExecuteScalar() ?? 0); // Если результат DBNull, присваиваем 0
        }

        return (activeUsers, documentUploadsLastMonth);
    }
}


