using BmmCore.Models.Options;
using System.Threading.Tasks;

namespace BmmCore.Processing
{
    public interface IBmmTransferer
    {
        public Task Transfer(TransferOptions options);
    }
}
