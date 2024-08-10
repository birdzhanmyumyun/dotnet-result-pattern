using Todos.Api.Domain;

namespace Todos.Api.Infra
{
    public class TodosRepository
    {
        private readonly TodosDbContext _context;

        public TodosRepository(TodosDbContext context)
        {
            _context = context;
        }

        public async Task<Todo> GetTodoById(Guid id)
        {
            return await _context.Todos.FindAsync(id);
        }

        public async Task<Todo> AddTodo(Todo todo)
        {
            await _context.Todos.AddAsync(todo);
            await _context.SaveChangesAsync();
            return todo;
        }
    }
}