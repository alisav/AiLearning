using AiLearning.Api.Todos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Relative SQLite paths resolve against the project directory, not the shell's working directory.
var connection = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("Todos"));
connection.DataSource = Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);
builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(connection.ToString()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<TodoDbContext>().Database.EnsureCreated();
}

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapTodoEndpoints();

app.Run();
