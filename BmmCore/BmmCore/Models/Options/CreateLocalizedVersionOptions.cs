using BmmCore.Models.Workspace;
using CommandLine;
using System;
using System.IO;

namespace BmmCore.Models.Options
{
    [Verb("new-localized", HelpText = "Create a new localized version of a file")]
    public class CreateLocalizedVersionOptions : IOptions
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

        public void Validate()
        {
            var workspaceFiles = new WorkspaceFiles(WorkspaceDirectory);
            if (!workspaceFiles.IsActivePath(FilePath))
            {
                throw new ArgumentException($"File path {FilePath} is not an active file path");
            }
            if (!File.Exists(FilePath))
            {
                throw new ArgumentException($"{FilePath} is not a valid file path");
            }
            var layerPath = workspaceFiles.GetLayerPath(Target, FilePath, true);
            if (File.Exists(layerPath))
            {
                throw new ArgumentException(
                    $"There is already a {CountryCode} localized version for {FilePath}"
                );
            }
        }
    }
}
