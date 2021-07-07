using BmmCore.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BmmCore.Models.Workspace
{
    class WorkspaceFiles
    {
        private string _workspacePath;
        private string _layerPath;

        public WorkspaceFiles(string workspaceDir)
        {
            _workspacePath = workspaceDir;
            _layerPath = Path.Join(_workspacePath, WorkspaceDefinitions.LayerFolder);
        }

        public IList<string> GetLinkedSourceFiles()
        {
            var files = GetAllSourceFiles();
            return files.Where(IsSymbolicLink).ToList();
        }

        public IList<string> GetActiveSourceFiles()
        {
            var files = GetAllSourceFiles();
            var activeFiles = new List<string>();

            foreach(var file in files)
            {
                var directory = Path.GetDirectoryName(file);
                if(!directory.StartsWith(_layerPath))
                {
                    activeFiles.Add(file);
                }
            }

            return activeFiles;
        }

        public IList<string> GetLayerSourceFiles(string countryCode = null, bool withIgnoreExt = false)
        {
            var layerDirectory = countryCode != null 
                ? Path.Join(_workspacePath, WorkspaceDefinitions.LayerFolder, countryCode)
                : Path.Join(_workspacePath, WorkspaceDefinitions.LayerFolder);

            var searchPattern = withIgnoreExt
                    ? $"*{WorkspaceDefinitions.AlExt}{WorkspaceDefinitions.IgnoreExt}"
                    : $"*{WorkspaceDefinitions.AlExt}";

            return Directory.GetFiles(layerDirectory, searchPattern, SearchOption.AllDirectories);
        }

        public bool IsLayerPath(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            return directory.StartsWith(_layerPath);
        }

        public bool IsActivePath(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            return !directory.StartsWith(_layerPath);
        }

        public string GetLayerPath(string countryCode, string filePath, bool includeIgnoreExt = false)
        {
            var layerFolder = Path.Join(WorkspaceDefinitions.LayerFolder, countryCode);
            var directory = Path.GetDirectoryName(filePath).Replace(_workspacePath, "");
            if(directory.StartsWith(layerFolder))
            {
                return filePath;
            }
            var layerPath = Path.Join(_workspacePath, layerFolder, GetRelativePath(filePath));

            if(includeIgnoreExt)
            {
                layerPath = layerPath + WorkspaceDefinitions.IgnoreExt;
            }

            return layerPath;
        }

        public string GetActivePath(string filePath, bool removeIgnoreExt = false)
        {
            if(!IsLayerPath(filePath))
            {
                throw new ArgumentException($"Path {filePath} is not a layer path.");
            }
            
            var relativePath = GetRelativePath(filePath).Replace(WorkspaceDefinitions.LayerFolder, "");
            relativePath = Regex.Replace(relativePath, @"\\+[^\\]+\\", "");

            if(removeIgnoreExt)
            {
                relativePath = RemoveIgnoreExt(relativePath);
            }

            return Path.Join(_workspacePath, relativePath);
        }

        public bool IsSymbolicLink(string path)
        {
            FileInfo pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        private string GetRelativePath(string filePath)
        {
            return filePath.Replace(_workspacePath, "");
        }

        private IList<string> GetAllSourceFiles()
        {
            return Directory.GetFiles(_workspacePath,$"*{WorkspaceDefinitions.AlExt}", SearchOption.AllDirectories);
        }

        private static string RemoveIgnoreExt(string path)
        {
            if (path.EndsWith(WorkspaceDefinitions.IgnoreExt))
            {
                var index = path.LastIndexOf(WorkspaceDefinitions.IgnoreExt);
                path = path.Substring(0, index);
            }
            return path;
        }
    }
}
