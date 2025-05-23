# .NET API Error Handling: Exceptions vs. Result Pattern

This repository demonstrates and compares two common approaches to error handling in .NET Core Web APIs:

1.  **Traditional Exception-Based Handling**: Relies on throwing and catching exceptions.
2.  **Result-Based Pattern**: Utilizes a custom `Result<T>` type to explicitly return success or a structured error, often leading to more robust and maintainable code, and facilitating richer error responses like Problem Details.

The goal is to showcase both patterns within a simple `Todos.Api` example project, allowing developers to explore and understand their respective implementations and implications.

## Traditional Exception-Based Handling

This approach is a common way to deal with errors in many .NET applications.

*   **Controller Logic**: In `src/BMYResultsPattern/Todos.Api/Controllers/Exceptions/ExceptionsTodosController.cs`, API endpoints directly call service methods (e.g., `_todoService.GetTodoOrThrow(id)`).
*   **Service Logic**: Methods like `GetTodoOrThrow(Guid id)` and `CreateTodoOrThrow(string title)` in `src/BMYResultsPattern/Todos.Api/Application/TodoService.cs` will throw exceptions when an operation cannot be completed successfully (e.g., `TodoNotFoundException` if a todo isn't found, or a generic `Exception` for validation errors like title length).
*   **Error Handling**: The controller uses `try-catch` blocks to catch these exceptions and typically returns an appropriate HTTP status code (e.g., 500 Internal Server Error, or a more specific one if the exception type is checked).

```csharp
// Example from ExceptionsTodosController.cs
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
        return StatusCode(500, new { message = ex.Message }); // Or Problem() for Problem Details
    }
}
```

While straightforward, this can sometimes lead to:
*   Less explicit error flow in the code.
*   A need for centralized exception handling middleware to avoid repetitive `try-catch` blocks and to standardize error responses.
*   Potential for exceptions to be missed or handled inconsistently.

## Result-Based Pattern (with Problem Details)

This pattern offers a more explicit way of handling operations that can succeed or fail. Instead of throwing exceptions for expected error conditions, methods return a special `Result<T>` object.

*   **Core Components**:
    *   `Result<T>` (in `src/BMYResultsPattern/Todos.Api/Application/Result.cs`): A generic wrapper that can hold either a successful value (`Value` of type `T`) or an `Error` object. It has properties like `IsSuccess` and `IsFailure` to check the outcome.
    *   `Error` (in `src/BMYResultsPattern/Todos.Api/Application/Error.cs`): A record type representing a specific error, containing an `HttpStatusCode` and a `Description`.
    *   `TodoErrors` (in `src/BMYResultsPattern/Todos.Api/Application/TodoErrors.cs`): A static class that defines specific `Error` instances (e.g., `TodoErrors.NotFound`, `TodoErrors.NameTooLong`).

*   **Service Logic**: Methods like `GetTodo(Guid id)` and `CreateTodo(string title)` in `src/BMYResultsPattern/Todos.Api/Application/TodoService.cs` now return `Result<Todo>`. If an operation fails (e.g., todo not found, validation error), they return `Result<Todo>.Failure(TodoErrors.SpecificError)`. Otherwise, they return `Result<Todo>.Success(actualTodoObject)`.

    ```csharp
    // Example from TodoService.cs
    public async Task<Result<Todo>> GetTodo(Guid id)
    {
        Todo todo = await _todosRepository.GetTodoById(id);

        if(todo == null)
        {
            return Result<Todo>.Failure(TodoErrors.NotFound); // Explicit failure result
        };

        return Result<Todo>.Success(todo); // Explicit success result
    }
    ```

*   **Controller Logic**: In `src/BMYResultsPattern/Todos.Api/Controllers/Results/ResultsTodosController.cs`, the controller checks the `IsFailure` property of the returned `Result<T>`.
    *   If `IsFailure` is true, it uses the `Error` object from the result to generate a Problem Details response (using its own `Problem(Error error)` helper method, which translates the `Error` record into an `ObjectResult`).
    *   If `IsSuccess` is true, it returns the `Value` from the result with an Ok (200) or Created (201) status.

    ```csharp
    // Example from ResultsTodosController.cs
    [HttpGet("{id}", Name = "GetTodo")]
    public async Task<ActionResult<Todo>> GetTodoById(Guid id)
    {
        try
        {
            Result<Todo> result = await _todoService.GetTodo(id);

            if(result.IsFailure)
            {
                return Problem(result.Error); // Uses the Error object for Problem Details
            }

            return Ok(result.Value);
        }
        // ... (outer try-catch for truly unexpected exceptions)
    }
    ```

**Benefits**:
*   **Explicit Outcomes**: Method signatures (`Result<T>`) clearly indicate that an operation can result in a defined set of errors.
*   **Rich Error Information**: The `Error` object carries structured information (status code, description) that maps well to Problem Details (RFC 7807).
*   **Improved Code Flow**: Reduces reliance on `try-catch` for business logic errors, making the control flow clearer. Failures are handled as regular return values.
*   **Type Safety**: Prevents forgetting to handle a potential failure, as the compiler enforces dealing with the `Result<T>` type.

## Project Structure

The demonstration is contained within the `Todos.Api` project located in `src/BMYResultsPattern/Todos.Api/`.

Key components include:

*   **`Controllers/`**:
    *   `Exceptions/ExceptionsTodosController.cs`: Implements API endpoints for Todo items using the traditional exception handling model. Endpoints are typically prefixed with `/exceptions/todos`.
    *   `Results/ResultsTodosController.cs`: Implements API endpoints for Todo items using the Result-based pattern. Endpoints are typically prefixed with `/results/todos`.
*   **`Application/`**:
    *   `TodoService.cs`: Contains the business logic for managing Todo items. It provides two sets of methods for each operation:
        *   One set that throws exceptions (e.g., `GetTodoOrThrow`, `CreateTodoOrThrow`).
        *   Another set that returns `Result<T>` objects (e.g., `GetTodo`, `CreateTodo`).
    *   `Result.cs`: Defines the generic `Result<T>` class.
    *   `Error.cs`: Defines the `Error` record used by `Result<T>` for failures.
    *   `TodoErrors.cs`: Defines specific `Error` instances for common Todo-related failures.
*   **`Domain/`**:
    *   `Todo.cs`: Represents the Todo entity.
*   **`Infra/`**:
    *   `TodosDbContext.cs`: The Entity Framework Core database context.
    *   `TodosRepository.cs`: Handles data access for Todo items.

This structure allows for a side-by-side comparison of the two error handling strategies by interacting with the different controller endpoints.

## k6 Load Testing Scripts

The repository includes several JavaScript files (`*.js`) in the `src/BMYResultsPattern/` directory (e.g., `exceptions-create-todo.js`, `results-get-todo.js`). These are load testing scripts written for [k6](https://k6.io/), an open-source load testing tool.

**Purpose**:

*   **Demonstrate Client Interaction**: They show how a client application might interact with the API endpoints for both the exception-based and result-based approaches.
*   **Verify Error Responses**: The scripts are configured to test specific scenarios, particularly error conditions, and assert the expected HTTP status codes and response bodies. For example:
    *   `exceptions-create-todo.js`: Attempts to create a todo with a title that is too long via the `/exceptions/todos` endpoint, expecting a 500-level error.
    *   `results-create-todo.js`: Attempts the same via the `/results/todos` endpoint, expecting a 400 Bad Request with Problem Details.
    *   `exceptions-get-todo.js`: Attempts to fetch a non-existent todo from `/exceptions/todos`, expecting a 500-level error.
    *   `results-get-todo.js`: Attempts the same from `/results/todos`, expecting a 404 Not Found with Problem Details.
*   **Illustrate Differences**: By comparing the scripts and their expected outcomes, you can see how clients need to handle errors differently depending on the API's error handling strategy. The result-based pattern generally provides more specific status codes and detailed error information directly to the client.

These scripts can be run using k6 to simulate traffic to the API and verify its behavior under different conditions.

## How to Run

### 1. Running the Todos.Api

The API is a standard .NET Core application.

1.  **Navigate to the API project directory**:
    ```bash
    cd src/BMYResultsPattern/Todos.Api
    ```
2.  **Ensure .NET SDK is installed**: You'll need the .NET SDK (version compatible with the project, likely .NET 6 or newer). You can find it [here](https://dotnet.microsoft.com/download).
3.  **Run the application**:
    ```bash
    dotnet run
    ```
    The API should now be running, typically on `http://localhost:5161` (or `https://localhost:7204` if HTTPS is configured and preferred). Check the console output for the exact URLs.

    The API uses an in-memory database by default, so no external database setup is required for basic operation. Migrations are included if you wish to configure it with a persistent database like SQL Server.

### 2. Running the k6 Load Tests

1.  **Install k6**: If you don't have it already, download and install k6 from [k6.io](https://k6.io/docs/getting-started/installation/).
2.  **Navigate to the scripts directory**:
    ```bash
    cd src/BMYResultsPattern
    ```
3.  **Run a specific script**:
    For example, to run the script that tests creating a todo with an overly long title using the "results" endpoint:
    ```bash
    k6 run results-create-todo.js
    ```
    Or for the "exceptions" endpoint:
    ```bash
    k6 run exceptions-create-todo.js
    ```

    Review the output from k6 to see the results of the checks (e.g., if the expected HTTP status codes were received).

## Comparison Summary

This repository illustrates two distinct error handling philosophies in .NET Web APIs:

*   **Traditional Exceptions**:
    *   **Pros**: Familiar to many developers, can be effective with global exception handlers.
    *   **Cons**: Can make error flow less explicit, may lead to over-reliance on `try-catch`, and exceptions for non-exceptional (but failing) business logic can be debated. Standardizing detailed error responses (like Problem Details) often requires additional middleware.

*   **Result-Based Pattern**:
    *   **Pros**: Makes success/failure outcomes very explicit in method signatures. Encourages deliberate handling of potential failures. Integrates well with returning detailed Problem Details responses. Improves predictability and makes it easier to reason about code paths.
    *   **Cons**: Can be more verbose. Requires a clear definition and consistent use of `Result<T>` and `Error` types. Might be unfamiliar to developers new to this pattern.

The choice between them depends on team preference, project complexity, and the desired level of explicitness in error handling. The Result pattern often shines in scenarios where clear, predictable error responses and robust handling of various failure states are critical.

By exploring the `Todos.Api` controllers and service methods, and by running the k6 tests against both sets of endpoints, developers can gain a practical understanding of these two approaches.
