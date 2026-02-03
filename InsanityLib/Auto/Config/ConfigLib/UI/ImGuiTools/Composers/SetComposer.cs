using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Composers;

internal class SetComposer : IImGuiComposer
{
    public bool CanComposeType(Type type) => type.FindGenericInterfaceDefinition(typeof(ISet<>)) is not null;

    public IImGuiComponent? Compose(ImGuiContext context, Type type)
    {
        var componentContainer = new ComponentCollection(context);
        var addButton = new SetAddButton(context, componentContainer);
        componentContainer.Components.Add(addButton);
        if(!context.TryGetValue(out var container) || container is not IEnumerable setEnumerable) return componentContainer;
        
        foreach(var item in setEnumerable) addButton.AddDisplay(item, true);

        return componentContainer;
    }

}
