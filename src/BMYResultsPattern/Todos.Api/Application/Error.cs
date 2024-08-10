using System.Net;

namespace Todos.Api.Application;

public sealed record Error(HttpStatusCode Code, string Description)
{
    public static readonly Error None = new(0, string.Empty);
}