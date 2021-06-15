using BmmCore.Models.Options;
using System.Threading.Tasks;

namespace BmmCore.Workspace
{
    interface IWorkspace
    {
        Task Initialize(InitializeOptions options);
    }
}
