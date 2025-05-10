using HarmonyLib;
using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Enums.Auto.Config;
using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools;
using InsanityLib.UI.ImGuiTools.Composers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;

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
            
        }

        /// <summary>
        /// Compose the IImGuiComponent
        /// </summary>
        public void ReCompose()
        {
            Restore();
            var context = new ImGuiContext(this, AccessTools.Property(typeof(AutoConfigLib), nameof(EditConfigInstance)), id: Path);
            Component = ImGuiComposer.TryCompose(context, ConfigInstance.GetType());
        }

        private IImGuiComponent Component; //TODO disposal
        
        /// <summary>
        /// Render the config
        /// </summary>
        public void Render()
        {
            if(Component == null)
            {
                ReCompose();
            }
            else
            {
                Component.SafeRender();
            }
        }
    }
}
