using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis
{
    public static class SyntaxTokenExtensions
    {
        public static SyntaxToken WithLeadingSpace(this SyntaxToken token) => token.WithLeadingTrivia(Space);
        public static SyntaxToken WithTrailingSpace(this SyntaxToken token) => token.WithTrailingTrivia(Space);

        public static SyntaxToken WithLeadingNewLine(this SyntaxToken token) => token.WithLeadingTrivia(EndOfLine(Environment.NewLine));
        public static SyntaxToken WithTrailingNewLine(this SyntaxToken token) => token.WithTrailingTrivia(EndOfLine(Environment.NewLine));

        public static SyntaxTokenList ToTokenList(this IEnumerable<SyntaxToken> tokens) => TokenList(tokens);
    }
}
