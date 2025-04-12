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

        public bool DefaultOnError { get; set; }

        public bool CreateIfNotExist { get; set; }

        public AutoConfigAttribute(string path, bool defaultOnError = true, bool createIfNotExist = true)
        {
            Path = path.EnsureFileExtension("json");
            DefaultOnError = defaultOnError;
            CreateIfNotExist = createIfNotExist;
        }
    }
}
