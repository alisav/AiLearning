namespace AiLearning.Api.Todos;

public class TodoItem
{
    public const int MaxTitleLength = 200;

    public int Id { get; set; }
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
}
