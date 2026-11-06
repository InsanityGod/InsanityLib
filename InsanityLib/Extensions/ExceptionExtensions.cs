using System;
using System.Collections.Generic;
using System.Text;

namespace InsanityLib.Extensions;

public static class ExceptionExtensions
{
    public static bool IsMissingMemberException(this Exception ex) => ex.Message.StartsWith("Could not find member");
}
