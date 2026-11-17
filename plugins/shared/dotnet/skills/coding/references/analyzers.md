# [ANALYZERS]

Diagnostics derive from resolved source; code actions transform immutable document snapshots.

## [01]-[REGISTRATION]

Select syntax actions for grammar, operation actions for bound uses, and symbol actions for declarations. Register relevant kinds before callbacks inspect candidates. Relationships within a body collect under operation-block start and end actions; relationships within a type collect under symbol start and end actions. Cross-file aggregates collect under compilation start and end actions.

Resolve dependency declarations in compilation-start state and register each rule with its required symbols. `GetSpecialType`, `WellKnownMemberNames`, and `DocumentationCommentId` supply compiler and API contracts; metadata names belong at dependency resolution. Derive overload sets through resolved owners. Missing dependencies disable their own matches without disabling unrelated rules or inventing substitute declarations.

Analyzer instances can serve successive compilations. Compilation-start closures own symbols and compilation-dependent caches. `EnableConcurrentExecution` permits concurrent callbacks; start and end actions establish lifecycle ordering, while collecting actions require concurrent storage or isolated ownership. End actions consume completed collections. Cancellation can omit end actions, so resource disposal belongs to a scope that completes independently of callback scheduling.

Consume semantic models supplied by contexts or operations; `Compilation.GetSemanticModel` creates another model outside shared analyzer state. Rules joining models across trees collect semantic-model actions and candidates independently, then consume an immutable model index at compilation end. Ordinary callback order establishes no readiness contract. End analysis uses its own cancellation token, and filtered editor requests establish no whole-compilation absence.

`ConfigureGeneratedCodeAnalysis` separates generated-source collection from diagnostic reporting. `Analyze` enables callbacks for generated facts; `ReportDiagnostics` controls findings at generated locations, including compilation-end reports. Semantic queries expose generated declarations independently of callback policy. Analyzer options come from `AnalyzerConfigOptionsProvider` at global, syntax-tree, or additional-file scope. Per-file configuration cannot be inferred from global options alone. Severity controls reporting and can affect scheduling; filtering produced diagnostics saves no analysis work.

## [02]-[BINDING]

Compare symbols through `SymbolEqualityComparer.Default`, or `IncludeNullability` when annotations affect the contract. Normalize to `OriginalDefinition` only when a rule equates declarations independently of construction. Preserve substitutions when inspecting a use. Reduced extension methods reach declarations through `ReducedFrom`; override chains and interface implementations retain their native relationships.

