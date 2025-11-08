using HarmonyLib;
using InsanityLib.UI.ImGuiTools.Contexts;
using InsanityLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace InsanityLib.UI.ImGuiTools.Components.Util;

public class SetAddButton : Button
{

    public readonly Type ValueType;
    public readonly IImGuiComponentContainer ComponentContainer;

    public SetAddButton(ImGuiContext context, IImGuiComponentContainer componentContainer) : base(context.New("addbutton", name: "add"), null)
    {
        ComponentContainer = componentContainer;
        if(!Context.TryGetValue(out var container)) return;

        var containerType = container.GetType();
        ValueType = containerType.FindGenericInterfaceDefinition(typeof(ISet<>))?.GenericTypeArguments[0];
        
        Action = AddItem;
    }

    public void AddItem() => typeof(SetAddButton).Method(nameof(AddItemInternal)).MakeGenericMethod(ValueType).Invoke(this);
    
    private void AddItemInternal<T>()
    {
        if(!Context.TryGetValue(out var container))
        {
            Context.ParentContext.TrySetValue(Context.ParentContext.Member.GetPrimaryType().AutoCreate(Context.ParentContext), this);
            if(!Context.TryGetValue(out container)) return;
        }

        if(container is not ISet<T> set) return;
        T value = default;
        AddDisplay(value, set.Add(value));
    }
    
    public void AddDisplay(object item, bool existsInSet)
    {
        var collection = new SameLineComponentCollection(Context)
        {
            Spread = new int?[2]
        };
        
        SetItemContext setItemContext = new(Context.TargetObject, Context.Member, ValueType, item, Context, $"key-{Guid.NewGuid()}", string.Empty)
        {
            ExistsInSet = existsInSet,
        };
        collection.ValidationResulProvider = setItemContext.SetValidation;
        if(!existsInSet) setItemContext.SetValidation.LastValidationResult = "Item already exists in set!";

        collection.Components.Add(new RemoveButton(ComponentContainer, collection, setItemContext)
        {
            FullWidth = false,
            FixedWidth = new Vector2(50, 0)
        });
        var component = ImGuiComposer.TryCompose(setItemContext, ValueType);
        if(component != null) collection.Components.Add(component);
        
        ComponentContainer.Components.Insert(ComponentContainer.Components.IndexOf(this), collection);
    }
}
