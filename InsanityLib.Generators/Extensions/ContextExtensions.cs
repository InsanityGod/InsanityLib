using InsanityLib.Generators.Contexts;
using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Extensions;

public static class ContextExtensions
{
    public static IDisposable Use(this IWriteableContainer container, IndentedTextWriter writer) => new UsingContext(writer, container);

    public static TryContext Catch(this TryContext tryContext, params CatchContext[] catchContexts)
    {
        if(tryContext.CatchContexts is not null)
        {
            tryContext.CatchContexts = [..tryContext.CatchContexts, ..catchContexts];
        }
        else tryContext.CatchContexts = catchContexts;

        return tryContext;
    }
}
