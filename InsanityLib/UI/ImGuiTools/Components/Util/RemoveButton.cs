using ImGuiNET;
using InsanityLib.Config.Util;
using InsanityLib.Interfaces.UI.ImGuiComponents;
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

        public RemoveButton(IImGuiComponentContainer parentContainer, IImGuiComponentContainer componentContainer, KeyContext keyContext) : base(keyContext.ParentContext.New($"{keyContext.Id}-removebutton", name: "X"), null)
        {
            ComponentContainer = componentContainer;
            Action = Remove;
            ParentContainer = parentContainer;
            KeyContext = keyContext;
        }

        private void Detach() => ParentContainer.Components.Remove(ComponentContainer);

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
                var currentKey = (int)KeyContext.CurrentKey;
                if(container is Array)
                {
                    var newArray = Array.CreateInstance(KeyContext.ValueContext.ValueType, list.Count - 1);

                    var index = 0; //TODO
                    for(var i = 0; i < list.Count; i++)
                    {
                        if(i == currentKey) continue;
                        newArray.SetValue(list[i], index++);
                    }
                }
                else list.RemoveAt(currentKey);

                ShiftKeys(currentKey);
            }

        }

        private void ShiftKeys(int fromKey)
        {
            if(KeyContext.KeyType != typeof(int)) throw new InvalidOperationException("Cannot shift keys for non-int key type");

            var keys = ParentContainer.Components.OfType<IImGuiComponentContainer>()
                .SelectMany(c => c.Components)
                .OfType<RemoveButton>()
                .Select(button => button.KeyContext);

            foreach (var keyContext in keys)
            {
                var key = (int)keyContext.CurrentKey;
                if (key > fromKey)
                {
                    key--;
                    keyContext.CurrentKey = key;
                    keyContext.LastValidKey = key;
                }
            }
        }
    }
}
