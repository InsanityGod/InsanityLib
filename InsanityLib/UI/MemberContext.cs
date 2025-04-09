using InsanityLib.Interfaces.UI;
using InsanityLib.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace InsanityLib.UI
{
    public class MemberContext : IDialogContext
    {
        public IDialogContext Context { get; }
        public MemberContext(IServiceProvider serviceProvider, object targetObject)
        {
            TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            ServiceProvider = serviceProvider;

            var targetType = targetObject.GetType();
            IsEditable = targetType.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly != true;

            Context = serviceProvider.GetService<IDialogContext>();
            if(Context != null)
            {
                IsEditable &= Context.IsEditable;
            }
        }

        public object TargetObject { get; }

        public bool IsEditable { get; }

        public ElementBounds ParentBounds { get; }

        public Vec2d Curor => Context.Curor;

        private readonly IServiceProvider ServiceProvider;
        public object GetService(Type serviceType) => serviceType == typeof(IDialogContext) || serviceType == typeof(MemberContext) ? this : ServiceProvider.GetService(serviceType);



    }
}
