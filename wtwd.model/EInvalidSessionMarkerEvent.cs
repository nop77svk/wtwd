namespace wtwd.Model;

public class EInvalidSessionMarkerEvent : Exception
{
    public EInvalidSessionMarkerEvent()
    {
    }

    public EInvalidSessionMarkerEvent(string? message) : base(message)
    {
    }

    public EInvalidSessionMarkerEvent(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
