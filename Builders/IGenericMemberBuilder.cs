using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IGenericMemberBuilder<TDerived>
        where TDerived : IGenericMemberBuilder<TDerived>
    {
        TDerived HasTypeParameter(string type);
        TDerived HasTypeParameters(params string[] types);
        TDerived HasTypeParameters(IEnumerable<string> types);

        TDerived HasTypeParameter(TypeParameterSyntax type);
        TDerived HasTypeParameters(params TypeParameterSyntax[] types);
        TDerived HasTypeParameters(IEnumerable<TypeParameterSyntax> types);

        TDerived HasTypeParameters(int count);

        TDerived WithoutTypeParameters();
    }
}
