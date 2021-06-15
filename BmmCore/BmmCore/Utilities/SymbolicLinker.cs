using System.Runtime.InteropServices;

namespace BmmCore.Utilities
{
    public static class SymbolicLinker
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public static bool Create(string source, string target, bool file)
        {
            var type = file? SymbolicLink.File: SymbolicLink.Directory;
            return CreateSymbolicLink(target, source, type);
        }
    }
}
