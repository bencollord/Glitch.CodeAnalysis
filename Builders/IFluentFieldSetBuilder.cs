using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IFluentFieldSetBuilder<TDerived> : IFieldSetBuilder 
        where TDerived : IFluentFieldSetBuilder<TDerived>
    {
        TDerived Field(string name, Action<FieldSyntaxBuilder> config);
        TDerived Field(string name, Type type, Action<FieldSyntaxBuilder> config);
        TDerived Field<T>(string name, Action<FieldSyntaxBuilder> config);
        TDerived Fields(Action<IFieldSetBuilder> config);
        TDerived WithField(FieldDeclarationSyntax field);
        TDerived WithField(FieldSyntaxBuilder field);
    }
}