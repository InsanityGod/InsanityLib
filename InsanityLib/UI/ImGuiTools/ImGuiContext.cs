using Cairo;
using InsanityLib.UI.ImGuiTools.Contexts;
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

            return ServiceProvider?.GetService(serviceType) ?? ParentContext?.GetService(serviceType);
        }

        public readonly ImGuiContext ParentContext;
        public readonly IServiceProvider ServiceProvider;

        private object targetObject;
        public object TargetObject { get => targetObject; protected set => targetObject = value; }

        public readonly MemberInfo Member;

        public readonly string Label;

        public string Description { get; protected set; }

        public readonly string Id;

        public virtual ImGuiContext New(string id = null, MemberInfo member = null, string name = null) => new(member == null ? TargetObject : Member.GetValue(TargetObject), member ?? Member, this, id, name);

        public ImGuiContext(object targetObject, MemberInfo member, ImGuiContext parentContext = null, string id = null, string name = null, IServiceProvider serviceProvider = null)
        {
            TargetObject = targetObject;
            Member = member;
            ServiceProvider = serviceProvider;
            PropertyChanged += Validate;

            var idBuilder = new StringBuilder();
            if (parentContext != null)
            {
                ParentContext = parentContext;
                AllowedToWrite = ParentContext.AllowedToWrite;

                idBuilder.Append(parentContext.Id);
                idBuilder.Append('-');
            }
            idBuilder.Append(id ?? Guid.NewGuid().ToString());
            Id = idBuilder.ToString();
            Label = $"{name ?? Member?.GetHumanReadableName()}##{Id}";

            if(Member == null)
            {
                CanRead = true;
                CanWrite = ParentContext is ValueContext;
                return;
            }
            CanRead = Member.CanGetValue();
            AllowedToRead = Member.GetCustomAttribute<BrowsableAttribute>()?.Browsable != false;

            CanWrite = Member.CanSetValue();
            AllowedToWrite &= Member.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly != true;

            var docs = Member.GetDocumentationContext();

            Description = docs.GetExtendedDescription().ReplaceSpecialSymbolsWithText(); //TODO maybe some way to repsect display format when using it for ImGui (so 0.3 shows up as 30%)
            if (string.IsNullOrWhiteSpace(Description)) Description = null;
            
        }

        public virtual Type ComposeType
        {
            get
            {
                if(Member == null) return TargetObject.GetType();

                return Member is MethodInfo ? typeof(MethodInfo) : Member.GetPrimaryType();
            }
        }

        public readonly bool CanRead;
        public readonly bool AllowedToRead = true;
        public readonly bool CanWrite;
        public readonly bool AllowedToWrite = true;

        public PropertyChangedEventHandler PropertyChanged { get; set; }
        public string LastValidationResult { get; set; }

        public virtual void Validate(object sender, PropertyChangedEventArgs args)
        {
            //Optional
        }

        public void NotifyChanged(object sender) => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(Member.Name));

        public virtual bool TryGetValue(out object value)
        {
            value = null;
            if(!CanRead) return false;
            else if(Member == null)
            {
                value = TargetObject;
                return true;
            }

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

        public virtual bool TrySetValue(object value, object ChangedBy)
        {
            if(!CanWrite) return false;

            if(Member == null && ParentContext is ValueContext)
            {
                var result = ParentContext.TryAutoSetValue(value, ChangedBy);
                if (result) ParentContext.TryGetValue(out targetObject);
                return result;
            }

            try
            {
                Member.SetValue(value, TargetObject);
                NotifyChanged(ChangedBy);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool TryAutoSetValue(object value, object ChangedBy)
        {
            if(!CanWrite) return false;
            try
            {
                if(!TrySetValue(value.AutoConvert(Member.GetPrimaryType()), this)) return false;
                NotifyChanged(ChangedBy);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
