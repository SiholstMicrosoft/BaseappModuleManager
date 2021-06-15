using CommandLine;

namespace BmmCore.Models.Options
{
    [Verb("init", HelpText = "Set the active country code for the workspace.")]
    public class InitializeOptions
    {
        [Value(0, Required = true, HelpText = "Directory path of workspace.")]
        public string WorkspaceDirectory { get; set; }

        [Option('c', "countrycode", Required = true, HelpText = "The active country code (e.g. W1)")]
        public string CountryCode { get; set; }
    }
}
