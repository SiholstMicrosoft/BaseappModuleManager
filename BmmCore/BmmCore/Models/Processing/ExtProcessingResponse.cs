using BmmCore.Models.Domain;
using System.Collections.Generic;

namespace BmmCore.Models.Processing
{
    public abstract class ExtProcessingResponse
    {
        public ExtProcessingRequest Request { get; }
        public IList<Field> ExtensionFields { get; }
        public IList<GlobalVariable> GlobalVariables { get; }
        public IList<Procedure> Procedures { get; }

        protected ExtProcessingResponse(
            ExtProcessingRequest request,
            IList<Field> extensionFields,
            IList<GlobalVariable> globalVariables,
            IList<Procedure> procedures)
        {
            Request = request;
            ExtensionFields = extensionFields;
            GlobalVariables = globalVariables;
            Procedures = procedures;
        }
    }
}
