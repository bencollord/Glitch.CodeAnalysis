using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Glitch.CodeAnalysis.Builders
{
    public interface IMemberBuilder<TDerived>
        where TDerived : IMemberBuilder<TDerived>
    {
        TDerived Internal();
        TDerived Private();
        TDerived PrivateProtected();
        TDerived Protected();
        TDerived ProtectedInternal();
        TDerived Public();
        
        TDerived Static();
        TDerived Instance();

        TDerived WithoutModifiers();

        TDerived WithAttribute(AttributeSyntax attribute);
        TDerived WithAttributeList(AttributeListSyntax attributeList);
        TDerived WithoutAttributes();
    }
}