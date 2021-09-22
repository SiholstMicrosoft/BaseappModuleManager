using BmmCore.Models.Domain;
using BmmCore.Models.Processing;
using BmmCore.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BmmCore.Processing
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
            var extensionFields = Field.FromSyntaxNode(root);
            var globalVariables = GlobalVariable.FromSyntaxNode(root);
            var procedures = Procedure.FromSyntaxNode(root);

            globalVariables.ForEach(x => x.PrefixNode(request.Prefix));
            procedures.ForEach(x => x.PrefixNode(request.Prefix));            

            return new StringExtProcessingResponse(
                request as StringExtProcessingRequest,
                extensionFields,
                globalVariables,
                procedures
            );
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
