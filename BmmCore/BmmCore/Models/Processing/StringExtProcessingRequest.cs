using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace BmmCore.Models.Processing
{
    public class StringExtProcessingRequest : ExtProcessingRequest
    {
        public string Content { get; }

        public StringExtProcessingRequest(string content, SyntaxKind rootKind, string prefix) : base(rootKind, prefix)
        {
            Content = content;
        }
    }
}
