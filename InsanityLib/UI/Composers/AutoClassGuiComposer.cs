using HarmonyLib;
using InsanityLib.Interfaces;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;

namespace InsanityLib.UI.Composers
{
    public class AutoClassGuiComposer : IAutoGuiComposer
    {

        public void ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value)
        {
            //TODO do something with member
            if(value == null) return;
            var memberContext = new MemberContext(provider, value);
            var recursiveProtection = provider.GetService<IRecursivePrevention>();
            //TODO Class context for setting/getting values
            var members = value.GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Public);

            //TODO sorting/grouping

            foreach (var memberInfo in members)
            {
                try
                {
                    switch (memberInfo)
                    {
                        case MethodInfo method:
                            //TODO for buttons
                            break;

                        case PropertyInfo property:
                            if (property.CanRead && property.GetIndexParameters().Length == 0)
                            {
                                var propValue = property.GetValue(value);
                                if (recursiveProtection.EnsureUnique(propValue))
                                {
                                    property.PropertyType
                                        .FindAutoGuiComposer()
                                        ?.ComposeObject(composer, memberContext, memberInfo, propValue);
                                }
                            }
                            break;

                        case FieldInfo field:
                            var fieldValue = field.GetValue(value);
                            if (recursiveProtection.EnsureUnique(fieldValue))
                            {
                                field.FieldType
                                .FindAutoGuiComposer()
                                ?.ComposeObject(composer, memberContext, memberInfo, fieldValue);
                            }
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        public bool IsValidForCompose(Type type) => type.IsClass && !type.IsArray && !typeof(IEnumerable).IsAssignableFrom(type);
    }
}
