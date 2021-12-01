using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Glitch.CodeAnalysis.Builders
{
    public interface ITypeBuilder<TDerived> 
        : IMemberBuilder<TDerived>, 
          IGenericMemberBuilder<TDerived>, 
          IFluentFieldSetBuilder<TDerived>, 
          IFluentConstructorSetBuilder<TDerived>, 
          IFluentPropertySetBuilder<TDerived>, 
          IFluentMethodSetBuilder<TDerived>
            where TDerived : ITypeBuilder<TDerived>
    {
        TDerived Extends<T>();
        TDerived Extends(Type baseType);
        TDerived Extends(string baseType);
        TDerived Extends(TypeSyntax baseType);
        TDerived Extends(BaseTypeSyntax baseType);

        TDerived Implements<T>();
        TDerived Implements(Type interfaceType);
        TDerived Implements(string interfaceType);
        TDerived Implements(TypeSyntax interfaceType);
        TDerived Implements(BaseTypeSyntax interfaceType);

        TDerived WithoutBaseTypes();

        TDerived ValueType();
        TDerived ReferenceType();
    }
}