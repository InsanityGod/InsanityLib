using InsanityLib.Auto.Command;
using InsanityLib.Documentation;
using InsanityLib.Extended.Enums;
using InsanityLib.Extended.Transitions;
using InsanityLib.Util;
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

        // These should be loaded by now
        api.World.Config.RemoveAttribute("insanitylib_configs");
    }

    public override void Dispose()
    {
        AutoDispose();
        if(ServiceContainer is null || _api is null) return;
        
        ReflectionUtil.LoadedSides &= ~_api.Side;
    }
}