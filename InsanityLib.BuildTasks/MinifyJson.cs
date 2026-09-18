
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsanityLib.BuildTasks;

public sealed class MinifyJson : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] Files { get; set; } = [];

    public override bool Execute()
    {
        foreach (var file in Files)
        {
            var path = file.GetMetadata("FullPath");

            try
            {
                Minify(path);

                Log.LogMessage(MessageImportance.Low, "Minified JSON: {0}", path);
            }
            catch (Exception ex)
            {
                Log.LogError("Failed to minify JSON '{0}': 1}", path, ex);
            }
        }

        return !Log.HasLoggedErrors;
    }

    private static void Minify(string path)
    {
        var json = File.ReadAllText(path);

        var token = JToken.Parse(json);

        File.WriteAllText(path,token.ToString(Formatting.None));
    }
}