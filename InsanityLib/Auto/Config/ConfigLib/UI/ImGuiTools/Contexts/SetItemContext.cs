using HarmonyLib;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Helpers;
using InsanityLib.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Contexts;

public class SetItemContext(object targetObject, MemberInfo member, Type valueType, object currentValue, ImGuiContext parentContext, string? id = null, string? name = null, IServiceProvider? serviceProvider = null) : ImGuiContext(targetObject, member, parentContext, id, name, serviceProvider)
{
    public override Type ComposeType => ValueType;
    
    public readonly Type ValueType = valueType;
    
    public bool ExistsInSet { get; internal set; }
    private object CurrentValue = currentValue;

    public readonly ValidationResultHolder SetValidation = new();
    
    public override ImGuiContext New(string? id = null, MemberInfo? member = null, string? name = null) => new(member is null ? TargetObject! : CurrentValue, member ?? Member, this, id, name);

    public override bool TryGetValue(out object? value)
    {
        if (!CanRead)
        {
            value = null;
            return false;
        }
        value = CurrentValue;
        return true;
    }

    public override bool TrySetValue(object? value, object ChangedBy)
    {
        SetValidation.LastValidationResult = null;

        if(!CanWrite) return false;
        
        if(Equals(value, CurrentValue)) return true;
        return (bool)typeof(SetItemContext)
            .GetMethod(nameof(TrySetValueInternal), AccessTools.allDeclared)!
            .MakeGenericMethod(ValueType)
            .Invoke(this, [value])!;
    }

    private bool TrySetValueInternal<T>(object value)
    {
        if(!base.TryGetValue(out var container) || container is not ISet<T> set || value is not T valueAsT) return false;
        if(ExistsInSet) set.Remove((T)CurrentValue);
        if (set.Contains(valueAsT))
        {
            ExistsInSet = false;
            SetValidation.LastValidationResult = "Item already exists in set!";
        }
        else if(set.Add(valueAsT))
        {
            CurrentValue = valueAsT;
            ExistsInSet = true;
            NotifyChanged(this);
        }

        return true;
    }

    public override bool TryAutoSetValue(object? value, object ChangedBy)
    {
        if(!CanWrite) return false;
        try
        {
            return TrySetValue(value.AutoConvert(ValueType), this);
        }
        catch
        {
            return false;
        }
    }

    public void Remove()
    {
        typeof(SetItemContext)
            .GetMethod(nameof(RemoveInternal), AccessTools.allDeclared)!
            .MakeGenericMethod(ValueType)
            .Invoke(this);
    }

    private void RemoveInternal<T>()
    {
        if(!ExistsInSet || !base.TryGetValue(out var container) || container is not ISet<T> set) return;
        set.Remove((T) CurrentValue);
    }
}
