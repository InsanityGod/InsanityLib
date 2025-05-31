using InsanityLib.Attributes.Auto.Config;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;

namespace InsanityLib.Config
{
    public class InsanityLibConfig
    {
        [AutoConfig("InsanityLibConfig.json", ServerSync = false, CreateIfNotExist = true, DefaultOnError = true)] 
        public static InsanityLibConfig Instance { get; set; }
        
        public AutoConfigConfig AutoConfig { get; set; } = new AutoConfigConfig();

        [Range(0f, float.PositiveInfinity)]
        public float Multiplier { get; set; } = 5f;

        public EnumTransitionType EnumTransitionType { get; set; } = EnumTransitionType.Burn;

        public EnumItemStorageFlags EnumItemStorageFlags { get; set; } = EnumItemStorageFlags.Alchemy;
    }
}
