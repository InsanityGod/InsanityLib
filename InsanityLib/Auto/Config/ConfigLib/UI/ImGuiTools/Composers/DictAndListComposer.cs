using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using System;
using System.Collections;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Composers;

internal class DictAndListComposer : IImGuiComposer
{
    public bool CanComposeType(Type type) => type.IsArray || typeof(IDictionary).IsAssignableFrom(type) || typeof(IList).IsAssignableFrom(type);

    public IImGuiComponent Compose(ImGuiContext context, Type type)
    {
        var componentContainer = new ComponentCollection(context);
        var addButton = new DictAndListAddButton(context, componentContainer);
        componentContainer.Components.Add(addButton);

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
