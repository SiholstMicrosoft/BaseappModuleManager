using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BmmCore.Models.Domain
{
    public class GlobalVariable : Variable
    {
        public SyntaxToken AccessModifier { get; }

        public GlobalVariable(GlobalVarSectionSyntax varSection, SyntaxNode node) : base(node)
        {
            AccessModifier = varSection.AccessModifier;
        }

        public static new List<GlobalVariable> FromSyntaxNode(SyntaxNode node)
        {
            var descendants = node.DescendantNodesAndSelf();

            var sections = descendants
                .Where(x => x.IsKind(SyntaxKind.GlobalVarSection))
                .Cast<GlobalVarSectionSyntax>();

            var variables = sections
                .SelectMany(section => section.Variables.Select(x => new GlobalVariable(section, x)))
                .ToList();

            return variables;
        }
    }
}
