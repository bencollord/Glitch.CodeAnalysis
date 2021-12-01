using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class PropertySyntaxBuilder : MemberSyntaxBuilder<PropertySyntaxBuilder, PropertyDeclarationSyntax>
    {
        private static readonly AccessorListSyntax DefaultAccessorList = AccessorList(List(new[]
        {
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken)
                ).WithKeyword(
                    Token(SyntaxKind.GetKeyword)
                )
        }));

        private PropertyDeclarationSyntax property;

        public PropertySyntaxBuilder(string name, Type type)
            : this(name, CreateTypeNode(type)) { }

        public PropertySyntaxBuilder(string name, TypeSyntax type) : base(name)
        {
            property = PropertyDeclaration(
                type, Identifier(name)
            ).WithAccessorList(
                DefaultAccessorList  
            );
        }

        public PropertySyntaxBuilder(PropertyDeclarationSyntax node) : base(node) { }

        public PropertySyntaxBuilder HasType<T>() => HasType(typeof(T));

        public PropertySyntaxBuilder HasType(Type type) => HasType(CreateTypeNode(type));

        public PropertySyntaxBuilder HasType(TypeSyntax type)
        {
            property = property.WithType(type);
            return this;
        }

        public PropertySyntaxBuilder WithSetter()
        {
            if (!property.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)))
            {
                property = property.AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken)
                        )
                );
            }

            return this;
        }

        public PropertySyntaxBuilder ReadOnly()
        {
            int setterIndex = property.AccessorList.Accessors.IndexOf(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

            if (setterIndex > -1)
            {
                property = property.RemoveNode(property.AccessorList.Accessors[setterIndex], SyntaxRemoveOptions.KeepNoTrivia);
            }

            return this;
        }

        protected override PropertyDeclarationSyntax ToMemberNode() => property;

        protected override void SetMemberContent(PropertyDeclarationSyntax node) => property = node;

        protected override SyntaxToken GetIdentifier(PropertyDeclarationSyntax node) => node.Identifier;

        protected override void ResetMemberContent()
        {
            property = property.WithAccessorList(DefaultAccessorList);
        }
    }
}
