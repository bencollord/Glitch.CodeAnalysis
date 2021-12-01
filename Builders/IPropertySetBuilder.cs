using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IPropertySetBuilder
    {
        PropertySyntaxBuilder Property(string name);
        PropertySyntaxBuilder Property(string name, Type type);
        PropertySyntaxBuilder Property(string name, TypeSyntax type);
        PropertySyntaxBuilder Property<T>(string name);
    }
}