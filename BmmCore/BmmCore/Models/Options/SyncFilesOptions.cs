using CommandLine;
using System.Collections.Generic;

namespace BmmCore.Models.Options
{
    public enum FileSyncEvent
    {
        Create,
        Delete,
        Rename
    }

    [Verb("file-sync", HelpText = "Sync workspace files due to an event")]
    public class SyncFilesOptions
    {
        [Value(0, Required = true, HelpText = "Directory path of workspace.")]
        public string WorkspaceDirectory { get; set; }

        [Option('c', "countrycode", Required = true, HelpText = "The active country code (e.g. W1)")]
        public string CountryCode { get; set; }

        [Option('f', "files", 
            Required = true, 
            HelpText = "File paths to sync (for rename include from/to file path pairs)"
        )]
        public IList<string> FilePaths { get; set; }
        
        [Option('e', "event", Required = true, HelpText = "Type of event (Create/Delete/Rename)")]
        public FileSyncEvent EventType { get; set; }
    }
}
