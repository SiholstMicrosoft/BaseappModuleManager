using Microsoft.Dynamics.Nav.CodeAnalysis;

namespace BmmCore.Models
{
    public abstract class ParseResult
    {
        public bool HasALCode { get; }
        public ParseRequest Request { get; }
        public SyntaxKind? RootKind { get; }

        public ParseResult(bool hasALCode, ParseRequest request, SyntaxKind? rootKind)
        {
            HasALCode = hasALCode;
            Request = request;
            RootKind = rootKind;
        }
    }
}
