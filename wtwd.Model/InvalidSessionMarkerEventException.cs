namespace NoP77svk.wtwd.Model;

public class InvalidSessionMarkerEventException
    : Exception
{
    public InvalidSessionMarkerEventException()
    {
    }

    public InvalidSessionMarkerEventException(string? message)
        : base(message)
    {
    }

    public InvalidSessionMarkerEventException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
