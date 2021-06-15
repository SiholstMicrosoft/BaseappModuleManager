using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Synchronizer.Definitions;
using Synchronizer.Models;
using SynchronizerTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synchronizer.Parsers
{
    public class BmmParser : IBmmParser
    {
        public ParseResult Parse(ParseRequest request)
        {
            var content = GetContent(request);
            var tree = SyntaxFactory.ParseSyntaxTree(content.Original);
            var root = tree.GetCompilationUnitRoot();

            var hasAlCode = RemoveCodeBlocks(content, root);
            CleanTags(content, root);
            if(!request.IncludeComments)
            {
                RemoveAllComments(content, root);
            }
            SyntaxKind? rootKind = hasAlCode ? root.Objects[0].Kind : null;

            return new StringParseResult(content.Current, hasAlCode, request as StringParseRequest, rootKind);
        }

        private static bool RemoveCodeBlocks(StringContentManager content, SyntaxNode root)
        {
            var nodes = root.DescendantNodes(_ => true).ToList();
            var removedNodes = new HashSet<SyntaxNode>();

            foreach (var node in nodes)
            {
                if (removedNodes.Contains(node))
                {
                    continue;
                }
                var trivias = node.GetLeadingTrivia().ToList();
                if (!HasTag(trivias, BmmTags.IgnoreTag))
                {
                    continue;
                }
                removedNodes.Add(node);
                foreach (var childNode in node.DescendantNodes(_ => true))
                {
                    removedNodes.Add(childNode);
                }
                content.RemoveSpan(node.FullSpan);
            }
            return nodes.Count > removedNodes.Count;
        }

        private static void CleanTags(StringContentManager content, SyntaxNode root)
        {
            var trivias = root.DescendantTrivia(_ => true).ToList();

            for(var i = 0; i < trivias.Count; i++)
            {
                if(trivias[i].Kind == SyntaxKind.LineCommentTrivia && trivias[i].ToString().Contains(BmmTags.IgnoreTag))
                {
                    RemoveComment(i, trivias, content);
                }
            }
        }

        private static void RemoveAllComments(StringContentManager content, SyntaxNode root)
        {
            var trivias = root.DescendantTrivia(_ => true).ToList();

            for (var i = 0; i < trivias.Count; i++)
            {
                if (trivias[i].Kind == SyntaxKind.LineCommentTrivia)
                {
                    RemoveComment(i, trivias, content);
                }
            }
        }

        private static StringContentManager GetContent(ParseRequest request)
        {
            if (request is StringParseRequest strRequest)
            {
                return new StringContentManager(strRequest.Content);
            }
            throw new ArgumentException($"Unsupported parse request type: ${request.GetType().Name}");
        }

        private static bool HasTag(IList<SyntaxTrivia> trivias, string tag)
        {
            return trivias.Any(x => x.ToString().Contains(tag));
        }

        private static void RemoveComment(int commentIndex, IList<SyntaxTrivia> trivias, StringContentManager content)
        {
            var trivia = trivias[commentIndex];
            content.RemoveSpan(trivia.FullSpan);

            // Remove end of line (if any).
            if (commentIndex + 1 < trivias.Count)
            {
                content.RemoveSpan(trivias[commentIndex + 1].FullSpan);
            }
            commentIndex--;

            // Remove preceding white-space.
            while (commentIndex >= 0 && trivias[commentIndex].Kind == SyntaxKind.WhiteSpaceTrivia)
            {
                content.RemoveSpan(trivias[commentIndex].FullSpan);
                commentIndex--;
            }
        }
    }
}
