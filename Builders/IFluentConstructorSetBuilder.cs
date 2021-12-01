using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IFluentConstructorSetBuilder<TDerived> : IConstructorSetBuilder 
        where TDerived : IFluentConstructorSetBuilder<TDerived>
    {
        TDerived HasConstructor(Action<ConstructorSyntaxBuilder> config);
        TDerived HasConstructors(Action<IConstructorSetBuilder> config);
        TDerived WithConstructor(ConstructorDeclarationSyntax ctor);
        TDerived WithConstructor(ConstructorSyntaxBuilder ctor);
        TDerived WithDefaultConstructor();
    }
}