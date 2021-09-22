using BmmCore.Models;

namespace BmmCore.Parsers
{
    interface IBmmParser
    {
        ParseResult Parse(ParseRequest request);
    }
}
