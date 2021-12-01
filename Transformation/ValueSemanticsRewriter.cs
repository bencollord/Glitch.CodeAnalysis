using Glitch.CodeAnalysis.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Transformation
{
    [Flags]
    public enum ValueSemanticsOptions
    {
        None = 0x0,
        IncludePrivateSetters = 0x2,
        ForceReadOnly = 0x4,
        OverrideToString = 0x8,
        ImplementIEquatable = 0x10
    }

    public class ValueSemanticsRewriter : CSharpSyntaxRewriter
    {
        private HashSet<PropertyDeclarationSyntax> equalityProperties = new HashSet<PropertyDeclarationSyntax>();
        private ValueSemanticsOptions options;

        public ValueSemanticsRewriter(ValueSemanticsOptions options = ValueSemanticsOptions.None)
        {
            this.options = options;
        }

        public bool IncludePrivateSetters 
        {
            get => options.HasFlag(ValueSemanticsOptions.IncludePrivateSetters);
            set => options = value ?
                options | ValueSemanticsOptions.IncludePrivateSetters :
                options & ~ValueSemanticsOptions.IncludePrivateSetters;
        }

        public bool ForceReadOnly
        {
            get => options.HasFlag(ValueSemanticsOptions.ForceReadOnly);
            set => options = value ?
                options | ValueSemanticsOptions.ForceReadOnly :
                options & ~ValueSemanticsOptions.ForceReadOnly;
        }

        public bool OverrideToString
        {
            get => options.HasFlag(ValueSemanticsOptions.OverrideToString);
            set => options = value ?
                options | ValueSemanticsOptions.OverrideToString :
                options & ~ValueSemanticsOptions.OverrideToString;
        }

        public bool ImplementIEquatable
        {
            get => options.HasFlag(ValueSemanticsOptions.ImplementIEquatable);
            set => options = value ?
                options | ValueSemanticsOptions.ImplementIEquatable :
                options & ~ValueSemanticsOptions.ImplementIEquatable;
        }


        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Clear properties from last visit
            equalityProperties.Clear();

            // Visit first to grab equality properties
            node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            // TODO Support IEquatable
            node = node.AddMembers(
                BuildConstructor(node.Identifier),
                BuildEquals(node.Identifier),
                BuildGetHashCode()
            );

            if (OverrideToString)
            {
                node = node.AddMembers(BuildToString());
            }

            return node;
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            // TODO Implement support for structs
            throw new NotSupportedException("Value types are not yet supported");
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            bool isPublic = node.Modifiers.Any(SyntaxKind.PublicKeyword);
            bool hasSetter = node.AccessorList.Accessors.Any(SyntaxKind.SetAccessorDeclaration);

            // TODO Implement AllowPrivateSetters
            // TODO Implement ForceReadOnly

            if (isPublic && !hasSetter)
            {
                equalityProperties.Add(node);
            }

            return base.VisitPropertyDeclaration(node);
        }

        private ConstructorDeclarationSyntax BuildConstructor(SyntaxToken typeName)
        {
            var parameterMap = from prop in equalityProperties
                               let nm = prop.Identifier.ToString().Decapitalize()
                               let id = Identifier(nm)
                               select new
                               {
                                   Property = prop,
                                   Parameter = Parameter(id)
                                       .WithType(prop.Type)
                               };

            return ConstructorDeclaration(typeName)
                .WithParameterList(
                    ParameterList(
                        SeparatedList(parameterMap.Select(p => p.Parameter))
                    )
                )
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword)
                )
                .WithBody(
                    Block(
                        List(
                            from p in parameterMap
                            let expr = AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(p.Property.Identifier),
                                IdentifierName(p.Parameter.Identifier)
                            )
                            select ExpressionStatement(expr)
                        )
                    )
                );
        }

        private MethodDeclarationSyntax BuildEquals(SyntaxToken typeName)
        {
            var method = new MethodSyntaxBuilder("Equals")
                .Public()
                .Override()
                .Returns<bool>();

            var parameter = method.Parameter<object>("obj").ToSyntaxNode();
            var other = Identifier("other");

            method.WithBody(b => b.AddStatement(
                IfStatement(
                    PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        ParenthesizedExpression(
                            IsPatternExpression(
                                IdentifierName(parameter.Identifier),
                                Token(SyntaxKind.IsKeyword),
                                DeclarationPattern(
                                    IdentifierName(typeName),
                                    SingleVariableDesignation(other)
                                )
                            )
                        )
                    ),
                    ReturnStatement(
                        LiteralExpression(SyntaxKind.FalseLiteralExpression)
                    )
            )).AddStatement(
                ReturnStatement(
                    equalityProperties
                        .Select(p => BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(
                                    p.Identifier
                                )
                            ),
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(other),
                                IdentifierName(
                                    p.Identifier
                                )
                            )
                        ))
                        .Aggregate((x, y) => BinaryExpression(SyntaxKind.LogicalAndExpression, x, y))
                )
            ));

            return method.ToSyntaxNode();
        }

        private MethodDeclarationSyntax BuildGetHashCode()
        {
            var method = new MethodSyntaxBuilder("GetHashCode")
                .Public()
                .Override()
                .Returns<int>();

            // TODO refactor method body handling to avoid all this nesting
            method.WithBody(b => 
            {
                b.DeclareVariable("hash",
                    ObjectCreationExpression(
                        IdentifierName("HashCode")
                    ).WithArgumentList(
                        ArgumentList()
                    )
                );

                foreach (var property in equalityProperties)
                {
                    b.AddStatement($"hash.Add({property.Identifier});");
                }

                b.Return("hash.ToHashCode()");
            });

            return method.ToSyntaxNode();
        }
        
        private MethodDeclarationSyntax BuildToString()
        {
            var method = new MethodSyntaxBuilder("ToString")
                .Public()
                .Override()
                .Returns<string>();

            // TODO Refactor method body handling
            method.WithBody(b => 
            {
                b.DeclareVariable("text",
                    ObjectCreationExpression(
                        ParseName("System.Text.StringBuilder")
                    ).WithArgumentList(
                        ArgumentList(default)
                    )
                );

                b.AddStatement(@"text.Append(""{ "");");

                var lastProperty = equalityProperties.Last();

                foreach (var property in equalityProperties)
                {
                    var propertyIdentifier = IdentifierName(property.Identifier);

                    var nameOf = InvocationExpression(
                            IdentifierName(
                                Identifier(default, SyntaxKind.NameOfKeyword, "nameof", "nameof", default)
                            ),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        propertyIdentifier
                                    )
                                )
                            )
                        );

                    var interpolated = InterpolatedStringExpression(
                        Token(SyntaxKind.InterpolatedStringStartToken),
                        List(
                            new InterpolatedStringContentSyntax[]
                            {
                                Interpolation(nameOf),
                                InterpolatedStringText(
                                    Token(default, SyntaxKind.InterpolatedStringTextToken, ": ", ": ", default)
                                ),
                                Interpolation(propertyIdentifier)
                            }
                        ),
                        Token(SyntaxKind.InterpolatedStringEndToken)
                    );

                    var invocation = InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("text"),
                                IdentifierName("Append")
                            ),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(interpolated)
                                )
                            )
                        );

                    b.AddStatement(invocation);

                    if (property == lastProperty)
                    {
                        b.AddStatement(
                            invocation.WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(", ")
                                            )
                                        )
                                    )
                                )
                            )
                        );
                    }
                }

                b.AddStatement(@"text.Append("" }"");");
                b.Return("text.ToString()");
            });

            return method.ToSyntaxNode();
        }
    }

}
