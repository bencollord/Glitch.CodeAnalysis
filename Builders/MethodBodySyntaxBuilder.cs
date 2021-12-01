using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class MethodBodySyntaxBuilder : SyntaxBuilder<MethodBodySyntaxBuilder, BlockSyntax>
    {
        private List<StatementSyntax> statements = new List<StatementSyntax>();

        public MethodBodySyntaxBuilder() { }

        public MethodBodySyntaxBuilder(BlockSyntax body) : base(body) { }

        public MethodBodySyntaxBuilder DeclareVariable(TypeSyntax type, string name) 
            => DeclareVariable(type, VariableDeclarator(name));

        public MethodBodySyntaxBuilder DeclareVariable(string name, ExpressionSyntax initializer)
            => DeclareVariable(IdentifierName("var"), name, initializer);

        public MethodBodySyntaxBuilder DeclareVariable(TypeSyntax type, string name, ExpressionSyntax initializer) 
            => DeclareVariable(type, CreateDeclarator(name, initializer));

        public MethodBodySyntaxBuilder AddStatement(string statement)
            => AddStatement(ParseStatement(statement));

        public MethodBodySyntaxBuilder AddStatement(ExpressionSyntax expression)
            => AddStatement(ExpressionStatement(expression));

        public MethodBodySyntaxBuilder AddStatement(StatementSyntax statement)
        {
            statements.Add(statement);
            return this;
        }

        public MethodBodySyntaxBuilder Return(string expression)
            => Return(ParseExpression(expression));

        public MethodBodySyntaxBuilder Return(ExpressionSyntax expression = default)
            => AddStatement(ReturnStatement(expression));

        protected override BlockSyntax BuildSyntaxNode() => Block(List(statements));

        protected override void SetContent(BlockSyntax node)
        {
            statements.Clear();
            statements.AddRange(node.Statements);
        }

        protected override void ResetContent()
        {
            statements.Clear();
        }

        private MethodBodySyntaxBuilder DeclareVariable(TypeSyntax type, VariableDeclaratorSyntax variable)
        {
            var statement = LocalDeclarationStatement(
                VariableDeclaration(type, 
                    SingletonSeparatedList(variable)));

            return AddStatement(statement);
        }

        private VariableDeclaratorSyntax CreateDeclarator(string name, ExpressionSyntax initializer)
            => VariableDeclarator(name)
                   .WithInitializer(
                       EqualsValueClause(initializer));
    }
}
