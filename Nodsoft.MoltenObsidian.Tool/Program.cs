// See https://aka.ms/new-console-template for more information

using Nodsoft.MoltenObsidian.Tool.Commands.Manifest;
using Nodsoft.MoltenObsidian.Tool.Commands.SSG;
using Spectre.Console.Cli;

CommandApp app = new();

app.Configure(static config =>
{
	config.SetApplicationName("moltenobsidian");

#if DEBUG
	config.PropagateExceptions();
	config.ValidateExamples();
#endif
	
	config.AddBranch("manifest", static manifestConfig =>
	{
		manifestConfig.SetDescription("Manage manifests");
		manifestConfig.AddCommand<GenerateManifestCommand>("generate");
	});
	
	config.AddBranch("ssg", static generateConfig =>
	{
		generateConfig.SetDescription("Export Vaults");
		generateConfig.AddCommand<GenerateStaticSite>("generate");
	});
});

return await app.RunAsync(args);