using InsanityLib.Constants;
using InsanityLib.UI.ImGuiTools.Components.Values;
using InsanityLib.UI.ImGuiTools.Interfaces;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace InsanityLib.UI.ImGuiTools.Composers;

public class ValueComposer : IImGuiComposer
{
    private readonly Dictionary<Type, Type> Renderers = new()
    {
        { typeof(string), typeof(StringComponent) },
        { typeof(int), typeof(IntegerComponent) },
        { typeof(bool), typeof(BooleanComponent) },
        { typeof(float), typeof(FloatComponent) },
        { typeof(double), typeof(DoubleComponent) },
        { typeof(AssetLocation), typeof(AssetLocationComponent) },
    };

    public bool CanComposeType(Type type) => Renderers.ContainsKey(type) || type.IsEnum || (Nullable.GetUnderlyingType(type) is Type underLyingType && CanComposeType(underLyingType));

    public IImGuiComponent Compose(ImGuiContext context, Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        Type componentType = null;
        
        if (type.IsEnum)
        {
            componentType = typeof(EnumComponent);
        }
        
        componentType ??= Renderers[type];

        try
        {
            return componentType.AutoCreate(context) as IImGuiComponent;
        }
        catch(Exception ex)
        {
            context.GetService<ICoreAPI>().Logger.Error(Logging.ComposeFailure, type, ex);
            return null;
        }
    }
}
