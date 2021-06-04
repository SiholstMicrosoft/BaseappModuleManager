using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Synchronizer.Models.Processing
{
    public abstract class ProcessingRequest
    {
        public SyntaxKind RootKind { get; }
        public string Prefix { get; }

        protected ProcessingRequest(SyntaxKind rootKind, string prefix)
        {
            RootKind = rootKind;
            Prefix = prefix;
        }
    }
}
