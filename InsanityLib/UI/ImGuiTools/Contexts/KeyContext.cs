using Cairo;
using InsanityLib.UI.ImGuiTools.Helpers;
using InsanityLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Contexts
{
    public class KeyContext : ImGuiContext
    {
        public override Type ComposeType => KeyType;
        public readonly Type KeyType;
        
        public readonly ValueContext ValueContext;
        
        public bool ExistsInDictionary { get; internal set; }

        public object LastValidKey { get; internal set; }

        public readonly ValidationResultHolder KeyValidation = new();

        public object CurrentKey { get; internal set; }
        
        public override ImGuiContext New(string id = null, MemberInfo member = null, string name = null) => new(member is null ? TargetObject : CurrentKey, member ?? Member, this, id, name);

        public KeyContext(object targetObject, MemberInfo member, Type keyType, object currentKey, Type valueType, ImGuiContext parentContext, string id = null, string name = null, IServiceProvider serviceProvider = null) : base(targetObject, member, parentContext, id, name, serviceProvider)
        {
            KeyType = keyType;
            LastValidKey = currentKey;
            CurrentKey = currentKey;

            ValueContext = new(TargetObject, Member, valueType, this, parentContext, $"{Id}-value", name: string.Empty);

            Description = null;
        }

        public override bool TryGetValue(out object value)
        {
            if (!CanRead)
            {
                value = null;
                return false;
            }
            value = CurrentKey;
            return true;
        }

        public override bool TrySetValue(object value, object ChangedBy)
        {
            KeyValidation.LastValidationResult = null;

            if(!CanWrite) return false;
            
            CurrentKey = value;
            if(value == LastValidKey) return true;

            if(base.TryGetValue(out var container) && container is IDictionary dict)
            {
                if (dict.Contains(value))
                {
                    KeyValidation.LastValidationResult = "Could not insert key, as it alrady exists in the dictionary!";
                    return false;
                }

                try
                {
                    //TODO check if other items where trying to use this key and remove their Duplicate Key! message
                    var toMove = ExistsInDictionary ? dict[LastValidKey] : ValueContext.CachedObject;
                    if(ExistsInDictionary) dict.Remove(LastValidKey);
                    dict.Add(value, toMove);
                    LastValidKey = value;

                    ExistsInDictionary = true;
                    NotifyChanged(this);
                    return true;
                }
                catch(Exception ex)
                {
                    KeyValidation.LastValidationResult = ex.ToString();
                    return false;
                }
            }
            else
            {
                KeyValidation.LastValidationResult = $"Could net set key for non IDictionary object '{value?.GetType()}'";
                return false;
            }
        }

        public override bool TryAutoSetValue(object value, object ChangedBy)
        {
            if(!CanWrite) return false;
            try
            {
                return TrySetValue(value.AutoConvert(KeyType), this);
            }
            catch
            {
                return false;
            }
        }
    }
}
