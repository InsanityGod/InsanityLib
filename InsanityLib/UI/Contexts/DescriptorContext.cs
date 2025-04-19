using InsanityLib.Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace InsanityLib.UI.Contexts
{
    public class DescriptorContext : IDialogContextRedirect
    {
        public IDialogContext Context { get; }

        public MemberInfo Member { get; }

        public DescriptorContext(IDialogContext context, MemberInfo member)
        {
            Context = context;
            Member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public string Path => $"{Context.Path}/@Descriptor";

        public string ExtendPath(MemberInfo member, Type type) => member == null ? $"{Path}/{Member.Name}" : throw new InvalidOperationException("Descriptor should not have memberInfo passed");

        public static string GetDescriptorPath(MemberContext memberContext, MemberInfo member) => $"{memberContext.Path}/@Descriptor/{member.Name}";
    }
}
