using ImGuiNET;
using InsanityLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Util;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using VSImGui;

#pragma warning disable S1699 // Constructors should only call non-overridable methods
namespace InsanityLib.UI.ImGuiTools.Components
{
    public abstract class ValueComponentBase<T> : ValueComponentBase
    {
        protected T value;
        public T Value => value;

        public override object ValueAsObject => Value;

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            OnValueChanged(this, new PropertyChangedEventArgs(Context.Member.Name));
            context.PropertyChanged += OnValueChanged;
        }

        protected virtual void OnValueChanged(object sender, PropertyChangedEventArgs args)
        {
            if(!Context.TryGetValue(out var obj)) return;

            isNull = IsNullable && obj is null;
            if (!IsNull) value = obj.AutoConvert<T>();

            Validate();
        }
    }

    public abstract class ValueComponentBase : ComponentBase
    {
        protected readonly ResetButton ResetButton;

        public bool IsNullable { get; }

        protected bool isNull;
        public bool IsNull => isNull;

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            ValidationAttributes = context.Member.GetCustomAttributes<ValidationAttribute>().ToArray();
            IsNullable = Nullable.GetUnderlyingType(context.Member.GetPrimaryType()) is not null;
            ValidationContext = new(context.TargetObject)
            {
                MemberName = context.Member.Name,
            };

            ResetButton = ResetButton.TryCreate(context);
        }

        #region validation
        public static Vector4 ValidationColor { get; } = new(Color.Red.R, Color.Red.G, Color.Red.B, Color.Red.A); //TODO config
        
        public ValidationContext ValidationContext { get; }

        public ValidationAttribute[] ValidationAttributes { get; }

        public string LastValidationResult { get; protected set; }
        
        public virtual bool Validate()
        {
            //TODO some way to check if the problem was solved, so we can notify other components to check as well
            if (ValidationAttributes.Length == 0) return true;
            if(!Context.TryGetValue(out var value)) return false;
            
            var builder = new StringBuilder();

            foreach (var attribute in ValidationAttributes)
            {
                var result = attribute.GetValidationResult(value, ValidationContext);
                if(result != ValidationResult.Success)
                {
                    builder.AppendLine(result.ToString());
                }
            }
            
            if(builder.Length > 0)
            {
                LastValidationResult = builder.ToString().ReplaceSpecialSymbolsWithText();
                return false;
            }

            LastValidationResult = null;
            return true;
        }
        #endregion validation
        
        public abstract object ValueAsObject { get; }
        public abstract void RenderValue();

        public override void Render()
        {
            ImGui.BeginDisabled(!Context.AllowedToWrite);
            
            ResetButton?.SafeRender();

            ImGui.BeginDisabled(isNull); //Normal value cannot be changed while null checkbox is checked
            try
            {
                RenderValue();
            }
            catch
            {
                //TODO logging
            }
            ImGui.EndDisabled();
            
            if(IsNullable)
            {
                ImGui.SameLine();
                if(ImGui.Checkbox($"Null##{Context.Id}-nullable", ref isNull))
                {
                    Context.TryAutoSetValue(isNull ? null : ValueAsObject, this);
                }
            }


            if(Context.Description is not null) Editors.DrawHint(Context.Description);
            
            ImGui.EndDisabled();
            
            if(!string.IsNullOrEmpty(LastValidationResult)) ImGui.TextColored(ValidationColor, LastValidationResult);
            if(!string.IsNullOrEmpty(Context.LastValidationResult)) ImGui.TextColored(ValidationColor, Context.LastValidationResult);
        }
    }
}
