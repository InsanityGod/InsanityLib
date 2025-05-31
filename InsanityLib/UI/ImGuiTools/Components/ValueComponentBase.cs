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

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            OnValueChanged(this, new PropertyChangedEventArgs(Context.Member.Name));
            context.PropertyChanged += OnValueChanged;
        }

        protected virtual void OnValueChanged(object sender, PropertyChangedEventArgs args)
        {
            value = Context.Member.GetValue(Context.TargetObject).AutoConvert<T>();
            Validate();
        }
    }

    public abstract class ValueComponentBase : ComponentBase
    {
        protected readonly ResetButton ResetButton;

        protected ValueComponentBase(ImGuiContext context) : base(context)
        {
            ValidationAttributes = context.Member.GetCustomAttributes<ValidationAttribute>().ToArray();

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
            if (ValidationAttributes.Length == 0) return true;
            if(!Context.TryGetValue(out var value)) return false;
            
            var builder = new StringBuilder();

            foreach (var attribute in ValidationAttributes)
            {
                var result = attribute.GetValidationResult(value, ValidationContext);
                if(result != ValidationResult.Success)
                {
                    if(builder.Length > 0) builder.Append(Environment.NewLine);
                    builder.Append(result.ToString());
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

        public abstract void RenderValue();

        public override void Render()
        {
            if(!Context.AllowedToWrite) ImGui.BeginDisabled();
            
            ResetButton?.SafeRender();

            try
            {
                RenderValue();
            }
            catch
            {
                //TODO logging
            }

            if(Context.Description != null) Editors.DrawHint(Context.Description);
            
            if(!Context.AllowedToWrite) ImGui.EndDisabled();
            
            if(!string.IsNullOrEmpty(LastValidationResult)) ImGui.TextColored(ValidationColor, LastValidationResult);
        }
    }
}
