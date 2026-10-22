using Microsoft.EntityFrameworkCore;

namespace AiLearning.Api.Todos;

public record CreateTodoRequest(string? Title);

public record UpdateTodoRequest(string? Title, bool IsCompleted);

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos");

        group.MapGet("/", async (TodoDbContext db) =>
            await db.Todos.AsNoTracking().OrderBy(t => t.Id).ToListAsync());

        group.MapPost("/", async (CreateTodoRequest request, TodoDbContext db) =>
        {
            if (ValidateTitle(request.Title) is { } error) return error;
            var item = new TodoItem { Title = request.Title!.Trim() };
            db.Todos.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/todos/{item.Id}", item);
        });

        group.MapPut("/{id:int}", async (int id, UpdateTodoRequest request, TodoDbContext db) =>
        {
            if (ValidateTitle(request.Title) is { } error) return error;
            var item = await db.Todos.FindAsync(id);
            if (item is null) return Results.NotFound();
            item.Title = request.Title!.Trim();
            item.IsCompleted = request.IsCompleted;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        });

        group.MapDelete("/{id:int}", async (int id, TodoDbContext db) =>
            await db.Todos.Where(t => t.Id == id).ExecuteDeleteAsync() > 0
                ? Results.NoContent()
                : Results.NotFound());
    }

    private static IResult? ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["title"] = ["Başlık boş olamaz."]
            });

        if (title.Trim().Length > TodoItem.MaxTitleLength)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["title"] = [$"Başlık en fazla {TodoItem.MaxTitleLength} karakter olabilir."]
            });

        return null;
    }
}
