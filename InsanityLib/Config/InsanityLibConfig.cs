using InsanityLib.Attributes.Auto.Config;
using InsanityLib.Enums.Auto.Commands;
using InsanityLib.Enums.Auto.Config;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        [AutoConfig("InsanityLibConfig.json", ServerSync = false, CreateIfNotExist = true, DefaultOnError = true)] 
        public static InsanityLibConfig Instance { get; set; }
         
        public AutoConfigConfig AutoConfig { get; set; } = new AutoConfigConfig();

    }
}
