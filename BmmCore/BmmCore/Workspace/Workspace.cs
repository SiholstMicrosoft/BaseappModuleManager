using BmmCore.Models.Options;
using BmmCore.Utilities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BmmCore.Workspace
{
    public class Workspace : IWorkspace
    {
        private const string _layerFolder = "layers";
        private const string _alExt = ".al";
        private const string _ignoreExt = ".ignore";
        private const string _defaultFolder = "W1";

        public Workspace()
        {

        }

        public Task Initialize(InitializeOptions options)
        {
            var workspacePath = Path.GetFullPath(options.WorkspaceDirectory);
            if(!Path.EndsInDirectorySeparator(workspacePath))
            {
                workspacePath += Path.DirectorySeparatorChar;
            }
            var files = Directory.GetFiles(workspacePath, $"*{_alExt}", SearchOption.AllDirectories);

            // Get rid of existing linked AL-files.
            var layerPath = Path.Join(workspacePath, _layerFolder);
            foreach (var file in files.Where(x => !x.StartsWith(layerPath)))
            {
                File.Delete(file);
            }

            // Rename files.
            foreach (var file in files.Where(x => x.StartsWith(layerPath)))
            {
                File.Move(file, file + _ignoreExt);
            }

            files = Directory.GetFiles(workspacePath, $"*{_alExt}{_ignoreExt}", SearchOption.AllDirectories);

            var targetLayerPath = Path.Join(workspacePath, _layerFolder, options.CountryCode);
            var defaultLayerPath = Path.Join(workspacePath, _layerFolder, _defaultFolder);

            // Link files from target country code.
            foreach (var file in files.Where(x => x.StartsWith(targetLayerPath)))
            {
                var layerDirectoryPart = Path.Join(_layerFolder, options.CountryCode) + Path.DirectorySeparatorChar;
                var targetFileName = Path.GetFileName(file).Replace(_ignoreExt, "");
                var targetFolder = Path.GetDirectoryName(file).Replace(layerDirectoryPart, "");
                var target = Path.Join(targetFolder, targetFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                SymbolicLinker.Create(file, target, true);
            }

            // Link files from default country code.
            foreach (var file in files.Where(x => x.StartsWith(defaultLayerPath)))
            {
                var layerDirectoryPart = Path.Join(_layerFolder, _defaultFolder) + Path.DirectorySeparatorChar;
                var targetFileName = Path.GetFileName(file).Replace(_ignoreExt, "");
                var targetFolder = Path.GetDirectoryName(file).Replace(layerDirectoryPart, "");
                var target = Path.Join(targetFolder, targetFileName);
                if(!File.Exists(target))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target));
                    SymbolicLinker.Create(file, target, true);
                }
            }

            return Task.CompletedTask;
        }
    }
}
