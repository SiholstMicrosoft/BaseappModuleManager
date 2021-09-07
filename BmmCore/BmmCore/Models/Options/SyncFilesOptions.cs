using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BmmCore.Models.Options
{
    public enum FileSyncEvent
    {
        Create,
        Delete,
        Rename
    }

    [Verb("file-sync", HelpText = "Sync workspace files due to an event")]
    public class SyncFilesOptions : IOptions
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

        public void Validate()
        {
            var invalidPaths = new List<string>();

            switch (EventType)
            {
                case FileSyncEvent.Create:
                    invalidPaths = FilePaths.Where(x => !File.Exists(x)).ToList();
                    break;
                case FileSyncEvent.Rename:
                    if (FilePaths.Count % 2 != 0)
                    {
                        throw new ArgumentException("Rename event requires from/to file path pairs");
                    }
                    for (var i = 1; i < FilePaths.Count; i += 2)
                    {
                        if (!File.Exists(FilePaths[i]))
                        {
                            invalidPaths.Add(FilePaths[i]);
                        }
                    }
                    break;
            }

            if (invalidPaths.Count > 0)
            {
                throw new ArgumentException(
                    $"Invalid file paths provided:{Environment.NewLine}" +
                    $"{string.Join(Environment.NewLine, invalidPaths)}"
                );
            }
        }
    }
}
