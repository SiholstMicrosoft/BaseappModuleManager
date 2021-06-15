using BmmTransferer.Models.Processing;
using System;

namespace BmmTransferer.Processing
{
    public class BmmTransferer : IBmmTransferer
    {
        public Progress<int> Transfer(TransferRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
