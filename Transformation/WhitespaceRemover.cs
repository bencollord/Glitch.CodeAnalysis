using Glitch.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text.RegularExpressions;

namespace Glitch.CodeAnalysis.Transformation
{
    public class WhitespaceRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node is null) return node;

            node = node.WithLeadingTrivia(
                    node.GetLeadingTrivia()
                        .WhereNot(IsWhitespace)
                ).WithTrailingTrivia(
                    node.GetTrailingTrivia()
                        .WhereNot(IsWhitespace)
                );

            return base.Visit(node);
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            token = token.WithLeadingTrivia(
                    token.LeadingTrivia
                         .WhereNot(IsWhitespace)
                ).WithTrailingTrivia(
                    token.TrailingTrivia
                         .WhereNot(IsWhitespace)
                );

            return base.VisitToken(token);
        }

        private bool IsWhitespace(SyntaxTrivia trivia) 
            => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
    }
}
