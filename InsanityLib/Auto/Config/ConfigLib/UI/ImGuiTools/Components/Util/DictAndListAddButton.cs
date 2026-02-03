using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Contexts;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;

public class DictAndListAddButton : Button
{
    public readonly Type? KeyType;

    public readonly Type? ValueType;
    public readonly IImGuiComponentContainer ComponentContainer;

    private int lastIndex = -1;

    public DictAndListAddButton(ImGuiContext context, IImGuiComponentContainer componentContainer) : base(context.New("addbutton", name: "add"), null)
    {
        ComponentContainer = componentContainer;
        Action = AddItem;
        if(!Context.TryGetValue(out var container)) return;

        var containerType = container!.GetType()!;
        var dictTypes = containerType.FindGenericInterfaceDefinition(typeof(IDictionary<,>))?.GenericTypeArguments;
        var listTypes = containerType.FindGenericInterfaceDefinition(typeof(ICollection<>))?.GenericTypeArguments;
        
        if(dictTypes is not null)
        {
            KeyType = dictTypes[0];
            ValueType = dictTypes[1];
        }
        else if(container is Array)
        {
            KeyType = typeof(int);
            ValueType = containerType.GetElementType()!;
        }
        else if(listTypes is not null)
        {
            ValueType = listTypes[0];
        }
    }

    public void AddItem()
    {
        if(!Context.TryGetValue(out var container))
        {
            Context.ParentContext!.TrySetValue(Context.ParentContext.Member!.GetPrimaryType()!.AutoCreate(Context.ParentContext), this);
            if(!Context.TryGetValue(out container)) return;
        }
        
        var value = ValueType?.AutoCreate(Context, false);

        if (container is IDictionary dict)
        {
            var key = KeyType?.AutoCreate(Context, false);

            if (!dict.Contains(key!))
            {
                dict.Add(key!, value);
                AddDisplay(key!, value!, true);
            }
            else AddDisplay(key!, value!, false);
        }
        else if(container is IList list)
        {
            var key = list.Count;
            if(container is Array)
            {
                var newArray = Array.CreateInstance(ValueType!, key + 1);
                list.CopyTo(newArray, 0);
                newArray.SetValue(value, key);

                Context.ParentContext!.TrySetValue(newArray, this);
                foreach (var item in ComponentContainer.Where(item => item.Context.TargetObject == container))
                {
                    item.Context.TargetObject = newArray;
                }

                AddDisplay(key, value!, true);
            }
            else
            {
                list.Add(value);
                AddDisplay(key, value!, true);
            }
        }
    }

    public void AddDisplay(object key, object item, bool existsInDictionary)
    {
        if(!Context.TryGetValue(out var container)) return;
        
        var collection = new SameLineComponentCollection(Context)
        {
            Spread = new int?[3]
        };

        KeyContext keyContext = new(Context.TargetObject!, Context.Member!, KeyType!, key, ValueType!, Context, $"key-{lastIndex++}", string.Empty)
        {
            ExistsInDictionary = existsInDictionary
        };
        collection.ValidationResulProvider = keyContext.KeyValidation;

        if (!keyContext.ExistsInDictionary)
        {
            keyContext.KeyValidation.LastValidationResult = "Could not insert key, as it alrady exists in the dictionary!";
        }

        collection.Components.Add(new RemoveButton(ComponentContainer, collection, keyContext)
        {
            FullWidth = false,
            FixedWidth = new Vector2(50, 0)
        });

        keyContext.ValueContext.CachedObject = item;
        if (container is IDictionary)
        {
            var keyComponent = ImGuiComposer.TryCompose(keyContext, KeyType!);
            if(keyComponent is not null)
            {
                collection.Spread[collection.Components.Count] = 200; //Set width for key column
                collection.Components.Add(keyComponent);
            }
        }
        
        var valueComponent = ImGuiComposer.TryCompose(keyContext.ValueContext, ValueType!);
        if(valueComponent is not null)
        {
            collection.Components.Add(valueComponent);
        }

        ComponentContainer.Components.Insert(ComponentContainer.Components.IndexOf(this), collection);
    }
}
