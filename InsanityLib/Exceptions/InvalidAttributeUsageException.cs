using System;

namespace InsanityLib.Exceptions;

public class InvalidAttributeUsageException : Exception
{
    public InvalidAttributeUsageException()
    {
    }

    public InvalidAttributeUsageException(string message) : base(message)
    {
    }

    public InvalidAttributeUsageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
