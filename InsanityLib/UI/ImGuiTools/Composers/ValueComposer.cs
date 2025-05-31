using InsanityLib.Interfaces.UI.ImGui;
using InsanityLib.UI.ImGuiTools.Components.Values;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Composers
{
    public class ValueComposer : IImGuiComposer
    {
        private readonly Dictionary<Type, Type> Renderers = new()
        {
            { typeof(string), typeof(StringComponent) },
            { typeof(int), typeof(IntegerComponent) },
            { typeof(bool), typeof(BooleanComponent) },
            { typeof(float), typeof(FloatComponent) },
            { typeof(double), typeof(DoubleComponent) },
            //TODO assetlocation
        };

        public bool CanComposeType(Type type) => Renderers.ContainsKey(type) || type.IsEnum;

        public IImGuiComponent Compose(ImGuiContext context, Type type)
        {
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
            catch
            {
                //TODO logging
                return null;
            }
        }
    }
}
