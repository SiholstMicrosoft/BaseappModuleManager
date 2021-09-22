using BmmCore.Models.Processing;

namespace BmmCore.Processing
{
    interface IBmmExtProcessor
    {
        bool IsExtensionObject(ExtProcessingRequest request);
        bool IsExtensionObjectSupported(ExtProcessingRequest request);
        ExtProcessingResponse Process(ExtProcessingRequest request);
    }
}
