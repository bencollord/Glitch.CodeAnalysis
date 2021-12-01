using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis
{
    public static class SyntaxNodeExtensions
    {
        public static TNode WithLeadingSpace<TNode>(this TNode node) where TNode : SyntaxNode => node.WithLeadingTrivia(Space);
        public static TNode WithTrailingSpace<TNode>(this TNode node) where TNode : SyntaxNode => node.WithTrailingTrivia(Space);

        public static TNode WithLeadingNewLine<TNode>(this TNode node) where TNode : SyntaxNode => node.WithLeadingTrivia(EndOfLine(Environment.NewLine));
        public static TNode WithTrailingNewLine<TNode>(this TNode node) where TNode : SyntaxNode => node.WithTrailingTrivia(EndOfLine(Environment.NewLine));

        public static IEnumerable<TNode> ChildNodes<TNode>(this SyntaxNode node) where TNode : SyntaxNode => node.ChildNodes().OfType<TNode>();
        public static IEnumerable<TNode> DescendantNodes<TNode>(this SyntaxNode node) where TNode : SyntaxNode => node.DescendantNodes().OfType<TNode>();

        public static IEnumerable<SyntaxToken> ChildTokens(this SyntaxNode node, SyntaxKind kind) => node.ChildTokens().Where(t => t.IsKind(kind));
        public static IEnumerable<SyntaxToken> DescendantTokens(this SyntaxNode node, SyntaxKind kind) => node.DescendantTokens().Where(t => t.IsKind(kind));

        public static TNode FirstDescendantOrSelf<TNode>(this SyntaxNode node, Func<TNode, bool> predicate = null) 
            where TNode : SyntaxNode
        {
            var queue = new Queue<SyntaxNode>();
            var current = node;

            do
            {
                if (current is TNode found && (predicate is null || predicate(found)))
                {
                    return found;
                }
                
                queue.EnqueueRange(current.ChildNodes());

                current = queue.Dequeue();
            }
            while (queue.Any());

            return null;
        }

        public static SyntaxList<TNode> ToSyntaxList<TNode>(this IEnumerable<TNode> nodes) where TNode : SyntaxNode => List(nodes);


        public static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this IEnumerable<TNode> nodes)
            where TNode : SyntaxNode => SeparatedList(nodes);

        public static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this IEnumerable<TNode> nodes, SyntaxToken separator)
            where TNode : SyntaxNode => SeparatedList(nodes, Enumerable.Repeat(separator, nodes.Count() - 1));
    }
}
