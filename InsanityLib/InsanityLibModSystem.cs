using InsanityLib.Auto.Command;
using InsanityLib.Auto.Config;
using InsanityLib.Documentation;
using InsanityLib.Extended.Enums;
using InsanityLib.Extended.Transitions;
using InsanityLib.Util;
using Newtonsoft.Json;
using System.Text;
using Vintagestory.API.Common;

namespace InsanityLib;

public partial class InsanityLibModSystem : ModSystem
{
    partial void OnTransitionTypeLoaded(ICoreAPI api, AssetLocation origin, TransitionType asset) => CustomTransition.ExtendedEnum.RegisterTransitionType(Mod.Logger, asset);

    public override void StartPre(ICoreAPI api)
    {
        RegisterApiServices(api);
        ReflectionUtil.LoadedSides |= api.Side;
        ExtendedEnum.EnumExtensions[typeof(EnumTransitionType)] = new ExtendedTransition();
        AutoSetup(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        AutoAssetsLoaded(api);
    }

    public void EnsureConfigLoaded(ICoreAPI api) => LoadAutoConfigs(api);

    public override void Start(ICoreAPI api)
    {
        AutoCommandAttribute.FindAndRegisterAutoCommands(api);

        // Clear documentation cache build up by auto registration code
        AssemblyDocumentationContext.ClearCache();
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        if(api.Side != EnumAppSide.Server) return;
        var configTree = api.World.Config.GetOrAddTreeAttribute("insanitylib_configs");

        foreach((var path, var config) in AutoConfig.Loaded)
        {
            configTree.SetBytes(path,  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(config.ConfigInstance, Formatting.None)));
        }
    }

    public override void Dispose()
    {
        AutoDispose();
        if(ServiceContainer is null || _api is null) return;
        
        ReflectionUtil.LoadedSides &= ~_api.Side;
    }
}