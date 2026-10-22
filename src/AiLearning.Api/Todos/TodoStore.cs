namespace AiLearning.Api.Todos;

public record TodoItem(int Id, string Title, bool IsCompleted);

public class TodoStore
{
    private readonly Lock _lock = new();
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public IReadOnlyList<TodoItem> GetAll()
    {
        lock (_lock) return _items.ToList();
    }

    public TodoItem Add(string title)
    {
        lock (_lock)
        {
            var item = new TodoItem(_nextId++, title, false);
            _items.Add(item);
            return item;
        }
    }

    public TodoItem? Update(int id, string title, bool isCompleted)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(t => t.Id == id);
            if (index < 0) return null;
            _items[index] = _items[index] with { Title = title, IsCompleted = isCompleted };
            return _items[index];
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _items.RemoveAll(t => t.Id == id) > 0;
    }
}
