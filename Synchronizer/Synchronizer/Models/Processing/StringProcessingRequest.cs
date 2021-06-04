using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace Synchronizer.Models.Processing
{
    public class StringProcessingRequest : ProcessingRequest
    {
        public string Content { get; }

        public StringProcessingRequest(string content, SyntaxKind rootKind, string prefix) : base(rootKind, prefix)
        {
            Content = content;
        }
    }
}
