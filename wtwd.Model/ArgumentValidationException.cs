namespace NoP77svk.wtwd.Model;

using System;

public class ArgumentValidationException<T> : Exception
{
    public T Expected { get; }
    public T Actual { get; }

    public ArgumentValidationException(T expected, T actual)
        : this(expected, actual, (string?)null)
    {
    }

    public ArgumentValidationException(T expected, T actual, string? message)
        : base(GetErrorMessage(expected, actual, message))
    {
        Expected = expected;
        Actual = actual;
    }

    public ArgumentValidationException(T expected, T actual, Exception inner)
        : this(expected, actual, null, inner)
    {
    }

    public ArgumentValidationException(T expected, T actual, string? message, Exception inner)
        : base(GetErrorMessage(expected, actual, message), inner)
    {
        Expected = expected;
        Actual = actual;
    }

    private static string GetErrorMessage(T expected, T actual, string? optionalMessage = null)
        => $"Argument validation failed! Expected: {expected}, Actual: {actual}"
        + (string.IsNullOrWhiteSpace(optionalMessage) ? string.Empty : $" | {optionalMessage}");
}
