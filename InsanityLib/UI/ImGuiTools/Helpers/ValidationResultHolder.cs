using InsanityLib.Util.Interfaces;

namespace InsanityLib.UI.ImGuiTools.Helpers;

public class ValidationResultHolder : IValidationResultProvider
{
    public string LastValidationResult { get; set; }
}
