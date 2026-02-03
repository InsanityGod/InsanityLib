using InsanityLib.Extensions;
using System;
using System.Collections;
using System.Reflection;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Contexts;

public class ValueContext : ImGuiContext
{
    public override Type ComposeType => ValueType;
    public readonly Type ValueType;

    public readonly KeyContext KeyContext;

    /// <summary>
    /// The initial object of the value.
    /// This is mostly usefull when the item could not be added to the collection due to key alrady existing
    /// </summary>
    public object? CachedObject { get; internal set; }

    public override ImGuiContext New(string? id = null, MemberInfo? member = null, string? name = null) => new(GetValueOrThrow(), member, this, id, name);

    private object GetValueOrThrow() => TryGetValue(out var value) ? value! : throw new InvalidOperationException();
    
    public ValueContext(object targetObject, MemberInfo member, Type valueType, KeyContext keyContext, ImGuiContext? parentContext = null, string? id = null, string? name = null, IServiceProvider? serviceProvider = null) : base(targetObject, member, parentContext, id, name, serviceProvider)
    {
        ValueType = valueType;
        KeyContext = keyContext;
        Description = null;
    }

    public override bool TryGetValue(out object? value)
    {
        if (!CanRead || !base.TryGetValue(out object? container))
        {
            value = null;
            return false;
        }

        if(!KeyContext.ExistsInDictionary)
        {
            value = CachedObject;
            return true;
        }

        if(container is IList list)
        {
            value = list[(int)KeyContext.LastValidKey!];
            return true;
        }
        else if(container is IDictionary dict)
        {
            value = dict[KeyContext.LastValidKey!];
            return true;
        }

        value = null;
        return false;
    }

    public override bool TrySetValue(object? value, object ChangedBy)
    {
        LastValidationResult = string.Empty;
        if(!CanWrite || !base.TryGetValue(out object? container)) return false;
        
        if (!KeyContext.ExistsInDictionary)
        {
            CachedObject = value;
            return false;
        }

        try
        {
            if(container is IList list)
            {
                list[(int)KeyContext.LastValidKey!] = value;
                NotifyChanged(ChangedBy);
                return true;
            }
            else if(container is IDictionary dict)
            {
                dict[KeyContext.LastValidKey!] = value;
                NotifyChanged(ChangedBy);
                return true;
            }
        }
        catch(Exception ex)
        {
            LastValidationResult = ex.ToString();
            return false;
        }

        return false;
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
}
