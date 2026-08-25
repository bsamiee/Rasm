using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Csp.Kernel;

namespace Rasm.Csp.Tests;

// --- [TYPES] ---------------------------------------------------------------------------

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class RuleSpecAttribute(string id) : Attribute {
    public string Id { get; } = id;
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class PositiveAttribute(params string[] ids) : Attribute {
    public IReadOnlyList<string> Ids { get; } = ids;
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class NegativeAttribute(params string[] ids) : Attribute {
    public IReadOnlyList<string> Ids { get; } = ids;
}

// --- [MODELS] --------------------------------------------------------------------------

internal sealed class CspTest : CSharpAnalyzerTest<Driver, DefaultVerifier> {
    public CspTest(
        string source,
        CspScope scope = CspScope.Domain,
        bool generatorRun = false,
        string? config = null,
        params (string Name, string Content)[] data) {
        TestCode = source;
        ReferenceAssemblies = Harness.References;
        TestState.AdditionalReferences.Add(typeof(CspScopeAttribute).Assembly);
        TestState.AnalyzerConfigFiles.Add(("/.globalconfig", GlobalConfig(scope, config)));
        foreach ((string name, string content) in data) TestState.AdditionalFiles.Add((name, content));
        string assemblyName = "Csp.Spec." + Fingerprint(source);
        SolutionTransforms.Add((solution, projectId) => solution.WithProjectAssemblyName(projectId, assemblyName));
        if (generatorRun) {
            SolutionTransforms.Add(static (solution, projectId) =>
                solution.AddAnalyzerReference(projectId, new AnalyzerFileReference(Harness.ThinktectureGeneratorPath.Value, Harness.GeneratorLoader.Instance)));
        }
    }

    protected override ParseOptions CreateParseOptions() =>
        ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.Preview);

    private static string Fingerprint(string source) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))[..16];

    private static string GlobalConfig(CspScope scope, string? config) =>
        "is_global = true\nbuild_property.CspScope = " + scope + (config is null ? string.Empty : "\n" + config) + "\n";
}

// --- [SERVICES] ------------------------------------------------------------------------

internal static class Harness {
    public static readonly ImmutableArray<PackageIdentity> MirroredPackages = [
        new PackageIdentity(id: "LanguageExt.Core", version: "5.0.0-beta-77"),
        new PackageIdentity(id: "Thinktecture.Runtime.Extensions", version: "10.4.0"),
    ];

    public static readonly ReferenceAssemblies References = new ReferenceAssemblies(
            targetFramework: "net10.0",
            referenceAssemblyPackage: new PackageIdentity(id: "Microsoft.NETCore.App.Ref", version: "10.0.0"),
            referenceAssemblyPath: Path.Combine("ref", "net10.0"))
        .AddPackages(MirroredPackages);

    internal static readonly Lazy<string> ThinktectureGeneratorPath = new(valueFactory: static () => {
        string versionDir = Path.GetFullPath(Path.Combine(typeof(UnionAttribute).Assembly.Location, "..", "..", ".."));
        string packagesRoot = Path.GetFullPath(Path.Combine(versionDir, "..", ".."));
        return Path.Combine(
            packagesRoot,
            "thinktecture.runtime.extensions.sourcegenerator",
            Path.GetFileName(versionDir),
            "analyzers", "dotnet", "cs",
            "Thinktecture.Runtime.Extensions.SourceGenerator.dll");
    });

    public static Task VerifyAsync(string source, params DiagnosticResult[] expected) =>
        VerifyAsync(new CspTest(source), expected);

    public static Task VerifyAsync(CspTest test, params DiagnosticResult[] expected) {
        ArgumentNullException.ThrowIfNull(argument: test);
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync(TestContext.Current.CancellationToken);
    }

    internal sealed class GeneratorLoader : IAnalyzerAssemblyLoader {
        public static readonly GeneratorLoader Instance = new();
        public void AddDependencyLocation(string fullPath) { }
        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
