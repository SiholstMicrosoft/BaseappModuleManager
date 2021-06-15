namespace Synchronizer.Models
{
    public class StringParseRequest : ParseRequest
    {
        public string Content { get; }


        public StringParseRequest(string content, bool includeComments) : base(includeComments)
        {
            Content = content;
        }
    }
}
