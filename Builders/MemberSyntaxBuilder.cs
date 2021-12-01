using Glitch.Collections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public abstract class MemberSyntaxBuilder<TDerived, TNode> : SyntaxBuilder<TDerived, TNode>, IMemberBuilder<TDerived>
        where TDerived : MemberSyntaxBuilder<TDerived, TNode>
        where TNode : MemberDeclarationSyntax
    {
        private readonly TDerived self;
        private SyntaxToken identifier;
        private SortedSet<SyntaxToken> modifiers = new SortedSet<SyntaxToken>(new ModifierComparer());
        private List<AttributeListSyntax> attributes = new List<AttributeListSyntax>();

        protected MemberSyntaxBuilder(string name)
            : this(Identifier(name)) { }

        protected MemberSyntaxBuilder(SyntaxToken identifier)
        {
            this.identifier = identifier;
            self = (TDerived)this;
        }

        protected MemberSyntaxBuilder(MemberSyntaxBuilder<TDerived, TNode> copy)
            : this(copy.identifier)
        {
            modifiers.AddRange(copy.modifiers);
        }

        protected MemberSyntaxBuilder(TNode content)
            : base(content) { }

        protected internal SyntaxToken Identifier => identifier;

        public TDerived WithoutModifiers()
        {
            modifiers.Clear();
            return self;
        }

        public TDerived Public() => SetAccessModifier(SyntaxKind.PublicKeyword);

        public TDerived Internal() => SetAccessModifier(SyntaxKind.InternalKeyword);

        public TDerived ProtectedInternal() => SetAccessModifier(SyntaxKind.ProtectedKeyword, SyntaxKind.InternalKeyword);
        
        public TDerived Protected() => SetAccessModifier(SyntaxKind.ProtectedKeyword);
        
        public TDerived PrivateProtected() => SetAccessModifier(SyntaxKind.PrivateKeyword, SyntaxKind.ProtectedKeyword);
        
        public TDerived Private() => SetAccessModifier(SyntaxKind.PrivateKeyword);

        public TDerived Static() => AddModifiers(SyntaxKind.StaticKeyword);

        public TDerived Instance()
        {
            modifiers.RemoveWhere(t => t.IsKind(SyntaxKind.StaticKeyword));
            return self;
        }

        public TDerived WithAttribute(AttributeSyntax attribute)
            => WithAttributeList(
                   AttributeList(
                       SingletonSeparatedList(attribute)));

        public TDerived WithAttributeList(AttributeListSyntax attributeList)
        {
            attributes.Add(attributeList);
            return self;
        }

        public TDerived WithoutAttributes()
        {
            attributes.Clear();
            return self;
        }

        protected sealed override TNode BuildSyntaxNode()
        {
            return (TNode)ToMemberNode()
                .WithModifiers(modifiers.ToTokenList())
                .WithAttributeLists(attributes.ToSyntaxList());
        }

        protected virtual TNode AddModifiers(TNode node) => (TNode)node.WithModifiers(TokenList(modifiers));

        protected TDerived AddModifiers(params SyntaxKind[] modifiers)
        {
            this.modifiers.UnionWith(modifiers.Select(Token));
            return self;
        }

        protected TDerived RemoveModifiers(params SyntaxKind[] modifiers)
        {
            this.modifiers.ExceptWith(modifiers.Select(Token));
            return self;
        }

        protected bool ContainsModifier(SyntaxKind kind) => modifiers.Any(m => m.IsKind(kind));

        protected sealed override void SetContent(TNode node)
        {
            Reset();
            attributes.AddRange(node.AttributeLists);
            modifiers.AddRange(node.Modifiers);
            identifier = GetIdentifier(node);
            SetMemberContent(node);
        }

        protected sealed override void ResetContent()
        {
            attributes.Clear();
            modifiers.Clear();
            ResetMemberContent();
        }

        protected abstract TNode ToMemberNode();

        protected abstract void ResetMemberContent();

        protected abstract SyntaxToken GetIdentifier(TNode node);

        protected abstract void SetMemberContent(TNode node);

        private TDerived SetAccessModifier(SyntaxKind modifier, SyntaxKind? secondModifier = null)
        {
            bool isCombinedModifier = (modifier == SyntaxKind.PrivateKeyword && secondModifier == SyntaxKind.ProtectedKeyword)
                                   || (modifier == SyntaxKind.ProtectedKeyword && secondModifier == SyntaxKind.InternalKeyword);

            Debug.Assert(!secondModifier.HasValue || isCombinedModifier, "Invalid combined modifier");

            modifiers.RemoveWhere(ModifierComparer.IsAccessModifier);
            modifiers.Add(Token(modifier));

            if (secondModifier.HasValue)
            {
                modifiers.Add(Token(secondModifier.Value));
            }

            return self;
        }

        private class ModifierComparer : Comparer<SyntaxToken>
        {
            private static readonly List<SyntaxKind> ModifierKinds = new List<SyntaxKind>
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.ExternKeyword,
                SyntaxKind.NewKeyword,
                SyntaxKind.VirtualKeyword,
                SyntaxKind.AbstractKeyword,
                SyntaxKind.SealedKeyword,
                SyntaxKind.OverrideKeyword,
                SyntaxKind.ReadOnlyKeyword,
                SyntaxKind.UnsafeKeyword,
                SyntaxKind.VolatileKeyword,
                SyntaxKind.AsyncKeyword
            };

            internal static bool IsAccessModifier(SyntaxToken token) => IsAccessModifier(token.Kind());

            internal static bool IsAccessModifier(SyntaxKind kind)
                => kind == SyntaxKind.PublicKeyword
                || kind == SyntaxKind.InternalKeyword
                || kind == SyntaxKind.ProtectedKeyword
                || kind == SyntaxKind.PrivateKeyword;

            public override int Compare(SyntaxToken x, SyntaxToken y) => Compare(x.Kind(), y.Kind());

            internal int Compare(SyntaxKind x, SyntaxKind y)
            {
                int xPos = ModifierKinds.Contains(x) ? ModifierKinds.IndexOf(x) : ModifierKinds.Count;
                int yPos = ModifierKinds.Contains(y) ? ModifierKinds.IndexOf(y) : ModifierKinds.Count;

                return xPos.CompareTo(yPos);
            }
        }
    }
}
