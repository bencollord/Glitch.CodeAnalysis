using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class ConstructorSyntaxBuilder : BaseMethodSyntaxBuilder<ConstructorSyntaxBuilder, ConstructorDeclarationSyntax>
    {
        private ConstructorInitializerSyntax initializer;

        public ConstructorSyntaxBuilder(string name)
            : base(name) { }

        public ConstructorSyntaxBuilder(SyntaxToken name)
            : base(name) { }

        public ConstructorSyntaxBuilder(ConstructorDeclarationSyntax node) : base(node) { }

        public ConstructorSyntaxBuilder WithBaseInitializer(params string[] arguments) => WithBaseInitializer(arguments?.Select(a => ParseExpression(a)));

        public ConstructorSyntaxBuilder WithBaseInitializer(params ExpressionSyntax[] arguments) => WithBaseInitializer(arguments.AsEnumerable());

        public ConstructorSyntaxBuilder WithBaseInitializer(IEnumerable<ExpressionSyntax> arguments) => WithBaseInitializer(arguments?.Select(Argument));

        public ConstructorSyntaxBuilder WithBaseInitializer(params ArgumentSyntax[] arguments) => WithBaseInitializer(arguments.AsEnumerable());

        public ConstructorSyntaxBuilder WithBaseInitializer(IEnumerable<ArgumentSyntax> arguments) => WithInitializer(SyntaxKind.BaseConstructorInitializer, arguments);

        public ConstructorSyntaxBuilder WithInitializer(params string[] arguments) => WithInitializer(arguments?.Select(a => ParseExpression(a)));
        
        public ConstructorSyntaxBuilder WithInitializer(params ExpressionSyntax[] arguments) => WithInitializer(arguments.AsEnumerable());
        
        public ConstructorSyntaxBuilder WithInitializer(IEnumerable<ExpressionSyntax> arguments) => WithInitializer(arguments?.Select(Argument));

        public ConstructorSyntaxBuilder WithInitializer(params ArgumentSyntax[] arguments) => WithInitializer(arguments.AsEnumerable());

        public ConstructorSyntaxBuilder WithInitializer(IEnumerable<ArgumentSyntax> arguments) => WithInitializer(SyntaxKind.ThisConstructorInitializer, arguments);

        public ConstructorSyntaxBuilder WithInitializer(ConstructorInitializerSyntax initializer)
        {
            this.initializer = initializer;
            return this;
        }

        protected override void ResetMethodContent()
        {
            initializer = null;
        }

        protected override SyntaxToken GetIdentifier(ConstructorDeclarationSyntax node) => node.Identifier;

        protected override void SetMethodContent(ConstructorDeclarationSyntax node)
        {
            initializer = node.Initializer;
        }

        protected override ConstructorDeclarationSyntax ToMethodNode() => ConstructorDeclaration(Identifier).WithInitializer(initializer);

        private ConstructorSyntaxBuilder WithInitializer(SyntaxKind kind, IEnumerable<ArgumentSyntax> arguments)
        {
            initializer = ConstructorInitializer(kind, ArgumentList(SeparatedList(arguments)));
            return this;
        }
    }
}
