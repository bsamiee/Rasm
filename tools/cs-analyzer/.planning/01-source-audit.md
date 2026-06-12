# [01] Source Audit — The Demolition Survey

Survey of the analyzer being torn down: `tools/cs-analyzer` (`Foundation.CSharp.Analyzers`), 4,186 source LOC + 3,973 test LOC + 79 harness LOC. One `DiagnosticAnalyzer` (`DomainStandardsAnalyzer`) + one `IIncrementalGenerator` (`UnionOpsGenerator`), 87 CSP descriptors (verified count, not 84), all `Error` + `NotConfigurable`. Every claim below is line-cited against tree HEAD and re-verified 2026-06-11. Findings not cited here are not findings; the verdicts in §9 are the binding demolition output the target architecture (doc 04) and parity map (doc 05) must satisfy.

## [1]-[GOD_FILES_AND_CONCERN_COUNT]

- `Kernel/SymbolFacts.cs` (1,495 LOC) is THE god file: ≥9 unrelated concerns in one static class — scope markers (`:49-71`), regex-pattern hand-parser (`:338-380`), async/task facts (`:391-445`), null/throw facts (`:449-465`), LanguageExt type tests (`:549-624`), manual-Op-admission-gate detection with ~45 private helpers (`:675-946`), receipt-name heuristics (`:947-1012`), manual-closed-union detection (`:1182-1405`), closed-union dispatch fact extraction (`:1406-1494`). The single `[SYNTAX_FACTS]` label at `:208` covers 1,280 lines. The CSP0742/0743 rule family alone owns 20+ single-purpose predicates (`IsKnownValidityProbe`, `IsConfirmableNativeStatusCondition`, `IsCountEqualityCondition`, `IsGuidNonEmptyCondition`, `MatchesDesignationOrInt`, `IsZeroLiteral`, `IsOpAcceptValueAdmissibleType×2`, `IsFiniteScalarType`, … `:675-946`).
- `tests/tools/cs-analyzer/RuleBehaviorTests.cs` (3,973 LOC): one class, one 900-line literal `Cases` array (`:9-900`) + ~118 facts.
- `Rules/ShapeRules.cs` (652), `Rules/FlowRules.cs` (486), `Rules/TypeShapeRules.cs` (450), `Rules/RuntimeRules.cs` (320): each mixes 8-15 unrelated rule families behind one static class.

## [2]-[FLAT_CODE_SPAM_AND_REPEATED_RULE_BODIES]

- THE dominant pattern: every `Check*` method is identical ceremony — compute booleans → N-tuple `switch` with exactly one armed row → `AnalyzerState.Report`. ~70 occurrences across the 4 Rules files (e.g. `FlowRules.cs:16-20,21-25,56-60,69-73,94-98,110-114,120-124,136-140`; `RuntimeRules.cs:63-67,79-83,84-88,102-106`; `TypeShapeRules.cs:23-26,126-129,139-142,149-152,179-183`; `ShapeRules.cs:72-77,360-363,438-441,455-458,484-487`). This is a data table wearing method costumes: `{Descriptor, ScopeGate, OperationKind/SymbolKind, Predicate, Locator, ArgsProjector}` per row. A table-driven rebuild collapses ~1,900 LOC of `Rules/*` to ~400 (verdict a).
- Pseudo-FP contortion to dodge `if`: `_ = (context, flagsEnum) switch { (_, true) => RegisterFlags(...), _ => 0 }` returning dummy ints for side effects (`TypeShapeRules.cs:363-366,375-378`); `AnalyzerState.Report` is a glorified null-check via switch-statement (`AnalyzerState.cs:52-58`); `TrackPrivateMethod` discards a switch over `TryAdd` (`AnalyzerState.cs:69-73`). Imperative survives anyway: `foreach` in `ReportEach` (`:59-63`) and `TrackInterfaceImplementations` (`:95-103`), `while` in `EnclosingInvocation` (`FlowRules.cs:454-468`), mutating `foreach`+`switch` in `OverloadAdjacencyDiagnostics` (`ShapeRules.cs:266-280`), unchecked hash loop (`ShapeRules.cs:227-235`). The file dogma and the code contradict.
- `RuleCatalog.All` hand-lists all 87 fields (`RuleCatalog.cs:215-228`); a field missing from `All` = analyzer crash (`ArgumentException` → AD0001 swallow) on first fire. No reflection/source-gen guard; `ReleaseDisciplineTests` does not catch it.
- `OverloadKey` hand-rolls `Equals`/`GetHashCode` with 397-prime mixing (`ShapeRules.cs:207-236`) — a record-struct or `ValueTuple` key erases ~30 LOC.

