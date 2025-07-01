using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Exceptions
{
    [Serializable]
    public class InvalidAttributeUsageException : Exception
    {
        public InvalidAttributeUsageException()
        {
        }

        public InvalidAttributeUsageException(string message) : base(message)
        {
        }

        public InvalidAttributeUsageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidAttributeUsageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
