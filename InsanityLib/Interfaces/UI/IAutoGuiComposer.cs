using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace InsanityLib.Interfaces.UI
{
    public interface IAutoGuiComposer<in T> : IAutoGuiComposer
    {
        public void Compose(GuiComposer composer, IServiceProvider provider, MemberInfo member, T value);

        void IAutoGuiComposer.ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value) => Compose(composer, provider, member, (T)value);

        bool IAutoGuiComposer.IsValidForCompose(Type type) => typeof(T).IsAssignableFrom(type);
    }

    public interface IAutoGuiComposer
    {
        public void ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value);

        public bool IsValidForCompose(Type type);
    }
}
