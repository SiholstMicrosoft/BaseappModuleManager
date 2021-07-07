using CommandLine;

namespace BmmCore.Models.Options
{
    [Verb("new-localized", HelpText = "Create a new localized version of a file")]
    public class CreateLocalizedVersionOptions
    {
        [Value(0, Required = true, HelpText = "Directory path of workspace.")]
        public string WorkspaceDirectory { get; set; }

        [Option('c', "countrycode", Required = true, HelpText = "The active country code (e.g. W1)")]
        public string CountryCode { get; set; }

        [Option('t', "target", 
            Required = true, 
            HelpText = "The target country code to create the localized version for (e.g. W1)"
        )]
        public string Target { get; set; }

        [Option('f', "file", Required = true, HelpText = "The file path of the target file to localize.")]
        public string FilePath { get; set; }
    }
}
