using ConfigLib;
using HarmonyLib;
using ImGuiNET;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Extensions;
using InsanityLib.Generators.Attributes;
using InsanityLib.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Auto.Config.ConfigLib;

public class AutoConfigLib(ICoreAPI api, IAutoConfig autoConfig)
{
    /// <summary>
    /// The actual config instance
    /// </summary>
    public readonly IAutoConfig Config = autoConfig;

    /// <summary>
    /// A copy of the config instance, to which we apply edits
    /// </summary>
    [ConfigDisplay(Hierarchy = EHierarchyDisplay.None)]
    public object? EditConfigInstance { get; set; }

    public readonly ICoreAPI Api = api;

    public bool Validate(out string result)
    {
        if(EditConfigInstance is null)
        {
            result = "No config instance to validate";
            return false;
        }

        var validationResult = EditConfigInstance.TryNestedValidate(Api.GetServiceProvider());
        //TODO Collect unsaved changes (due to keys not being added to dictionary yet)
        if (!validationResult.IsValid)
        {
            result = string.Join(Environment.NewLine, validationResult.Results);
            return false;
        }
        else
        {
            result = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Save the actual config
    /// </summary>
    public void Save()
    {
        //TODO permissions for editing on servers

        if (Validate(out var validationResult)) SaveInternal();
        else
        {
            var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfigLib), nameof(Save)), id: "ValidationPopup", serviceProvider: Api.GetServiceProvider());
            BlockingPopup = new Popup(context)
            {
                Title = "Validation Failed",
                AcceptLabel = "Save Anyway##ValidationPopup-accept",
                AcceptCallback = SaveInternal,
                RejectLabel = "Cancel##ValidationPopup-cancel",
                Text = validationResult.ReplaceSpecialSymbolsWithText(),
            };
        }
    }

    private void SaveInternal() => AutoConfig.TrySaveConfig(EditConfigInstance, Config.AssociatedType, Config.RelativePath, Api, Api.Logger);

    /// <summary>
    /// Discards all changes
    /// </summary>
    public void Restore(bool loadFromDisk = false)
    {
        string? json = null;
        if (loadFromDisk)
        {
            string path = Path.Combine(GamePaths.ModConfig, Config.RelativePath);
			if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else Api.Logger.Warning("[InsanityLib] could not restore {0} from disk as it no longer exists, defaulting to loaded config", Config.RelativePath);
        }

        //Defaulting to loaded config
        json ??= JsonConvert.SerializeObject(Config.ConfigInstance, Formatting.None); //JSON copy object for editing
        
        EditConfigInstance = JsonConvert.DeserializeObject(json, Config.AssociatedType);
        ReCompose();
    }

    /// <summary>
    /// Reset everything to it's default values
    /// </summary>
    public void Defaults()
    {
        var newInstance = Config.AssociatedType.AutoCreate(Api.GetServiceProvider());
        if(newInstance is not null) EditConfigInstance = newInstance;
        ReCompose();
    }

    /// <summary>
    /// TODO Allow for hooking live update code (right now it just recomposes the UI)
    /// </summary>
    public void Reload()
    {
        ReCompose();
    }

    /// <summary>
    /// Compose the IImGuiComponent
    /// </summary>
    public void ReCompose()
    {
        if(EditConfigInstance is null) return; //Nothing to compose
        try
        {
            var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfigLib), nameof(EditConfigInstance)), id: Config.RelativePath, serviceProvider: Api.GetServiceProvider());
            Component = ImGuiComposer.TryCompose(context, Config.AssociatedType);
            ComposeError = null;
        }
        catch(Exception ex)
        {
            ComposeError = ex.ToString();
        }
    }

    public static object? CurrentContextMenuClaim { get; set; } = null;
    public static object? ContextMenuOwner { get; set; } = null;

    public static bool ContextMenuOpen { get; set; }
    public string? ComposeError { get; private set; }
    
    private IImGuiComponent? Component; //TODO disposal

    public static Action? PostRenderCallback { get; set; } = null;

    /// <summary>
    /// Render the config
    /// </summary>
    public void Render()
    {
        if(!string.IsNullOrEmpty(ComposeError))
        {
            ImGui.Text(ComposeError);
            return;
        }
        
        if (Component is null)
        {
            if(EditConfigInstance is null) Restore();
            ReCompose();
        }

        Component?.SafeRender();
        ContextMenuOpen = false;
        
        PostRenderCallback?.Invoke();

        PostRenderCallback = null; //reset after render
        
        if(ContextMenuOwner is not null && CurrentContextMenuClaim is null) ImGui.CloseCurrentPopup(); //Close the current popup

        ContextMenuOwner = CurrentContextMenuClaim;
        CurrentContextMenuClaim = null; //reset after render
    }

    [AutoClear]
    internal static Dictionary<string, AutoConfigLib> ConfigLibEntries { get; } = [];
    
    internal static void Cleanup()
    {
        foreach(var config in ConfigLibEntries.Values)
        {
            config.EditConfigInstance = null;
            config.Component = null;
        }
    }

    //TODO call this!
    internal static void RegisterConfigLibEvents(ICoreAPI api)
    {
        if(api.Side == EnumAppSide.Server) return;

        var configLib = api.ModLoader.GetModSystem<ConfigLibModSystem>();
        configLib.ConfigWindowClosed += Cleanup;
    }

    public static Popup? BlockingPopup { get; set; }

    public static void NotifyUserOfException(Exception ex, IImGuiComponent component)
    {
        var clientApi = InsanityLibModSystem.GlobalServiceContainer.GetService<ICoreClientAPI>()!;
        var context = new ImGuiContext(component, null, id: "ErrorPopup", serviceProvider: clientApi.GetServiceProvider());
        BlockingPopup = new Popup(context)
        {
            Title = ex.Message,
            Text = ex.ToString(),
            AcceptLabel = "Continue##ErrorPopup-Continue",
            RejectLabel = "Copy Error And Continue##ErrorPopup-Copy-Continue",
            RejectCallback = () => clientApi.Forms.SetClipboardText(ex.ToString())
        };
    }

    internal void RegisterToConfigLib(ICoreAPI api) => api.ModLoader.GetModSystem<ConfigLibModSystem>().RegisterCustomConfig(Config.RelativePath, (domain, buttons) =>
    {
        var serverConfigOnClient = !ReflectionUtil.SideLoaded(EnumAppSide.Server) && Config.ServerSync;

        ImGui.BeginDisabled(serverConfigOnClient);
        if (serverConfigOnClient)
        {
            ImGui.Text("Client side editing of server config is not supported yet");
            ImGui.NewLine();
        }

        if (buttons.Save) Save();
        if (buttons.Restore) Restore(true);

        //TODO discard changes method
        if (buttons.Defaults) Defaults();
        if (buttons.Reload) Reload();

        Render();
        if (BlockingPopup is not null)
        {
            BlockingPopup.SafeRender();
            if (!BlockingPopup.IsOpen) BlockingPopup = null;
        }

        ImGui.EndDisabled();
        return new ControlButtons
        {
            Save = !serverConfigOnClient, //Only server can save for now
            Restore = !serverConfigOnClient,
            Defaults = !serverConfigOnClient,
            Reload = false
        };
    });
}
