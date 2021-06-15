using BmmTransferer.Models.Processing;
using System;

namespace BmmTransferer.Processing
{
    public interface IBmmTransferer
    {
        public Progress<int> Transfer(TransferRequest request);
    }
}
