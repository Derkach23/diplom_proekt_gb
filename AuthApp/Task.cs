public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Executor { get; set; } // Имя исполнителя
    public int ExecutorId { get; set; } // Идентификатор исполнителя, если нужно
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int UserId { get; set; } // Идентификатор пользователя, создавшего задачу
    public string Status { get; set; } // Статус задачи
}
