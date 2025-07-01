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
        private static IAutoGuiComposer[] Composers;
        
        internal static void CollectAutoGuiComposers(this IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            Composers ??= AccessTools.AllTypes()
                .Where(type =>  !type.IsInterface && !type.IsAbstract && typeof(IAutoGuiComposer).IsAssignableFrom(type))
                .Select(type =>
                {
                    var result = type.AutoCreate(provider, true);
                    if (result is null) logger?.Warning($"[InsanityLib] Failed to create AutoUI composer instance of '{type}'");
                    return result;
                })
                .OfType<IAutoGuiComposer>()
                .ToArray();
        }

        public static IAutoGuiComposer FindAutoGuiComposer(this Type type)
        {
            var reflectionMatch = typeof(IAutoGuiComposer<>).MakeGenericType(type).FindMatch(Composers, composer => composer.IsValidForCompose(type));
            if (reflectionMatch is not null) return reflectionMatch;
            return Array.Find(Composers, composer => composer.IsValidForCompose(type));
        }

        public static GuiComposer AddAutoComposed(this GuiComposer composer, IServiceProvider provider, MemberInfo member, object value)
        {
            value.GetType()
                .FindAutoGuiComposer()
                ?.ComposeObject(composer, provider, member, value);

            return composer;
        }
    }
}
