using System.CodeDom.Compiler;

namespace InsanityLib.Generators.Interfaces;

public delegate void WriteDelgate(IndentedTextWriter writer);

public interface IWriteableContainer
{
    public void WriteStart(IndentedTextWriter writer);

    public void WriteEnd(IndentedTextWriter writer);
}

public interface IWriteable
{
    public void Write(IndentedTextWriter writer);
}
