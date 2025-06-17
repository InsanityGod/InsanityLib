using HarmonyLib;
using ImGuiNET;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Attributes.Auto.Config.UI;
using InsanityLib.Enums.Auto.Config;
using InsanityLib.Enums.Auto.Config.UI;
using InsanityLib.Interfaces.UI.ImGuiComponents;
using InsanityLib.UI.ImGuiTools;
using InsanityLib.Util;
using Newtonsoft.Json;
using System;
using System.Text;
using Vintagestory.API.Common;
using YamlDotNet.Core.Tokens;

namespace InsanityLib.Config.Util
{
    public class AutoConfigLib
    {
        /// <summary>
        /// The actual config instance
        /// </summary>
        public readonly object ConfigInstance;

        /// <summary>
        /// A copy of the config instance, to which we apply edits
        /// </summary>
        [ConfigDisplay(Hierarchy = EHierarchyDisplay.None)]
        public object EditConfigInstance { get; set; } //TODO disposal

        public readonly ICoreAPI Api;

        /// <summary>
        /// Path to the config file
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Wether this config is synced from server
        /// </summary>
        public readonly bool ServerSync;

        public readonly EConfigLibMode ConfigLibMode;

        public AutoConfigLib(ICoreAPI api, object instance, AutoConfigAttribute attr)
        {
            Api = api;
            ConfigInstance = instance;
            Path = attr.Path;
            ServerSync = attr.ServerSync;
            ConfigLibMode = attr.ConfigLibMode;
        }

        public bool Validate()
        {
            //TODO
            return true;
        }

        /// <summary>
        /// Save the actual config
        /// </summary>
        public void Save()
        {

        }

        /// <summary>
        /// Discards all changes
        /// </summary>
        public void Restore()
        {
            //JSON copy object for editing
            string json = JsonConvert.SerializeObject(ConfigInstance, Formatting.None);
            EditConfigInstance = JsonConvert.DeserializeObject(json, ConfigInstance.GetType());
        }

        /// <summary>
        /// Reset everything to it's default values
        /// </summary>
        public void Defaults()
        {
        }

        /// <summary>
        /// Load values from disk / worldconfig
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
            try
            {
                Restore();
                var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfigLib), nameof(EditConfigInstance)), id: Path, serviceProvider: Api.GetServiceContainer());
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
            
            if (Component == null) ReCompose();

            Component?.SafeRender();
            ContextMenuOpen = false;
            
            PostRenderCallback?.Invoke();

            PostRenderCallback = null; //reset after render
            
            if(ContextMenuOwner != null && CurrentContextMenuClaim == null) ImGui.CloseCurrentPopup(); //Close the current popup

            ContextMenuOwner = CurrentContextMenuClaim;
            CurrentContextMenuClaim = null; //reset after render
        }
    }
}
