using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Synchronizer.Models.Processing;
using SynchronizerTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synchronizer.Processing
{
    public class BmmExtProcessor : IBmmExtProcessor
    {
        private static readonly HashSet<SyntaxKind> _extensionObjectKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.TableExtensionObject,
            SyntaxKind.PageExtensionObject,
            SyntaxKind.ReportExtensionObject,
        };
        private static readonly HashSet<SyntaxKind> _supportedExtensionObjectKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.TableExtensionObject
        };

        private enum ProcedureType
        {
            Local,
            Public
        }

        public bool IsExtensionObject(ExtProcessingRequest request)
        {
            var nodes = GetApplicationObjectNodes(request);
            return nodes.Any(x => _extensionObjectKinds.Contains(x.Kind));
        }

        public bool IsExtensionObjectSupported(ExtProcessingRequest request)
        {
            var nodes = GetApplicationObjectNodes(request);
            return nodes.Any(x => _supportedExtensionObjectKinds.Contains(x.Kind));
        }

        public ExtProcessingResponse Process(ExtProcessingRequest request)
        {
            var root = GetCompilationUnitNode(request);
            var extensionFields = GetExtensionFields(root);
            var globalVariables = GetGlobalVariables(root);
            var procedures = GetProcedures(root);

            return new StringExtProcessingResponse(
                request as StringExtProcessingRequest,
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

        private static IList<SyntaxNode> PrefixContent(
            string prefix,
            List<SyntaxNode> nodes,
            List<SyntaxNode> globalVariables,
            List<SyntaxNode> procedures
            )
        {
            var globalVariableNames = GetVariableNames(globalVariables);
            var localProcedureNames = GetProcedureNames(procedures, ProcedureType.Local);
            var publicProcedureNames = GetProcedureNames(procedures, ProcedureType.Public);
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
                        // Method type identifier reference.
                        case SyntaxKind.MethodDeclaration:
                        case SyntaxKind.InvocationExpression:
                            {
                                var name = identifierNode.GetIdentifierOrLiteralValue();

                                // Only prefix local procedures as they are hidden from external code.
                                if (localProcedureNames.Contains(name))
                                {
                                    var pos = identifierNode.Span.Start - node.FullSpan.Start;
                                    content.InsertText(pos, prefix);
                                }
                            }
                            break;

                        // Otherwise, it is a normal identifier (variable) reference.
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

        private static HashSet<string> GetProcedureNames(IList<SyntaxNode> nodes, ProcedureType type)
        {
            var names = new HashSet<string>();

            foreach(var node in nodes)
            {
                var methodDeclarations = node
                    .DescendantNodesAndSelf(_ => true).Where(x => x.Kind == SyntaxKind.MethodDeclaration);

                foreach(var methodDeclaration in methodDeclarations)
                {
                    var decendantNodes = methodDeclaration.DescendantNodes(x => x.Kind == SyntaxKind.MethodDeclaration);
                    var tokens = methodDeclaration.DescendantTokens(x => x.Kind == SyntaxKind.MethodDeclaration);

                    switch (type)
                    {
                        case ProcedureType.Local:
                            if (tokens.All(x => x.Kind != SyntaxKind.LocalKeyword))
                            {
                                continue;
                            }
                            break;
                        case ProcedureType.Public:
                            if (tokens.Any(x => x.Kind == SyntaxKind.LocalKeyword))
                            {                             
                                continue;
                            }
                            break;
                    }
                    var identifier = decendantNodes.First(x => x.Kind == SyntaxKind.IdentifierName);
                    names.Add(identifier.GetIdentifierOrLiteralValue());
                }
            }
            return names;
        }

        private static StringContentManager GetContent(ExtProcessingRequest request)
        {
            if (request is StringExtProcessingRequest strRequest)
            {
                return new StringContentManager(strRequest.Content);
            }
            throw new ArgumentException($"Unsupported extension processing request type: ${request.GetType().Name}");
        }

        private static SyntaxNode GetCompilationUnitNode(ExtProcessingRequest request)
        {
            var content = GetContent(request);
            var tree = SyntaxFactory.ParseSyntaxTree(content.Original);
            return tree.GetCompilationUnitRoot();
        }

        private static List<SyntaxNode> GetApplicationObjectNodes(ExtProcessingRequest request)
        {
            var root = GetCompilationUnitNode(request);
            return root.DescendantNodes(node => 
                node.Kind == SyntaxKind.CompilationUnit || node.Kind.IsApplicationObject()).ToList();
        }
    }
}
