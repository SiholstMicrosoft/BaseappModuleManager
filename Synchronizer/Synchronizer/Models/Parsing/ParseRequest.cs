namespace Synchronizer.Models
{
    public abstract class ParseRequest
    {
        public bool IncludeComments { get; }

        protected ParseRequest(bool includeComments)
        {
            IncludeComments = includeComments;
        }
    }
}
