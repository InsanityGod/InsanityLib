using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Exceptions
{
    [Serializable]
    public class HarmonyInjectionException : Exception
    {
        public HarmonyInjectionException()
        {
        }

        public HarmonyInjectionException(string message) : base(message)
        {
        }

        public HarmonyInjectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HarmonyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
