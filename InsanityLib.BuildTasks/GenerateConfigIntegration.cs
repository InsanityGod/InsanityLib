using InsanityLib.BuildTasks.IMM;
using Microsoft.Build.Framework;
using System.Text.Json;

namespace InsanityLib.BuildTasks;


public class ConfigTranslations
{
    public IMMConfig IMM { get; set; } = new();

}

public sealed class GenerateConfigIntegration : Microsoft.Build.Utilities.Task
{
    [Required]
    public string AssemblyPath { get; set; } = null!;

    [Required]
    public string OutputPath { get; set; } = null!;

    [Required]
    public string ModID { get; set; } = null!;

    public override bool Execute()
    {
        try
        {
            return GenerateJSON();
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }

    public bool GenerateJSON()
    {
        Log.LogMessage(MessageImportance.Low, "Generating config files at: {0}", OutputPath);

        var configs = new Dictionary<string, ConfigTranslations>();

        AnalyzeProject(AssemblyPath, configs);


        foreach(var config in configs)
        {
            Log.LogMessage(MessageImportance.Low, "Found config: {0}", config.Key);

            //TODO other config serializations
        }

        if(configs.Count > 0)
        {
            var IMM = new IMMConfigColection
            {
                Configuration = [.. configs.Values.Select(config => config.IMM)]
            };

            var jsonPath = Path.Combine(OutputPath, "assets", ModID, "config", "imm.json");

            var directory = Path.GetDirectoryName(jsonPath)!;
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(IMM, new JsonSerializerOptions
            {
                    WriteIndented = true
            });

            File.WriteAllText(jsonPath, json);

            Log.LogMessage(MessageImportance.High, "Generated IMM config file: {0}", jsonPath);
        }
        
        return true;
    }

    private void AnalyzeProject(string projectPath, Dictionary<string, ConfigTranslations> configs)
    {

    }
}