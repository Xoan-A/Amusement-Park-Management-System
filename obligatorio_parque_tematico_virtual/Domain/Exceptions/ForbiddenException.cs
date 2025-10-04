using System;

namespace Domain.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Forbidden access")
    {
    }

    public ForbiddenException(string message) : base(message)
    {
    }
}
