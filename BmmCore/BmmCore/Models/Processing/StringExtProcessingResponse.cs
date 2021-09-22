using BmmCore.Models.Domain;
using System.Collections.Generic;

namespace BmmCore.Models.Processing
{
    public class StringExtProcessingResponse : ExtProcessingResponse
    {
        public new StringExtProcessingRequest Request { get; }

        public StringExtProcessingResponse(
            StringExtProcessingRequest request,
            IList<Field> extensionFields,
            IList<GlobalVariable> globalVariables,
            IList<Procedure> procedures
        ) 
            : base(request, extensionFields, globalVariables, procedures)
        {
            Request = request;
        }
    }
}
