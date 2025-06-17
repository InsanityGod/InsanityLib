using ImGuiNET;
using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Contexts;
using InsanityLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using YamlDotNet.Core.Tokens;

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class RemoveButton : Button
    {
        public readonly IImGuiComponentContainer ParentContainer;
        public readonly IImGuiComponentContainer ComponentContainer;

        public readonly KeyContext KeyContext;

        public RemoveButton(IImGuiComponentContainer parentContainer, IImGuiComponentContainer componentContainer, KeyContext keyContext) : base(keyContext.ParentContext.New("removebutton", name: "X"), null)
        {
            ComponentContainer = componentContainer;
            Action = Remove;
            ParentContainer = parentContainer;
            KeyContext = keyContext;
        }

        //TODO initialization/removal for Complex Types?
        public void Remove()
        {
            if(!Context.TryGetValue(out var container)) return;

            ParentContainer.Components.Remove(ComponentContainer);

            if (container is IDictionary dict)
            {
                if (KeyContext.ExistsInDictionary)
                {
                    dict.Remove(KeyContext.LastValidKey);
                }
            }
            else if(container is IList list)
            {
                if(container is Array)
                {
                    var newArray = Array.CreateInstance(KeyContext.ValueContext.ValueType, list.Count - 1);
                    
                    //for(var i = 0; i < )

                    if(!KeyContext.ValueContext.TryGetValue(out var toSkip)) return;

                    var index = 0; //TODO
                    foreach(var item in list)
                    {
                        if(item != toSkip)
                        {
                            newArray.SetValue(item, index++);
                        }
                    }
                }
                else list.RemoveAt((int)KeyContext.LastValidKey);
            }
        }
    }
}
