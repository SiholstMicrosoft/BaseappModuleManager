using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace BmmCore.Models
{
    public class StringParseResult: ParseResult
    {
        public new StringParseRequest Request { get; }

        public string Content { get; }

        public StringParseResult(
            string content,
            bool hasALCode,
            StringParseRequest request,
            SyntaxKind? rootKind) : base(hasALCode, request, rootKind)
        {
            Content = content;
            Request = request;
        }
    }
}
