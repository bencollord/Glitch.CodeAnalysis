using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Glitch.CodeAnalysis.Builders
{
    public abstract class BaseMethodSyntaxBuilder<TDerived, TNode> : MemberSyntaxBuilder<TDerived, TNode>
        where TDerived : BaseMethodSyntaxBuilder<TDerived, TNode>
        where TNode : BaseMethodDeclarationSyntax
    {
        private readonly TDerived self;
        private MethodBodySyntaxBuilder body = new MethodBodySyntaxBuilder();
        private List<ParameterSyntaxBuilder> parameters = new List<ParameterSyntaxBuilder>();

        protected BaseMethodSyntaxBuilder(string name) : base(name) 
        {
            self = (TDerived)this;
        }

        protected BaseMethodSyntaxBuilder(SyntaxToken name) : base(name)
        {
            self = (TDerived)this;
        }

        protected BaseMethodSyntaxBuilder(TNode node) : base(node) 
        {
            self = (TDerived)this;
        }

        public TDerived HasParameter<T>(string name) => HasParameter(name, typeof(T));

        public TDerived HasParameter(string name, Type type) => HasParameter(new ParameterSyntaxBuilder(name, type));
        
        public TDerived HasParameter(string name, TypeSyntax type) => HasParameter(new ParameterSyntaxBuilder(name, type));

        public TDerived HasParameter(ParameterSyntaxBuilder parameter)
        {
            parameters.Add(parameter);
            return self;
        }

        public ParameterSyntaxBuilder Parameter<T>(string name) => Parameter(name, typeof(T));

        public ParameterSyntaxBuilder Parameter(string name, Type type)
        {
            var parameter = new ParameterSyntaxBuilder(name, type);
            HasParameter(parameter);
            return parameter;
        }

        public TDerived WithBody(Action<MethodBodySyntaxBuilder> config)
        {
            body = new MethodBodySyntaxBuilder();
            config(body);
            return self;
        }

        public TDerived WithBody(BlockSyntax body)
        {
            this.body = new MethodBodySyntaxBuilder(body);
            return self;
        }

        public TDerived WithEmptyBody()
        {
            body.Reset();
            return self;
        }

        protected sealed override TNode ToMemberNode()
        {
            var method = ToMethodNode();

            if (parameters.Any())
            {
                method = (TNode)method.WithParameterList(
                    ParameterList(
                        SeparatedList(parameters.Select(p => p.ToSyntaxNode()))));
            }

            return (TNode)method.WithBody(body.ToSyntaxNode());
        }


        protected sealed override void SetMemberContent(TNode node)
        {
            body = new MethodBodySyntaxBuilder(node.Body);
            parameters.Clear();
            parameters.AddRange(node.ParameterList.Parameters.Select(p => new ParameterSyntaxBuilder(p)));
            SetMethodContent(node);
        }

        protected sealed override void ResetMemberContent()
        {
            parameters.Clear();
            body.Reset();
            ResetMethodContent();
        }

        protected abstract TNode ToMethodNode();

        protected abstract void SetMethodContent(TNode node);

        protected abstract void ResetMethodContent();
    }
}
