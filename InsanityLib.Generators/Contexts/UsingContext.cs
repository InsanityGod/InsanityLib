using InsanityLib.Generators.Interfaces;
using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Contexts;

internal readonly struct UsingContext : IDisposable
{
    private readonly IndentedTextWriter _writer;

    private readonly IWriteableContainer _writeableContainer;

    public UsingContext(IndentedTextWriter writer, IWriteableContainer writeableContainer)
    {
        _writer = writer;
        _writeableContainer = writeableContainer;
        _writeableContainer.WriteStart(writer);
    }

    public void Dispose() => _writeableContainer.WriteEnd(_writer);
}
