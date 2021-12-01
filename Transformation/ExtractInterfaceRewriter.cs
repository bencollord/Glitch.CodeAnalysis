using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Glitch.CodeAnalysis.Transformation
{
    public class ExtractInterfaceRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword) || !node.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
            {
                return null;
            }

            node = node
                .WithModifiers(SyntaxFactory.TokenList())
                .WithBody(null)
                .WithExpressionBody(null)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());

            return base.VisitMethodDeclaration(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword) || !node.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
            {
                return null;
            }

            node = node
                .WithModifiers(SyntaxFactory.TokenList())
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            if (node.Modifiers.Any())
            {
                return null;
            }

            node = node
                .WithBody(null)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            return base.VisitAccessorDeclaration(node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return null;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return null;
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            return null;
        }
    }
}
