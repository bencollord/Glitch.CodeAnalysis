using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Glitch.CodeAnalysis.Builders
{
    internal class TypeParameterListBuilder : SyntaxBuilder<TypeParameterListBuilder, TypeParameterListSyntax>, IEnumerable<TypeParameterSyntax>
    {
        private List<TypeParameterSyntax> typeParameters = new List<TypeParameterSyntax>();

        public TypeParameterListBuilder() { }

        public TypeParameterListBuilder(IEnumerable<TypeParameterSyntax> typeParameters)
        {
            AddRange(typeParameters);
        }

        public TypeParameterSyntax this[int index]
        {
            get => typeParameters[index];
            set => typeParameters[index] = value;
        }

        public int Count => typeParameters.Count;

        internal TypeParameterListBuilder Add(string type) => Add(SyntaxFactory.TypeParameter(type));

        internal TypeParameterListBuilder Add(TypeParameterSyntax type)
        {
            typeParameters.Add(type);
            return this;
        }

        internal TypeParameterListBuilder AddRange(params string[] types) => AddRange(types.AsEnumerable());

        internal TypeParameterListBuilder AddRange(IEnumerable<string> types) => AddRange(types.Select(SyntaxFactory.TypeParameter));

        internal TypeParameterListBuilder AddRange(params TypeParameterSyntax[] types) => AddRange(types.AsEnumerable());

        internal TypeParameterListBuilder AddRange(IEnumerable<TypeParameterSyntax> types)
        {
            typeParameters.AddRange(types);
            return this;
        }

        internal TypeParameterListBuilder AddRange(int count) => AddRange(Enumerable.Range(typeParameters.Count, count).Select(c => $"T{c}"));

        internal TypeParameterListBuilder Insert(int index, string type) => Insert(index, SyntaxFactory.TypeParameter(type));

        internal TypeParameterListBuilder Insert(int index, TypeParameterSyntax type)
        {
            typeParameters.Insert(index, type);
            return this;
        }

        internal TypeParameterListBuilder InsertRange(int index, params string[] types) => InsertRange(index, types.AsEnumerable());

        internal TypeParameterListBuilder InsertRange(int index, IEnumerable<string> types) => InsertRange(index, types.Select(SyntaxFactory.TypeParameter));

        internal TypeParameterListBuilder InsertRange(int index, params TypeParameterSyntax[] types) => InsertRange(index, types.AsEnumerable());

        internal TypeParameterListBuilder InsertRange(int index, IEnumerable<TypeParameterSyntax> types)
        {
            typeParameters.InsertRange(index, types);
            return this;
        }

        internal TypeParameterListBuilder Remove(string type) => Remove(SyntaxFactory.TypeParameter(type));

        internal TypeParameterListBuilder Remove(TypeParameterSyntax type)
        {
            typeParameters.Remove(type);
            return this;
        }

        internal TypeParameterListBuilder RemoveRange(int start, int count)
        {
            typeParameters.RemoveRange(start, count);
            return this;
        }

        internal TypeParameterListBuilder RemoveAt(int index)
        {
            typeParameters.RemoveAt(index);
            return this;
        }

        internal TypeParameterListBuilder Clear()
        {
            typeParameters.Clear();
            return this;
        }

        public IEnumerator<TypeParameterSyntax> GetEnumerator() => typeParameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override TypeParameterListSyntax BuildSyntaxNode() 
            => SyntaxFactory.TypeParameterList(typeParameters.ToSeparatedList());

        protected override void ResetContent() => Clear();

        protected override void SetContent(TypeParameterListSyntax node)
        {
            Clear();
            typeParameters.AddRange(node.Parameters);
        }
    }
}
