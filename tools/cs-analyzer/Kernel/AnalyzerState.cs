using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Foundation.CSharp.Analyzers.Kernel;

// --- [SERVICES] ----------------------------------------------------------------

internal sealed class AnalyzerState {
    // --- [CONSTANTS] -----------------------------------------------------------

    private static readonly IEqualityComparer<ISymbol> SymbolComparer = SymbolEqualityComparer.Default;
    private static readonly IEqualityComparer<INamedTypeSymbol> NamedTypeComparer = SymbolEqualityComparer.Default;

    // --- [FIELDS] --------------------------------------------------------------

    private readonly ConcurrentDictionary<ISymbol, ScopeInfo> _scopeCache = new(comparer: SymbolComparer);
    private readonly ConcurrentDictionary<IMethodSymbol, byte> _privateMethods = new(comparer: SymbolComparer);
    private readonly ConcurrentDictionary<IMethodSymbol, int> _methodInvocationCounts = new(comparer: SymbolComparer);
    private readonly ConcurrentDictionary<INamedTypeSymbol, byte> _namedTypes = new(comparer: NamedTypeComparer);
    private readonly ConcurrentDictionary<INamedTypeSymbol, ImmutableHashSet<INamedTypeSymbol>> _derivedTypes = new(comparer: NamedTypeComparer);
    private readonly ConcurrentDictionary<INamedTypeSymbol, ImmutableHashSet<INamedTypeSymbol>> _interfaceImplementations = new(comparer: NamedTypeComparer);
    private readonly ConcurrentDictionary<INamedTypeSymbol, byte> _flagsEnums = new(comparer: NamedTypeComparer);
    private readonly ConcurrentDictionary<INamedTypeSymbol, byte> _flagsEnumCompositionSites = new(comparer: NamedTypeComparer);
    private readonly ConcurrentBag<ClosedUnionDispatchFact> _closedUnionDispatches = [];

    // --- [CONSTRUCTORS] --------------------------------------------------------

    private AnalyzerState(Compilation compilation, INamespaceSymbol? languageExtNamespace, INamedTypeSymbol? languageExtCommonErrorType) {
        Compilation = compilation;
        LanguageExtNamespace = languageExtNamespace;
        LanguageExtCommonErrorType = languageExtCommonErrorType;
    }

    // --- [FACTORIES] -----------------------------------------------------------

    internal static AnalyzerState Create(Compilation compilation) {
        INamespaceSymbol? languageExtNamespace = compilation.GetTypeByMetadataName("LanguageExt.Option`1")?.ContainingNamespace;
        INamedTypeSymbol? languageExtCommonErrorType = compilation.GetTypeByMetadataName("LanguageExt.Common.Error");
        return new AnalyzerState(compilation: compilation, languageExtNamespace: languageExtNamespace, languageExtCommonErrorType: languageExtCommonErrorType);
    }

    // --- [PROPERTIES] ----------------------------------------------------------

    internal Compilation Compilation { get; }
    internal INamespaceSymbol? LanguageExtNamespace { get; }
    internal INamedTypeSymbol? LanguageExtCommonErrorType { get; }

    // --- [OPERATIONS] ----------------------------------------------------------

    internal static void Report(Action<Diagnostic> report, Diagnostic? diagnostic) {
        switch (diagnostic) {
            case Diagnostic d:
                report(d);
                break;
        }
    }
    internal static void ReportEach(Action<Diagnostic> report, IEnumerable<Diagnostic> diagnostics) {
        foreach (Diagnostic d in diagnostics) {
            report(d);
        }
    }
    internal ScopeInfo ScopeFor(ISymbol symbol) =>
        _scopeCache.GetOrAdd(key: symbol, valueFactory: ScopeModel.Classify);

    // --- [TRACKING] ------------------------------------------------------------

