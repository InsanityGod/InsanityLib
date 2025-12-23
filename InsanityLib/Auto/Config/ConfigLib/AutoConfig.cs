using HarmonyLib;
using ImGuiNET;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Components.Util;
using InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Util;
using InsanityLib.Util.AutoRegistry;
using Newtonsoft.Json;
using System;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace InsanityLib.Auto.Config.ConfigLib;

public class AutoConfig(ICoreAPI api, object instance, AutoConfigAttribute attr)
{
    /// <summary>
    /// The actual config instance
    /// </summary>
    public readonly object ConfigInstance = instance;

    /// <summary>
    /// A copy of the config instance, to which we apply edits
    /// </summary>
    [ConfigDisplay(Hierarchy = EHierarchyDisplay.None)]
    public object EditConfigInstance { get; set; } //TODO disposal?

    public readonly ICoreAPI Api = api;

    /// <summary>
    /// Path to the config file
    /// </summary>
    public readonly string Path = attr.Path;

    /// <summary>
    /// Wether this config is synced from server
    /// </summary>
    public readonly bool ServerSync = attr.ServerSync;

    public bool Validate(out string result)
    {
        var validationResult = EditConfigInstance.TryNestedValidate(Api.GetServiceContainer());
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
            var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfig), nameof(Save)), id: "ValidationPopup", serviceProvider: Api.GetServiceContainer());
            AutoConfigUtil.BlockingPopup = new Popup(context)
            {
                Title = "Validation Failed",
                AcceptLabel = "Save Anyway##ValidationPopup-accept",
                AcceptCallback = SaveInternal,
                RejectLabel = "Cancel##ValidationPopup-cancel",
                Text = validationResult.ReplaceSpecialSymbolsWithText(),
            };
        }
    }

    private void SaveInternal()
    {
        try
        {
            AutoConfigUtil.StoreModConfig(Api, EditConfigInstance, Path);
        }
        catch
        {
            //TODO open error popup
        }
    }

    /// <summary>
    /// Discards all changes
    /// </summary>
    public void Restore(bool loadFromDisk = false)
    {
        string json = null;
        if (loadFromDisk)
        {
            string path = System.IO.Path.Combine(GamePaths.ModConfig, Path);
			    if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else Api.Logger.Warning("[InsanityLib] could not restore {0} from disk as it no longer exists, defaulting to loaded config", Path);
        }

        //Defaulting to loaded config
        json ??= JsonConvert.SerializeObject(ConfigInstance, Formatting.None); //JSON copy object for editing
        
        EditConfigInstance = JsonConvert.DeserializeObject(json, ConfigInstance.GetType());
        ReCompose();
    }

    /// <summary>
    /// Reset everything to it's default values
    /// </summary>
    public void Defaults()
    {
        var newInstance = ConfigInstance.GetType().AutoCreate(Api.GetServiceContainer());
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
            var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfig), nameof(EditConfigInstance)), id: Path, serviceProvider: Api.GetServiceContainer());
            Component = ImGuiComposer.TryCompose(context, ConfigInstance.GetType());
            ComposeError = null;
        }
        catch(Exception ex)
        {
            ComposeError = ex.ToString();
        }
    }

    public static object CurrentContextMenuClaim { get; set; } = null;
    public static object ContextMenuOwner { get; set; } = null;

    public static bool ContextMenuOpen { get; set; }
    public string ComposeError { get; private set; }
    
    private IImGuiComponent Component; //TODO disposal

    public static Action PostRenderCallback { get; set; } = null;

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

    public static void Cleanup()
    {
        foreach(var config in AutoConfigUtil.LoadedConfigs.Values)
        {
            config.EditConfigInstance = null;
            config.Component = null;
        }
    }
}