Match a generic interface declaration by definition, then bind members of each constructed interface:

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Contracts {
    extension(INamedTypeSymbol type) {
        public ImmutableArray<ISymbol> Implementations(INamedTypeSymbol contract, CancellationToken token) =>
            [.. type.AllInterfaces
                .Where(face => {
                    token.ThrowIfCancellationRequested();
                    return SymbolEqualityComparer.Default.Equals(face.OriginalDefinition, contract.OriginalDefinition);
                })
                .SelectMany(static face => face.GetMembers())
                .Where(static member => member.Kind is SymbolKind.Method or SymbolKind.Property or SymbolKind.Event)
                .Select(member => {
                    token.ThrowIfCancellationRequested();
                    return type.FindImplementationForInterfaceMember(member);
                }).OfType<ISymbol>()];
    }
}
```

`FindImplementationForInterfaceMember` receives constructed members and supplies implicit, explicit, inherited, and default interface implementations. Explicit implementation lists alone miss ordinary implementations. Interface members without an implementation produce no result; returned symbols keep their constructed meaning.

`IArgumentOperation.Parameter` identifies bound parameters independently of argument spelling or source order. `ArgumentKind` distinguishes explicit arguments, defaults, and parameter-array expansion. Parameter ordinals describe declaration correspondence, not evaluation order. Array accesses expose indices through `IArrayElementReferenceOperation`; property parameters describe indexers alone.

Expression equivalence retains selected overloads, instantiated types, constant kinds, comparer behavior, and observable evaluation. Read `ConstantValue` before removing implicit conversions. Decimal scale and floating-point representations matter when consumers observe them. Property reads, indexers, conversions, operators, and calls can execute user code. Speculative binding establishes replacement binding, not purity or equivalent effects.

`nameof` method groups can expose candidates through `SymbolInfo.CandidateSymbols` without ordinary reference operations. `GetAwaitExpressionInfo` supplies bound awaiter members and dynamic-await information. Name filters reduce candidate work; semantic identity decides ownership. Missing symbols, invalid operations, and absent models are legitimate answers for incomplete editor input. Report only violations established by resolved evidence.

## [03]-[SCOPES_AND_FLOW]

Collect shared facts once at their narrowest common scope. Repeated whole-body walks from individual operations multiply traversal work. `Descendants` and `DescendantsAndSelf` use an explicit stack; `OperationWalker` recurses. Descendant enumeration enters nested functions, so body analysis defines function boundaries explicitly. Parent, base-type, and override chains iterate toward their endpoints. Pointwise questions short-circuit without building an index.

Use callback contexts' control-flow queries for their executable roots. Nested functions require local-function or anonymous-function graphs obtained from their actual parent graph. Declarations do not establish invocation; delegate targets and captured values follow definitions reaching each invocation. Opaque callbacks retain their external execution contracts.

`ControlFlowGraph` contains a lowered operation tree distinct from `OriginalOperation`. Object identity does not cross those trees, and one syntax span can identify different lowered events. Read `BasicBlock.BranchValue` beside block operations. Initializers can lower to assignments; flow captures identify intermediate values introduced by lowering.

Reaching definitions replace overwritten values and join predecessor alternatives at each read. Unknown incoming values differ from unreachable reads. Normal successors do not enumerate every implicit exception transfer; exception regions and required failure semantics determine additional paths. `FinallyRegions` execute in order before a branch destination, and `StructuredExceptionHandling` completion resumes its saved continuation. Traversal state includes remaining continuations, so one finally body can reach different destinations. Throwing branches without destinations require their exception-region semantics as well.

Recursive generic queries need a finite analysis domain. Deduplicating constructed types cannot terminate expansion that continually nests type arguments; deduplicating original definitions loses substitutions. Summaries over type parameters and query modes join alternatives monotonically to a least fixed point. Follow member, argument, and indirection relationships only when the rule examines them. Graph packages supply traversal, strongly connected components, and dependency ordering; transfer functions and joins define analysis meaning.

## [04]-[CACHES]

Cache derived facts where repeated consumers share semantic inputs. Keys retain constructed types, query modes, positions, and flow continuations whenever those facts affect an answer. Semantic models stay paired with their syntax trees and compilation. `LookupSymbols` depends on source position, so file-wide results cannot replace contextual lookup.

Compilation ownership bounds retained syntax, symbols, and models. Weak compilation keys support shared analysis without retaining completed compilations. Concurrent dictionary factories can run repeatedly; factories compute unpublished values without changing another index or acquiring resources. Keep related facts in one cached result so atomic insertion publishes their relationship together.

Publish completed answers, including absence, without retaining cancellation or thrown exceptions. Exception-caching `Lazy` instances can preserve a canceled computation for later requests. `SyntaxTreeValueProvider` uses weak keys, accepts no callback cancellation token, and converts callback exceptions into failed lookups. Keep cancellable semantic work in caches that preserve cancellation. Observe callback tokens during long traversals, pass tokens into supported native queries, and propagate cancellation to Roslyn without a diagnostic or fallback answer.

## [05]-[EDITS]

Code actions read semantic facts from their captured document snapshot. Primary and additional locations identify source sites; diagnostic properties preserve facts a fix consumes. Resolve cross-document locations through their solution documents. Display text serves readers and does not select behavior. Rewrites preserve evaluation count, argument order, conversions, scope, and binding outside their intended correction.

Group findings by original node and construct replacements from descendants already rewritten by `ReplaceNodes`:

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Edits {
    extension(SyntaxNode root) {
        public SyntaxNode Replace<TNode>(
            IEnumerable<(TNode Node, Diagnostic Diagnostic)> sites,
            Func<TNode, ImmutableArray<Diagnostic>, TNode> replace, CancellationToken token) where TNode : SyntaxNode {
            ILookup<TNode, Diagnostic> findings = sites.Select(site => {
                token.ThrowIfCancellationRequested();
                return site;
            }).ToLookup(static site => site.Node, static site => site.Diagnostic);
            return root.ReplaceNodes(findings.Select(static group => group.Key), (original, current) => {
                token.ThrowIfCancellationRequested();
                return current.CopyAnnotationsTo(replace(current, [.. findings[original]]))
                    .WithAdditionalAnnotations(Formatter.Annotation);
            });
        }
    }
}
```

