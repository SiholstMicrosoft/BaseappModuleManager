using BmmCore.Definitions;
using BmmCore.Models.Options;
using BmmCore.Models.Workspace;
using BmmCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BmmCore.Workspace
{
    public class Workspace : IWorkspace
    {
        public Task Initialize(InitializeOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);

            // Get rid of linked AL-files.
            var linkedFiles = workspaceFiles.GetLinkedSourceFiles();
            foreach (var file in linkedFiles)
            {
                File.Delete(file);
            }

            // Move AL-files to correct layer files.
            var activeFiles = workspaceFiles.GetActiveSourceFiles();
            foreach (var file in activeFiles)
            {
                MoveFileToLayerFolder(file, options.CountryCode, workspaceFiles);
            }

            // Rename files.
            var layerFiles = workspaceFiles.GetLayerSourceFiles();
            foreach (var file in layerFiles)
            {
                File.Move(file, file + WorkspaceDefinitions.IgnoreExt);
            }

            // Link files from target country code.
            layerFiles = workspaceFiles.GetLayerSourceFiles(options.CountryCode, true);
            foreach (var file in layerFiles)
            {
                CreateSymbolicLink(file, workspaceFiles);
            }

            // Link files from default country code.
            layerFiles = workspaceFiles.GetLayerSourceFiles(WorkspaceDefinitions.DefaultCountryCode, true);
            foreach (var file in layerFiles)
            {
                CreateSymbolicLink(file, workspaceFiles);
            }

            return Task.CompletedTask;
        }

        public Task SyncFiles(SyncFilesOptions options)
        {
            ValidateOptions(options);

            return options.EventType switch
            {
                FileSyncEvent.Create => OnCreateFiles(options),
                FileSyncEvent.Delete => OnDeleteFiles(options),
                FileSyncEvent.Rename => OnRenameFiles(options),
                _ => throw new ArgumentException($"Unexpected file sync event {options.EventType}"),
            };
        }

        public Task CreateLocalizedVersion(CreateLocalizedVersionOptions options)
        {
            ValidateOptions(options);
            
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            var layerPath = workspaceFiles.GetLayerPath(options.Target, options.FilePath, true);

            var layerDir = Path.GetDirectoryName(layerPath);
            if (!Directory.Exists(layerDir))
            {
                Directory.CreateDirectory(layerDir);
            }
            File.Copy(options.FilePath, layerPath);

            // Link to localized version if target is current active version.
            if(options.CountryCode == options.Target)
            {
                File.Delete(options.FilePath);
                CreateSymbolicLink(layerPath, workspaceFiles);
            }

            return Task.CompletedTask;
        }

        private static Task OnCreateFiles(SyncFilesOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            foreach(var file in options.FilePaths)
            {
                var newPath = MoveFileToLayerFolder(file, options.CountryCode, workspaceFiles);
                CreateSymbolicLink(newPath, workspaceFiles);
            }
            return Task.CompletedTask;
        }

        private static Task OnDeleteFiles(SyncFilesOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            foreach (var file in options.FilePaths)
            {
                var path = workspaceFiles.GetLayerPath(options.CountryCode, file, true);
                if(!File.Exists(path))
                {
                    path = workspaceFiles.GetLayerPath(WorkspaceDefinitions.DefaultCountryCode, file, true);
                }
                File.Delete(path);

                // Re-link file if default country code is existing.
                path = workspaceFiles.GetLayerPath(WorkspaceDefinitions.DefaultCountryCode, file, true);
                if(File.Exists(path))
                {
                    CreateSymbolicLink(path, workspaceFiles);
                }
            }
            return Task.CompletedTask;
        }

        private static Task OnRenameFiles(SyncFilesOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            var fromPaths = options.FilePaths.Where((_, index) => index % 2 == 0);
            var toPaths = options.FilePaths.Where((_, index) => index % 2 == 1);
            
            foreach(var (from, to) in fromPaths.Zip(toPaths))
            {
                if (!workspaceFiles.IsSymbolicLink(to))
                {
                    continue;
                }
                var pathFrom = from.Replace(options.WorkspaceDirectory, "") + WorkspaceDefinitions.IgnoreExt;
                var pathTo = to.Replace(options.WorkspaceDirectory, "") + WorkspaceDefinitions.IgnoreExt;

                // Find matching files and rename them.
                var files = workspaceFiles.GetLayerSourceFiles(withIgnoreExt: true).Where(x => x.EndsWith(pathFrom));
                foreach(var file in files)
                {
                    File.Move(file, file.Replace(pathFrom, pathTo));
                }

                // Re-link file.
                File.Delete(to);
                var layerPath = workspaceFiles.GetLayerPath(options.CountryCode, to, true);
                if(!File.Exists(layerPath))
                {
                    layerPath = workspaceFiles.GetLayerPath(WorkspaceDefinitions.DefaultCountryCode, to, true);
                }
                CreateSymbolicLink(layerPath, workspaceFiles);
            }
            return Task.CompletedTask;
        }

        private static void CreateSymbolicLink(string source, WorkspaceFiles workspaceFiles)
        {
            var target = workspaceFiles.GetActivePath(source, true);

            if (File.Exists(target))
            {
                if(workspaceFiles.IsSymbolicLink(target))
                {
                    return;
                }
                throw new Exception(
                    $"Failed creating symbolic file link from '{source}' to '{target}'. " +
                    "Existing non-symbolic file is present."
                );
            }
            if (!SymbolicLinker.Create(source, target, true))
            {
                throw new Exception($"Failed creating symbolic file link from '{source}' to '{target}'");
            }
        }

        private static string MoveFileToLayerFolder(string file, string countryCode, WorkspaceFiles workspaceFiles)
        {
            var layerPath = workspaceFiles.GetLayerPath(WorkspaceDefinitions.DefaultCountryCode, file, true);
            if (File.Exists(layerPath) && countryCode != WorkspaceDefinitions.DefaultCountryCode)
            {
                layerPath = workspaceFiles.GetLayerPath(countryCode, file, true);
            }
            if (File.Exists(layerPath))
            {
                throw new Exception($"Cannot move file {file} into layer destination {layerPath}. File already exists.");
            }
            File.Move(file, layerPath);
            return layerPath;
        }

        private static void ValidateOptions(SyncFilesOptions options)
        {
            var invalidPaths = new List<string>();    

            switch(options.EventType)
            {
                case FileSyncEvent.Create:
                    invalidPaths = options.FilePaths.Where(x => !File.Exists(x)).ToList();
                    break;
                case FileSyncEvent.Rename:
                    if(options.FilePaths.Count % 2 != 0)
                    {
                        throw new ArgumentException("Rename event requires from/to file path pairs");
                    }
                    for(var i = 1; i < options.FilePaths.Count; i += 2)
                    {
                        if(!File.Exists(options.FilePaths[i]))
                        {
                            invalidPaths.Add(options.FilePaths[i]);
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

        private static void ValidateOptions(CreateLocalizedVersionOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            if (!workspaceFiles.IsActivePath(options.FilePath))
            {
                throw new ArgumentException($"File path {options.FilePath} is not an active file path");
            }
            if(!File.Exists(options.FilePath))
            {
                throw new ArgumentException($"{options.FilePath} is not a valid file path");
            }
            var layerPath = workspaceFiles.GetLayerPath(options.Target, options.FilePath, true);
            if(File.Exists(layerPath))
            {
                throw new ArgumentException(
                    $"There is already a {options.CountryCode} localized version for {options.FilePath}"
                );
            }
        }
    }
}
