# [04] Target Architecture — Folder, Kernel, Rule Surface, Hardening

The settled architecture for the rebuilt analyzer (assembly renamed `Csp.Analyzer`, root namespace `Rasm.Csp`). Evidence base: the demolition verdicts (doc 01 §10), the doctrine partition (doc 02), the landscape shortlist (doc 03 §12), and the five red-team-forced design changes (doc 08), all of which are ALREADY resolved into the structures below. Where a structure exists because the red-team forced it, the lane is cited inline. Deviations not argued here are violations.

Baseline: 87 carried descriptors (`Kernel/RuleCatalog.cs:215-228`, all Error + NotConfigurable). The rebuild reduces the 87 to **57 active rules** (48 Law + 9 Pressure, of which 2 Law take new CSP09xx IDs), drops 10, retires 32 IDs (tombstoned), and adds 6 net-new (CSP0901-0906) for **63 total active**. Full exhaustive partition (verified disjoint sum = 87) in doc 05 §6.

## [1]-[FOLDER_ARCHITECTURE]

Add-a-rule contract (CORRECTED per 08 A1 — the 1+1+1 promise was arithmetically false): **catalog row + rule file + test file + AnalyzerReleases.Unshipped.md row + `analyzer.md#cspNNNN` anchor**. The meta-test (doc 06) asserts the Unshipped row exists so omission fails LOCALLY in the test rail, not at downstream build.

```
tools/cs-analyzer/
  Csp.Analyzer.csproj            netstandard2.0; IsRoslynComponent; EnforceExtendedAnalyzerRules;
                                 MS.CodeAnalysis.CSharp 5.3.0 + MS.CodeAnalysis.Analyzers 5.3.0 (PrivateAssets=all);
                                 <AdditionalFiles Include="AnalyzerReleases.*.md"/> (activates real RS2000-RS2008);
                                 drop stray <Compile Remove="tests/**"/> and producer-side EmitCompilerGeneratedFiles.
  AnalyzerReleases.Shipped.md    full history incl. Removed Rules tombstones for genuinely-shipped retirees only
                                 (NOT 0016/0716/0721/0722 — never shipped, see doc 05 §RESERVED + 08 C3).
  AnalyzerReleases.Unshipped.md
  Contracts/                     SEPARATE netstandard2.0 PROJECT Csp.Contracts.csproj (08 G1 — NOT a Compile-linked
                                 source file; the linked file re-creates CS0436 under the IVT grants). Zero-dependency,
                                 referenced NORMALLY by every project (ordinary ProjectReference, ReferenceOutputAssembly
                                 default). Owns: [CspScope(Scope)] (assembly+type targets), [BoundaryAdapter],
                                 [CspExempt(Justification = "...")]. Single type identity => no Skip list.
  Kernel/
    Driver.cs   (~80 LOC)        THE single sealed DiagnosticAnalyzer. Initialize is sealed ceremony:
                                 EnableConcurrentExecution (RS1026) + ConfigureGeneratedCodeAnalysis(None) (RS1025)
                                 + RegisterCompilationStartAction(ctx => new CompilationFacts(ctx) then register
                                 Catalog bindings bucketed by trigger kind). Zero per-rule boilerplate
                                 (SonarDiagnosticAnalyzer model). SupportedDiagnostics = Catalog.All descriptors.
    Catalog.cs  (~140 LOC)       ImmutableArray<RuleRow> All = [CSP0001.Row, CSP0002.Row, ...] + the per-category
                                 trivial-row tables (08 A3); Tier enum {Law=Error, Pressure=Warning, Info};
                                 Category enum {Flow, Shape, Surface, Rail, Boundary, Perf}; help-base URI;
                                 Describe factory (§3 — replaces the Error-only Err).
    Row.cs      (~60 LOC)        RuleRow record (see §3). RuleBinding = trigger + Check delegate over RuleContext.
                                 This is demolition verdict (a): ~70 identical Check* bodies become data rows + ONE executor.
    Facts.cs    (~150 LOC)       Per-compilation state (RS1008/RS1009). ALL well-known symbols resolved ONCE via
                                 GetTypeByMetadataName at CompilationStart, compared with SymbolEqualityComparer
                                 (verdict c — bans the stringly ToDisplayString()=="System.Threading.Tasks.Task" pattern).
                                 Config readers (§5). Scope resolution (§4).
    Walkers.cs  (~150 LOC)       Carried verbatim: ExtractReceiver/UnwrapReceiver/UnwrapLambda/UnwrapValue
                                 (01 §9.3) + terminal-Match-position analysis (01 §9.4). Closure detection stays the
                                 EXISTING IOperation walk (ILocalReferenceOperation/IParameterReferenceOperation +
                                 ContainingSymbol compare) — NOT IFlowCaptureOperation/AnalyzeDataFlow (08 B1).
  Rules/
    Flow/CSP0001.ImperativeControlFlow.cs    one FILE per rule with real algebra, named <ID>.<Name>.cs, exposing
    Shape/CSP0701.PrimitiveShape.cs          `internal static RuleRow Row { get; }` (descriptor metadata + bindings).
    Surface/CSP0005.OverloadSpam.cs          Folders = Category. Per-category ROW TABLES for trivial syntax-kind bans
    Rail/...  Boundary/...  Perf/...          (0001/0010/0015-class etc., below the predicate-complexity bar — 08 A3),
                                              so the ~20 keyword bans do not each spawn a file (repo DEEP_SURFACES).
                                              SymbolFacts.cs (1,495 LOC, 9 concerns) DISSOLVED into Walkers + per-rule
                                              predicates co-located with their rule.
tests/tools/cs-analyzer/
  Csp.Analyzer.Tests.csproj
  Harness.cs                     thin wrapper over CSharpAnalyzerVerifier<Driver, DefaultVerifier>
  Meta/CatalogInvariants.cs      reflection meta-tests (doc 06 §META)
  Rules/<ID>.<Name>Tests.cs      one per rule: >=1 positive (markup span) + >=1 negative + per-exemption case
libs/csharp/Rasm.Generators/     UnionOpsGenerator EXTRACTED from the analyzer assembly (it emits
                                 global::Rasm.Domain.Op so it is a Rasm component, not a Foundation component;
                                 01 §7). Rebuilt on ForAttributeWithMetadataName + equatable record model; NO
                                 Compilation capture (current kills incrementality); IndentedTextWriter + #nullable
                                 enable; caseless-union now surfaced by config-gated CSP0802, not silent skip.
```

