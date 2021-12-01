using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IFluentPropertySetBuilder<TDerived> : IPropertySetBuilder 
        where TDerived : IFluentPropertySetBuilder<TDerived>
    {
        TDerived Property(string name, Action<PropertySyntaxBuilder> config);
        TDerived Property(string name, Type type, Action<PropertySyntaxBuilder> config);
        TDerived Property<T>(string name, Action<PropertySyntaxBuilder> config);
        TDerived Properties(Action<IPropertySetBuilder> config);
        TDerived WithProperty(PropertyDeclarationSyntax property);
        TDerived WithProperty(PropertySyntaxBuilder property);
    }
}