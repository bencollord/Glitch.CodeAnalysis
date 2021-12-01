namespace Glitch.CodeAnalysis.Builders
{
    public interface IMethodSetBuilder
    {
        MethodSyntaxBuilder HasMethod(string name);
    }
}
