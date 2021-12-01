using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class FieldSyntaxBuilder : MemberSyntaxBuilder<FieldSyntaxBuilder, FieldDeclarationSyntax>, IDefaultValueBuilder<FieldSyntaxBuilder>
    {
        private TypeSyntax type;
        private VariableDeclaratorSyntax variable;

        public FieldSyntaxBuilder(string name, Type type)
            : this(name, CreateTypeNode(type)) { }

        public FieldSyntaxBuilder(string name, TypeSyntax type)
            : base(name)
        {
            this.type = type;
            variable = VariableDeclarator(name);
        }

        public FieldSyntaxBuilder(FieldDeclarationSyntax node)
            : base(node) { }

        public FieldSyntaxBuilder HasType<T>() => HasType(typeof(T));

        public FieldSyntaxBuilder HasType(Type type) => HasType(ParseTypeName(type.FullName));

        public FieldSyntaxBuilder HasType(TypeSyntax type)
        {
            this.type = type;
            return this;
        }

        public FieldSyntaxBuilder ReadOnly()
        {
            AddModifiers(SyntaxKind.ReadOnlyKeyword);
            return this;
        }

        public FieldSyntaxBuilder HasDefaultValue(bool value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(int value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(short value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(long value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(uint value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(ushort value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(ulong value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(float value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(double value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(decimal value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(char value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(string value) => HasDefaultValue(CreateLiteral(value));
        public FieldSyntaxBuilder HasDefaultValue(Enum value) => HasDefaultValue(CreateLiteral(value));

        public FieldSyntaxBuilder HasDefaultValue(ExpressionSyntax value)
        {
            variable = variable.WithInitializer(
                EqualsValueClause(value));
            return this;
        }

        protected override FieldDeclarationSyntax ToMemberNode()
            => FieldDeclaration(
                   VariableDeclaration(type,
                       SingletonSeparatedList(variable)
                   ));

        protected override SyntaxToken GetIdentifier(FieldDeclarationSyntax node)
        {
            if (node.Declaration.Variables.Count() > 1)
            {
                throw new NotSupportedException("Fields with more than one variable declaration are not supported");
            }

            return node.Declaration.Variables
                .Select(d => d.Identifier)
                .SingleOrDefault();
        }

        protected override void SetMemberContent(FieldDeclarationSyntax node)
        {
            if (node.Declaration.Variables.Count() > 1)
            {
                throw new NotSupportedException("Fields with more than one variable declaration are not supported");
            }

            type = node.Declaration.Type;
            variable = node.Declaration.Variables.SingleOrDefault();
        }

        protected override void ResetMemberContent()
        {
            variable = variable.WithInitializer(null);
        }
    }
}
