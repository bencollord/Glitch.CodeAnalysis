using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Transformation
{
    // TODO Normalize variable assignments
    public class WhitespaceNormalizer : CSharpSyntaxRewriter
    {
        private const string DefaultIndentation = "    ";
        
        private string indentation;
        private int depth = 0;

        public WhitespaceNormalizer() 
            : this(DefaultIndentation) { }

        public WhitespaceNormalizer(string indentation)
        {
            this.indentation = indentation;
        }

        public bool AddLineSeparatorsBetweenProperties { get; set; }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var result = (CompilationUnitSyntax)base.VisitCompilationUnit(node);

            if (result.Usings.Any())
            {
                var lastUsing = result.Usings.Last();

                result = result.WithUsings(
                    result.Usings.Replace(
                        lastUsing,
                        lastUsing.WithSemicolonToken(
                            lastUsing.SemicolonToken.WithTrailingTrivia(
                                lastUsing.SemicolonToken.TrailingTrivia.Add(CarriageReturnLineFeed)
                            )
                        )
                    )
                );
            }

            return result;
        }

        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            return node.WithUsingKeyword(
                    node.UsingKeyword.WithTrailingSpace()
                ).WithSemicolonToken(
                    node.SemicolonToken.WithTrailingNewLine()
                );
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var normalized = node.WithNamespaceKeyword(
                    node.NamespaceKeyword.WithTrailingSpace()
                ).WithName(
                    node.Name.WithTrailingNewLine()
                ).WithOpenBraceToken(
                    node.OpenBraceToken.WithTrailingNewLine()
                );

            depth++;
            normalized = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(normalized);
            depth--;

            return normalized;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            depth++;
            var visited = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            depth--;

            return OrganizeMembers(
                    NormalizeTypeDeclaration(visited)
                );
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            depth++;
            var visited = (StructDeclarationSyntax)base.VisitStructDeclaration(node);
            depth--;

            return OrganizeMembers(
                    NormalizeTypeDeclaration(visited)
                );
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            var normalized = NormalizeTypeDeclaration(node);
            depth++;
            var result = base.VisitEnumDeclaration(normalized);
            depth--;
            return result;
        }

        public override SyntaxNode VisitBaseList(BaseListSyntax node)
        {
            return node.WithTypes(
                    node.Types
                        .Select(t => t.WithLeadingSpace())
                        .ToSeparatedList(Token(SyntaxKind.CommaToken))
                ).WithColonToken(
                    node.ColonToken
                        .WithLeadingSpace()
                );
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var normalized = NormalizeMemberDeclaration(node)
                .WithDeclaration(
                    node.Declaration
                        .WithType(
                            node.Declaration.Type
                                .WithTrailingSpace()
                        )
                ).WithSemicolonToken(
                    node.SemicolonToken
                        .WithTrailingNewLine()
                );
            
            return base.VisitFieldDeclaration(normalized);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return base.VisitConstructorDeclaration(
                    NormalizeMethodOrConstructor(node)
                );
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var normalized = NormalizeMemberDeclaration(node)
                .WithType(
                    node.Type.WithTrailingSpace()
                );

            return base.VisitPropertyDeclaration(AddTrailingNewLine(normalized));
        }

        public override SyntaxNode VisitAccessorList(AccessorListSyntax node)
        {
            bool isAutoProperty = node.Accessors.All(a => a.Body is null && a.ExpressionBody is null);

            var openBraceToken = isAutoProperty ? node.OpenBraceToken.WithLeadingSpace().WithTrailingSpace()
                                                : NormalizeBlockDelimiter(node.OpenBraceToken);
            var closeBraceToken = isAutoProperty ? node.CloseBraceToken.WithTrailingNewLine()
                                                 : NormalizeBlockDelimiter(node.CloseBraceToken);

            var normalized = node
                .WithOpenBraceToken(openBraceToken)
                .WithCloseBraceToken(closeBraceToken);

            return base.VisitAccessorList(normalized);
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            if (node.Modifiers.Any())
            {
                node = node.WithModifiers(
                    node.Modifiers
                        .Select(m => m.WithTrailingSpace())
                        .ToTokenList()
                );
            }

            if (node.Body != null)
            {
                return base.VisitAccessorDeclaration(
                        node.WithKeyword(
                            node.Keyword.WithTrailingNewLine()
                        )
                    );
            }

            if (node.ExpressionBody != null)
            {
                return base.VisitAccessorDeclaration(
                        node.WithSemicolonToken(
                            node.SemicolonToken.WithTrailingNewLine()
                        )
                    );
            }

            return node.WithSemicolonToken(
                    node.SemicolonToken
                        .WithTrailingSpace()
                );
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var normalized = NormalizeMethodOrConstructor(node)
                .WithReturnType(
                    node.ReturnType
                        .WithTrailingSpace()
                );

            if (!normalized.SemicolonToken.IsKind(SyntaxKind.None))
            {
                normalized = normalized.WithSemicolonToken(
                    normalized.SemicolonToken.WithTrailingNewLine()
                );
            }

            return base.VisitMethodDeclaration(normalized);
        }

        private TNode NormalizeMethodOrConstructor<TNode>(TNode node)
            where TNode : BaseMethodDeclarationSyntax
        {
            BaseMethodDeclarationSyntax normalized = NormalizeMemberDeclaration(node);

            if (normalized.Body != null && normalized.Body.Statements.Any())
            {
                // Add new line only if body is not empty
                normalized = normalized.WithParameterList(
                        normalized.ParameterList
                            .WithCloseParenToken(
                                normalized.ParameterList.CloseParenToken
                                    .WithTrailingNewLine()
                            )
                    );
            }

            return (TNode)normalized;
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node) => node.NormalizeWhitespace();

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            if (!(node.Statement is BlockSyntax))
            {
                node = node.WithStatement(
                        Block(node.Statement)
                    );
            }

            return base.VisitIfStatement(node);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (!node.Statements.Any())
            {
                return node.WithOpenBraceToken(
                        node.OpenBraceToken
                            .WithLeadingSpace()
                            .WithTrailingSpace()
                    ).WithCloseBraceToken(
                        node.CloseBraceToken
                            .WithTrailingNewLine()
                    );
            }

            var normalized = node.WithOpenBraceToken(
                    NormalizeBlockDelimiter(node.OpenBraceToken)
                ).WithCloseBraceToken(
                    NormalizeBlockDelimiter(node.CloseBraceToken)
                );

            depth++;

            normalized = normalized.WithStatements(
                    node.Statements
                        .Select(s => s is BlockSyntax b ? VisitBlock(b) : base.Visit(s.NormalizeWhitespace()).WithLeadingTrivia(Indent()))
                        .ToSyntaxList()
                );

            if (normalized.Statements.Count > 1 && normalized.Statements.Last() is ReturnStatementSyntax r)
            {
                var replacement = r.WithLeadingTrivia(
                        r.GetLeadingTrivia()
                         .Insert(0, CarriageReturnLineFeed)
                    );

                normalized = normalized.ReplaceNode(r, replacement);
            }

            depth--;

            return normalized;
        }

        public override SyntaxNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            var leadingWhitespace = Space;

            // Methods add a newline after the argument list, so we'll want to indent it.
            if (node.Parent is MethodDeclarationSyntax)
            {
                leadingWhitespace = Indent(depth + 1);
            }

            return node.NormalizeWhitespace()
                .WithArrowToken(
                    node.ArrowToken
                        .WithLeadingTrivia(leadingWhitespace)
                        .WithTrailingSpace()
                );
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            return base.VisitAssignmentExpression(
                    node.WithOperatorToken(
                        node.OperatorToken.WithTrailingSpace() // HACK This is brittle and will only work for very specific situations
                    )
                );
        }

        public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            return base.VisitEqualsValueClause(
                    node.WithEqualsToken(
                        node.EqualsToken
                            .WithLeadingSpace()
                            .WithTrailingSpace()
                    )
                );
        }

        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.SemicolonToken) && token.Parent is StatementSyntax)
            {
                return token.WithTrailingNewLine();
            }

            return base.VisitToken(token);
        }

        private TypeDeclarationSyntax OrganizeMembers(TypeDeclarationSyntax node)
        {
            var members = new List<MemberDeclarationSyntax>();

            void AddMemberBlock(IEnumerable<MemberDeclarationSyntax> newMembers)
            {
                if (members.Any() && newMembers.Any())
                {
                    int index = members.Count - 1;
                    members[index] = AddTrailingNewLine(members[index]);
                }

                members.AddRange(newMembers);
            }

            var fields = node.Members.Where(m => m.IsKind(SyntaxKind.FieldDeclaration));
            var constants = fields.Where(f => f.ChildTokens().Any(t => t.Kind() == SyntaxKind.ConstKeyword));
            var staticFields = fields.Where(f => f.ChildTokens().Any(t => t.Kind() == SyntaxKind.StaticKeyword));

            fields = fields.Except(constants).Except(staticFields);

            AddMemberBlock(constants);
            AddMemberBlock(staticFields);
            AddMemberBlock(fields);

            var constructors = node.Members.Where(m => m.IsKind(SyntaxKind.ConstructorDeclaration));

            AddMemberBlock(AddLineSeparators(constructors));

            var properties = node.Members.Where(m => m.IsKind(SyntaxKind.PropertyDeclaration));

            if (AddLineSeparatorsBetweenProperties)
            {
                properties = AddLineSeparators(properties);
            }

            AddMemberBlock(properties);

            var eventFields = node.Members.Where(m => m.IsKind(SyntaxKind.EventFieldDeclaration));

            AddMemberBlock(eventFields);

            var eventProperties = node.Members.Where(m => m.IsKind(SyntaxKind.EventDeclaration));

            if (AddLineSeparatorsBetweenProperties)
            {
                eventProperties = AddLineSeparators(eventProperties);
            }

            AddMemberBlock(eventProperties);

            var methods = node.Members.Where(m => m.IsKind(SyntaxKind.MethodDeclaration));

            AddMemberBlock(AddLineSeparators(methods));

            var types = node.Members.Where(m => m.IsKind(SyntaxKind.ClassDeclaration)
                                             || m.IsKind(SyntaxKind.StructDeclaration)
                                             || m.IsKind(SyntaxKind.EnumDeclaration)
                                             || m.IsKind(SyntaxKind.InterfaceDeclaration));

            AddMemberBlock(AddLineSeparators(types));

            return node.WithMembers(members.ToSyntaxList());
        }

        private TNode AddTrailingNewLine<TNode>(TNode node)
            where TNode : SyntaxNode
        {
            var lastToken = node.GetLastToken();

            return node.ReplaceToken(
                    lastToken,
                    lastToken.WithTrailingTrivia(
                        lastToken.TrailingTrivia.Add(CarriageReturnLineFeed)
                    )
                );
        }

        private IEnumerable<MemberDeclarationSyntax> AddLineSeparators(IEnumerable<MemberDeclarationSyntax> members)
        {
            if (members is null || !members.Any())
            {
                yield break;
            }

            using var cursor = members.GetEnumerator();

            cursor.MoveNext();
            yield return cursor.Current; // Do not add line to first element

            while (cursor.MoveNext())
            {
                var firstToken = cursor.Current.GetFirstToken();

                yield return cursor.Current.ReplaceToken(
                        firstToken,
                        firstToken.WithLeadingTrivia(
                            firstToken.LeadingTrivia
                                .Insert(0, CarriageReturnLineFeed)
                        )
                    );
            }
        }

        private TType NormalizeTypeDeclaration<TType>(TType node)
            where TType : BaseTypeDeclarationSyntax
        {
            return NormalizeMemberDeclaration(node)
                .WithIdentifier(
                    node.Identifier
                        .WithLeadingSpace()
                ).WithOpenBraceToken(
                    node.OpenBraceToken
                        .WithLeadingTrivia(CarriageReturnLineFeed, Indent(depth))
                        .WithTrailingNewLine()
                ).WithCloseBraceToken(
                    node.CloseBraceToken
                        .WithLeadingTrivia(Indent(depth))
                        .WithTrailingNewLine()
                ).CastAs<TType>();
        }

        private TMember NormalizeMemberDeclaration<TMember>(TMember node)
            where TMember : MemberDeclarationSyntax
        {
            var attributes = node.AttributeLists
                .Select(a => a.WithLeadingTrivia(Indent())
                              .WithTrailingNewLine())
                .ToSyntaxList();

            var member = node.WithModifiers(
                    node.Modifiers.Select(m => m.WithTrailingSpace()).ToTokenList());

            return node.WithModifiers(
                    node.Modifiers
                        .Select(m => m.WithTrailingSpace())
                        .ToTokenList()
                ).WithAttributeLists(List<AttributeListSyntax>()) // Remove attributes so that leading indent doesn't get applied to them
                 .WithLeadingTrivia(Indent())
                 .WithAttributeLists(attributes)
                 .CastAs<TMember>();
        }

        private SyntaxToken NormalizeBlockDelimiter(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.OpenBraceToken) || token.IsKind(SyntaxKind.CloseBraceToken))
            {
                return token.WithLeadingTrivia(Indent())
                            .WithTrailingNewLine();
            }

            return token;
        }

        private SyntaxTrivia Indent() => Indent(depth);

        private SyntaxTrivia Indent(int count) 
            => count > 0 ? Whitespace(indentation.Repeat(count)) : default;
    }
}
