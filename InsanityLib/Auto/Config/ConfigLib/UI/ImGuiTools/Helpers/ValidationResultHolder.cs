using InsanityLib.Util.Interfaces;

namespace InsanityLib.Auto.Config.ConfigLib.UI.ImGuiTools.Helpers;

public class ValidationResultHolder : IValidationResultProvider
{
    public string LastValidationResult { get; set; }
}
