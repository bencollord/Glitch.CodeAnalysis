using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IFieldSetBuilder
    {
        FieldSyntaxBuilder Field(string name);
        FieldSyntaxBuilder Field(string name, Type type);
        FieldSyntaxBuilder Field(string name, TypeSyntax type);
        FieldSyntaxBuilder Field<T>(string name);
    }
}