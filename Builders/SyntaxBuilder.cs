using Glitch.CodeAnalysis.Transformation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public abstract class SyntaxBuilder<TDerived, TNode> 
        where TDerived : SyntaxBuilder<TDerived, TNode>
        where TNode : SyntaxNode
    {
        private static readonly Dictionary<Type, SyntaxKind> PredefinedTypes = new Dictionary<Type, SyntaxKind>
        {
            [typeof(void)]    = SyntaxKind.VoidKeyword,
            [typeof(object)]  = SyntaxKind.ObjectKeyword,
            [typeof(int)]     = SyntaxKind.IntKeyword,
            [typeof(short)]   = SyntaxKind.ShortKeyword,
            [typeof(long)]    = SyntaxKind.LongKeyword,
            [typeof(uint)]    = SyntaxKind.UIntKeyword,
            [typeof(ushort)]  = SyntaxKind.UShortKeyword,
            [typeof(ulong)]   = SyntaxKind.ULongKeyword,
            [typeof(byte)]    = SyntaxKind.ByteKeyword,
            [typeof(sbyte)]   = SyntaxKind.SByteKeyword,
            [typeof(float)]   = SyntaxKind.FloatKeyword,
            [typeof(double)]  = SyntaxKind.DoubleKeyword,
            [typeof(decimal)] = SyntaxKind.DecimalKeyword,
            [typeof(char)]    = SyntaxKind.CharKeyword,
            [typeof(string)]  = SyntaxKind.StringKeyword,
            [typeof(bool)]    = SyntaxKind.BoolKeyword
        };

        private List<(CSharpSyntaxRewriter Rewriter, int Order)> rewriters = new List<(CSharpSyntaxRewriter, int Order)>();

        protected SyntaxBuilder() { }
        
        protected SyntaxBuilder(TNode content) 
        {
            SetContent(content);
        }

        public TDerived NormalizeWhitespace() => WithRewriter(new WhitespaceNormalizer(), 10000);

        public TDerived RemoveWhitespace() => WithRewriter(new WhitespaceRemover(), 9999);

        public TDerived WithRewriter(CSharpSyntaxRewriter rewriter) => WithRewriter(rewriter, rewriters.Count);

        public TDerived WithRewriter(CSharpSyntaxRewriter rewriter, int order)
        {
            Check.NotNull(rewriter, nameof(rewriter));
            rewriters.Add((rewriter, order));
            return (TDerived)this;
        }

        public TDerived Reset()
        {
            rewriters.Clear();
            ResetContent();
            return (TDerived)this;
        }

        public TNode ToSyntaxNode()
        {
            var node = BuildSyntaxNode();

            if (rewriters.Any())
            {
                node = rewriters
                    .OrderBy(r => r.Order)
                    .Aggregate(node, (n, r) => (TNode)r.Rewriter.Visit(n));
            }

            return node;
        }

        public override string ToString() => ToSyntaxNode().ToString();

        // TEMP This only exists to temporarily serve the TypeSyntaxBuilder class until it can be refactored
        protected T Rewrite<T>(T node) where T : SyntaxNode
        {
            if (rewriters.Any())
            {
                node = rewriters
                    .OrderBy(r => r.Order)
                    .Aggregate(node, (n, r) => (T)r.Rewriter.Visit(n));
            }

            return node;
        }


        protected abstract TNode BuildSyntaxNode();

        protected abstract void SetContent(TNode node);
        
        protected abstract void ResetContent();

        protected static TypeSyntax CreateTypeNode<T>() => CreateTypeNode(typeof(T));

        protected static TypeSyntax CreateTypeNode(Type type)
        {
            if (PredefinedTypes.ContainsKey(type))
            {
                return PredefinedType(
                        Token(PredefinedTypes[type]));
            }

            return ParseTypeName(type.FullName);
        }

        protected static LiteralExpressionSyntax CreateLiteral(bool value) => LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression, Token(value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword));
        protected static LiteralExpressionSyntax CreateLiteral(int value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(short value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(long value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(uint value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(ushort value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(ulong value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(float value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(double value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(decimal value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(char value) => LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(value));
        protected static LiteralExpressionSyntax CreateLiteral(string value) => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
        protected static ExpressionSyntax CreateLiteral(Enum value) => ParseExpression($"{value.GetType().Name}.{value}");

        protected static bool TryCreateLiteral(object value, out ExpressionSyntax literal)
        {
            switch (value)
            {
                case bool val:
                    literal = CreateLiteral(val);
                    break;
                case int val:
                    literal = CreateLiteral(val);
                    break;
                case short val:
                    literal = CreateLiteral(val);
                    break;
                case long val:
                    literal = CreateLiteral(val);
                    break;
                case uint val:
                    literal = CreateLiteral(val);
                    break;
                case ushort val:
                    literal = CreateLiteral(val);
                    break;
                case ulong val:
                    literal = CreateLiteral(val);
                    break;
                case float val:
                    literal = CreateLiteral(val);
                    break;
                case double val:
                    literal = CreateLiteral(val);
                    break;
                case decimal val:
                    literal = CreateLiteral(val);
                    break;
                case char val:
                    literal = CreateLiteral(val);
                    break;
                case string val:
                    literal = CreateLiteral(val);
                    break;
                case Enum val:
                    literal = CreateLiteral(val);
                    break;
                default:
                    literal = default;
                    return false;
            }

            return true;
        }

        protected static SyntaxToken CreateToken(SyntaxKind kind, string value) 
            => Token(SyntaxTriviaList.Empty, kind, value, value, SyntaxTriviaList.Empty);
    }
}
