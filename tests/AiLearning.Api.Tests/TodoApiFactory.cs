using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AiLearning.Api.Tests;

/// <summary>
/// Hosts the API in-memory against the given SQLite file instead of the development database.
/// </summary>
public sealed class TodoApiFactory(string databasePath) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development makes Program.cs create the schema on startup.
        builder.UseEnvironment("Development");
        // UseSetting is applied before Program.cs reads the connection string.
        builder.UseSetting("ConnectionStrings:Todos", $"Data Source={databasePath}");
    }
}
