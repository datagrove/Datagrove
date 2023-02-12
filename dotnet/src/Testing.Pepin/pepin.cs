using System.CommandLine;
using Datagrove.Testing.Pepinillo;

class Program
{
    public static int Main(string[] args)
    {
        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "log verbosely");

        var projectArgument = new Argument<string>(
            name: "project",
            description: "source project");
        projectArgument.Arity = ArgumentArity.ZeroOrOne;

        var rootCommand = new RootCommand("Pepin compiler for Cucumber language");
        //rootCommand.AddOption(fileOption);

        var buildCommand = new Command("build", "Compile cucumber into c#")
            {
                verboseOption,
            };
        buildCommand.Add(projectArgument);
        rootCommand.Add(buildCommand);

        buildCommand.SetHandler(async (project, verbose) =>
            {
                await Build(project, verbose);
            },
            projectArgument,
            verboseOption);

        return rootCommand.InvokeAsync(args).Result;
    }

    internal static async Task Build(string? project, bool verbose)
    {
        project ??= Directory.GetCurrentDirectory();
        await Pepin.build(project);
    }
}
