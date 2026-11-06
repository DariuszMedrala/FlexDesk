namespace FlexDesk.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException For(string entity, object key) =>
        new($"{entity} with id '{key}' was not found.");
}
