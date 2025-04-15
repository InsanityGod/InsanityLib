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
        public string Path { get; set; }

        public bool DefaultOnError { get; init; } = true;

        public bool CreateIfNotExist { get; init; } = true;

        public bool ServerSync { get; init; }

        public AutoConfigAttribute(string path) => Path = path.EnsureFileExtension("json");
    }
}
