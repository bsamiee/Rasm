using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rasm.Csp.Kernel;

// --- [SERVICES] ------------------------------------------------------------------------

// RS1008/RS1009 state lives at CompilationStart; rule checks stay total and stateless.
internal sealed class CompilationFacts {
    private const string ScopeAttributeMetadataName = "Rasm.Csp.CspScopeAttribute";
    private const string ScopeBuildProperty = "build_property.CspScope";
    private const string ScopeTreeKey = "csp.scope";

    private readonly CspScope? _assemblyScope;
    private readonly ImmutableDictionary<string, ImmutableHashSet<ISymbol>> _banned;
    private readonly Compilation _compilation;
    private readonly AnalyzerConfigOptionsProvider _options;
    private readonly ConcurrentDictionary<string, INamedTypeSymbol?> _wellKnown = new(StringComparer.Ordinal);

    public CompilationFacts(CompilationStartAnalysisContext context) {
        _ = context ?? throw new ArgumentNullException(paramName: nameof(context));
        _compilation = context.Compilation;
        _options = context.Options.AnalyzerConfigOptionsProvider;
        _banned = ResolveBanned(_compilation);
        PrefixVocabulary = Vocabulary.Prefixes;
        _assemblyScope = DeclaredAssemblyScope(_compilation, _options, WellKnown(ScopeAttributeMetadataName));
    }

    public ImmutableArray<string> PrefixVocabulary { get; }

    // Cache metadata probes; callers compare with SymbolEqualityComparer.
    public INamedTypeSymbol? WellKnown(string metadataName) =>
        _wellKnown.GetOrAdd(metadataName, _compilation.GetTypeByMetadataName);

    // Scope priority: type marker, tree option, then build or assembly marker; undeclared is Domain.
    public CspScope ScopeOf(SyntaxTree tree, ISymbol? symbol) {
        _ = tree ?? throw new ArgumentNullException(paramName: nameof(tree));
        return TypeScope(symbol, WellKnown(ScopeAttributeMetadataName)) switch {
            CspScope fromType => fromType,
            _ => TreeScope(tree) switch {
                CspScope fromTree => fromTree,
                _ => _assemblyScope ?? CspScope.Domain,
            },
        };
    }

    public bool HasSection(string section) => _banned.ContainsKey(section);

    public bool IsBanned(string section, ISymbol symbol) =>
        symbol is not null
        && _banned.TryGetValue(section, out ImmutableHashSet<ISymbol>? banned)
        && banned.Contains(symbol.OriginalDefinition);

    public bool TryParameter(string ruleId, string name, out string value) {
        bool found = _options.GlobalOptions.TryGetValue("csp." + ruleId + "." + name, out string? configured)
            && !string.IsNullOrEmpty(configured);
        value = found ? configured! : string.Empty;
        return found;
    }

    private CspScope? TreeScope(SyntaxTree tree) =>
        _options.GetOptions(tree).TryGetValue(ScopeTreeKey, out string? declared)
            && Enum.TryParse(declared, ignoreCase: true, out CspScope fromTree)
            ? fromTree
            : null;

    private static CspScope? AttributeScope(ISymbol symbol, INamedTypeSymbol? marker) {
        foreach (AttributeData attribute in symbol.GetAttributes()) {
            bool match = SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker)
                && attribute.ConstructorArguments.Length > 0
                && attribute.ConstructorArguments[0].Value is int;
            if (match) return (CspScope)attribute.ConstructorArguments[0].Value!;
        }
        return null;
    }

    private static CspScope? DeclaredAssemblyScope(Compilation compilation, AnalyzerConfigOptionsProvider options, INamedTypeSymbol? marker) =>
        options.GlobalOptions.TryGetValue(ScopeBuildProperty, out string? declared)
            && Enum.TryParse(declared, ignoreCase: true, out CspScope fromBuild)
            ? fromBuild
            : marker is null ? null : AttributeScope(compilation.Assembly, marker);

    private static CspScope? TypeScope(ISymbol? symbol, INamedTypeSymbol? marker) {
        if (marker is null) return null;
        for (INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol?.ContainingType; type is not null; type = type.ContainingType)
            if (AttributeScope(type, marker) is CspScope declared) return declared;
        return null;
    }

    private static ImmutableDictionary<string, ImmutableHashSet<ISymbol>> ResolveBanned(Compilation compilation) {
        ImmutableDictionary<string, ImmutableHashSet<ISymbol>>.Builder banned =
            ImmutableDictionary.CreateBuilder<string, ImmutableHashSet<ISymbol>>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, ImmutableArray<string>> section in Vocabulary.BannedSections) {
            ImmutableHashSet<ISymbol>.Builder symbols = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);
            foreach (string docId in section.Value)
                foreach (ISymbol symbol in DocumentationCommentId.GetSymbolsForDeclarationId(docId, compilation))
                    _ = symbols.Add(symbol.OriginalDefinition);
            banned.Add(section.Key, symbols.ToImmutable());
        }
        return banned.ToImmutable();
    }
}
