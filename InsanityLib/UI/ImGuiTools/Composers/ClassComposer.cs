using InsanityLib.Config;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using System;
using System.Linq;
using System.Reflection;

namespace InsanityLib.UI.ImGuiTools.Composers;

public class ClassComposer : IImGuiComposer
{
    public bool CanComposeType(Type type) => type.IsComplexClassType();

    public IImGuiComponent Compose(ImGuiContext context, Type type)
    {
        if(!context.TryGetValue(out var classInstance) || classInstance is null)
        {
            return new LateInitialize(context);
        }

        var container = new ComponentCollection(context);

        var memberGroups = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.InvokeMethod)
            .GroupBy(member => member.GetCustomAttribute<System.ComponentModel.CategoryAttribute>()?.Category)
            .OrderByDescending(group => group.Key is null)
            .ThenBy(group => group.Key)
            .ToList();
        
        foreach(var group in memberGroups)
        {
            var groupContainer = container;
            if(memberGroups.Count > 0) 
            {
                groupContainer = new ComponentCollection(context)
                {
                    LabelOverride = group.Key,
                    HideDescription = true,
                    Spacing = true
                };
                groupContainer.DisplayProperties.Hierarchy = group.Key is null ? EHierarchyDisplay.None : EHierarchyDisplay.Seperator;
                container.Components.Add(groupContainer);
            }

            foreach(var member in group)
            {
                if(member.DeclaringType == typeof(object)) continue;
                if(member is not PropertyInfo && member is not FieldInfo && member is not MethodInfo) continue;

                var memberContext = context.New(member.Name, member);

                var component = ImGuiComposer.TryCompose(memberContext);
                if(component is not null) groupContainer.Components.Add(component);
            }

            if(InsanityLibConfig.Instance.AutoConfig.AutoElementOrdering) groupContainer.Components.Sort(SortGroup);
        }

        return container;
    }

    private int SortGroup(IImGuiComponent x, IImGuiComponent y)
    {
        // Check if either component is a container and get their hierarchy display
        static EHierarchyDisplay GetHierarchy(IImGuiComponent comp) => comp is IImGuiComponentContainer container ? container.DisplayProperties.Hierarchy : EHierarchyDisplay.None;

        var hx = GetHierarchy(x);
        var hy = GetHierarchy(y);

        // EHierarchyDisplay.None: no sorting, keep original order
        if (hx == EHierarchyDisplay.None && hy == EHierarchyDisplay.None) return 0;

        // Define order: None < Seperator < DropDown
        static int Order(EHierarchyDisplay h) => h switch
        {
            EHierarchyDisplay.None => 0,
            EHierarchyDisplay.Seperator => 1,
            EHierarchyDisplay.DropDown => 2,
            _ => 0
        };

        return Order(hx).CompareTo(Order(hy));
    }
}
