using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IFluentMethodSetBuilder<TDerived> : IMethodSetBuilder 
        where TDerived : IFluentMethodSetBuilder<TDerived>
    {
        TDerived HasMethod(string name, Action<MethodSyntaxBuilder> config);
        TDerived HasMethods(Action<IMethodSetBuilder> config);
        TDerived WithMethod(MethodDeclarationSyntax method);
        TDerived WithMethod(MethodSyntaxBuilder method);
    }
}