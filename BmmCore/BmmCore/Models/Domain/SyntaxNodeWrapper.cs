using BmmCore.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace BmmCore.Models.Domain
{
    public abstract class SyntaxNodeWrapper
    {
        public SyntaxNode SyntaxNode { get; }
        public SyntaxNode PrefixedSyntaxNode { get; private set; }

        public string Prefix { get; private set; }

        protected SyntaxNodeWrapper(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
            PrefixedSyntaxNode = SyntaxNode;
        }

        public void PrefixNode(string prefix)
        {
            if(!string.IsNullOrEmpty(Prefix))
            {
                throw new InvalidOperationException("The syntax node has already been prexifed");
            }
            Prefix = prefix;

            var content = new StringContentManager(SyntaxNode.ToFullString());
            var locations = GetPrefixLocations();

            locations.ForEach(pos =>
            {
                if (content.Original[pos] == '\"')
                {
                    content.InsertText(pos + 1, Prefix + ' ');
                }
                else
                {
                    content.InsertText(pos, Prefix);
                }
            });
            PrefixedSyntaxNode = SyntaxFactory.ParseSyntaxTree(content.Current).GetRoot();
        }

        protected abstract List<int> GetPrefixLocations();
    }
}
