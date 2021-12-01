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
    public sealed class TypeSyntaxBuilder : MemberSyntaxBuilder<TypeSyntaxBuilder, TypeDeclarationSyntax>, ITypeBuilder<TypeSyntaxBuilder>
    {
        private SyntaxKind keyword = SyntaxKind.ClassKeyword;
        private SyntaxKind declarationKind = SyntaxKind.ClassDeclaration;
        private List<UsingDirectiveSyntax> usingDirectives = new List<UsingDirectiveSyntax>();
        private NameSyntax namespaceName;
        private List<TypeParameterSyntax> typeParameters = new List<TypeParameterSyntax>();
        private LinkedList<BaseTypeSyntax> baseTypes = new LinkedList<BaseTypeSyntax>();
        private List<FieldSyntaxBuilder> fields = new List<FieldSyntaxBuilder>();
        private List<ConstructorSyntaxBuilder> constructors = new List<ConstructorSyntaxBuilder>();
        private List<PropertySyntaxBuilder> properties = new List<PropertySyntaxBuilder>();
        private List<MethodSyntaxBuilder> methods = new List<MethodSyntaxBuilder>();

        public TypeSyntaxBuilder(string name)
            : this(Identifier(name)) { }

        public TypeSyntaxBuilder(SyntaxToken identifier)
            : base(identifier) { }

        public TypeSyntaxBuilder(TypeDeclarationSyntax node)
            : base(node) { }

        public TypeSyntaxBuilder Namespace(string namespaceName) => Namespace(ParseName(namespaceName));

        public TypeSyntaxBuilder Namespace(NameSyntax namespaceName)
        {
            this.namespaceName = namespaceName;
            return this;
        }

        public TypeSyntaxBuilder SubNamespace(string namespaceName) => SubNamespace(ParseName(namespaceName));

        public TypeSyntaxBuilder SubNamespace(NameSyntax namespaceName)
        {
            NameSyntax Combine(NameSyntax left, NameSyntax right)
            {
                if (right is SimpleNameSyntax simple)
                {
                    return QualifiedName(left, simple);
                }

                if (right is QualifiedNameSyntax qualified)
                {
                    return QualifiedName(Combine(left, qualified.Left), qualified.Right);
                }

                throw new NotSupportedException($"Cannot use '{right}' as a subnamespace");
            }

            if (this.namespaceName is null)
            {
                return Namespace(namespaceName);
            }

            this.namespaceName = Combine(this.namespaceName, namespaceName);
            return this;
        }

        public TypeSyntaxBuilder Using(string namespaceName) => Using(ParseName(namespaceName));

        public TypeSyntaxBuilder Using(params string[] namespaceNames) => Using(namespaceNames.AsEnumerable());

        public TypeSyntaxBuilder Using(IEnumerable<string> namespaceNames) => Using(namespaceNames.Select(n => ParseName(n)));

        public TypeSyntaxBuilder Using(NameSyntax namespaceName)
        {
            usingDirectives.Add(
                UsingDirective(namespaceName)
                    .WithUsingKeyword(
                        Token(SyntaxKind.UsingKeyword))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken)));

            return this;
        }

        public TypeSyntaxBuilder Using(params NameSyntax[] namespaceNames)
            => Using(namespaceNames.AsEnumerable());

        public TypeSyntaxBuilder Using(IEnumerable<NameSyntax> namespaceNames)
        {
            foreach (var ns in namespaceNames)
            {
                Using(ns);
            }

            return this;
        }

        // TODO There's no way to tell whether a type is a class or interface without a semantic model
        //      It's a little janky to just have Extends add to the front of the list while Implements 
        //      adds to the back of the list.
        public TypeSyntaxBuilder Extends<T>() => Extends(typeof(T));

        public TypeSyntaxBuilder Extends(Type baseType) => Extends(CreateTypeNode(baseType));

        public TypeSyntaxBuilder Extends(string baseType) => Extends(ParseTypeName(baseType));

        public TypeSyntaxBuilder Extends(TypeSyntax baseType) => Extends(SimpleBaseType(baseType));

        public TypeSyntaxBuilder Extends(BaseTypeSyntax baseType)
        {
            baseTypes.AddFirst(baseType);
            return this;
        }

        public TypeSyntaxBuilder Implements<T>() => Implements(typeof(T));

        public TypeSyntaxBuilder Implements(Type baseType) => Implements(CreateTypeNode(baseType));

        public TypeSyntaxBuilder Implements(string interfaceType) => Implements(ParseTypeName(interfaceType));

        public TypeSyntaxBuilder Implements(TypeSyntax interfaceType) => Implements(SimpleBaseType(interfaceType));

        public TypeSyntaxBuilder Implements(BaseTypeSyntax interfaceType)
        {
            baseTypes.AddLast(interfaceType);
            return this;
        }

        public TypeSyntaxBuilder WithoutBaseTypes()
        {
            baseTypes.Clear();
            return this;
        }

        public TypeSyntaxBuilder ValueType()
        {
            keyword = SyntaxKind.StructKeyword;
            declarationKind = SyntaxKind.StructDeclaration;
            return this;
        }

        public TypeSyntaxBuilder ReferenceType()
        {
            keyword = SyntaxKind.ClassKeyword;
            declarationKind = SyntaxKind.ClassDeclaration;
            return this;
        }

        public TypeSyntaxBuilder HasTypeParameter(string type) => HasTypeParameter(TypeParameter(type));

        public TypeSyntaxBuilder HasTypeParameters(params string[] types) => HasTypeParameters(types.AsEnumerable());

        public TypeSyntaxBuilder HasTypeParameters(IEnumerable<string> types) => HasTypeParameters(types.Select(TypeParameter));

        // TODO Has/With conventions - Should one add and one replace? It's inconsistent right now
        public TypeSyntaxBuilder HasTypeParameter(TypeParameterSyntax type)
        {
            typeParameters.Add(type);
            return this;
        }

        public TypeSyntaxBuilder HasTypeParameters(params TypeParameterSyntax[] types) => HasTypeParameters(types.AsEnumerable());

        public TypeSyntaxBuilder HasTypeParameters(IEnumerable<TypeParameterSyntax> types)
        {
            typeParameters.Clear();
            typeParameters.AddRange(types);
            return this;
        }

        public TypeSyntaxBuilder HasTypeParameters(int count) => HasTypeParameters(Enumerable.Range(0, count).Select(c => $"T{c}"));

        public TypeSyntaxBuilder WithoutTypeParameters()
        {
            typeParameters.Clear();
            return this;
        }

        public FieldSyntaxBuilder Field(string name) => Field<object>(name);

        public FieldSyntaxBuilder Field<T>(string name) => Field(name, typeof(T));

        public FieldSyntaxBuilder Field(string name, Type type) => Field(name, CreateTypeNode(type));

        public FieldSyntaxBuilder Field(string name, TypeSyntax type)
        {
            var field = new FieldSyntaxBuilder(name, type);
            WithField(field.Private());
            return field;
        }

        public TypeSyntaxBuilder Field(string name, Action<FieldSyntaxBuilder> config)
            => Field<object>(name, config);

        public TypeSyntaxBuilder Field<T>(string name, Action<FieldSyntaxBuilder> config)
            => Field(name, typeof(T), config);

        public TypeSyntaxBuilder Field(string name, Type type, Action<FieldSyntaxBuilder> config)
        {
            config(Field(name, type));
            return this;
        }

        public TypeSyntaxBuilder Fields(Action<IFieldSetBuilder> config)
        {
            config(this);
            return this;
        }

        public TypeSyntaxBuilder WithField(FieldDeclarationSyntax field) => WithField(new FieldSyntaxBuilder(field));

        public TypeSyntaxBuilder WithField(FieldSyntaxBuilder field)
        {
            fields.Add(field);
            return this;
        }

        public TypeSyntaxBuilder WithoutFields()
        {
            fields.Clear();
            return this;
        }

        public ConstructorSyntaxBuilder HasConstructor()
        {
            var ctor = new ConstructorSyntaxBuilder(Identifier);
            WithConstructor(ctor.Public());
            return ctor;
        }

        public TypeSyntaxBuilder HasConstructor(Action<ConstructorSyntaxBuilder> config)
        {
            config(HasConstructor());
            return this;
        }

        public TypeSyntaxBuilder HasConstructors(Action<IConstructorSetBuilder> config)
        {
            config(this);
            return this;
        }

        public TypeSyntaxBuilder WithDefaultConstructor()
        {
            var ctor = ConstructorDeclaration(Identifier)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword)
                ).WithBody(
                    Block()
                );

            return WithConstructor(ctor);
        }

        public TypeSyntaxBuilder WithConstructor(ConstructorDeclarationSyntax ctor) => WithConstructor(new ConstructorSyntaxBuilder(ctor));

        public TypeSyntaxBuilder WithConstructor(ConstructorSyntaxBuilder ctor)
        {
            constructors.Add(ctor);
            return this;
        }

        public TypeSyntaxBuilder WithoutConstructors()
        {
            constructors.Clear();
            return this;
        }

        public PropertySyntaxBuilder Property(string name) => Property<object>(name);

        public PropertySyntaxBuilder Property<T>(string name) => Property(name, typeof(T));

        public PropertySyntaxBuilder Property(string name, Type type) => Property(name, CreateTypeNode(type));

        public PropertySyntaxBuilder Property(string name, TypeSyntax type)
        {
            var property = new PropertySyntaxBuilder(name, type);
            WithProperty(property.Public());
            return property;
        }

        public TypeSyntaxBuilder Property(string name, Action<PropertySyntaxBuilder> config)
            => Property<object>(name, config);

        public TypeSyntaxBuilder Property<T>(string name, Action<PropertySyntaxBuilder> config)
            => Property(name, typeof(T), config);

        public TypeSyntaxBuilder Property(string name, Type type, Action<PropertySyntaxBuilder> config)
        {
            config(Property(name, type));
            return this;
        }

        public TypeSyntaxBuilder Properties(Action<IPropertySetBuilder> config)
        {
            config(this);
            return this;
        }

        public TypeSyntaxBuilder WithProperty(PropertyDeclarationSyntax property) => WithProperty(new PropertySyntaxBuilder(property));

        public TypeSyntaxBuilder WithProperty(PropertySyntaxBuilder property)
        {
            properties.Add(property);
            return this;
        }

        public TypeSyntaxBuilder WithoutProperties()
        {
            properties.Clear();
            return this;
        }

        public MethodSyntaxBuilder HasMethod(string name)
        {
            var method = new MethodSyntaxBuilder(name);
            WithMethod(method.Public());
            return method;
        }

        public TypeSyntaxBuilder HasMethod(string name, Action<MethodSyntaxBuilder> config)
        {
            config(HasMethod(name));
            return this;
        }

        public TypeSyntaxBuilder HasMethods(Action<IMethodSetBuilder> config)
        {
            config(this);
            return this;
        }

        public TypeSyntaxBuilder WithMethod(MethodDeclarationSyntax method) => WithMethod(new MethodSyntaxBuilder(method));

        public TypeSyntaxBuilder WithMethod(MethodSyntaxBuilder method)
        {
            methods.Add(method);
            return this;
        }

        public TypeSyntaxBuilder WithoutMethods()
        {
            methods.Clear();
            return this;
        }

        public TypeSyntaxBuilder UseValueSemantics(ValueSemanticsOptions options = ValueSemanticsOptions.None) => WithRewriter(new ValueSemanticsRewriter(options));

        public CompilationUnitSyntax ToCompilationUnit()
        {
            // TEMP This rewrite method won't always be available
            return Rewrite(CompilationUnit()
                .WithUsings(
                    List(usingDirectives)
                ).AddMembers(
                    NamespaceDeclaration(namespaceName)
                        .WithNamespaceKeyword(
                            Token(SyntaxKind.NamespaceKeyword)
                        ).AddMembers(BuildSyntaxNode())
                ));
        }

        public override string ToString() => ToCompilationUnit().ToString().Trim();

        protected override TypeDeclarationSyntax ToMemberNode()
        {
            var memberList = new List<MemberDeclarationSyntax>();

            memberList.AddRange(fields.Select(x => x.ToSyntaxNode()));
            memberList.AddRange(constructors.Select(x => x.ToSyntaxNode()));
            memberList.AddRange(properties.Select(x => x.ToSyntaxNode()));
            memberList.AddRange(methods.Select(x => x.ToSyntaxNode()));

            var declaration = TypeDeclaration(
                declarationKind, Identifier
            ).WithMembers(
                List(memberList)
            ).WithKeyword(
                Token(keyword)
            ).WithOpenBraceToken(
                Token(SyntaxKind.OpenBraceToken)
            ).WithCloseBraceToken(
                Token(SyntaxKind.CloseBraceToken)
            );

            if (baseTypes.Any())
            {
                declaration = declaration.WithBaseList(
                    BaseList(
                        SeparatedList(baseTypes)
                    )
                );
            }

            if (typeParameters.Any())
            {
                declaration = declaration.WithTypeParameterList(
                    TypeParameterList(
                        SeparatedList(typeParameters)    
                    )
                );
            }

            return declaration;
        }

        protected override SyntaxToken GetIdentifier(TypeDeclarationSyntax node) => node.Identifier;

        protected override void SetMemberContent(TypeDeclarationSyntax node)
        {
            Reset();

            keyword = node.Keyword.Kind();
            declarationKind = node.Kind();

            if (node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>() is var ns && ns != null)
            {
                namespaceName = ns.Name;
            }

            if (node.FirstAncestorOrSelf<CompilationUnitSyntax>() is var c && c != null)
            {
                usingDirectives.AddRange(c.Usings);
            }

            baseTypes.Clear();

            var baseList = node.BaseList?.Types.ToList();

            fields = node.Members
                .OfType<FieldDeclarationSyntax>()
                .Select(f => new FieldSyntaxBuilder(f))
                .ToList();

            constructors = node.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Select(m => new ConstructorSyntaxBuilder(m))
                .ToList();

            properties = node.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => new PropertySyntaxBuilder(p))
                .ToList();

            methods = node.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(m => new MethodSyntaxBuilder(m))
                .ToList();
        }

        protected override void ResetMemberContent()
        {
            keyword = SyntaxKind.ClassKeyword;
            declarationKind = SyntaxKind.ClassDeclaration;
            usingDirectives.Clear();
            namespaceName = null;
            baseTypes.Clear();
            fields.Clear();
            constructors.Clear();
            properties.Clear();
            methods.Clear();
        }
    }
}
