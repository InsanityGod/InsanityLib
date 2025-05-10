using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    public class ClassComposer : IImGuiComposer
    {
        public bool CanComposeType(Type type) => type.IsComplexClassType();

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {

            var container = new ComponentCollection(context);

            foreach(var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if(member.DeclaringType == typeof(object)) continue;
                if(member is not PropertyInfo && member is not FieldInfo && member is not MethodInfo) continue;
                if(member is MethodInfo method && (method.IsBackingField() || Array.Exists(
                    type.GetProperties(),
                    prop => prop.GetGetMethod() == method || prop.GetSetMethod() == method //Ensure it's not a getter/setter
                ))) continue;

                var memberContext = context.New(member.Name, member);

                var component = ImGuiComposer.TryCompose(memberContext);
                if(component != null) container.Components.Add(component);
            }

            return container;
        }
    }
}
