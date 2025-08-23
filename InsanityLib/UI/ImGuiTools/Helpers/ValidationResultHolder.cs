using InsanityLib.Interfaces.UI;

namespace InsanityLib.UI.ImGuiTools.Helpers;

public class ValidationResultHolder : IValidationResultProvider
{
    public string LastValidationResult { get; set; }
}
