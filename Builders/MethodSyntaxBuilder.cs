using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public sealed class MethodSyntaxBuilder : BaseMethodSyntaxBuilder<MethodSyntaxBuilder, MethodDeclarationSyntax>, IGenericMemberBuilder<MethodSyntaxBuilder>
    {
        private static readonly TypeSyntax VoidType = CreateTypeNode(typeof(void));

        private TypeSyntax returnType;
        private TypeParameterListBuilder typeParameters = new TypeParameterListBuilder();

        public MethodSyntaxBuilder(string name)
            : base(name)
        {
            returnType = VoidType;
        }

        public MethodSyntaxBuilder(MethodDeclarationSyntax node) : base(node) { }

        public MethodSyntaxBuilder HasTypeParameter(string type)
        {
            typeParameters.Add(type);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameters(params string[] types)
        {
            typeParameters.AddRange(types);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameters(IEnumerable<string> types)
        {
            typeParameters.AddRange(types);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameter(TypeParameterSyntax type)
        {
            typeParameters.Add(type);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameters(params TypeParameterSyntax[] types)
        {
            typeParameters.AddRange(types);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameters(IEnumerable<TypeParameterSyntax> types)
        {
            typeParameters.AddRange(types);
            return this;
        }

        public MethodSyntaxBuilder HasTypeParameters(int count)
        {
            typeParameters.AddRange(count);
            return this;
        }

        public MethodSyntaxBuilder WithoutTypeParameters()
        {
            typeParameters.Clear();
            return this;
        }

        public MethodSyntaxBuilder Override() 
            => RemoveModifiers(SyntaxKind.VirtualKeyword, SyntaxKind.AbstractKeyword, SyntaxKind.StaticKeyword)
                   .AddModifiers(SyntaxKind.OverrideKeyword);

        public static MethodSyntaxBuilder Extern(string dllName)
        {
            // TODO 
            throw new NotImplementedException();
        }

        public MethodSyntaxBuilder HideBase()
            => RemoveModifiers(SyntaxKind.OverrideKeyword)
                   .AddModifiers(SyntaxKind.NewKeyword);

        public MethodSyntaxBuilder Virtual()
            => RemoveModifiers(SyntaxKind.OverrideKeyword, SyntaxKind.StaticKeyword, SyntaxKind.SealedKeyword)
                   .AddModifiers(SyntaxKind.VirtualKeyword);

        public MethodSyntaxBuilder Abstract()
        {
            // TODO Clear method body
            return RemoveModifiers(SyntaxKind.OverrideKeyword, SyntaxKind.StaticKeyword, SyntaxKind.SealedKeyword, SyntaxKind.VirtualKeyword)
                .AddModifiers(SyntaxKind.AbstractKeyword);
        }

        public MethodSyntaxBuilder Sealed()
            => RemoveModifiers(SyntaxKind.AbstractKeyword, SyntaxKind.StaticKeyword, SyntaxKind.VirtualKeyword)
                   .AddModifiers(SyntaxKind.SealedKeyword);


        public MethodSyntaxBuilder Unsafe() => AddModifiers(SyntaxKind.UnsafeKeyword);

        public MethodSyntaxBuilder Async()
        {
            if (ContainsModifier(SyntaxKind.AsyncKeyword))
            {
                return this;
            }

            AddModifiers(SyntaxKind.AsyncKeyword);

            var taskType = CreateTypeNode<Task>();

            if (!returnType.IsEquivalentTo(VoidType))
            {
                // HACK Downcasting the return type of a method you don't own is bad, m'kay?
                var hackyDowncastQualifiedName = (QualifiedNameSyntax)taskType;

                taskType = QualifiedName(
                    hackyDowncastQualifiedName.Left,
                    GenericName(
                        hackyDowncastQualifiedName.Right.Identifier, 
                        TypeArgumentList(
                            SingletonSeparatedList(returnType)
                        )
                    )
                );
            }

            return Returns(taskType);
        }

        public MethodSyntaxBuilder Returns<T>() => Returns(typeof(T));

        public MethodSyntaxBuilder Returns(Type returnType) => Returns(CreateTypeNode(returnType));

        public MethodSyntaxBuilder Returns(TypeSyntax returnType)
        {
            this.returnType = returnType;
            return this;
        }

        protected override SyntaxToken GetIdentifier(MethodDeclarationSyntax node) => node.Identifier;

        protected override MethodDeclarationSyntax ToMethodNode()
        {
            var node = MethodDeclaration(returnType, Identifier);

            if (typeParameters.Any())
            {
                node = node.WithTypeParameterList(typeParameters.ToSyntaxNode());
            }

            return node;
        }

        protected override void SetMethodContent(MethodDeclarationSyntax node)
        {
            returnType = node.ReturnType;
            typeParameters.Clear();
            typeParameters.AddRange(node.TypeParameterList.Parameters);
        }

        protected override void ResetMethodContent()
        {
            returnType = VoidType;
            typeParameters.Clear();
        }
    }
}