## [3]-[HAND_ROLLED_MACHINERY_ROSLYN_PROVIDES]

- Test harness (`Infrastructure/AnalyzerTestHarness.cs`) re-implements `Microsoft.CodeAnalysis.Testing`: hand-built compilation from `TRUSTED_PLATFORM_ASSEMBLIES` (`:68-78`), hand directory-walk for repo root (`:53-61`), no markup-span verification anywhere. The standard `CSharpAnalyzerVerifier` gives `{|CSP0001:span|}` assertions for free.
- `ReleaseDisciplineTests.cs` (`:54-66,99-108`) re-implements Roslyn release tracking (RS2000-RS2008) by regex-parsing `AnalyzerReleases.*.md` and regex-scanning source for `Diagnostic.Create(RuleCatalog.CSPxxxx` — because the csproj never wires the md files as `AdditionalFiles`, so the real release-tracking analyzer is inert. `AnalyzerReleases.Shipped.md` is a 2-line header (verified empty); the ID gaps have no tombstones.
- Closure-capture detection hand-walks descendants (`RuntimeRules.cs:47-59`; `FlowRules.cs:240-258`) — but note this walk is an IOperation walk (`ILocalReferenceOperation`/`IParameterReferenceOperation` + `ContainingSymbol` compare), symbol-correct and cheap (see 08 B1 — the demolition's "hand descendant walk" characterization is wrong and the `IFlowCaptureOperation` re-base is rejected).
- `IsSimpleCharsetLengthRegex` hand-parses regex text char-by-char with `IndexOf` arithmetic (`SymbolFacts.cs:338-380`) — a fragile mini-parser for CSP0607.
- Stringly type identity everywhere instead of cached symbol comparison: `ToDisplayString() == "System.Threading.Tasks.Task"` at `SymbolFacts.cs:392-399,416,425-440`, `RuntimeRules.cs:80,85,103,109,122-124,135,275,279`. `AnalyzerState` caches exactly 2 well-known types (`:39-40`); the rebuild hoists ALL via `GetTypeByMetadataName` at CompilationStart, compared with `SymbolEqualityComparer` (verdict c).
- `PrimaryConstructor` guessed by `OrderByDescending(Parameters.Length).First` (`SymbolFacts.cs:1046-1051`) instead of matching the ctor whose `DeclaringSyntaxReferences` is the `TypeDeclaration` parameter list — wrong when a secondary ctor has more params.

## [4]-[WEAK_TYPING_AND_NAME_HEURISTIC_RULES]

These directly violate the repo's own `CLAUDE.md [4]` ban on coupling analyzer rules to project namespaces/paths/one-off symbols.

- Member-name-fragment classification: `OperationalReceiptNames` (`"Added","Changed","Moved"…` `SymbolFacts.cs:231-235`) and `ProofReceiptFragments` (`"Path","Status","Kernel","Grid"…` `:236-240`) classify receipt types by substring — `"Path"`/`"Status"` match a huge name surface; CSP0730/0731/0732 are built on this sand.
- Hardcoded project type/member names inside generic-sounding rules: `IsProjectionOwner` → `"AtomProjection"`/`"OpAcceptance"`/`"Self"`/`"Value"`/`"Custom"`/`"AcceptResults"` (`:651-654`); `IsMutationReceiptDocumentWrapper` → `"MutationReceipt"`/`"DocumentReceipt"` (`:979-1012`); `IsConfirmOwner`/`IsDemandOwner` → method names `"Confirm"`/`"Demand"` (`FlowRules.cs:307-316`); `AcceptValueIsValidTypeNames` hardcodes 21 Rhino geometry SIMPLE names matched by `Name` only — any type named `"Line"`/`"Interval"`/`"Box"` anywhere qualifies (`SymbolFacts.cs:222-225`); `IsErrorLike` → `Name == "Error" || "Expected"` (`:1079-1081`); `HasReceiptFactStream` → field type `ToDisplayString().Contains("Seq<")` (`:952-958`).
- `TypeShapeRules.cs:324` group-key includes `symbol.GetType().Name` — the RUNTIME Roslyn implementation class name (e.g. `SourceOrdinaryMethodSymbol`) as a discriminator; version-fragile; should be `ISymbol.Kind`.
- `ScopeKind` is a hand-rolled 5-bool class (`SymbolFacts.cs:14-45`) — fine concept, weak encoding.

## [5]-[SCOPE_CLASSIFICATION_THE_CENTRAL_FRAGILITY]

`ScopeModel.Classify` (`SymbolFacts.cs:161-206`) + `Markers` (`:49-71`) decide EVERYTHING by namespace/path substring. This is the primary adversarial hole for an agent-enforcement tool (verdict b).

- Boundary by path fragment `"Adapter.cs"`/`"Boundary.cs"`/`"Endpoint.cs"`/`"Controller.cs"` or namespace `".Rhino"`/`".Grasshopper"`/`".Adapter"` (`:52-53`) — an agent evades ALL domain rules by file rename or namespace suffix; the acknowledged-but-unbuilt "BoundaryAdapter exemption auditability" rule (`RuleCatalog.cs:14-16`) is exactly the missing countermeasure.
- Composition by substring `"Infrastructure"`/`"Composition"`/`"Bootstrap"`/`"DependencyInjection"` ANYWHERE in path (`:66-67,503-505`) — a file named `CompositionLaw.cs` in Domain silently exits domain discipline.
- Repo-layout literals baked in: `"/libs/csharp/Rasm/Analysis/"` (`:65`), `"/Rasm.Grasshopper/"` (`:53`) — breaks on any folder move; violates the project-agnostic rule.
- `IsHotPath` = namespace contains `".Performance"` OR path contains `"Performance"` (case-insensitive, `:94-96`) — any `PerformanceReport.cs` gets LINQ banned (CSP0601) as unsuppressable Error.
- `Classify` reads `DeclaringSyntaxReferences[0]` only (`:166-169`) — partial types classified by whichever declaration Roslyn lists first; ordering-fragile.
- Test detection includes assembly-name `EndsWith ".Tests"` and path `"Test.cs"`/`"Tests.cs"` substrings (`:51,172-173,202-205`).
- Culture posture is otherwise sound (Ordinal/OrdinalIgnoreCase explicit throughout; `InvariantCulture` in quantifier parse `:372-375`).

## [6]-[PERF_HAZARDS]

- O(types × wholeNamespaceTree) walks: `ClosedUnionCases` recursively scans `compilation.GlobalNamespace` for EVERY named type via `CheckDiscriminatedUnionShape` (`SymbolFacts.cs:1015-1024` ← `TypeShapeRules.cs:87`), AGAIN per `Switch` invocation via `TryClosedUnionDispatch` (`:1415`), and a THIRD copy in `UnionOpsGenerator.UnionCases` (`UnionOpsGenerator.cs:39-49`). Quadratic on large compilations.
- `UnionOpsGenerator` captures `Compilation` inside `IncrementalValuesProvider` (`UnionOpsGenerator.cs:16-20`) — destroys incrementality, pins compilations.
- `OperationKind.Literal` registered globally (`DomainStandardsAnalyzer.cs:44`) → `CheckHardcodedOtlp` runs `Uri.TryCreate` on EVERY string literal (`RuntimeRules.cs:211-225`).
- Every `IInvocationOperation` runs 23 checks (`AnalyzerDispatcher.cs:74-100`), most doing repeated `ContainingNamespace.ToDisplayString()` allocations; `ScopeInfo.IsHotPath` re-does 2 `string.Contains` per access per rule.
- Switch expressions walked twice: `OperationKind.SwitchExpression` (`DomainStandardsAnalyzer.cs:37`) AND `SyntaxKind.SwitchExpression` (`:54`) with CSP0739 emitted from both pipelines (`FlowRules.cs:267-281` vs `:272-291`) — split-brain duplicate machinery for one rule.
- Per-member `GetSemanticModel`/`GetDeclaredSymbol` in `CheckOverloadAdjacency` (`ShapeRules.cs:286`); `ImplementedInterfaces` doing `AllInterfaces × GetMembers × FindImplementationForInterfaceMember` per method (`:310-323`); `CheckPassiveSiblingSurfaceFamily` does `GetSemanticModel`+`GetOperation` per member (`TypeShapeRules.cs:311-328`); `HasCrossSlotProjection` walks all descendants of every ≥3-Option-slot type (`SymbolFacts.cs:1141-1154`).
- GOOD perf hygiene that exists: `RegisterCompilationStartAction` batching with per-compilation `AnalyzerState` (`DomainStandardsAnalyzer.cs:27`), `EnableConcurrentExecution` (`:26`), `GeneratedCodeAnalysisFlags.None` (`:25`), `ConcurrentDictionary` state with `SymbolEqualityComparer` (`AnalyzerState.cs:18-26`), scope memoization (`:64-65`).
- Compilation-end rules (CSP0501/0503/0724/0740/0744, `AnalyzerDispatcher.cs:190-196`) depend on the whole compilation being visited — correct for CLI builds, but IDE live analysis does not reliably run end actions, so these are batch-build-only and undocumented.
- Tests run `concurrentAnalysis:false` (`AnalyzerTestHarness.cs:43`) while production enables concurrency — the concurrent path is never tested.

## [7]-[CORRECTNESS_AND_BEHAVIOR_FRAGILITY]

- All 87 rules `Error` + `WellKnownDiagnosticTags.NotConfigurable` (`RuleCatalog.cs:21,26`): severity cannot be tuned, diagnostics cannot be suppressed via editorconfig/pragma — any false positive is a hard build break whose only escape is editing the analyzer itself (the de facto repo workflow; see MEMORY.md CSP0705 episode). Deliberate, but it makes every heuristic rule a production-outage vector.
- Single-use-helper counting (`AnalyzerState.cs:74-82,123-127`): counts invocation operations only — method-group references uncounted (a private method invoked once + passed once as a delegate still reports "inline it"); recursion self-calls inflate; cross-project callers invisible (per-compilation). The same per-compilation blindness makes CSP0501 InterfacePollution fire on interfaces implemented in sibling assemblies.
- `IsRegexMatchCall` is dead logic in CSP0002/0705 gates (`FlowRules.cs:44-51`): the tuple requires `IsLanguageExtMatch=true`, which already excludes `Regex.Match`; the two can never both be true (08 D3: deletion is safe).
- CSP0729 adjacency adds the seed member unconditionally (`ShapeRules.cs:269-280`); the interface-exemption logic only filters subsequent members (`IsMisplacedOverload`), making exemption asymmetric by declaration order.
- The generator emits `global::Rasm.Domain.Op` and keys on `Rasm.Domain.GenerateUnionOpsAttribute` (`UnionOpsGenerator.cs:13,68`) — the analyzer ASSEMBLY is hard-coupled to one consumer project; attributes live in `libs/csharp/Rasm/Domain/Validation.cs:9-12`.
- The generator emits cases via `List<string>` + `string.Join("\n")` (no `IndentedTextWriter`, no `#nullable`, no `#pragma`) and silently skips when `cases.IsEmpty` (`:53-55`) — `[GenerateUnionOps]` on a caseless union produces nothing and CSP0802 stays satisfied.
- Tests prove rules match by NAME: fake `namespace LanguageExt { class Fin<T> }` shims satisfy the matchers (`RuleBehaviorTests.cs:288+`) — assembly identity is never checked anywhere.

## [8]-[RULE_CATALOG_STRUCTURE_AND_TEST_COVERAGE]

- Declaration: 87 `static readonly DiagnosticDescriptor` fields built by one `Err()` factory (`RuleCatalog.cs:25-26`), grouped by themed banners, with high-value XML docs on ~10 newer rules (CSP0005:34-42, 0723:126-133, 0724-0728:134-183, 0802:203-211) carrying exemption semantics; `All` hand-enumerated (`:215-228`). Registration: single analyzer registers 3 symbol kinds, 17 operation kinds, 5 syntax kinds + compilation-end (`DomainStandardsAnalyzer.cs:29-55`); central `AnalyzerDispatcher` fans out via `switch` over `(IsAnalyzable, node)` (`AnalyzerDispatcher.cs:16-196`). Deferred-rule backlog in a comment (`RuleCatalog.cs:8-16`). `isEnabledByDefault:true` on all; no `CodeFixProvider`; no `DiagnosticSuppressor`; no `.editorconfig` keys read anywhere.
- Guard tests force ≥1 positive per active rule (`RuleBehaviorTests.cs:924-945`), every descriptor emitted somewhere (`ReleaseDisciplineTests.cs:37-53`), and unshipped-md matched to `SupportedDiagnostics` (`:11-25`). So all 87 rules have exactly ≥1 positive test.
- Negative (clean-code) tests exist for only ~29-31 rules (CSP0001/0002/0003/0005/0203/0405/0406/0506/0701/0703/0705/0723/0724/0725/0727/0729/0730-0732/0734-0745). ~53 rules — the entire CSP0004-0608 foundation band plus most of 0702-0720 — have NO negative test, so their false-positive boundary is untested, violating the repo's own positive+negative mandate.
- Assertion depth is shallow: the table Theory checks first-match severity/category/`location≠None` only (`:904-922`); no span, no message-args, no fire-count; duplicate-emission regressions invisible.
- Harness compiles single-tree against TPA refs — LanguageExt/Thinktecture/Rhino behavior testable only through in-source shims, which is why nothing verifies real-package shapes.

## [8a]-[PACKAGING_AND_WIRING]

- Reaches builds via `Directory.Build.props:350-352` `ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false"`, conditioned by `SkipLocalCSharpAnalyzerReference` (self + analyzer tests skip, `:84-93`). `BoundaryContracts.cs` is Compile-linked into consumers (`:354-356`) so the marker attributes need no runtime dependency — but the Skip list (Rhino/GH/test/bridge, `:94`) means those projects can't use `[BoundaryAdapter]` markers, and that Skip set matches the IVT grants exactly (`:250-256`) — so the linked-source approach is structurally constrained by CS0436-under-IVT (08 G1).
- `CsAnalyzer.csproj`: `netstandard2.0`, `EnforceExtendedAnalyzerRules`, `IsRoslynComponent` — correct; references `Microsoft.CodeAnalysis.CSharp` 5.3.0 + `Microsoft.CodeAnalysis.Analyzers` 5.3.0 (`Directory.Packages.props:127-132`). Missing: `AdditionalFiles` for AnalyzerReleases md (release tracking inert despite the Analyzers package being referenced); stray `<Compile Remove="tests/**"/>` for a non-existent tests dir; pointless producer-side `EmitCompilerGeneratedFiles`.
- Zero CSP suppressions exist in libs/apps (rg: 0 `#pragma warning disable CSP` / `SuppressMessage(CSP)`) — either rules are clean or evasion happens via scope spoofing; no telemetry distinguishes.

## [9]-[GENUINELY_GOOD_CARRY_FORWARD]

1. Single-analyzer + central dispatcher + per-compilation state architecture (one registration pass, batched compilation-start, concurrency enabled) — the right skeleton.
2. Scope taxonomy (Generated/Test/Boundary/Domain/Application/Shared/Analysis/Composition) with attribute escape hatches and the build-injected contracts file — keep the CONCEPT, replace substring classification with explicit project-level MSBuild properties or assembly/type attributes (machine-checkable, unspoofable).
3. Receiver-walk combinators `ExtractReceiver`/`UnwrapReceiver`/`UnwrapLambda`/`UnwrapValue` handling extension-method receivers in `Arguments[0]` (`SymbolFacts.cs:253-283`) — subtle and correct; reuse verbatim.
4. `IsBoundaryMatchUsage` terminal-position analysis (return / last-expression-statement / arrow-body equivalence, `SymbolFacts.cs:469-486`) — a real operationalization of "Match only at boundary".
5. Meta-tests as catalog invariants (every rule emitted, every rule positively tested, release-md sync) — keep, move release tracking to native RS2000, add a per-rule negative-test invariant.
6. Exemption algebra encoded in rules: CSP0005's params-`ReadOnlySpan`/Union-pair/input-shape-polymorphism relaxations with rationale (`ShapeRules.cs:148-202`); CSP0728's strict `Try.lift→Run→MapFail` chain that deliberately permits Op-level `MapFail` discards (`RuleCatalog.cs:166-177`); CSP0727's parenthesization-aware precedence trap born from a real production bug (`:156-165`). These encode doctrine nuance agents need.
7. Compilation-end aggregation for whole-program facts (interface pollution, flags-enum composition, closed-union plan fusion) — correct Roslyn idiom.
8. Decision-useful message texts naming the exact remediation API ("use `Seq<T>.Fold` or `TraverseFin`", "`guard(condition, error).ToFin()`") — ideal for agents; keep the remediation-in-message convention (`RuleCatalog.cs:123,148,193`).
9. Deterministic harness details: hashed assembly names per source, ordered diagnostics, analyzer-exception rethrow (`AnalyzerTestHarness.cs:42,62-67`).

## [10]-[REBUILD_CRITICAL_VERDICTS]

The eight binding outputs the target architecture (04) and parity map (05) must satisfy:

| [ID] | [VERDICT] | [LANDS IN] |
| :--: | --------- | ---------- |
| (a) | Replace ~70 hand-written rule bodies with a declarative `RuleRow` table + one generic executor | 04 §KERNEL, 05 |
| (b) | Replace substring scope classification with explicit, build-asserted scope declaration | 04 §SCOPE, 07 §SCOPE |
| (c) | Hoist all well-known types to a CompilationStart symbol cache; ban `ToDisplayString` in predicates | 04 §FACTS, 06 meta-grep |
| (d) | Excise every project-symbol-name heuristic into semantic attributes or `AdditionalFiles`/`AnalyzerConfigOptions` config | 04 §CONFIG, 05 |
| (e) | Mandate positive+negative pairs per rule via meta-test | 06 |
| (f) | Adopt `Microsoft.CodeAnalysis.Testing` with span markup | 06 |
| (g) | Fix generator incrementality and decouple it from `Rasm.Domain` (extract to consuming project) | 04 §GENERATOR, 07 |
| (h) | Keep Error severity for decidable law but replace `NotConfigurable` with an audited escape hatch (heuristic rules + unsuppressable errors = analyzer-edit-as-pressure-valve, the current de facto workflow) | 04 §TIERS, 05 §TIERS |
