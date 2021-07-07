using BmmCore.Models.Options;
using System.Threading.Tasks;

namespace BmmCore.Workspace
{
    interface IWorkspace
    {
        Task Initialize(InitializeOptions options);
        Task SyncFiles(SyncFilesOptions options);
        Task CreateLocalizedVersion(CreateLocalizedVersionOptions options);
    }
}