    internal void TrackPrivateMethod(IMethodSymbol method) =>
        _ = (method.DeclaredAccessibility, method.MethodKind) switch {
            (Accessibility.Private, MethodKind.Ordinary) => _privateMethods.TryAdd(key: method, value: 0),
            _ => false,
        };
    internal void TrackMethodInvocation(IMethodSymbol? method) =>
        _ = method switch {
            IMethodSymbol calledMethod
                when calledMethod.DeclaredAccessibility == Accessibility.Private
                     && calledMethod.MethodKind == MethodKind.Ordinary
                     && SymbolEqualityComparer.Default.Equals(calledMethod.ContainingAssembly, Compilation.Assembly)
                => _methodInvocationCounts.AddOrUpdate(key: calledMethod, addValueFactory: static _ => 1, updateValueFactory: static (_, current) => current + 1),
            _ => 0,
        };
    internal void TrackNamedType(INamedTypeSymbol namedType) {
        _ = _namedTypes.TryAdd(key: namedType, value: 0);
        _ = namedType.BaseType switch {
            INamedTypeSymbol baseType => _derivedTypes.AddOrUpdate(
                key: baseType.OriginalDefinition,
                addValueFactory: _ => ImmutableHashSet.Create<INamedTypeSymbol>(SymbolEqualityComparer.Default, namedType),
                updateValueFactory: (_, current) => current.Add(namedType)),
            _ => [],
        };
    }
    internal void TrackInterfaceImplementations(INamedTypeSymbol namedType) {
        // Roslyn API enumeration -- not domain code
        foreach (INamedTypeSymbol interfaceSymbol in (namedType.TypeKind, namedType.IsAbstract, namedType.IsStatic) switch {
            (TypeKind.Class, false, false) or (TypeKind.Struct, false, _) => namedType.AllInterfaces,
            _ => [],
        }) {
            _ = _interfaceImplementations.AddOrUpdate(
                key: interfaceSymbol,
                addValueFactory: _ => ImmutableHashSet.Create<INamedTypeSymbol>(SymbolEqualityComparer.Default, namedType),
                updateValueFactory: (_, current) => current.Add(namedType));
        }
    }
    internal void TrackFlagsEnum(INamedTypeSymbol enumType) =>
        _ = _flagsEnums.TryAdd(key: enumType, value: 0);
    internal void TrackFlagsEnumCompositionSite(INamedTypeSymbol enumType) =>
        _ = _flagsEnumCompositionSites.TryAdd(key: enumType, value: 0);
    internal void TrackClosedUnionDispatch(ISymbol containingSymbol, IInvocationOperation invocation) {
        if (SymbolFacts.TryClosedUnionDispatch(compilation: Compilation, containingSymbol: containingSymbol, invocation: invocation, fact: out ClosedUnionDispatchFact fact)) {
            _closedUnionDispatches.Add(item: fact);
        }
    }

    // --- [REPORTS] -------------------------------------------------------------

    internal ImmutableArray<(INamedTypeSymbol Interface, INamedTypeSymbol Implementation)> SingleImplementationInterfaces() =>
        [
            .. _interfaceImplementations
                .Where(entry => entry.Value.Count == 1)
                .Select(entry => (Interface: entry.Key, Implementation: entry.Value.Single())),
        ];
    internal ImmutableArray<IMethodSymbol> SingleUsePrivateMethods() =>
        [
            .. _privateMethods.Keys
                .Where(method => _methodInvocationCounts.TryGetValue(key: method, value: out int count) && count == 1),
        ];
    internal ImmutableArray<INamedTypeSymbol> FlagsEnumsWithoutComposition() =>
        [
            .. _flagsEnums.Keys
                .Where(enumType => !_flagsEnumCompositionSites.ContainsKey(key: enumType)),
        ];
    internal ImmutableArray<INamedTypeSymbol> NamedTypes() => [.. _namedTypes.Keys];
    internal ImmutableHashSet<INamedTypeSymbol> DerivedTypes(INamedTypeSymbol baseType) =>
        _derivedTypes.TryGetValue(key: baseType.OriginalDefinition, value: out ImmutableHashSet<INamedTypeSymbol> derived)
            ? derived
            : [];
    internal ImmutableArray<ClosedUnionDispatchFact> ClosedUnionDispatches() => [.. _closedUnionDispatches];

}
