namespace FlexDesk.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException For(string entity, object key) =>
        new($"Nie znaleziono żądanego zasobu ({entity}: {key}).");
}
