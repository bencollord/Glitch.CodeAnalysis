using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class ParameterSyntaxBuilder : SyntaxBuilder<ParameterSyntaxBuilder, ParameterSyntax>, IDefaultValueBuilder<ParameterSyntaxBuilder>
    {
        private ParameterSyntax parameter;

        public ParameterSyntaxBuilder(string name, Type type) 
            : this(name, CreateTypeNode(type)) { }

        public ParameterSyntaxBuilder(string name, TypeSyntax type)
        {
            parameter = Parameter(Identifier(name))
                .WithType(type);
        }

        public ParameterSyntaxBuilder(ParameterSyntax node) : base(node) { }

        public ParameterSyntaxBuilder Reference() => SetModifier(SyntaxKind.RefKeyword);

        public ParameterSyntaxBuilder Output() => SetModifier(SyntaxKind.OutKeyword);

        public ParameterSyntaxBuilder HasDefaultValue(bool value)    => SetDefaultValue(LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression, Token(value ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword)));
        public ParameterSyntaxBuilder HasDefaultValue(int value)     => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(short value)   => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(long value)    => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(uint value)    => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(ushort value)  => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(ulong value)   => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(float value)   => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(double value)  => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(decimal value) => SetDefaultValue(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(char value)    => SetDefaultValue(LiteralExpression(SyntaxKind.CharacterLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(string value)  => SetDefaultValue(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value)));
        public ParameterSyntaxBuilder HasDefaultValue(Enum value)    => SetDefaultValue(ParseExpression($"{value.GetType().Name}.{value}"));

        protected override ParameterSyntax BuildSyntaxNode() => parameter;

        protected override void SetContent(ParameterSyntax node) => parameter = node;

        protected override void ResetContent()
        {
            parameter = parameter
                .WithDefault(null)
                .WithModifiers(TokenList());
        }

        private ParameterSyntaxBuilder SetDefaultValue(ExpressionSyntax value)
        {
            parameter = parameter.WithDefault(
                EqualsValueClause(value));
            return this;
        }

        private ParameterSyntaxBuilder SetModifier(SyntaxKind kind)
        {
            parameter = parameter.WithModifiers(
                TokenList(
                    Token(kind)));

            return this;
        }
    }
}
