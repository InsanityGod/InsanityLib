using Cairo;
using InsanityLib.Interfaces.UI.ImGuiComponents;
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
    internal class SetComposer : IImGuiComposer
    {
        public bool CanComposeType(Type type) => type.FindGenericInterfaceDefinition(typeof(ISet<>)) is not null;

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
            var componentContainer = new ComponentCollection(context);
            var addButton = new SetAddButton(context, componentContainer);
            componentContainer.Components.Add(addButton);
            if(!context.TryGetValue(out var container) || container is not IEnumerable setEnumerable) return componentContainer;
            
            foreach(var item in setEnumerable) addButton.AddDisplay(item, true);

            return componentContainer;
        }

    }
}
