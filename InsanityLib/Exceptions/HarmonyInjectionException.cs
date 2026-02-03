using System;

namespace InsanityLib.Exceptions;

public class HarmonyInjectionException : Exception
{
    public HarmonyInjectionException()
    {
    }

    public HarmonyInjectionException(string message) : base(message)
    {
    }

    public HarmonyInjectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
