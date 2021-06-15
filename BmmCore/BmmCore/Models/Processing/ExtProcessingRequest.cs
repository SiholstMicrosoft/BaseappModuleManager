using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Synchronizer.Models.Processing
{
    public abstract class ExtProcessingRequest
    {
        public SyntaxKind RootKind { get; }
        public string Prefix { get; }

        protected ExtProcessingRequest(SyntaxKind rootKind, string prefix)
        {
            RootKind = rootKind;
            Prefix = prefix;
        }
    }
}
