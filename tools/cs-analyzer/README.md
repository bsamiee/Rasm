# [CS_ANALYZER]

`tools/cs-analyzer` is the compiled form of the C# doctrine. The doctrine pages under `docs/stacks/csharp` legislate; this analyzer enforces. Its loop is one-directional: a doctrine page states a law, a rule row turns the matching anti-pattern into a build diagnostic, and no rule introduces law of its own. When a finding lands, the response is governed by the doctrine — a true positive is product pressure (fix the shape), a false positive is rule pressure (refine the row), and suppression is neither.

## [01]-[LAW]

The analyzer enforces the doctrine's `[RULE_ENFORCEMENT]` laws from `docs/stacks/csharp/README.md` §4.

- Promotion: a rule exists only for an anti-pattern that is doctrine-breaking yet passes the compiler, `.editorconfig`, and every shipped analyzer. Style preferences, one-off review notes, and patterns an existing mechanical gate already rejects are out of scope.
- Shape: a rule describes the semantic shape of the anti-pattern — trigger, predicate, exemption route — never namespaces, paths, or one-off symbols. Every row ships with positive spans that must fire and valid compact code that must not.
- Register: the rule inventory is the code. The catalog array, the release ledgers, and the vocabulary data are the only inventory; no prose catalog of rules exists anywhere, including in this file.
- Finding: a diagnostic is architecture pressure. Fixing the shape in the product clears a true positive; refining the row clears a false positive or a fix that adds ceremony without improving the system.

## [02]-[SCOPE]

A rule fires only inside the doctrine scope it targets. Scope resolves per analyzed symbol through `CompilationFacts.ScopeOf`, in priority order: a `[CspScope]` type marker, then the `csp.scope` per-tree config, then the `build_property.CspScope` MSBuild value or an assembly-level `[CspScope]` marker; an undeclared scope is `Domain`. `Directory.Build.props` derives `CspScope` from project classification — `Test` for test and testkit projects, `Boundary` for plugin and bridge projects, `Tooling` for benchmark and analyzer projects — and libraries and apps declare it explicitly.

Each rule row carries a `ScopeGate` flag set; the driver gates the row against the resolved scope before the check runs, so a `DomainOrApplication` rule never fires at a boundary seam and an `Everywhere` rule fires across all seven scopes. `HotPath` stays domain-gated and admits only perf rules. `[BoundaryAdapter]` marks the one seam where foreign shapes are legal, and `[CspExempt(justification)]` is the explicit, justification-bearing escape; both ship from the single `Csp.Contracts` assembly referenced by every consuming project, so one type identity carries the markers across host, test, and bridge code.

## [03]-[RULE_REGISTER]

The register is three code surfaces, not prose.

- Catalog (`Kernel/Catalog.cs`): `Catalog.All` is the single rule registry — one `ImmutableArray<RuleRow>` of `(Descriptor, Tier, ScopeGate, Bindings)` rows. `Catalog.Reserved` holds retired or reserved ids. `Describe` builds each `DiagnosticDescriptor`, mapping `Tier` to severity (`Law` -> error, `Pressure` -> warning, `Info` -> info), category to the `Category` axis, and the doctrine anchor into the descriptor description. The help link resolves to `tools/cs-analyzer/docs/rules/<id>.md`; sync tests reject orphan rule prose against this surface.
- Release ledgers (`AnalyzerReleases.Shipped.md`, `AnalyzerReleases.Unshipped.md`): the two `AdditionalFiles` consumed by the Microsoft release-tracking analyzer. They are the authoritative id-and-severity ledger; a row added to `Catalog.All` is reconciled here, and divergence between catalog and ledger is a build failure.
- Vocabulary (`Kernel/Vocabulary.cs`): the data a rule discriminates against. `Prefixes` holds the operation-name prefix family that the modal-arity law rejects; `BannedSections` is a `DocumentationCommentId`-keyed map of forbidden host and BCL surfaces by section — `mutable-collections`, `time`, `ambient-state`, `admission-gate`, `admissible-types`. Rows resolve per compilation through `DocumentationCommentId.GetSymbolsForDeclarationId`; an unresolvable row stays inert rather than faulting.

`Kernel/Row.cs` owns the row algebra: `RuleRow`, `RuleBinding` with its `Syntax` / `Operation` / `Symbol` / `SymbolStart` / `CompilationEnd` trigger factories (Roslyn `enum` kinds erased to `int` so the netstandard2.0 surface holds), and the `RuleContext` `ref struct` each check receives — carrying node, operation, symbol, the context-handed `SemanticModel`, resolved scope, and `CompilationFacts`. `Report` is the only diagnostic sink; it stamps `tier`, `doctrine`, and `scope` into SARIF properties. `Kernel/Walkers.cs` owns the shared syntax and operation unwrap helpers checks compose. `Kernel/Driver.cs` is the single `DiagnosticAnalyzer`: it folds `Catalog.All` into per-Roslyn-kind buckets at `CompilationStart`, registers one dispatch per kind, and routes each trigger to its scope-gated slots. Compilation-end rules are batch-build-only and skipped by IDE live analysis.

`Generators/UnionOpsGenerator.cs` ships inside the same assembly: an `IIncrementalGenerator` that binds union cases to domain operation provenance from `[GenerateUnionOps]` targets. The analyzer assembly is therefore both the doctrine's enforcer and its `SelfOp` source generator, which is why `Directory.Build.props` references it with no `Exists()` gate — a missing analyzer project fails the build loudly rather than compiling consumers without analysis or generation.

## [04]-[BUILD_STATE]

`Csp.Analyzer.csproj` targets `netstandard2.0`, sets `IsRoslynComponent`, enforces extended analyzer rules, and references `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`, and `PolySharp` as private assets. The `Contracts/` tree compiles separately into `Csp.Contracts` and is excluded from the analyzer compile. `Directory.Build.props` wires the analyzer into every non-analyzer C# project as an `OutputItemType="Analyzer"` project reference with `ReferenceOutputAssembly="false"`; the analyzer and its test project carry `SkipLocalCSharpAnalyzerReference` to avoid self-reference, and `Csp.Contracts` is project-referenced by every consumer except the Roslyn component and itself.

The kernel and dispatch driver are finalized and the assembly builds (Debug and Release outputs present). `Catalog.All` is currently empty: the rule-hosting machinery — scope resolution, bucketed dispatch, tier-to-severity mapping, vocabulary resolution, SARIF stamping, and the release-ledger contract — is complete and inert, ready to host promoted rule rows. The release ledgers carry no shipped or unshipped rule rows yet. Each promotion lands as one `RuleRow` in `Catalog.All`, one ledger entry, any vocabulary additions, and the rule's positive and negative spans in `Csp.Analyzer.Tests`.
