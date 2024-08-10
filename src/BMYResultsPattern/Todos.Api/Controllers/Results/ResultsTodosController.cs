using Microsoft.AspNetCore.Mvc;
using Todos.Api.Application;
using Todos.Api.Contracts;
using Todos.Api.Domain;

namespace Todos.Api.Controllers.Results;

[ApiController]
[Route("results/todos")]
public class ResultsTodosController : ControllerBase
{
    private readonly TodoService _todoService;
    private readonly ILogger<ResultsTodosController> _logger;

    public ResultsTodosController(TodoService todoService, ILogger<ResultsTodosController> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    [HttpGet("{id}", Name = "GetTodo")]
    public async Task<ActionResult<Todo>> GetTodoById(Guid id)
    {
        try
        {
            Result<Todo> result = await _todoService.GetTodo(id);

            if(result.IsFailure)
            {
                return Problem(result.Error);
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting a todo");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost(Name = "CreateTodo")]
    public async Task<ActionResult<Todo>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        try
        {
            Result<Todo> result = await _todoService.CreateTodo(request.Title);
            if (result.IsFailure)
            {
                return Problem(result.Error);
            }
            return CreatedAtRoute("GetTodoOrThrow", new { id = result.Value.Id }, result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a todo");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private ObjectResult Problem(Error error)
    {
        return Problem(statusCode: (int)error.Code, title: error.Description);
    }
}