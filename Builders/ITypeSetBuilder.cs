namespace Glitch.CodeAnalysis.Builders
{
    public interface ITypeSetBuilder
    {
        TypeSyntaxBuilder HasType(string name);
    }
}