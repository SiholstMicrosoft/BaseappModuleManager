using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Synchronizer.Models.Processing;
using SynchronizerTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synchronizer.Processing
{
    public class BmmProcessorV1 : IBmmProcessor
    {
        public ProcessingResponse Process(ProcessingRequest request)
        {
            var content = GetContent(request);
            var tree = SyntaxFactory.ParseSyntaxTree(content.Original);
            var root = tree.GetCompilationUnitRoot();

            var extensionFields = GetExtensionFields(root);
            var globalVariables = GetGlobalVariables(root);
            var procedures = GetProcedures(root);

            return new StringProcessingResponse(
                request as StringProcessingRequest,
                extensionFields,
                PrefixContent(request.Prefix, globalVariables, globalVariables, procedures),
                PrefixContent(request.Prefix, procedures, globalVariables, procedures)
            );
        }

        private static List<SyntaxNode> GetExtensionFields(SyntaxNode root)
        {
            var nodes = root.DescendantNodes(node =>
                node.Kind == SyntaxKind.CompilationUnit || node.Kind.IsApplicationObject());

            var fieldExtensionList = nodes
                .FirstOrDefault(x => x.Kind == SyntaxKind.FieldExtensionList) as FieldExtensionListSyntax;

            if(fieldExtensionList == null)
            {
                return new List<SyntaxNode>();
            }

            return fieldExtensionList.Fields.Select(x => x as SyntaxNode).ToList();
        }

        private static List<SyntaxNode> GetGlobalVariables(SyntaxNode root)
        {
            var nodes = root.DescendantNodes(node =>
                node.Kind == SyntaxKind.CompilationUnit || node.Kind.IsApplicationObject());

            var globalVarSection = nodes
                .FirstOrDefault(x => x.Kind == SyntaxKind.GlobalVarSection) as GlobalVarSectionSyntax;

            if(globalVarSection == null)
            {
                return new List<SyntaxNode>();
            }

            return globalVarSection.Variables.Select(x => x as SyntaxNode).ToList();
        }

        private static List<SyntaxNode> GetProcedures(SyntaxNode root)
        {
            var nodes = root.DescendantNodes(node =>
                node.Kind == SyntaxKind.CompilationUnit || node.Kind.IsApplicationObject());

            var procedures = nodes.Where(x => x.Kind == SyntaxKind.MethodDeclaration).ToList();

            return procedures;
        }

        private static StringContentManager GetContent(ProcessingRequest request)
        {
            if (request is StringProcessingRequest strRequest)
            {
                return new StringContentManager(strRequest.Content);
            }
            throw new ArgumentException($"Unsupported extension processing request type: ${request.GetType().Name}");
        }

        private static IList<SyntaxNode> PrefixContent(
            string prefix,
            List<SyntaxNode> nodes,
            List<SyntaxNode> globalVariables,
            List<SyntaxNode> procedures
            )
        {
            var globalVariableNames = GetVariableNames(globalVariables);
            var procedureNames = GetProcedureNames(procedures);
            var list = new List<SyntaxNode>();

            foreach(var node in nodes)
            {
                var localVariables = GetLocalVariableNames(node);
                var identifierNodes = node.DescendantNodes(_ => true).Where(x => x.Kind == SyntaxKind.IdentifierName);

                var content = new StringContentManager(node.ToFullString());

                foreach (var identifierNode in identifierNodes)
                {
                    switch(identifierNode.Parent.Kind)
                    {
                        case SyntaxKind.MethodDeclaration:
                        case SyntaxKind.InvocationExpression:
                            {
                                var name = identifierNode.GetIdentifierOrLiteralValue();
                                if (procedureNames.Contains(name))
                                {
                                    var pos = identifierNode.Span.Start - node.FullSpan.Start;
                                    content.InsertText(pos, prefix);
                                }
                            }
                            break;

                        default:
                            {
                                var name = identifierNode.GetIdentifierOrLiteralValue();
                                if (!localVariables.Contains(name) && globalVariableNames.Contains(name))
                                {
                                    var pos = identifierNode.Span.Start - node.FullSpan.Start;
                                    content.InsertText(pos, prefix);
                                }
                            }
                            break;
                    }
                }
                list.Add(SyntaxFactory.ParseSyntaxTree(content.Current).GetRoot());
            }
            return list;
        }

        private static HashSet<string> GetLocalVariableNames(SyntaxNode node)
        {
            var varSections = node
                    .DescendantNodes(_ => true)
                    .Where(x => x.Kind == SyntaxKind.VarSection);

            var names = new HashSet<string>();

            foreach(var varSection in varSections)
            {
                var varNodes = varSection.DescendantNodes(_ => true).Where(x => x.Kind == SyntaxKind.IdentifierName);
                names.UnionWith(varNodes.Select(x => x.GetIdentifierOrLiteralValue()));
            }
            return names;
        }

        private static HashSet<string> GetVariableNames(IList<SyntaxNode> nodes)
        {
            var names = new HashSet<string>();

            foreach (var node in nodes)
            {
                var variableDeclarations = node
                    .DescendantNodesAndSelf(_ => true).Where(x => x.Kind == SyntaxKind.VariableDeclaration);

                foreach (var variableDeclaration in variableDeclarations)
                {
                    var identifier = variableDeclaration
                        .DescendantNodes(node => node.Kind == SyntaxKind.VariableDeclaration)
                        .First(x => x.Kind == SyntaxKind.IdentifierName);
                    names.Add(identifier.GetIdentifierOrLiteralValue());
                }
            }
            return names;
        }

        private static HashSet<string> GetProcedureNames(IList<SyntaxNode> nodes)
        {
            var names = new HashSet<string>();

            foreach(var node in nodes)
            {
                var methodDeclarations = node
                    .DescendantNodesAndSelf(_ => true).Where(x => x.Kind == SyntaxKind.MethodDeclaration);

                foreach(var methodDeclaration in methodDeclarations)
                {
                    var identifier = methodDeclaration
                        .DescendantNodes(node => node.Kind == SyntaxKind.MethodDeclaration)
                        .First(x => x.Kind == SyntaxKind.IdentifierName);
                    names.Add(identifier.GetIdentifierOrLiteralValue());
                }
            }
            return names;
        }
    }
}
