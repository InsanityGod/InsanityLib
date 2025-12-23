using InsanityLib.Auto.Config.ConfigLib.UI.Interfaces;
using InsanityLib.Constants;
using InsanityLib.Documentation;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using InsanityLib.Util.Interfaces;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace InsanityLib.Auto.Config.ConfigLib.UI;

/// <summary>Class for automated Gui generation.</summary>
/// <param name="capi">The client api.</param>
/// <param name="target">The object to display as a Gui.</param>
/// <exception cref="ArgumentNullException">If target is null</exception>
public class AutoGuiDialog(ICoreClientAPI capi, object target) : GuiDialog(capi), IDialogContext, IRecursivePrevention, IDisposable
{
    private readonly IServiceProvider serviceProvider = capi.GetServiceContainer();

    /// <summary>The the object currently being displayed/edited</summary>
    public object TargetObject { get; } = target ?? throw new ArgumentNullException(nameof(target));

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

    private List<Action> AfterComposeCallbacks { get; } = [];
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
                .AddAutoComposed(this, null, TargetObject)
            .EndChildElements();
         
        //TODO scrolling
        
        SingleComposer.Compose(false);

        foreach(var callback in AfterComposeCallbacks) callback();

        //Clean temporary state
        AfterComposeCallbacks.Clear();
        recursionPrevention.Clear();
        AssemblyDocumentationContext.ClearCache();
        Cursor.X = 0;
        Cursor.Y = 0;
    }

    private void Close() => TryClose();

    public override bool TryOpen(bool withFocus)
    {
        try
        {
            if(SingleComposer is null) Compose();
            return base.TryOpen(withFocus);
        }
        catch(Exception ex)
        {
            capi.Logger.Error(Logging.ExecutionFailedTemplate, nameof(AutoGuiDialog), TargetObject, ex);
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
                capi.Logger.Error(Logging.ExecutionFailedTemplate, nameof(OnClose), OnClose, ex);
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

    private readonly HashSet<object> recursionPrevention = [];

    public bool EnsureUnique(object obj)
    {
        if(obj is null || !obj.GetType().IsClass) return true;
        return recursionPrevention.Add(obj);
    }
}
