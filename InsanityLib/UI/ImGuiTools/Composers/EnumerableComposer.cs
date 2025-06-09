using Cairo;
using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    internal class EnumerableComposer : IImGuiComposer
    {
        public bool CanComposeType(Type type) => type.IsArray || typeof(IDictionary).IsAssignableFrom(type) || typeof(IList).IsAssignableFrom(type);

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
            var componentContainer = new ComponentCollection(context);
            var addButton = new AddButton(context, componentContainer);
            componentContainer.Components.Add(addButton);
            
            //TODO sperator component

            if(context.TryGetValue(out var container))
            {
                if(container is IDictionary dict)
                {
                    foreach(var key in dict.Keys)
                    {
                        addButton.AddDisplay(key, dict[key], true);
                    }
                }
                else if(container is IList list)
                {
                    for(var i = 0 ; i < list.Count; i++)
                    {
                        addButton.AddDisplay(i, list[i], true);
                    }
                }
            }

            return componentContainer;
        }

    }
}
