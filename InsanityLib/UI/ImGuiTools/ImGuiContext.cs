using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace InsanityLib.UI.ImGuiTools
{
    public class ImGuiContext : IServiceProvider
    {
        public virtual object GetService(Type serviceType)
        {
            if(serviceType.IsInstanceOfType(this)) return this;
            return ParentContext?.GetService(serviceType);
        }

        public readonly ImGuiContext ParentContext;

        public readonly object TargetObject;

        public readonly MemberInfo Member;

        public readonly string Label;

        public readonly string Description;

        public readonly string Id;

        public ImGuiContext New(string id = null, MemberInfo member = null, string name = null) => new(member == null ? TargetObject : Member.GetValue(TargetObject), member ?? Member, this, id, name);

        public ImGuiContext(object targetObject, MemberInfo member, ImGuiContext parentContext = null, string id = null, string name = null)
        {
            TargetObject = targetObject;
            Member = member;

            var idBuilder = new StringBuilder("##");
            if (parentContext != null)
            {
                ParentContext = parentContext;

                idBuilder.Append(parentContext.Id);
                idBuilder.Append('-');
            }
            idBuilder.Append(id ?? Guid.NewGuid().ToString());
            Id = idBuilder.ToString();

            CanRead = Member.CanGetValue();
            AllowedToRead = Member.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false;
            
            CanWrite = Member.CanSetValue();
            AllowedToWrite = ParentContext.AllowedToWrite && Member.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly != true;

            Label = $"{name ?? Member.GetHumanReadableName()}{Id}";

            var docs = Member.GetDocumentationContext();

            Description = docs.GetDescription();
            if (string.IsNullOrWhiteSpace(Description)) Description = null;
        }

        public readonly bool CanRead;
        public readonly bool AllowedToRead = true;
        public readonly bool CanWrite;
        public readonly bool AllowedToWrite = true;

        public bool TryGetValue(out object value)
        {
            value = null;
            if(!CanRead) return false;
            try
            {
                value = Member.GetValue(TargetObject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TrySetValue(object value)
        {
            if(!CanWrite) return false;
            try
            {
                Member.SetValue(value, TargetObject);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
