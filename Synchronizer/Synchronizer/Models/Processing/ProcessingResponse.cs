using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Collections.Generic;

namespace Synchronizer.Models.Processing
{
    public abstract class ProcessingResponse
    {
        public ProcessingRequest Request { get; }
        public IList<SyntaxNode> ExtensionFields { get; }
        public IList<SyntaxNode> GlobalVariables { get; }
        public IList<SyntaxNode> Procedures { get; }

        protected ProcessingResponse(
            ProcessingRequest request,
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
