using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Glitch.CodeAnalysis.Transformation
{
    internal class ExtractUsingsRewriter : CSharpSyntaxRewriter
    {
        private SortedSet<NameSyntax> usingDirectives = new SortedSet<NameSyntax>(new UsingDirectiveComparer());

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var aliased = node.Usings.Where(u => u.Alias != null);
            var @static = node.Usings.Where(u => !u.StaticKeyword.IsKind(SyntaxKind.None));
            var saved = node.Usings
                .Except(aliased)
                .Except(@static)
                .Select(u => u.Name);

            usingDirectives.UnionWith(saved);

            node = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

            var usings = usingDirectives
                .Select(SyntaxFactory.UsingDirective)
                .Concat(aliased)
                .Concat(@static);

            return node.WithUsings(SyntaxFactory.List(usings));
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            // TODO
            return base.VisitNamespaceDeclaration(node);
        }

        public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
        {
            if (node.ToString().Contains("Guid"))
            {
                Console.WriteLine(node);
            }

            usingDirectives.Add(node.Left);

            return base.Visit(node.Right);
        }

        private class UsingDirectiveComparer : Comparer<NameSyntax>
        {
            public override int Compare(NameSyntax x, NameSyntax y)
            {
                if (x is null && y is null)
                {
                    return 0;
                }

                if (x is null)
                {
                    return y is null ? 0 : -1;
                }

                if (y is null)
                {
                    return 1;
                }

                if (x.IsEquivalentTo(y))
                {
                    return 0;
                }

                if (IsSystem(x))
                {
                    return IsSystem(y) ? x.ToString().CompareTo(y.ToString()) : 1;
                }

                if (IsSystem(y))
                {
                    return -1;
                }

                return x.ToString().CompareTo(y);
            }

            private bool IsSystem(NameSyntax name) 
                => name != null && name.ToString().StartsWith("System");
        }
    }
}
