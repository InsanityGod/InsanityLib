using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Composers;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools
{
    public static class ImGuiComposer
    {
        public static ICollection<IImGuiComposer> Composers { get; } = new List<IImGuiComposer>()
        {
            new EnumerableComposer(),
            new ValueComposer(),
            new ClassComposer(),
        };

        public static IImGuiComponent TryCompose(ImGuiContext context, Type type = null)
        {
            type ??= context.Member is MethodInfo ? typeof(MethodInfo) : context.Member.GetPrimaryType();

            return Composers.FirstOrDefault(composer => composer.CanComposeType(type))?.Compose(context, type);
        }
    }
}
