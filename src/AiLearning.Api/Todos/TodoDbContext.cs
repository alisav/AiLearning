using Microsoft.EntityFrameworkCore;

namespace AiLearning.Api.Todos;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.Property(t => t.Title).HasMaxLength(TodoItem.MaxTitleLength).IsRequired();
        });
    }
}
