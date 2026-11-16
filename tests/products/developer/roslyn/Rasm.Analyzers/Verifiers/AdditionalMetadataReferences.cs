using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Testing;
using Thinktecture.CodeAnalysis.SmartEnums;
using Xunit;

namespace Rasm.Analyzers.Tests.Verifiers;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class AdditionalMetadataReferences {
    private static readonly FrozenDictionary<string, string> Stated = toSeq(typeof(AdditionalMetadataReferences).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        .Choose(static held => Optional(held.Value).Map(value => (held.Key, Value: value)))
        .ToFrozenDictionary(static held => held.Key, static held => held.Value, StringComparer.Ordinal);

    private static readonly string[] TargetingPack = Stated[nameof(TargetingPack)].Split('|');

    public static ReferenceAssemblies Framework { get; } = new(TargetingPack[2], new PackageIdentity(TargetingPack[0], TargetingPack[1]), Path.Combine("ref", TargetingPack[2]));

    public static ImmutableArray<PortableExecutableReference> Packages { get; } = [.. Stated
        .Where(static reference => reference.Key.StartsWith("AdditionalReference.", StringComparison.Ordinal))
        .Select(static reference => MetadataReference.CreateFromFile(reference.Value))];

    public static ImmutableArray<Type> Generators { get; } = [.. typeof(SmartEnumSourceGenerator).Assembly.GetExportedTypes()
        .Where(static type => type.IsDefined(typeof(GeneratorAttribute), inherit: false))];

    public static async Task<CSharpCompilation> CompiledAsync(string name, string source, CancellationToken token) =>
        CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, cancellationToken: token)], await Framework.ResolveAsync(LanguageNames.CSharp, token).ConfigureAwait(false), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    public static PortableExecutableReference Emitted(CSharpCompilation compilation, CancellationToken token) {
        using MemoryStream image = new();
        EmitResult emitted = compilation.Emit(image, cancellationToken: token);
        Assert.True(emitted.Success, string.Join('\n', emitted.Diagnostics));
        image.Position = 0;
        return MetadataReference.CreateFromStream(image);
    }
}
