using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Collections.Generic;

namespace Synchronizer.Models.Processing
{
    public class StringProcessingResponse : ProcessingResponse
    {
        public new StringProcessingRequest Request { get; }

        public StringProcessingResponse(
            StringProcessingRequest request,
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
