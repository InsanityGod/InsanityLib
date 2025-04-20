using HarmonyLib;
using InsanityLib.Interfaces;
using InsanityLib.Interfaces.UI;
using InsanityLib.UI.Contexts;
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

namespace InsanityLib.UI.Composers.Complex
{
    public class AutoClassGuiComposer : IAutoGuiComposer
    {

        public void ComposeObject(GuiComposer composer, IServiceProvider provider, MemberInfo member, object value)
        {
            if (provider is MemberContext) return;
            //TODO skipping sub complex classes for now
            //TODO do something with member
            if (value == null) return;
            var memberContext = new MemberContext(provider, member, value);
            var recursiveProtection = provider.GetService<IRecursivePrevention>();
            
            var members = value.GetType()
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Where(member => member.DeclaringType != typeof(object));
            
            //TODO sorting/grouping

            foreach (var memberInfo in members)
            {
                try
                {
                    switch (memberInfo)
                    {
                        case MethodInfo method:
                            if(method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;
                            if (method.CanAutoInvoke(provider))
                            {
                                typeof(MethodBase).FindAutoGuiComposer()
                                    .ComposeObject(composer, memberContext, memberInfo, method);
                            }
                            break;

                        case PropertyInfo property:
                            if (property.CanGetValue())
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
                    //TODO
                }
            }

            var descriptors = members.Select(member => DescriptorContext.GetDescriptorPath(memberContext, member))
                .Select(key => (Key: key, Element: composer.GetElement(key)))
                .Where(pair => pair.Element != null)
                .ToList();
            var xAllignment = descriptors.Max(pair => pair.Element.Bounds.fixedX + pair.Element.Bounds.fixedWidth);
            foreach (var descriptor in descriptors)
            {
                var matchingContent = composer.GetElement(descriptor.Key.Replace("/@Descriptor", string.Empty));
                if (matchingContent != null)//BlockSoil/RemapToLiquidsLayer
                {
                    matchingContent.Bounds.fixedX = xAllignment;
                }
            }
        }

        public bool IsValidForCompose(Type type) => type.IsClass && !type.IsArray && !typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(MethodBase);
    }
}
