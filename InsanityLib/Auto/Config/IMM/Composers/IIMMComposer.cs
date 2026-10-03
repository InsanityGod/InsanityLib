using IntegratedModManager.Config;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace InsanityLib.Auto.Config.IMM.Composers;

public interface IIMMComposer
{
    public bool CanWriteType(IMMComposerContext context, MemberInfo member, JsonProperty? property, JsonContract contract, out bool requiresAdvanced);

    void Write(IMMComposerContext context, MemberInfo member, JsonProperty? property, JsonContract contract, ImmAdvancedSchema? advanced);
}
