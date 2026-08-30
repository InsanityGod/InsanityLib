using HarmonyLib;
using InsanityLib.Auto.Command;
using InsanityLib.Auto.Config;
using InsanityLib.Documentation;
using InsanityLib.Extended.Enums;
using InsanityLib.Extended.Traits;
using InsanityLib.Extended.Traits.Compatibility.XLib;
using InsanityLib.Extended.Traits.Interfaces;
using InsanityLib.Extended.Transitions;
using InsanityLib.Extensions;
using InsanityLib.Util;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.API.Common;

[assembly: IgnoresAccessChecksTo("VSEssentials")]
namespace InsanityLib;

//TODO maybe a feature system (so specific features can fail "safely")
//TODO maybe add some utility for sending packets with callback logic

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

    partial void ManualPatches(Harmony harmony, ICoreAPI api)
    {
        if(AccessTools.Method("DynamicClassesModSystem.DynamicClassesModSystem:LoadOrCreateTraitsConfig") is not null)
        {
            TryPatchCategory(harmony, "feature:extendedtraits_dynamictraits", Mod.Logger);
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        AutoAssetsLoaded(api);
        if(AccessTools.TypeByName("XLib.XLeveling.XLeveling") is not null)
        {
            RegisterXLibCompat(api);
        }
    }

    private void RegisterXLibCompat(ICoreAPI api)
    {
        var xlibTraitInterface = new XLibTraitInterface(api);
        xlibTraitInterface.TryRegisterTraits(ExtendedTraits.Values);
        TraitSystems.Add(xlibTraitInterface);
    }

    public void EnsureConfigLoaded(ICoreAPI api) => LoadAutoConfigs(api);

    public override void Start(ICoreAPI api)
    {
        AutoCommandAttribute.FindAndRegisterAutoCommands(api);

        TraitSystems.Add(new VanillaTraitInterface(api));
        ServiceContainer!.AddService<ITraitSystemInterface>(this);

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