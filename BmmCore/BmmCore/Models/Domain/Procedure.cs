using BmmCore.Extensions;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BmmCore.Models.Domain
{
    public class Procedure : SyntaxNodeWrapper
    {
        public string Name { get; }
        public SyntaxToken AccessModifier { get; }
        private readonly SyntaxNode _identifierNode;

        public Procedure(MethodDeclarationSyntax node) : base(node)
        {
            AccessModifier = node.AccessModifier;
            _identifierNode = node.DescendantNodes().First(x => x.IsKind(SyntaxKind.IdentifierName));
            Name = _identifierNode.GetIdentifierOrLiteralValue();
        }

        protected override List<int> GetPrefixLocations()
        {
            var root = SyntaxNode.Parent;
            while(root.Parent != null)
            {
                root = root.Parent;
            }

            var globalVariables = GlobalVariable.FromSyntaxNode(root);            
            var localVariables = Variable.FromSyntaxNode(SyntaxNode);
            var localProcedures = FromSyntaxNode(root).Where(x => x.AccessModifier.IsKind(SyntaxKind.LocalKeyword));

            var globalVariablesLookup = new HashSet<string>(globalVariables.Select(x => x.Name));
            var localVariablesLookup = new HashSet<string>(localVariables.Select(x => x.Name));
            var localProceduresLookup = new HashSet<string>(localProcedures.Select(x => x.Name));
            var locations = new List<int>();

            if (AccessModifier.IsKind(SyntaxKind.LocalKeyword))
            {
                locations.Add(_identifierNode.Span.Start - SyntaxNode.FullSpan.Start);
            }

            var identifiers = SyntaxNode.DescendantNodes()
                .Where(x => x.IsKind(SyntaxKind.IdentifierName))
                .Select(x => new Variable(x));

            foreach (var id in identifiers)
            {
                // Prefix local method references.
                if(localProceduresLookup.Contains(id.Name) && id.SyntaxNode.IsMethodReference())
                {
                    locations.Add(id.SyntaxNode.Span.Start - SyntaxNode.FullSpan.Start);
                }
                // Check for unambigious global variable references.
                else if(globalVariablesLookup.Contains(id.Name) && id.SyntaxNode.IsVariableReference())
                {
                    // Check scope (we can't handle ambiguity issues requiring parent object).
                    var scope = id.SyntaxNode.GetScope();

                    // If there is no scope and no overriding variable, we know it is a global variable reference.
                    if(scope == CodeScope.None && !localVariablesLookup.Contains(id.Name))
                    {
                        locations.Add(id.SyntaxNode.Span.Start - SyntaxNode.FullSpan.Start);
                    }
                }
            }
            return locations;
        }

        public static List<Procedure> FromSyntaxNode(SyntaxNode node)
        {
            var descendants = node.DescendantNodesAndSelf();

            var declarations = descendants
                .Where(x => x.IsKind(SyntaxKind.MethodDeclaration))
                .Cast<MethodDeclarationSyntax>();

            var procedures = declarations.Select(x => new Procedure(x)).ToList();
            return procedures;
        }
    }
}
