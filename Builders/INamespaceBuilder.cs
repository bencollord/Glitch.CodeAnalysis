using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Glitch.CodeAnalysis.Builders
{
    public interface INamespaceBuilder<TDerived>
        where TDerived : INamespaceBuilder<TDerived>
    {
        TDerived Namespace(string namespaceName);
        TDerived Namespace(NameSyntax namespaceName);
        
        TDerived SubNamespace(string namespaceName);
        TDerived SubNamespace(NameSyntax namespaceName);

        TDerived Using(string namespaceName);
        TDerived Using(NameSyntax namespaceName);
        TDerived Using(params string[] namespaceNames);
        TDerived Using(params NameSyntax[] namespaceNames);
        TDerived Using(IEnumerable<string> namespaceNames);
        TDerived Using(IEnumerable<NameSyntax> namespaceNames);
        TDerived WithoutUsings();
    }
}
