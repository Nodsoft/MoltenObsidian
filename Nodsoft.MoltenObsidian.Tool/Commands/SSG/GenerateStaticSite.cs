using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Nodsoft.MoltenObsidian.Tool.Commands.SSG;

[PublicAPI]
public sealed class GenerateStaticSiteCommandSettings : CommandSettings
{
    [CommandOption("-f|--from-folder <PATH_TO_VAULT>"), Description("Path to the local moltenobsidian vault")]
    public string FilePathString { get; private set; } = string.Empty;

    public DirectoryInfo? FilePath { get; private set; }

    [CommandOption("-u|--from-url <PATH_TO_MANIFEST>"), Description("Full url to the manifest file for remote vault")]
    public string UrlString { get; private set; } = string.Empty;
    
    public Uri? UrlPath { get; private set; }

    [CommandOption("-o|--output-path <OUTPUT_PATH>"), Description("Directory to write the output files to")]
    public string OutputPathString { get; private set; } = string.Empty;

    public FileInfo? OutputPath { get; private set; }

    public override ValidationResult Validate()
    {
        if (FilePathString is "" && (FilePath = new(FilePathString)) is { Exists: false })
        {
            return ValidationResult.Error($"The vault path {FilePathString} does not exist");
        }
        
        if (OutputPathString is not (null or "") && (OutputPath = new(OutputPathString)) is { Exists: false })
        {
            return ValidationResult.Error($"The output path '{OutputPath}' does not exist.");
        }

        if (UrlString is not "")
        {
            if (!Uri.IsWellFormedUriString(UrlString, UriKind.Absolute))
            {
                return ValidationResult.Error($"The url {UrlString} is not valid or is not an absolute path");
            }
            if (!UrlString.EndsWith("moltenobsidian.manifest.json"))
            {
                return ValidationResult.Error($"The Url must end in moltenobsidian.manifest.json");
            }
            UrlPath = new Uri(UrlString);
        }
        return ValidationResult.Success();
    }
}

[UsedImplicitly]
public sealed class GenerateStaticSite: AsyncCommand<GenerateStaticSiteCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateStaticSiteCommandSettings settings)
    {
        Console.WriteLine(settings);
        return await Task.Factory.StartNew(() => 2);
    }
}