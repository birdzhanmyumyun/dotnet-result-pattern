using Microsoft.AspNetCore.Mvc;
using Todos.Api.Domain;
using Todos.Api.Contracts;
using Todos.Api.Application;

namespace Todos.Api.Controllers.Exceptions
{
    [ApiController]
    [Route("exceptions/todos")]
    public class ExceptionsTodosController : ControllerBase
    {
        private readonly TodoService _todoService;
        private readonly ILogger<ExceptionsTodosController> _logger;

        public ExceptionsTodosController(TodoService todoService, ILogger<ExceptionsTodosController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetTodoOrThrow")]
        public async Task<ActionResult<Todo>> GetTodoById(Guid id)
        {
            try
            {
                var todo = await _todoService.GetTodoOrThrow(id);
                return Ok(todo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting a todo");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost(Name = "CreateTodoOrThrow")]
        public async Task<ActionResult<Todo>> CreateTodo([FromBody] CreateTodoRequest request)
        {
            try
            {
                Todo createdTodo = await _todoService.CreateTodoOrThrow(request.Title);
                return CreatedAtRoute("GetTodoOrThrow", new { id = createdTodo.Id }, createdTodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a todo");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}