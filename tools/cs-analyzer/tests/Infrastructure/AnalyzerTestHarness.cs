using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Foundation.CSharp.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.CSharp.Analyzers.Tests.Infrastructure;

internal static class AnalyzerTestHarness {
    private static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
    private static readonly CSharpCompilationOptions CompilationOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        checkOverflow: true,
        nullableContextOptions: NullableContextOptions.Enable,
        concurrentBuild: false);
    private static readonly ImmutableArray<MetadataReference> FrameworkReferences = LoadFrameworkReferences();
    private static readonly DomainStandardsAnalyzer Analyzer = new();
    private static readonly string AnalyzerRelativeDirectory = Path.Combine("tools", "cs-analyzer");
    private static readonly string AnalyzerProjectRelativePath = Path.Combine(AnalyzerRelativeDirectory, "CsAnalyzer.csproj");

    internal static string RepositoryRoot { get; } = ResolveRepositoryRoot(startPath: AppContext.BaseDirectory);
    internal static string AnalyzerDirectory { get; } = Path.Combine(RepositoryRoot, AnalyzerRelativeDirectory);
    internal static string UnshippedReleasePath { get; } = Path.Combine(AnalyzerDirectory, "AnalyzerReleases.Unshipped.md");
    internal static string ShippedReleasePath { get; } = Path.Combine(AnalyzerDirectory, "AnalyzerReleases.Shipped.md");

    internal static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics() =>
        [.. Analyzer.SupportedDiagnostics.OrderBy(static descriptor => descriptor.Id, StringComparer.Ordinal)];
    internal static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, string filePath) {
        SourceText sourceText = SourceText.From(source, Encoding.UTF8);
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(text: sourceText, options: ParseOptions, path: filePath, cancellationToken: CancellationToken.None);
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: BuildAssemblyName(source: source, filePath: filePath),
            syntaxTrees: [syntaxTree],
            references: FrameworkReferences,
            options: CompilationOptions);
        AnalyzerOptions analyzerOptions = new(additionalFiles: []);
        CompilationWithAnalyzersOptions options = new(
            options: analyzerOptions,
            onAnalyzerException: static (exception, _, _) => throw new InvalidOperationException(message: "Analyzer execution threw an exception.", innerException: exception),
            concurrentAnalysis: false,
            logAnalyzerExecutionTime: false,
            reportSuppressedDiagnostics: false);
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers: [Analyzer], analysisOptions: options);
        ImmutableArray<Diagnostic> diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(CancellationToken.None).ConfigureAwait(true);
        return [.. diagnostics
            .OrderBy(static diagnostic => diagnostic.Id, StringComparer.Ordinal)
            .ThenBy(static diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line)];
    }

    private static string ResolveRepositoryRoot(string startPath) =>
        ResolveRepositoryRoot(new DirectoryInfo(startPath));

    private static string ResolveRepositoryRoot(DirectoryInfo? directory) =>
        directory switch {
            null => throw new InvalidOperationException(message: "Unable to resolve repository root from test host base directory."),
            { FullName: string fullName } when File.Exists(Path.Combine(fullName, AnalyzerProjectRelativePath)) => fullName,
            _ => ResolveRepositoryRoot(directory.Parent),
        };
    private static string BuildAssemblyName(string source, string filePath) {
        byte[] hashInput = Encoding.UTF8.GetBytes($"{filePath}\n{source}");
        byte[] hash = SHA256.HashData(hashInput);
        string suffix = Convert.ToHexString(hash.AsSpan(start: 0, length: 8));
        return $"Foundation.CSharp.Analyzers.Tests.{suffix}";
    }
    private static ImmutableArray<MetadataReference> LoadFrameworkReferences() {
        string trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string
            ?? throw new InvalidOperationException(message: "TRUSTED_PLATFORM_ASSEMBLIES was not available.");
        string analyzerAssemblyPath = typeof(DomainStandardsAnalyzer).Assembly.Location;
        IEnumerable<MetadataReference> references = trustedPlatformAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Append(analyzerAssemblyPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => MetadataReference.CreateFromFile(path));
        return [.. references];
    }
}
