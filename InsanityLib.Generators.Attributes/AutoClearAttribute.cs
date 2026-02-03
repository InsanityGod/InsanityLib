using System.Diagnostics;

namespace InsanityLib.Generators.Attributes;

/// <summary>
/// Automatically calls the "Clear" method of a static field or property when AutoDispose is called.
/// </summary>
[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class AutoClearAttribute : Attribute;