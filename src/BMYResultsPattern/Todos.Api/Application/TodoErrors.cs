using System.Net;

namespace Todos.Api.Application;

public static class TodoErrors
{
    public static readonly Error NotFound = new(HttpStatusCode.NotFound, "The specified Todo does not exist.");
    public static readonly Error NameTooLong = new(HttpStatusCode.BadRequest, "The name is too long. Please try again with a no longer than 50 characters.");
}
