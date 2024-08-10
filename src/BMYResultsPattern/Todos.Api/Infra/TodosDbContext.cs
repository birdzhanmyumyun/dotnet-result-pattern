using Microsoft.EntityFrameworkCore;
using Todos.Api.Domain;

namespace Todos.Api.Infra
{
    public class TodosDbContext : DbContext
    {
        public TodosDbContext(DbContextOptions<TodosDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}