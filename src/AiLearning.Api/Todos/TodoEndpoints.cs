namespace AiLearning.Api.Todos;

public record CreateTodoRequest(string? Title);

public record UpdateTodoRequest(string? Title, bool IsCompleted);

public static class TodoEndpoints
{
    private const int MaxTitleLength = 200;

    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos");

        group.MapGet("/", (TodoStore store) => store.GetAll());

        group.MapPost("/", (CreateTodoRequest request, TodoStore store) =>
        {
            if (ValidateTitle(request.Title) is { } error) return error;
            var item = store.Add(request.Title!.Trim());
            return Results.Created($"/api/todos/{item.Id}", item);
        });

        group.MapPut("/{id:int}", (int id, UpdateTodoRequest request, TodoStore store) =>
        {
            if (ValidateTitle(request.Title) is { } error) return error;
            var item = store.Update(id, request.Title!.Trim(), request.IsCompleted);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapDelete("/{id:int}", (int id, TodoStore store) =>
            store.Delete(id) ? Results.NoContent() : Results.NotFound());
    }

    private static IResult? ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["title"] = ["Başlık boş olamaz."]
            });

        if (title.Trim().Length > MaxTitleLength)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["title"] = [$"Başlık en fazla {MaxTitleLength} karakter olabilir."]
            });

        return null;
    }
}
