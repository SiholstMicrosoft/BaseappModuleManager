using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BmmCore.Models.Domain
{
    public class Field : SyntaxNodeWrapper
    {
        public string Name { get; }
        private readonly SyntaxNode _identifierNode;

        public Field(SyntaxNode node) : base(node)
        {
            _identifierNode = node.DescendantNodesAndSelf().First(x => x.IsKind(SyntaxKind.IdentifierName));
            Name = _identifierNode.GetIdentifierOrLiteralValue();
        }

        protected override List<int> GetPrefixLocations()
        {
            return new List<int>();
        }

        public static List<Field> FromSyntaxNode(SyntaxNode node)
        {
            var descendants = node.DescendantNodesAndSelf();

            var sections = descendants
                .Where(x => x.IsKind(SyntaxKind.FieldExtensionList))
                .Cast<FieldExtensionListSyntax>();

            var fields = sections
                .SelectMany(section => section.Fields.Select(x => new Field(x)))
                .ToList();

            return fields;
        }
    }
}
