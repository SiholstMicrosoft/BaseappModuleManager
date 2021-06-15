using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Collections.Generic;

namespace Synchronizer.Models.Processing
{
    public abstract class ExtProcessingResponse
    {
        public ExtProcessingRequest Request { get; }
        public IList<SyntaxNode> ExtensionFields { get; }
        public IList<SyntaxNode> GlobalVariables { get; }
        public IList<SyntaxNode> Procedures { get; }

        protected ExtProcessingResponse(
            ExtProcessingRequest request,
            IList<SyntaxNode> extensionFields,
            IList<SyntaxNode> globalVariables,
            IList<SyntaxNode> procedures)
        {
            Request = request;
            ExtensionFields = extensionFields;
            GlobalVariables = globalVariables;
            Procedures = procedures;
        }
    }
}
