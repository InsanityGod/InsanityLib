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

namespace InsanityLib.UI.ImGuiTools.Components.Util
{
    public class AddButton : Button
    {
        public readonly Type KeyType;

        public readonly Type ValueType;
        public readonly IImGuiComponentContainer ComponentContainer;

        private int lastIndex = 0;

        public AddButton(ImGuiContext context, IImGuiComponentContainer componentContainer) : base(context.New("addbutton", name: "add"), null)
        {
            ComponentContainer = componentContainer;
            Action = AddItem;
            if(!Context.TryGetValue(out var container)) return;

            var containerType = container.GetType();
            var dictTypes = containerType.FindGenericInterfaceDefinition(typeof(IDictionary<,>))?.GenericTypeArguments;
            var listTypes = containerType.FindGenericInterfaceDefinition(typeof(ICollection<>))?.GenericTypeArguments;
            
            if(dictTypes != null)
            {
                KeyType = dictTypes[0];
                ValueType = dictTypes[1];
            }
            else if(container is Array)
            {
                KeyType = typeof(int);
                ValueType = containerType.GetElementType();
            }
            else if(listTypes != null)
            {
                ValueType = listTypes[0];
            }
        }

        public void AddItem()
        {
            if(!Context.TryGetValue(out var container))
            {
                Context.ParentContext.TrySetValue(Context.ParentContext.Member.GetPrimaryType().AutoCreate(Context.ParentContext), this);
                if(!Context.TryGetValue(out container)) return;
            }
            
            var value = ValueType?.AutoCreate(Context, false);

            if (container is IDictionary dict)
            {
                var key = KeyType?.AutoCreate(Context, false);

                if (!dict.Contains(key))
                {
                    dict.Add(key, value);
                    AddDisplay(key, value, true);
                }
                else AddDisplay(key, value, false);
            }
            else if(container is IList list)
            {
                var key = list.Count;
                if(container is Array)
                {
                    var newArray = Array.CreateInstance(ValueType, key + 1);
                    list.CopyTo(newArray, 0);
                    newArray.SetValue(value, key);
                    AddDisplay(key, value, true);
                }
                else
                {
                    list.Add(value);
                    AddDisplay(key, value, true);
                }
            }
            //TODO sets
        }

        public void AddDisplay(object key, object item, bool existsInDictionary)
        {
            if(!Context.TryGetValue(out var container)) return;
            var collection = new SameLineComponentCollection(Context)
            {
                Spread = new int?[3]
            };

            KeyContext keyContext = new(Context.TargetObject, Context.Member, KeyType, key, ValueType, Context, $"key-{lastIndex++}", string.Empty)
            {
                ExistsInDictionary = existsInDictionary
            };

            if (!keyContext.ExistsInDictionary)
            {
                keyContext.LastValidationResult = "Duplicate Key!"; //TODO
            }

            keyContext.ValueContext.CachedObject = item;
            if (container is IDictionary)
            {
                var keyComponent = ImGuiComposer.TryCompose(keyContext, KeyType);
                if(keyComponent != null)
                {
                    collection.Spread[collection.Components.Count] = 200; //Set width for key column
                    collection.Components.Add(keyComponent);
                }
            }

            
            var valueComponent = ImGuiComposer.TryCompose(keyContext.ValueContext, ValueType);
            if(valueComponent != null) collection.Components.Add(valueComponent);
            //TODO delete button
            //TODO test Set / HashSet

            ComponentContainer.Components.Insert(ComponentContainer.Components.IndexOf(this), collection);
        }
    }
}
