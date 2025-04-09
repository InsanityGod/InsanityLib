using HarmonyLib;
using InsanityLib.Attributes.Auto;
using InsanityLib.Config;
using InsanityLib.Constants;
using InsanityLib.Interfaces.UI;
using InsanityLib.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace InsanityLib.Util.AutoRegistry
{
    public static class AutoUI
    {
        [AutoDefaultValue]
        internal static IAutoGuiComposer[] Composers;
        
        internal static void CollectAutoGuiComposers(this IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            Composers ??= AccessTools.AllTypes()
                .Where(type => typeof(IAutoGuiComposer).IsAssignableFrom(type))
                .Select(type =>
                {
                    var result = type.AutoCreate(provider, true);
                    if (result == null) logger?.Warning($"[InsanityLib] Failed to create AutoUI composer instance of '{type}'");
                    return result;
                })
                .OfType<IAutoGuiComposer>() //TODO check if this also filters out null values
                .ToArray();
        }

        public static IAutoGuiComposer FindAutoGuiComposer(this Type type)
        {
            var reflectionMatch = (IAutoGuiComposer)typeof(IAutoGuiComposer<>).MakeGenericType(type).FindMatch(Composers);
            if (reflectionMatch != null) return reflectionMatch;
            return Array.Find(Composers, composer => composer.IsValidForCompose(type));
        }

        public static GuiComposer AddAutoComposed(this GuiComposer composer, IServiceProvider provider, MemberInfo member, object value)
        {
            value.GetType()
                .FindAutoGuiComposer()
                ?.ComposeObject(composer, provider, member, value);

            return composer;
        }

        public static bool OpenAutoGui(this ICoreClientAPI api, object obj, bool editable = true, bool disposeOnClose = true)
        {
            try
            {
                var dialog = new AutoGuiDialog(api, obj, editable, disposeOnClose);
                dialog.Compose(api);
                return dialog.TryOpen();
            }
            catch(Exception ex)
            {
                api.GetService<ILogger>()?.Error(Logging.ExecutionFailedTemplate, nameof(OpenAutoGui), obj, ex);
                return false;
            }
        }
    }
}