## [2]-[KERNEL_DESIGN]

**2.1 Descriptor catalog as data.** Each rule file owns its full row; `Catalog.All` is the only registry. The meta-test (doc 06) reflects over the assembly for every `RuleRow`-typed static and asserts membership in `All` — this eliminates the hand-list crash CLASS (08 A2 REFUTES the AD0001 framing: Driver registers FROM `All`, so an omitted row leaves the rule INERT and its mandated positive markup test FAILS; the meta-test double-covers; the residual surfaces only in the test rail, acceptable under the repo's split gates).

**2.2 Registration machinery.** Driver groups `All`'s bindings by trigger kind; each OperationKind/SyntaxKind registered exactly once, dispatching over a pre-bucketed array of rows (no dictionary per node). Trigger assignment (08 B3 CORRECTED):
- `SymbolStartAction` only for genuinely per-TYPE accumulators (CSP0729 overload adjacency).
- `CompilationEnd` for true cross-type/whole-compilation accumulation: CSP0501 (cross-type interface implementors), CSP0724 (compilation-wide bitwise callsites), CSP0503 (whole-compilation invocation counts), CSP0740/0744. DOCUMENTED batch-build-only (IDE live analysis does not reliably run end actions; doc 02, doc 03 §3).
- End actions NEVER used for disposal.

**2.3 Per-compilation indices.** Union-case index, overload families, and well-known symbols built ONCE at CompilationStart/SymbolStart, killing every O(types × namespace-tree) walk (01 §6; the 0740 triple walk is real, 08 B5).

## [3]-[RULE_ROW_MODEL_AND_KERNEL_SIGNATURES]

The complete signature surface — an implementer writes every public/internal member from these declarations without inventing one; bodies follow §2 and §9. The hand-written closed families below are the named exemption to the generated-owner doctrine: Thinktecture is FORBIDDEN inside the analyzer assembly (§10), so row/binding/gate families are plain records and flags enums — the same closed-family law, zero runtime dependencies.

```csharp
// --- Csp.Contracts (netstandard2.0, zero dependencies, PUBLIC — referenced normally by every project) ---
public enum CspScope { Domain, Application, Boundary, Composition, HotPath, Test, Tooling }

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class CspScopeAttribute(CspScope scope) : Attribute { public CspScope Scope { get; } = scope; }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class BoundaryAdapterAttribute : Attribute;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
              | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CspExemptAttribute(string justification) : Attribute { public string Justification { get; } = justification; }

// --- Kernel/Catalog.cs ---
internal enum Tier : byte { Law, Pressure, Info }              // Law=Error, Pressure=Warning, Info=Info
internal enum Category : byte { Flow, Shape, Surface, Rail, Boundary, Perf }

[Flags]
internal enum ScopeGate : byte {
    Domain = 1, Application = 2, Boundary = 4, Composition = 8, HotPath = 16, Test = 32, Tooling = 64,
    // HotPath is a domain SPECIALIZATION: it adds the perf band (0601), never exits domain discipline (08 D2).
    DomainOrApplication = Domain | Application | HotPath,
    Everywhere = Domain | Application | Boundary | Composition | HotPath | Test | Tooling,
}

internal static class Catalog {
    public static readonly ImmutableArray<RuleRow> All;        // the ONLY registry; meta-test reflects assembly RuleRow statics into it
    public static readonly ImmutableArray<string> Reserved;    // "CSP0016","CSP0716","CSP0721","CSP0722" — never emitted, never reused (doc 05 §7)
    internal static DiagnosticDescriptor Describe(             // replaces the old Error-only `Err` factory
        string id, string title, string messageFormat,         // messageFormat in the §7 grammar
        Category category, Tier tier, string doctrineAnchor);  // Description = doctrine page#section; helpLinkUri = analyzer.md#cspNNNN
}

// --- Kernel/Row.cs ---
internal sealed record RuleRow(
    DiagnosticDescriptor Descriptor,
    Tier                 Tier,
    ScopeGate            Scope,       // doc 05 §10 matrix; meta-test asserts exactly one declared gate per active rule
    ImmutableArray<RuleBinding> Bindings);

internal delegate void RuleCheck(in RuleContext context);
// NAMED delegate, NOT Action<RuleContext>: RuleContext is a ref struct, and a ref struct is ILLEGAL as a
// generic type argument on netstandard2.0 (`allows ref struct` anti-constraint needs C# 13 + net9.0 runtime
// support, absent here). A delegate PARAMETER of ref-struct type is legal. Action<RuleContext> does not compile.

internal enum TriggerKind : byte { Syntax, Operation, Symbol, SymbolStart, CompilationEnd }

internal sealed record RuleBinding(TriggerKind Trigger, RuleCheck Check, ImmutableArray<int> Kinds) {
    public static RuleBinding Syntax(RuleCheck check, params SyntaxKind[] kinds);        // the three kind enums erase
    public static RuleBinding Operation(RuleCheck check, params OperationKind[] kinds);  // to one int slot; TriggerKind
    public static RuleBinding Symbol(RuleCheck check, params SymbolKind[] kinds);        // recovers the meaning
    public static RuleBinding SymbolStart(RuleCheck check, SymbolKind kind);             // per-type accumulators (0729, 0905)
    public static RuleBinding CompilationEnd(RuleCheck check);                           // batch-only band (§2.2)
}                                                                                        // Check is total; no throw (§9 AD0001)

internal readonly ref struct RuleContext {
    public SyntaxNode? Node { get; }             // Syntax triggers
    public IOperation? Operation { get; }        // Operation triggers
    public ISymbol? Symbol { get; }              // Symbol/SymbolStart triggers
    public SemanticModel? Model { get; }         // ONLY the context-handed model (RS1030); never Compilation.GetSemanticModel
    public CompilationFacts Facts { get; }
    public CspScope Scope { get; }               // resolved per §4 priority BEFORE the check runs; the gate is already applied
    public CancellationToken Cancel { get; }
    public void Report(Location location, params object?[] args);  // projects messageFormat; display formatting lives ONLY here (06 §5d)
}

// --- Kernel/Facts.cs ---
internal sealed class CompilationFacts {
    public CompilationFacts(CompilationStartAnalysisContext context);        // resolves everything below ONCE
    public INamedTypeSymbol? WellKnown(string metadataName);                 // cached GetTypeByMetadataName; SymbolEqualityComparer
    public CspScope ScopeOf(SyntaxTree tree, ISymbol? symbol);               // build_property → assembly [CspScope] → csp.scope / type [CspScope]
    public bool HasSection(string section);                                  // CSP0901 config-absence integrity (§5)
    public bool IsBanned(string section, ISymbol symbol);                    // CspBannedSymbols.txt → DocumentationCommentId-resolved symbol sets
    public ImmutableArray<string> PrefixVocabulary { get; }                  // CspPrefixVocabulary.txt (0708)
    public bool TryParameter(string ruleId, string name, out string value);  // csp.CSP####.<param> via AnalyzerConfigOptionsProvider
    // per-compilation indices (§2.3): union-case index, overload families;
    // ConcurrentDictionary + SymbolEqualityComparer accumulators serve the CompilationEnd band.
}

// --- Kernel/Driver.cs ---
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Driver : DiagnosticAnalyzer {
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }  // Catalog.All projected once
    public override void Initialize(AnalysisContext context);   // §2.2 ceremony; registers FROM Catalog.All, pre-bucketed by TriggerKind
}

// --- tests/Harness.cs (entry types only; mechanics are doc 06's layer) ---
internal sealed class CspTest : CSharpAnalyzerTest<Driver, DefaultVerifier> { }
// ctor pins ReferenceAssemblies to the repo TFM + AddPackages(LanguageExt.Core, Thinktecture.Runtime.Extensions)
// at the Directory.Packages.props versions (§10 mirror law); generator-run mode adds the Thinktecture
// source generator via SolutionTransforms (doc 06 §3).
```

A rule with real algebra (0005, 0729, 0742) keeps its own file exposing `internal static RuleRow Row { get; }`. Trivial syntax-kind bans collapse into a per-category row TABLE inside the Category folder (08 A3). The descriptor singleton lives on the row; never constructed per callback (doc 03 §5c).

## [4]-[SCOPE_MODEL_THE_CENTRAL_REBUILD]

Substring classification (`SymbolFacts.cs:49-71,161-206`) is DELETED (verdict b). Replacement, in priority order:

1. **`build_property.CspScope`** per project via `CompilerVisibleProperty` in `Directory.Build.props`, values `Domain|Application|Boundary|Composition|HotPath|Test|Tooling`. (NOTE 08 D1: `Exempt` is NOT a scope value — tooling projects simply skip the analyzer reference, which is already diffable in Build.props; an `Exempt` value would resurrect the Skip-list with no inventory.)
2. **Assembly-level `[CspScope]`** as equal-priority declaration.
3. **PER-TREE override via `.editorconfig` `csp.scope` key** read from `AnalyzerConfigOptionsProvider.GetOptions(tree)` AND **type-level `[CspScope]`** (08 D2 — the forced change). REQUIRED because assembly-level scope alone is COARSER than today: `Rasm` is ONE csproj containing `Analysis/` (a HotPath sub-tree), and declaring `Rasm.Rhino CspScope=Boundary` wholesale would strip domain-flow enforcement from its pure kernels — an unacknowledged regression. Per-tree/type override expresses HotPath and re-asserts domain discipline inside a boundary project.
4. **Member-level `[BoundaryAdapter]`/`[CspExempt(Justification:)]`** for in-method exemptions.

Scope becomes unspoofable without a diffable build-file/attribute/editorconfig edit. Partial-type first-declaration fragility (`SymbolFacts.cs:166-169`) disappears because scope is assembly/tree/type-level, never `DeclaringSyntaxReferences[0]`.

**Rule×Scope applicability matrix (08 D2 — PUBLISHED in the catalog, the most consequential behavior surface).** Each `RuleRow.Scope` is a `ScopeGate` declaring which scopes the rule fires in. Canonical gates: `DomainOrApplication` (the bulk of flow/shape/rail rules), `HotPath` (CSP0601 LINQ ban — fires ONLY in HotPath, killing the `PerformanceReport.cs` FP), `Boundary` (admission/sentinel rules fire AT the boundary), `Everywhere` (CSP0901/0902/0903 integrity rules). The matrix is a data table in `Catalog.cs`, asserted complete by meta-test, and rendered into `analyzer.md`. The settled per-rule assignments (default-gate algebra + the exhaustive exception list) are doc 05 §10 — gate changes during the port must be argued in the rule's `analyzer.md` anchor, never made silently.

## [5]-[CONFIG_SURFACE]

(a) Standard `dotnet_diagnostic.CSP####.severity` — works because `NotConfigurable` is dropped. (b) Rule parameters as `csp.CSP####.<param>` via `AnalyzerConfigOptionsProvider` (e.g. `csp.CSP0741.case_threshold`, default 10 per the Op+Result break-even memory). (c) AdditionalFiles in DocumentationCommentId format (BannedApi model, doc 03 §9): `CspBannedSymbols.txt` sections drive 0011 (mutable/concurrent collections), 0007 (time APIs), 0723 (ambient host state, replacing the hardcoded `Rhino.*` check), 0742 (admission-gate API + admissible types, replacing 20+ hardcoded predicates and 21 Rhino type names at `SymbolFacts.cs:222-225,675-946`); `CspPrefixVocabulary.txt` drives 0708. This excises every project-symbol heuristic from analyzer source (verdict d; CLAUDE.md no-namespace-coupling law).

**Config-absence integrity (08 F4 — the forced hardening).** "Config absent ⇒ data-driven rule silent" would convert deletion/rename of `CspBannedSymbols.txt` into SILENT disablement of five Law rules — exactly the fragility the charter forbids. RESOLUTION: CSP0901 (extended) errors when a data-driven rule is enabled in a non-tooling scope WITHOUT its AdditionalFiles section; and the data files ship IN the rebuild change itself (not deferred).

## [6]-[SEVERITY_AND_CATEGORY_TAXONOMY]

`Tier.Law` = Error, configurable, suppression audited by CSP0902 — replaces `NotConfigurable`, ending analyzer-edit-as-pressure-valve (verdict h). `Tier.Pressure` = Warning for density heuristics (doctrine: findings are "architecture pressure"/"hypotheses"; the uniform-Error catalog mis-signals agents and is the documented FP source for the receipt family). `Tier.Info` for inventories. Categories `{Flow, Shape, Surface, Rail, Boundary, Perf}` enable `dotnet_analyzer_diagnostic.category-X.severity` band tuning.

**Pressure-band plumbing (08 F1 — the forced change; "only assay-side change" was FALSE).** Two verified facts break the naive tier taxonomy: (i) `Directory.Build.props:75` sets `TreatWarningsAsErrors=true` repo-wide, so `Pressure=Warning` promotes to error — build-failing and indistinguishable from Law; (ii) assay's build runs `/clp:ErrorsOnly` (`catalog.py:160`), so un-promoted warnings NEVER reach the agent rail and CSP0903 Info is invisible. RESOLUTION (settled into doc 07): `<WarningsNotAsErrors>$(WarningsNotAsErrors);CSP-Pressure-band-IDs</WarningsNotAsErrors>` (or category-severity config) keeps Pressure as warning; assay's console logger must include warnings OR adopt `/errorlog` SARIF; CSP0903 Info needs a non-console emission path (SARIF or a dedicated inventory artifact).

## [7]-[AGENT_CONSUMABLE_DIAGNOSTIC_CONTRACT]

`messageFormat` grammar, single line (assay parseability): `"<fact naming the symbol>; fix: <exact canonical API/transform>; exempt: <route>"` — carries forward the remediation-in-message convention (`RuleCatalog.cs:148,177,193`). `DiagnosticDescriptor.Description` = doctrine citation (page#section). `helpLinkUri` = stable anchor `analyzer.md#cspNNNN`.

**Span** = narrowest fixable node (operator token, member identifier), NEVER the whole declaration.

**Properties bag posture (08 E1 — the forced trim).** `Diagnostic.Properties` (`fix`/`doctrine`/`scope`) and `helpLinkUri` are SPECULATIVE under the design's own consumption contract: assay decodes console lines `error|warning CSP####` only; bags and help links flow solely through SARIF/IDE, which the agent rail does not use today. DECISION: keep `fix`/`doctrine`/`scope` strictly IN the single-line message; populate the Properties bag and `helpLinkUri` ONLY IF doc 07's SARIF adoption lands (then the bag earns its keep). The single-line message is the load-bearing surface; the bag/help are conditional on the SARIF path.

No CodeFixProviders at rebuild (agents act on message+span, not lightbulbs — doc 03 §10); the Properties seam is reserved, not populated, until SARIF.

## [8]-[GENERATOR_EXTRACTION]

`UnionOpsGenerator` moves to `libs/csharp/Rasm.Generators/` (it emits `global::Rasm.Domain.Op`, keys `Rasm.Domain.GenerateUnionOpsAttribute` — a Rasm consumer concern, not Foundation). Rebuild: `ForAttributeWithMetadataName` + equatable record model (no `Compilation` capture — current `:16-20` kills incrementality), `IndentedTextWriter` + `#nullable enable`. Caseless-union emits nothing today AND leaves CSP0802 satisfied (`:53-55`); the rebuilt CSP0802 (config-gated on the attribute metadata names from `build_property`, decoupled from `Rasm.Domain`) surfaces it. Build.props references the generator as an Analyzer ONLY by Rasm-family projects (08 G3).

## [9]-[HARDENING_POSTURE]

- **Statelessness:** analyzer type holds zero mutable state (RS1008/RS1009); all state in the CompilationStart-created `CompilationFacts`.
- **Concurrency:** `EnableConcurrentExecution` sealed in Driver; `ConcurrentDictionary` + `SymbolEqualityComparer` for accumulators (carry `AnalyzerState.cs:18-26`); concurrent path exercised in tests (doc 06).
- **Determinism/culture:** Ordinal/OrdinalIgnoreCase exclusively (current code already sound); no `DateTime`/`Random`/env reads in analyzer source; meta-grep enforces.
- **Perf:** never `Compilation.GetSemanticModel` in callbacks (RS1030, doc 03 §5a) — use the context model; OperationActions for semantic rules, SyntaxNodeActions for keyword bans; NO `OperationKind.Literal` global registration (the 0605 hazard is dropped); no LINQ in hot callbacks; `CancellationToken` threaded through walkers; per-compilation indices built once.
- **Generated-code policy:** `GeneratedCodeAnalysisFlags.None` (Thinktecture/source-gen partials carry `[GeneratedCode]`, skipped); generator-emitted Rasm code never analyzed.
- **AD0001 elimination:** catalog completeness meta-tested; rule bodies total (no throw); harness rethrows analyzer exceptions.
- **Self-application (the binding posture for the analyzer's OWN source):** `docs/stacks/csharp/` is the BINDING authority for what the rules enforce; the analyzer assembly itself is `CspScope=Tooling` and keeps the carried self-skip (`SkipLocalCSharpAnalyzerReference`). Its source follows coding-csharp expression discipline at the architectural layer (catalog-as-data, immutable rows, total check bodies); registered callbacks and walkers are the DOCUMENTED perf boundary where Roslyn doctrine binds instead — imperative `foreach` over LINQ, pooled builders, cancellation bail-out (doc 03 §5d). The 01 §2 switch-for-side-effect ceremony (`_ = (...) switch` returning dummy ints) is banned in BOTH regimes: a statement loop inside a hot callback is the honest boundary form; the pseudo-FP contortion is not. This settles the dogma-vs-code contradiction doc 01 §2 indicts instead of re-importing it.
- **Adversarial scope integrity (the closed primary hole):** scope is build-property/attribute/editorconfig-declared (§4); file rename and namespace suffix no longer change enforcement; CSP0901 errors on undeclared scope AND on a data-driven rule without its config section; CSP0902 errors on unjustified suppression AND on a Law-tier `dotnet_diagnostic.CSP*.severity` downgrade (08 F3); CSP0903 makes every attribute exemption visible. The four together close the demolition audit's primary adversarial hole.

## [10]-[PACKAGE_TABLE] (authoritative — every package the rebuild touches)

Central-package-management law: versions live ONLY in `Directory.Packages.props`; every project reference is versionless. The `Microsoft.CodeAnalysis.*` 5.3.0 pins already exist (`Directory.Packages.props:127-132`); **`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` is the ONE new central pin the rebuild adds** (verified absent from the props file; current stable 1.1.x band, doc 03 §7).

| [PROJECT] | [PACKAGE] | [PIN] | [OWNS] | [POSTURE] |
| --------- | --------- | ----- | ------ | --------- |
| `Csp.Analyzer` | `Microsoft.CodeAnalysis.CSharp` | 5.3.0 existing | Roslyn analyzer API: syntax, IOperation, symbols | `PrivateAssets="all"` — build-only, never flows |
| `Csp.Analyzer` | `Microsoft.CodeAnalysis.Analyzers` | 5.3.0 existing | RS#### self-enforcement + RS2000-RS2008 release tracking | `PrivateAssets="all"`; AnalyzerReleases md wired as AdditionalFiles |
| `Csp.Contracts` | — none — | — | scope/exemption attributes (§3) | zero-dependency by contract: anything added here flows to EVERY project |
| `Rasm.Generators` | `Microsoft.CodeAnalysis.CSharp` | 5.3.0 existing | `IIncrementalGenerator` API (§8) | `PrivateAssets="all"` |
| `Csp.Analyzer.Tests` | `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` | **NEW pin** | `CSharpAnalyzerTest`/`CSharpAnalyzerVerifier` + `DefaultVerifier` harness | test-only |
| `Csp.Analyzer.Tests` | `xunit.v3.*` | 3.2.2 existing | test framework under the MTP runner | repo-standard test stack |
| `Csp.Analyzer.Tests` | `LanguageExt.Core` | 5.0.0-beta-77 existing | REAL rail types in test sources — ends name-shim matching (doc 06 §2) | `ReferenceAssemblies.AddPackages`, mirroring the central pin |
| `Csp.Analyzer.Tests` | `Thinktecture.Runtime.Extensions` | 10.2.0 existing | REAL generated-owner surfaces + generator-run path (doc 06 §3) | `AddPackages` + `SolutionTransforms`, mirroring the central pin |

FORBIDDEN in `Csp.Analyzer` and `Csp.Contracts`: Thinktecture, LanguageExt, and every other runtime package. An analyzer assembly loads into the compiler process — RS1038 restricts references to `Microsoft.CodeAnalysis.*`, and any runtime dependency would have to ship beside the analyzer DLL. The doctrine's generated owners (`[Union]`/`[SmartEnum]`/`[ValueObject]`) are therefore unreachable HERE; the §3 hand-written closed families are the named platform-forced substitute — same closed-family law, zero dependencies. The harness resolves `AddPackages` versions through NuGet, not CPM: the two test-package versions MUST mirror `Directory.Packages.props`, and doc 06 §5(h) meta-tests the mirror so a central bump cannot silently desynchronize test truth from repo truth.
