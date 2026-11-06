namespace InsanityLib.Interfaces;

public interface IAttributeProvider<out T> : IAttributeProvider
{
    public new T Attributes { get; }

    object? IAttributeProvider.Attributes => Attributes;
}

public interface IAttributeProvider
{
    public object? Attributes { get; }
}