Replacement functions own trivia placement, including comments inside removed syntax. Annotation copying retains tracked identities through replacement. `TrackNodes` and `GetCurrentNode` preserve correspondence across sequential edits. Dependent declaration removals follow dependency order; source positions cannot order references across members. `SyntaxGenerator` constructs escaped identifiers and type expressions. Simplification and formatting annotations delegate cleanup to Roslyn; parentheses preserve precedence until simplification establishes safe removal.

`FixAllProvider.Create` coordinates independent document transformations with document, project, and solution scopes by default. Its callback contributes document content only; additional scopes require an explicit declaration. `BatchFixer` merges independent actions and retains nonconflicting edits. Overlapping edits and shared declarations require coordinated transformations. Equivalence keys identify semantic alternatives independently of occurrence names. Cross-document or project changes return a solution action with a provider owning that scope.

## [06]-[LOADING]

Compiler and editor hosts own Roslyn and framework assembly identities. Target frameworks describe compile-time APIs without establishing compatibility with loaded host assemblies. Direct package references own each named API; references to host-supplied compiler APIs exclude runtime assets. `PrivateAssets` controls propagation without preventing runtime copying.

Analyzer project references can return runtime dependencies through `TargetPathWithTargetPlatformMoniker` before `GetTargetPath`. `RuntimeCopyLocalItems` supplies NuGet's resolved closure; `IncludeRuntimeDependency=false` and `ResolveableAssembly=false` keep companion paths out of application runtime and reference assembly items. Deployment supplies required dependency paths to the analyzer loader while retaining host-owned compiler assemblies.

`DiagnosticAnalyzerAttribute` and `ExportCodeFixProviderAttribute` declare discovery metadata. Construction and instance lifetime belong to the consuming host. Composition attributes apply when that host uses their container; export metadata alone establishes no shared-instance behavior.

## [07]-[MEASUREMENT]

Build duration, compiler startup, and analyzer execution measure different costs. Incremental builds can skip compilation; compiler servers retain JIT and metadata state outside the measured client process. Compare equal source, package resolution, target, worker settings, and compiler-server policy. Record wall time, CPU, and peak resident memory separately.

Measure fresh analyzer drivers; repeated queries on one `CompilationWithAnalyzers` can return retained results. Prepare parsing, references, and generator output before timing. State whether compiler binding ran before driver creation; driver queries include orchestration and required binding. Inject elapsed-time measurement and retain one process-wide allocation interval across asynchronous work:

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Measurements {
    extension(Compilation compilation) {
        public async Task<(AnalysisResult Result, Duration Elapsed, long Allocated)> MeasureAsync(
            ImmutableArray<DiagnosticAnalyzer> analyzers, CompilationWithAnalyzersOptions options,
            TimeProvider time, CancellationToken token) {
            CompilationWithAnalyzers driver = compilation.WithAnalyzers(analyzers, options);
            long allocated = GC.GetTotalAllocatedBytes(precise: true);
            long started = time.GetTimestamp();
            AnalysisResult result = await driver.GetAnalysisResultAsync(token).ConfigureAwait(false);
            return (result, time.GetElapsedTime(started).ToDuration(), GC.GetTotalAllocatedBytes(precise: true) - allocated);
        }
    }
}
```

Process-wide allocation counters include other managed work and exclude native allocations. Isolated workloads make intervals attributable; thread allocation counters cannot follow asynchronous continuations. Enable `LogAnalyzerExecutionTime` for execution telemetry. Telemetry counts registered actions, not executions or individual descriptors. Overlapping analyzer timers can exceed wall time; multi-rule analyzers require separate workload attribution.

Compare cold and warm execution, serial and concurrent analysis, cancellation after analysis begins, and successive compilations with distinct symbols. Vary declaration depth, reference count, generic nesting, branch alternatives, and graph cycles to expose scaling and retained-state defects. Performance comparisons preserve diagnostic multiplicity, locations, and rewrite semantics alongside time and memory.
