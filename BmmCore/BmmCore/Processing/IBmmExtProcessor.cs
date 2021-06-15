using Synchronizer.Models.Processing;

namespace Synchronizer.Processing
{
    interface IBmmExtProcessor
    {
        bool IsExtensionObject(ExtProcessingRequest request);
        bool IsExtensionObjectSupported(ExtProcessingRequest request);
        ExtProcessingResponse Process(ExtProcessingRequest request);
    }
}
