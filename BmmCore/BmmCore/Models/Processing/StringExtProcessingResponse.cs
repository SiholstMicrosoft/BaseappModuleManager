using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Collections.Generic;

namespace Synchronizer.Models.Processing
{
    public class StringExtProcessingResponse : ExtProcessingResponse
    {
        public new StringExtProcessingRequest Request { get; }

        public StringExtProcessingResponse(
            StringExtProcessingRequest request,
            IList<SyntaxNode> extensionFields,
            IList<SyntaxNode> globalVariables,
            IList<SyntaxNode> procedures
        ) 
            : base(request, extensionFields, globalVariables, procedures)
        {
            Request = request;
        }
    }
}
