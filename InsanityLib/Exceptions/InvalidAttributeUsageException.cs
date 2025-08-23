using System;
using System.Runtime.Serialization;

namespace InsanityLib.Exceptions;

[Serializable]
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

    protected InvalidAttributeUsageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
