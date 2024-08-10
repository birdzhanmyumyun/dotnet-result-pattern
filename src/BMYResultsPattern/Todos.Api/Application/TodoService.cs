using Todos.Api.Domain;
using Todos.Api.Infra;

namespace Todos.Api.Application;

public class TodoService
{
    private readonly TodosRepository _todosRepository;

    public TodoService(TodosRepository todosRepository)
    {
        _todosRepository = todosRepository;
    }

    public async Task<Todo> CreateTodoOrThrow(string title)
    {
       
        if(title.Length > 50)
        {
            throw new Exception("Title is too long");
        }

        Todo todo = new Todo
        {
            Title = title,
            CreatedAt = DateTime.Now
        };

        return await _todosRepository.AddTodo(todo);
    }

    public async Task<Todo> GetTodoOrThrow(Guid id)
    {
        Todo todo = await _todosRepository.GetTodoById(id);
        if(todo == null)
        {
            throw new Exception("Todo not found");
        };
        return todo;
    }

    public async Task<Result<Todo>> CreateTodo(string title)
    {
        if (title.Length > 50)
        {
            return Result<Todo>.Failure(TodoErrors.NameTooLong);
        }

        Todo todo = new Todo
        {
            Title = title,
            CreatedAt = DateTime.Now
        };

        var createdTodo = await _todosRepository.AddTodo(todo);

        return Result<Todo>.Success(createdTodo);
    }

    public async Task<Result<Todo>> GetTodo(Guid id)
    {
        Todo todo = await _todosRepository.GetTodoById(id);

        if(todo == null)
        {
            return Result<Todo>.Failure(TodoErrors.NotFound);
        };

        return Result<Todo>.Success(todo);
    }
}
