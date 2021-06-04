using Synchronizer.Models.Processing;

namespace Synchronizer.Processing
{
    interface IBmmProcessor
    {
        ProcessingResponse Process(ProcessingRequest request);
    }
}
