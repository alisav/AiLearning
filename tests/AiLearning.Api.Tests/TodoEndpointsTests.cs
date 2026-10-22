using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AiLearning.Api.Tests;

public sealed class TodoEndpointsTests : IDisposable
{
    private record TodoDto(int Id, string Title, bool IsCompleted);

    // xUnit creates a new instance per test, so every test gets its own database file.
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"ailearning-tests-{Guid.NewGuid():N}.db");

    private TodoApiFactory _factory;
    private HttpClient _client;

    public TodoEndpointsTests()
    {
        _factory = new TodoApiFactory(_databasePath);
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-shm", "-wal" })
            File.Delete(_databasePath + suffix);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("""{"status":"ok"}""", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Startup_CreatesIsolatedTestDatabase()
    {
        await _client.GetAsync("/api/todos");

        Assert.True(File.Exists(_databasePath));
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var todos = await _client.GetFromJsonAsync<List<TodoDto>>("/api/todos");

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Create_ValidTitle_Returns201WithTrimmedTitle()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = "  Süt al  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(todo);
        Assert.Equal("Süt al", todo.Title);
        Assert.False(todo.IsCompleted);
        Assert.Equal($"/api/todos/{todo.Id}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Create_MaxLengthTitle_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = new string('a', 200) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(201)]
    public async Task Create_InvalidTitle_Returns400(object? title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = ToTitle(title) });

        await AssertTitleValidationError(response);
        Assert.Empty(await GetAll());
    }

    [Fact]
    public async Task GetAll_ReturnsCreatedTodosInOrder()
    {
        await Create("Birinci");
        await Create("İkinci");

        var todos = await GetAll();

        Assert.Equal(["Birinci", "İkinci"], todos.Select(t => t.Title));
    }

    [Fact]
    public async Task Update_ExistingTodo_Returns200AndSavesChanges()
    {
        var created = await Create("Kod yaz");

        var response = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}", new { title = " Test yaz ", isCompleted = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expected = new TodoDto(created.Id, "Test yaz", true);
        Assert.Equal(expected, await response.Content.ReadFromJsonAsync<TodoDto>());
        Assert.Equal([expected], await GetAll());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData(201)]
    public async Task Update_InvalidTitle_Returns400AndKeepsTodo(object? title)
    {
        var created = await Create("Kod yaz");

        var response = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}", new { title = ToTitle(title), isCompleted = true });

        await AssertTitleValidationError(response);
        Assert.Equal([created], await GetAll());
    }

    [Fact]
    public async Task Update_MissingTodo_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/api/todos/999", new { title = "Yok", isCompleted = true });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingTodo_Returns204AndRemovesIt()
    {
        var keep = await Create("Kalacak");
        var remove = await Create("Silinecek");

        var response = await _client.DeleteAsync($"/api/todos/{remove.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal([keep], await GetAll());
    }

    [Fact]
    public async Task Delete_MissingTodo_Returns404()
    {
        var response = await _client.DeleteAsync("/api/todos/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Todos_PersistAcrossApplicationRestart()
    {
        var created = await Create("Kalıcı görev");
        await _client.PutAsJsonAsync($"/api/todos/{created.Id}", new { title = "Kalıcı görev", isCompleted = true });

        await RestartApplication();

        Assert.Equal([created with { IsCompleted = true }], await GetAll());
    }

    private async Task RestartApplication()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        SqliteConnection.ClearAllPools();

        _factory = new TodoApiFactory(_databasePath);
        _client = _factory.CreateClient();
    }

    private async Task<TodoDto> Create(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoDto>())!;
    }

    private async Task<List<TodoDto>> GetAll() =>
        (await _client.GetFromJsonAsync<List<TodoDto>>("/api/todos"))!;

    // Inline data can't hold a 201-character string, so an int means "a string of that length".
    private static string? ToTitle(object? title) =>
        title is int length ? new string('a', length) : (string?)title;

    private static async Task AssertTitleValidationError(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("title", out _));
    }
}
