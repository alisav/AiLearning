using AiLearning.Api.Todos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoStore>();

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapTodoEndpoints();

app.Run();
