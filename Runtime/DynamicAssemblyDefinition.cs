using Glitch.CodeAnalysis.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Glitch.CodeAnalysis.Runtime
{
    public class DynamicAssemblyDefinition
    {
        private string name;
        private string defaultNamespace;
        private List<TypeSyntaxBuilder> types = new List<TypeSyntaxBuilder>();
        private List<MetadataReference> references = new List<MetadataReference>();

        public DynamicAssemblyDefinition(string name)
        {
            this.name = name;
            defaultNamespace = name;
            ReferenceCoreLibraries();
        }

        public static DynamicAssemblyDefinition Anonymous() => new DynamicAssemblyDefinition(Path.GetRandomFileName());

        public DynamicAssemblyDefinition UseDefaultNamespace(string defaultNamespace)
        {
            this.defaultNamespace = defaultNamespace;
            return this;
        }

        public TypeSyntaxBuilder DefineType(string name)
        {
            var type = new TypeSyntaxBuilder(name);
            
            type.Namespace(defaultNamespace)
                .Using("System");

            types.Add(type);

            return type;
        }

        public DynamicAssemblyDefinition DefineType(string name, Action<TypeSyntaxBuilder> config)
        {
            config(DefineType(name));
            return this;
        }

        public TypeSyntaxBuilder DefineAnonymousType() => DefineType($"<AnonymousType>_{Guid.NewGuid()}");

        public DynamicAssemblyDefinition DefineAnonymousType(Action<TypeSyntaxBuilder> config)
        {
            config(DefineAnonymousType());
            return this;
        }

        public DynamicAssemblyDefinition Reference(Assembly assembly)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
            return this;
        }

        public Assembly Build()
        {
            var trees = from t in types
                        let comp = t.ToCompilationUnit()
                        select SyntaxFactory.SyntaxTree(comp);

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilation = CSharpCompilation.Create(name, trees, references.ToArray(), options);

            using var stream = new MemoryStream();

            var result = compilation.Emit(stream);

            if (!result.Success)
            {
                var errors = result.Diagnostics.Where(e => e.Severity == DiagnosticSeverity.Error);

                throw new DynamicCompilationException(errors);
            }

            stream.Seek(0, SeekOrigin.Begin);

            return Assembly.Load(stream.ToArray());
        }

        private void ReferenceCoreLibraries()
        {
            Reference(typeof(object).Assembly)
                .Reference(typeof(Enumerable).Assembly);
        }
    }
}
