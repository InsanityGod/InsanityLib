using System.Diagnostics;

namespace InsanityLib.Generators.Attributes;


/// <param name="channelName">The name of the channel to register this as a handler on, will default to ModID if null</param>
[Conditional("CompileTimeOnly")]
[AttributeUsage(AttributeTargets.Method)]
public class AutoNetworkMessageAttribute(string channelName = null) : Attribute
{
    public string ChannelName { get; } = channelName;
}
