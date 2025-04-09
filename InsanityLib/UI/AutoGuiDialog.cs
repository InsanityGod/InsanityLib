using InsanityLib.Interfaces;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace InsanityLib.UI
{
    public class AutoGuiDialog : GuiDialog, IDialogContext, IRecursivePrevention
    {
        private readonly ServiceContainer ServiceContainer;
        
        public object TargetObject { get; }

        public bool IsEditable { get; }

        public AutoGuiDialog(ICoreClientAPI capi, object target, bool editable = true, bool disposeOnClose = true) : base(capi)
        {
            TargetObject = target ?? throw new ArgumentNullException(nameof(target));
            IsEditable = editable;
            DisposeOnClose = disposeOnClose;

            ServiceContainer = new ServiceContainer(capi.GetServiceContainer());
        }

        private string toggleKeyCombinationCode;

        public override string ToggleKeyCombinationCode => toggleKeyCombinationCode;

        public ElementBounds ParentBounds { get; } = new ElementBounds
        {
            Alignment = EnumDialogArea.None,
            BothSizing = ElementSizing.FitToChildren,
            percentWidth = 1.0,
            percentHeight = 1.0,
            fixedY = GuiStyle.TitleBarHeight,
            fixedPaddingX = GuiStyle.DialogToScreenPadding,
            fixedPaddingY = GuiStyle.DialogToScreenPadding
        };

        public Vec2d Curor { get; } = new Vec2d(0, 0);

        public void Compose(ICoreClientAPI api)
        {
            SingleComposer = capi.Gui
                .CreateCompo("AutoDialog", ElementStdBounds.AutosizedMainDialog)
                .AddShadedDialogBG(ElementBounds.Fill)
                .AddDialogTitleBar(TargetObject.GetType().GetHumanReadableName(), Close)
                .BeginChildElements(ParentBounds)
                .AddAutoComposed(this, null, TargetObject)
                .Compose(false);

            recursionPrevention.Clear();
        }
        public bool DisposeOnClose { get; init; }
        private void Close() => TryClose();

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                if(DisposeOnClose) Dispose();

                return true;
            }
            return false;
        }

        public object GetService(Type serviceType)
        {
            if(serviceType.IsInstanceOfType(this)) return this;
            return ServiceContainer.GetService(serviceType);
        }

        private readonly HashSet<object> recursionPrevention = new();
        public bool EnsureUnique(object obj)
        {
            if(obj == null || !obj.GetType().IsClass) return true;
            return recursionPrevention.Add(obj);
        }
    }
}
