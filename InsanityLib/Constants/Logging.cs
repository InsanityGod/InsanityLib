namespace InsanityLib.Constants;

public static class Logging
{
    public const string ExecutionFailed = "[InsanityLib] failed executing '{0}' on '{1}': {2}";
    public const string ExternalExecutionFailed = "[InsanityLib] [{0}] failed executing '{1}' on '{2}': {3}";
    
    public const string AutoDefaultFailed = "[InsanityLib] failed setting auto default value on static member {0}:\n{1}";
    
    public const string DefaultExecutionFailed = "[InsanityLib] failed executing '{0}' on '{1}', using default value instead: {2}";
    public const string ConfigOutdated = "[InsanityLib] Config '{0}' is outdated/invalid ('{1}' should have been '{2}' instead of '{3}')";
    public const string AutoConfigUpdate = "[InsanityLib] Config '{0}' is outdated/invalid ('{1}' should have been '{2}' instead of '{3}') attempting auto update (you should check the config afterwards)";

    public const string EncounteredValidationError = "[InsanityLib] Encountered validation error while validating '{0}', error: '{1}' at path '{2}'";
    public const string AutoFixFailed = "[InsanityLib] Could not auto fix validation error while validating '{0}', error: '{1}' at path '{2}'";
    public const string AutoFixSucceed = "[InsanityLib] Auto fixed validation error while validating '{0}', error: '{1}' at path '{2}', new value: '{3}'";

    public const string PatchPathResolverFailed = "[InsanityLib] Patch {0} in {1} failed because path {2} could not be resolved: {3}";
    public const string PatchUnmentCondition = "[InsanityLib] Patch {0} in {1}: Unmet IsValue condition for '{2}' ({3}!={4})";
    
    public const string PathResolverFailed = "[InsanityLib] [{0}] Resolved: '{1}', Unresolved: '{2}', Reason: {3}";

    public const string ModRequirementNotMet = "[InsanityLib] Mod requirement not met for using [{0}], Missing mod: '{1}'";
    public const string ModRequirementNotMetDefaulting = "[InsanityLib] Mod requirement not met for using [{0}] defaulting to [{1}], Missing mod: '{2}'";

    public const string ComposeFailure = "[InsanityLib] failed to compose {0}: {1}";

    public const string AutoCommandSetupFailed = "[InsanityLib] [{0}] Failed to register command for {1}: {2}";

    public const string InvalidAttributeUsage = "[InsanityLib] [{0}] Encountered invalid usage of '{1}' on '{2}': {3}";
}
