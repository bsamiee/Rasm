# [CS_ANALYZER]

`tools/cs-analyzer` is the compiled form of the C# doctrine. Doctrine pages under `docs/stacks/csharp` legislate; this analyzer enforces. Its loop is one-directional: a doctrine page states a law, a rule row turns the matching anti-pattern into a build diagnostic, and no rule introduces law of its own. When a finding lands, the response is governed by the doctrine — a true positive is product pressure (fix the shape), a false positive is rule pressure (refine the row), and suppression is neither.

## [01]-[LAW]

This analyzer enforces the doctrine's `[RULE_ENFORCEMENT]` laws from `docs/stacks/csharp/README.md` §4.

- Promotion admits an anti-pattern that breaks doctrine while the compiler, `.editorconfig`, and shipped analyzers pass it; style earns no row.
- Shape states each anti-pattern's trigger, predicate, and exemption route, shipping firing spans beside valid code that stays silent.
- Register: the rule inventory is the code — catalog array, release ledgers, and vocabulary data.
- Findings are architecture pressure: a fix adding ceremony without improving the system convicts the row, never the product.

## [02]-[SCOPE]

Every rule fires inside the doctrine scope it targets alone. `CompilationFacts.ScopeOf` resolves scope per symbol by priority: a `[CspScope]` type marker, the `csp.scope` per-tree config, then the `build_property.CspScope` value or an assembly-level marker; undeclared reads `Domain`. `Directory.Build.props` derives it from classification — `Test` for test, testkit, and scenario-kit projects, `Boundary` for plugin and bridge projects, `Tooling` for analyzer and analyzer-contract projects — and libraries and apps declare it explicitly.

Each rule row carries a `ScopeGate` flag set; the driver gates the row against the resolved scope before the check runs, so a `DomainOrApplication` rule never fires at a boundary seam and an `Everywhere` rule fires across every declared scope. `HotPath` stays domain-gated and admits only perf rules. `[BoundaryAdapter]` marks the one seam where foreign shapes are legal, and `[CspExempt(justification)]` is the explicit, justification-bearing escape; both ship from the one `Csp.Contracts` assembly every consumer references, so one type identity carries the markers everywhere.

## [03]-[RULE_REGISTER]

- `Kernel/Catalog.cs`: `Catalog.All` is the one registry of `(Descriptor, Tier, ScopeGate, Bindings)` rows, `Catalog.Reserved` holding retired ids.
- `Describe` builds each `DiagnosticDescriptor` off `Tier`, category, and the doctrine anchor; `CatalogInvariants` rejects an orphan `[RuleSpec]` id.
- `AnalyzerReleases.Shipped.md` and `.Unshipped.md` are the id-and-severity ledger; divergence from `Catalog.All` fails the build.
- `Kernel/Vocabulary.cs` holds the data rules discriminate on, resolved per compilation by `DocumentationCommentId`; an unresolvable row stays inert.

`Kernel/Row.cs` owns the row algebra: `RuleRow`, `RuleBinding` with its `Syntax` / `Operation` / `Symbol` / `SymbolStart` / `CompilationEnd` trigger factories (Roslyn `enum` kinds erased to `int` so the netstandard2.0 surface holds), and the `RuleContext` `ref struct` each check receives — carrying node, operation, symbol, the context-handed `SemanticModel`, resolved scope, and `CompilationFacts`. `Report` is the only diagnostic sink; it stamps `tier`, `doctrine`, and `scope` into SARIF properties.

`Kernel/Walkers.cs` owns the shared syntax and operation unwrap helpers checks compose. `Kernel/Driver.cs` is the single `DiagnosticAnalyzer`: it folds `Catalog.All` into per-Roslyn-kind buckets at `CompilationStart`, registers one dispatch per kind, and routes each trigger to its scope-gated slots. Compilation-end rules are batch-build-only and skipped by IDE live analysis.

`Generators/UnionOpsGenerator.cs` ships inside the same assembly: an `IIncrementalGenerator` that binds union cases to domain operation provenance from `[GenerateUnionOps]` targets. This assembly is therefore both the doctrine's enforcer and its `SelfOp` source generator, which is why `Directory.Build.props` references it with no `Exists()` gate — a missing analyzer project fails the build loudly rather than compiling consumers without analysis or generation.

## [04]-[BUILD_STATE]

`Csp.Analyzer.csproj` targets `netstandard2.0`, sets `IsRoslynComponent`, enforces extended analyzer rules, and references `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`, and `PolySharp` as private assets. `Contracts/` compiles separately into `Csp.Contracts` and is excluded from the analyzer compile.

`Directory.Build.props` wires the analyzer into every non-analyzer C# project as an `OutputItemType="Analyzer"` project reference with `ReferenceOutputAssembly="false"`; the analyzer, its test project, and `Csp.Contracts` carry `SkipLocalCSharpAnalyzerReference` to avoid self-reference, and `Csp.Contracts` is project-referenced by every consumer except the Roslyn component and itself.

Scope resolution, bucketed dispatch, tier-to-severity mapping, vocabulary resolution, SARIF stamping, and the release-ledger contract host rows without structural change. Each promotion lands as one `RuleRow` in `Catalog.All`, one ledger entry, any vocabulary additions, and the rule's positive and negative spans in `Csp.Analyzer.Tests`.
