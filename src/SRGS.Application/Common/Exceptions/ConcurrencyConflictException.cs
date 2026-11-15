namespace SRGS.Application.Common.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The request was modified by another operation. Reload it and try again.")
    {
    }

    public ConcurrencyConflictException(Exception innerException)
        : base("The request was modified by another operation. Reload it and try again.", innerException)
    {
    }
}
