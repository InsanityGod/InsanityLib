using InsanityLib.Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsanityLib.UI.ImGuiTools.Helpers
{
    public class ValidationResultHolder : IValidationResultProvider
    {
        public string LastValidationResult { get; set; }
    }
}
