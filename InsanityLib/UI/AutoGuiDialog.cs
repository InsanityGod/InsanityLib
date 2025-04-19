using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Interfaces;
using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace InsanityLib.UI
{
    /// <summary>Class for automated Gui generation.</summary>
    public class AutoGuiDialog : GuiDialog, IDialogContext, IRecursivePrevention, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        
        /// <summary>The the object currently being displayed/edited</summary>
        public object TargetObject { get; }

        /// <summary>
        /// Whether editing is allowed.<br/>
        /// </summary>
        public bool IsEditable { get; } = true;

        /// <summary>
        /// The path of the current base path.<br/>
        /// (this expands in deeper contexts)
        /// </summary>
        public string Path => Identifier;

        /// <summary>The identifier of the dialog.</summary>
        public string Identifier { get; init; } = $"AutoGui-{Guid.NewGuid()}";

        /// <summary>Whether the dialog should be disposed when closed.</summary>
        public bool DisposeOnClose { get; set; } = true;
        
        /// <summary>The action to execute when the dialog closed</summary>
        public Action OnClose { get; set; }

        /// <param name="capi">The client api.</param>
        /// <param name="target">The object to display as a Gui.</param>
        /// <exception cref="ArgumentNullException">If target is null</exception>
        public AutoGuiDialog(ICoreClientAPI capi, object target) : base(capi)
        {
            TargetObject = target ?? throw new ArgumentNullException(nameof(target));
            serviceProvider = capi.GetServiceContainer();
        }

        private List<Action> AfterComposeCallbacks { get; } = new();
        public void RegisterAfterComposeCallback(Action action) => AfterComposeCallbacks.Add(action);

        public override string ToggleKeyCombinationCode { get; }

        public Vec2d Cursor { get; } = new Vec2d(0, 0);

        public void Compose()
        {
            var parentBounds = new ElementBounds
            {
                Alignment = EnumDialogArea.None,
                BothSizing = ElementSizing.FitToChildren,
                percentWidth = 1.0,
                percentHeight = 1.0,
                fixedY = GuiStyle.TitleBarHeight,
                fixedPaddingX = GuiStyle.DialogToScreenPadding,
                fixedPaddingY = GuiStyle.DialogToScreenPadding
            };

            SingleComposer = capi.Gui
                .CreateCompo(Identifier, ElementStdBounds.AutosizedMainDialog)
                .AddShadedDialogBG(ElementBounds.Fill)
                .AddDialogTitleBar(TargetObject.GetType().GetHumanReadableName(), Close)
                .BeginChildElements(parentBounds)
                    //.BeginClip(parentBounds)
                        .AddAutoComposed(this, null, TargetObject)
                    //.EndClip();
                .EndChildElements();
             
            //TODO make scrolling work
            //parentBounds.CalcWorldBounds();
            //
            //var maxHeight = ClientSettings.ScreenHeight / 2;
            //var totalHeight = parentBounds.absInnerHeight;
            //if(totalHeight > maxHeight)
            //{
            //    parentBounds.BothSizing = ElementSizing.Fixed;
            //    parentBounds.fixedX = parentBounds.absFixedX;
            //    parentBounds.fixedY = parentBounds.absFixedY;
            //    parentBounds.fixedWidth = parentBounds.absInnerWidth;
            //    parentBounds.fixedHeight = maxHeight;
            //
            //    ElementBounds scrollbarBounds = parentBounds.RightCopy().WithFixedWidth(20);
            //    
            //    SingleComposer.AddVerticalScrollbar(
            //        value =>
            //        {
            //            parentBounds.fixedY = 5 - value;
            //
            //            parentBounds.CalcWorldBounds();
            //            //SingleComposer.ReCompose();
            //        },
            //        scrollbarBounds,
            //        "scrollbar"
            //    );
            //}
            
            SingleComposer.Compose(false);
            
            //SingleComposer.GetScrollbar("scrollbar")?.SetHeights((float)parentBounds.fixedHeight, (float)totalHeight);

            foreach(var callback in AfterComposeCallbacks) callback();

            //Clean temporary state
            AfterComposeCallbacks.Clear();
            recursionPrevention.Clear();
            DocumentationUtil.ClearCache();
            Cursor.X = 0;
            Cursor.Y = 0;
        }

        private void Close() => TryClose();

        public override bool TryOpen(bool withFocus)
        {
            try
            {
                if(SingleComposer == null) Compose();
                return base.TryOpen(withFocus);
            }
            catch(Exception ex)
            {
                capi.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(AutoGuiDialog), TargetObject, ex);
                return false;
            }
        }

        public override bool TryClose()
        {
            if (base.TryClose())
            {
                if(DisposeOnClose) Dispose();
                try
                {
                    OnClose?.Invoke();
                }
                catch (Exception ex)
                {
                    capi.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(OnClose), OnClose, ex);
                }

                return true;
            }
            return false;
        }

        public override bool UnregisterOnClose => true; //This actually makes it unregister in the api
        public override void Dispose()
        {
            base.Dispose();

            var game = capi.World as ClientMain;

            //Clear the gui from the cache (this is a seperate thing for some reason)
            game.GuiComposers.ClearCached(Identifier);
        }

        public object GetService(Type serviceType)
        {
            if(serviceType.IsInstanceOfType(this)) return this;
            return serviceProvider.GetService(serviceType);
        }

        private readonly HashSet<object> recursionPrevention = new();

        public bool EnsureUnique(object obj)
        {
            if(obj == null || !obj.GetType().IsClass) return true;
            return recursionPrevention.Add(obj);
        }
    }
}
