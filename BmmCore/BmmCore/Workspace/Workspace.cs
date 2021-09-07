using BmmCore.Definitions;
using BmmCore.Models.Options;
using BmmCore.Models.Workspace;
using BmmCore.Utilities;
using System;
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
            return options.EventType switch
            {
                FileSyncEvent.Create => EventCreateFiles(options),
                FileSyncEvent.Delete => EventDeleteFiles(options),
                FileSyncEvent.Rename => EventRenameFiles(options),
                _ => throw new ArgumentException($"Unexpected file sync event {options.EventType}"),
            };
        }

        public Task CreateLocalizedVersion(CreateLocalizedVersionOptions options)
        {
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

        private static Task EventCreateFiles(SyncFilesOptions options)
        {
            var workspaceFiles = new WorkspaceFiles(options.WorkspaceDirectory);
            foreach(var file in options.FilePaths)
            {
                var newPath = MoveFileToLayerFolder(file, options.CountryCode, workspaceFiles);
                CreateSymbolicLink(newPath, workspaceFiles);
            }
            return Task.CompletedTask;
        }

        private static Task EventDeleteFiles(SyncFilesOptions options)
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

        private static Task EventRenameFiles(SyncFilesOptions options)
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
    }
}
