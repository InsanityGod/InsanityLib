using InsanityLib.UI.ImGuiTools.Composers;
using InsanityLib.UI.ImGuiTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsanityLib.UI.ImGuiTools;

public static class ImGuiComposer
{
    public static ICollection<IImGuiComposer> Composers { get; } = new List<IImGuiComposer>()
    {
        new SetComposer(),
        new DictAndListComposer(),
        new ValueComposer(),
        new ClassComposer(),
        new MethodComposer(),
    };

    public static IImGuiComponent TryCompose(ImGuiContext context, Type type = null)
    {
        type ??= context.ComposeType;

        return Composers.FirstOrDefault(composer => composer.CanComposeType(type))?.Compose(context, type);
    }
}
