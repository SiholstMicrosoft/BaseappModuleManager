using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System;

namespace BmmCore.Extensions
{
    public enum CodeScope
    {
        None,
        RecordMemberAccess,
        ObjectMemberAccess,
        WithRecord,
        WithObject
    }

    public static class SyntaxNodeExtensions
    {
        /// <summary>
        /// Checks if the identifier belogns to a variable reference.
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static bool IsVariableReference(this SyntaxNode syntax)
        {
            if(syntax.Parent == null || !syntax.IsKind(SyntaxKind.IdentifierName))
            {
                return false;
            }
            var parent = syntax.Parent;
            return 
                parent.IsKind(SyntaxKind.AssignmentStatement) || 
                parent.IsKind(SyntaxKind.MemberAccessExpression) || 
                parent.IsKind(SyntaxKind.ArgumentList);
        }

        /// <summary>
        /// Checks if the identifier belongs to a method reference.
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static bool IsMethodReference(this SyntaxNode syntax)
        {
            if (syntax.Parent == null || !syntax.IsKind(SyntaxKind.IdentifierName))
            {
                return false;
            }
            var parent = syntax.Parent;
            return parent.IsKind(SyntaxKind.InvocationExpression) || parent.IsKind(SyntaxKind.ExpressionStatement);
        }

        /// <summary>
        /// Gets the scope of the syntax node (e.g. having a With Statement) to determine any ambiguity.
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static CodeScope GetScope(this SyntaxNode syntax)
        {
            syntax = syntax.Parent;
            while(syntax?.Parent != null)
            {
                if(syntax.IsKind(SyntaxKind.MemberAccessExpression))
                {
                    var expr = syntax as MemberAccessExpressionSyntax;
                    if (expr.Expression.GetIdentifierOrLiteralValue().Equals("rec", StringComparison.OrdinalIgnoreCase))
                    {
                        return CodeScope.RecordMemberAccess;
                    }
                    else
                    {
                        return CodeScope.ObjectMemberAccess;
                    }
                }
                else if (syntax.IsKind(SyntaxKind.WithStatement))
                {
                    var with = syntax as WithStatementSyntax;
                    if (with.WithId.GetIdentifierOrLiteralValue().Equals("rec", StringComparison.OrdinalIgnoreCase))
                    {
                        return CodeScope.WithRecord;
                    }
                    else
                    {
                        return CodeScope.WithObject;
                    }
                }
                syntax = syntax.Parent;
            }
            return CodeScope.None;
        }
    }
}
