using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Contexts;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using System;
using System.Collections;
using System.Linq;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;

public class RemoveButton : Button
{
    public readonly IImGuiComponentContainer ParentContainer;
    public readonly IImGuiComponentContainer ComponentContainer;

    public readonly KeyContext KeyContext;
    public readonly SetItemContext SetItemContext;

    private RemoveButton(IImGuiComponentContainer parentContainer, IImGuiComponentContainer componentContainer, ImGuiContext context) : base(context.ParentContext.New($"{context.Id}-removebutton", name: "X"), null)
    {
        ComponentContainer = componentContainer;
        Action = Remove;
        ParentContainer = parentContainer;
    }
    public RemoveButton(IImGuiComponentContainer parentContainer, IImGuiComponentContainer componentContainer, KeyContext keyContext) : this(parentContainer, componentContainer, keyContext as ImGuiContext) => KeyContext = keyContext;
    public RemoveButton(IImGuiComponentContainer parentContainer, IImGuiComponentContainer componentContainer, SetItemContext setItemContext) : this(parentContainer, componentContainer, setItemContext as ImGuiContext) => SetItemContext = setItemContext;

    public void Remove()
    {
        if(!Context.TryGetValue(out var container)) return;

        ParentContainer.Components.Remove(ComponentContainer);
        if(SetItemContext is not null)
        {
            SetItemContext.Remove();
            return;
        }

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

                var index = 0;
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
