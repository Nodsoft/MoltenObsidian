// See https://aka.ms/new-console-template for more information

using Nodsoft.MoltenObsidian.Tool.Commands.Manifest;
using Spectre.Console.Cli;

CommandApp app = new();
app.Configure(static config =>
{
	config.SetApplicationName("Molten Obsidian - Utility CLI Tool");
	
#if DEBUG
	config.PropagateExceptions();
	config.ValidateExamples();
#endif
	
	config.AddBranch("manifest", static manifestConfig =>
	{
		manifestConfig.SetDescription("Manage manifests");
		manifestConfig.AddCommand<GenerateManifestCommand>("generate");
	});
});

return await app.RunAsync(args);