using InsanityLib.Auto.Config;
using InsanityLib.Auto.Config.IMM;
using Vintagestory.API.Common;

namespace InsanityLib;

public class AssetGenerationSystem : ModSystem
{
    // Right in between compatibility folder logic (0.04) and json patch logic (0.05)
    public override double ExecuteOrder() => -0.2;

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);

        if (api.ModLoader.IsModEnabled("integratedmodmanager"))
        {
            IMMConfigGenerator.GenerateAll(api);
        }
    }
}
