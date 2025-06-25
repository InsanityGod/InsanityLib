using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.Interfaces.UI
{
    public interface IValidationResultProvider
    {
        string LastValidationResult { get; }
    }
}
