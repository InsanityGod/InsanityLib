using HarmonyLib;
using InsanityLib.Constants;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory;
using Vintagestory.API.Common;

namespace InsanityLib.Attributes.Auto.Config
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AutoConfigAttribute : AutoDefaultValueAttribute
    {
        /// <summary>
        /// The path to the config file. <br />
        /// (relative to the config folder)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether it should load default values if the config file encounters an error while loading. <br />
        /// </summary>
        public bool DefaultOnError { get; init; } = true;

        /// <summary>
        /// Whether the config should be automatically created if it does not exist yet.
        /// </summary>
        public bool CreateIfNotExist { get; init; } = true;

        /// <summary>
        /// Whether the config file should be synced from server to client
        /// </summary>
        public bool ServerSync { get; init; }

        /// <param name="path">Path to the config file. (<see cref="Path"/>)</param>
        public AutoConfigAttribute(string path) => Path = path.EnsureFileExtension("json");
    }
}
