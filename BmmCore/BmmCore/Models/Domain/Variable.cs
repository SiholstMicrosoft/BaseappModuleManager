using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BmmCore.Models.Domain
{
    public class Variable : SyntaxNodeWrapper
    {
        public string Name { get; }
        private readonly SyntaxNode _identifierNode;

        public Variable(SyntaxNode node) : base(node)
        {
            _identifierNode = node.DescendantNodesAndSelf().First(x => x.IsKind(SyntaxKind.IdentifierName));
            Name = _identifierNode.GetIdentifierOrLiteralValue();
        }

        protected override List<int> GetPrefixLocations()
        {
            return new List<int> { _identifierNode.Span.Start - SyntaxNode.FullSpan.Start };
        }

        public static List<Variable> FromSyntaxNode(SyntaxNode node)
        {
            var descendants = node.DescendantNodesAndSelf();

            var sections = descendants
                .Where(x => x.IsKind(SyntaxKind.VarSection))
                .Cast<VarSectionSyntax>();

            var variables = sections
                .SelectMany(section => section.Variables.Select(x => new Variable(x)))
                .ToList();

            return variables;
        }
    }
}
