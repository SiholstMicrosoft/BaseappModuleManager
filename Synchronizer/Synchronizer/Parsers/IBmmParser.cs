using Synchronizer.Models;

namespace Synchronizer.Parsers
{
    interface IBmmParser
    {
        ParseResult Parse(ParseRequest request);
    }
}
