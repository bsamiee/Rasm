# efcore-law — bedrock

## context shape and lifecycle

- The context is a unit-of-work capsule, not a service: one sealed context type per store profile, owning model declaration, interceptor admission, and the provider row — never resolved as a long-lived dependency.
- Model initialization happens at the first operation, not at construction: constructing a context is free; the first `Add` or query pays the model build. Boot gates that need the model warm must issue a deliberate first operation.
- Pooling is the default posture for hot paths: `AddDbContextPool` or `new PooledDbContextFactory<TContext>(options, poolSize)`; the pool ceiling defaults to 1024.
- Exceeding the pool ceiling does not fail — the pool silently degrades to transient creation, a performance cliff with no error signal; size the ceiling to peak concurrent brackets.
- A pooled context is effectively singleton state: `OnConfiguring` runs exactly once on first creation, so anything written there is frozen for the pool's lifetime.
- Per-acquisition state (tenant discriminant, actor identity) is injected by a wrapping `IDbContextFactory<TContext>` that acquires from the pooled factory and stamps the instance before handing it out.
- The wiring shape for stamped pooling is three registrations: the pooled factory as singleton (`AddPooledDbContextFactory`), the stamping factory as scoped, and the context resolved scoped from the stamping factory — collapsing any of the three breaks either pooling or the stamp.
- Pool return resets EF-owned state only; ADO.NET state (a manually opened `DbConnection`, driver session settings) is never reset and leaks across unrelated acquisitions — restore driver state before the bracket closes.
- Pooling halves single-op latency and cuts per-op allocation roughly tenfold; it is orthogonal to driver connection pooling, which the store composes and never re-implements.
- `PooledDbContextFactory` beats DI-injected pooled contexts by a small constant (DI pool management adds overhead) — prefer the factory in measured lanes.
- Connections open per operation and close immediately after, regardless of context lifetime — context pooling never holds connections out of the driver pool.
- `EnableThreadSafetyChecks(false)` deletes the concurrent-use detector; admitted only as a measured-lane policy row after the no-shared-context invariant is proven structurally (the bracket makes sharing impossible).
- `UseQueryTrackingBehavior(QueryTrackingBehavior...)` declares a context-wide tracking default — the read-profile context row sets no-tracking once instead of per-query annotations.
- `EnableDetailedErrors` wraps every data-reader value read in per-column fault evidence — a gated-diagnostics row for development and incident profiles, never the production default (per-value wrapping has measurable cost).
- One memory governor owns both caches: EF self-configures an internal memory cache with size limit 10240, where a compiled query costs 10 units and a built model 100; `UseMemoryCache` admits a shared, observable instance.
- When a shared memory cache declares a size limit, every consumer must size its entries or insertion throws — the limit is a contract, not a hint.
- A query-cache hit rate stable below 100% after warmup is a shape-instability diagnostic: some lane is building expression trees with embedded constants.
- Each distinct options permutation builds its own internal service provider; `EnableServiceProviderCaching` and `UseRootApplicationServiceProvider` govern the cache — profiles vary options as data on few rows, never as per-request permutations, or internal providers proliferate.
- Transaction posture is declared, not improvised: `AutoTransactionBehavior` selects when saves get implicit transactions; `AutoSavepointsEnabled` (default true) nests savepoints inside caller-owned transactions so a failed save rolls back to the savepoint, not the whole transaction.
- Lifecycle is a total legal-transition law: built → gated → serving → draining, every transition a declared row; a store entering serving without passing its gates is the rejected path, not an optimization.
- `EnsureCreated` and migration-applied stores are mutually exclusive entries: `EnsureCreated` bypasses the history mechanism, so a later migration apply fails; the ephemeral/test profile row is the only admission for `EnsureCreated`/`EnsureDeleted`.
- `CanConnect` is a gate probe, never a serving-state substitute — reachability is one input to the gated state, not the verdict.

## compiled models

- A compiled model replaces runtime model building with a generated partial class deriving `RuntimeModel`, exposing a static `Instance`, consumed via `UseModel(GeneratedModel.Instance)`.
- The generated `partial void Initialize()` / `partial void Customize()` hooks are the only sanctioned customization points on the generated model.
- The payoff threshold is model size, not query volume: hundreds of entity types and up; compiling a small model buys nothing and costs a standing regeneration obligation.
- Exclusions are structural, not tunable: global query filters (named filters included), lazy-loading and change-tracking proxies, value converters referencing private methods, and custom `IModelCacheKeyFactory` all foreclose the compiled-model row.
- The filter conflict is composition-detectable: a profile declaring named query filters cannot also declare the compiled-model row — reject the pairing at composition, never discover it at runtime.
- Staleness is never self-detected — the model must be regenerated on every model change; the obligation is the row's standing cost.
- The runtime half of the fingerprint gate: under `UseModel`, `databaseFacade.HasPendingModelChanges()` (service twin `IMigrator.HasPendingModelChanges()`) diffs the active — compiled — model against the latest migration snapshot.
- A `true` verdict converts "added a migration, forgot to regenerate" into a typed boot rejection — the one staleness class the runtime can see, because the snapshot advanced while the compiled model did not.
- The inverse hole remains open at runtime: a model edit with neither a new migration nor regeneration is invisible to every runtime check — close it at build time with regenerate-and-diff, not at boot.
- Multiple compiled models per context type live in distinct namespaces and are selected at runtime by a configuration discriminant; this is the sanctioned replacement for `IModelCacheKeyFactory` under compiled models.
- Build-time generation: the `Microsoft.EntityFrameworkCore.Tasks` package is non-transitive — every generating project references it directly.
- The MSBuild knobs: `EFOptimizeContext=true` enables; `EFScaffoldModelStage` and `EFPrecompileQueriesStage` stage generation (`build` | `publish` | `none`); `DbContextName` scopes to one context; `EFTargetNamespace`, `EFOutputDir`, `EFNullable` shape output.
- MSBuild-path constraint one: the context project must be design-time self-sufficient — `IDesignTimeDbContextFactory<TContext>` in the context project when host wiring lives in another project, because a separate startup project would invert the build dependency.
- MSBuild-path constraint two: partial-hook customization is unavailable (the project must compile before the model generates), and AOT-support code is always emitted whether or not AOT publishing is intended.
- CLI route: `dotnet ef dbcontext optimize --output-dir --namespace`; the CLI path keeps partial-hook customization available.
- Compiled queries are the per-query analogue: `EF.CompileQuery` / `EF.CompileAsyncQuery` produce thread-safe delegates that bypass cache lookup entirely.
- Compiled-query constraints: one model only (multi-model context configurations are unsupported), simple scalar parameters only — member or method accesses inside parameter expressions do not compile.
- Compiled-query gains scale with operator count: the deeper the expression tree, the larger the win; trivial queries gain almost nothing.

## complex types and JSON columns

- Complex types are the document-modeling owner: value semantics end-to-end — assignment copies, comparison is by content — so one value legally aliases into two slots, and content equality translates correctly in queries.
- Structs are admitted as complex types; collections of structs are not.
- Optional complex types are admitted but require at least one required member on the complex type — an all-optional complex type is a model-validation rejection.
- Two mappings from one declaration: table splitting (flattened prefixed columns — the default) and `ToJson(jsonColumnName)` on the `ComplexPropertyBuilder` for a single document column.
- Complex collections (`ComplexCollection` on the entity builder) exist only in the JSON mapping — a collection of complex values cannot table-split.
- `HasJsonPropertyName` renames a member inside the document; `HasColumnType` pins the column's store type — both live on the complex builders.
- Owned-entity JSON is the rejected form for new models: reference semantics forbid aliasing one value into two slots, and identity bookkeeping shadows every instance.
- The decisive owned-versus-complex asymmetry: set-based update of document interiors is foreclosed for owned mappings and fully supported for complex ones — the model-shape choice silently decides which write lane the aggregate may ever use.
- Migrating owned → complex is a model-shape change, not a data change, when the column mapping is held constant — the stored document is identical.
- Primitive collections (`PrimitiveCollection`) and parameterized query collections share one translation-mode axis with three rows: multi-parameter expansion with cardinality padding (the default — stable SQL shape, visible cardinality), single JSON-array parameter, inlined constants.
- The mode is declared globally via `UseParameterizedCollectionMode(ParameterTranslationMode...)` or the `TranslateParameterizedCollectionsToConstants` / `...ToParameters` rows, and overridden per call site with `EF.Constant` / `EF.Parameter`.
- Padding duplicates the last value upward so eight values cost ten parameters but one plan — cardinality buckets, not per-cardinality SQL.
- Inlined constants are redacted from logs by default (replaced with `?`); `EnableSensitiveDataLogging` is the single opt-back-in — no per-lane redaction toggles exist at this layer.

## generated converters and convention admission

- Generated domain types cross into the store through exactly one admission: `UseThinktectureValueConverters` on the options builder installs the conventions plugin; every keyed smart enum, value object, and keyed union then maps through its derived converter.
- Per-type converters are derived, never declared; a hand-written converter for a generated type is the rejected form.
- Scope overrides narrow, never widen: `AddThinktectureValueConverters` at model scope, `HasThinktectureValueConverter` per property.
- Complex-type properties and primitive-collection elements are covered by their own builder extensions — document interiors carry generated types with zero bespoke mapping.
- Max-length policy is conversion metadata, not a column annotation: `Configuration.Default` versus `Configuration.NoMaxLength`, specialized by `SmartEnumConfiguration` and `KeyedValueObjectConfiguration`.
- The conventions plugin runs at model build, so a compiled model bakes the converters in — conversion costs nothing at runtime that it did not already cost at mapping.
- The private-method exclusion bites at this seam: a converter factory referencing non-public members runs fine without model compilation and fails only when the model compiles; keep generated-type bridges public or internal as a standing rule.
- `ConfigureConventions` is the model-wide admission seam for everything else: `Properties<T>().HaveConversion<...>()`, `DefaultTypeMapping<TScalar>`, `ComplexProperties<TProperty>`, `IgnoreAny<T>`, and `Conventions` add/replace/remove.
- Per-property conversion declarations outside the conventions seam are the drift form — one type, one mapping, declared once.

## naming conventions

- Naming is schema policy declared once at the options builder — `UseSnakeCaseNamingConvention` and siblings — rewriting table, column, key, index, and constraint identifiers through one `INameRewriter` plugin at model build.
- Consequences cascade for free: migrations record rewritten names as schema facts, and a compiled model carries them at zero runtime cost — the rewriter never runs in production.
- One naming policy per suite; a store profile overrides it only as an explicit profile row; hand-patched per-object names are the rejected form the policy deletes.
- Changing the naming policy on a live schema is a full-rename migration — every identifier diffs at once; the policy is a day-zero decision, not a refactor.

## interceptor spine

- One spine, three altitudes, all admitted through `AddInterceptors(params IInterceptor[])` at the options builder.
- Singleton/compilation altitude: `IMaterializationInterceptor` (creating/created/initializing/initialized instance), `IInstantiationBindingInterceptor`, `IQueryExpressionInterceptor.QueryCompilationStarting` — all `ISingletonInterceptor`, stateless by contract.
- Unit-of-work altitude: `ISaveChangesInterceptor` — `SavingChanges`/`SavedChanges`/`SaveChangesFailed` plus async twins — observing the tracked graph before and after save.
- Wire altitude: `IDbCommandInterceptor` (the full command matrix — creating/created/initialized, reader/scalar/non-query executing/executed, failed, canceled, data-reader closing/disposing), `IDbConnectionInterceptor`, `IDbTransactionInterceptor`.
- Registration order is execution order; per-contract aggregators compose multiple registrations into one composite per altitude.
- The built-in `IIdentityResolutionInterceptor` pair (ignoring / updating) is the canonical tracked-conflict policy — select one as a row, never hand-roll resolution.
- Suppression is the policy lever: `InterceptionResult<T>.SuppressWithResult(value)` converts an interceptor into a gate — a read-only profile rejects `SavingChanges` by suppressing with zero.
- Suppression composes down the chain: later interceptors observe `HasResult` and respect the verdict, so a gate placed first in registration order is a gate for the whole composite.
- Every interceptor member has a pass-through default implementation, so a partial implementation compiles silently — implementing only the sync member leaves the async path uninterception; both modality twins are mandatory, and a sync-only interceptor is a defect.
- `IQueryExpressionInterceptor` runs at query compilation and its output is cached with the query: the rewrite must be a pure function of expression shape; a per-execution dynamic rewrite is the rejected form because the first execution's result replays forever.
- The spine is the attach point for tracked-save fact emission; set-based and bulk operations never pass through the unit-of-work altitude — they surface only at the wire altitude — so fact emission for those lanes is owned elsewhere and must not be expected here.
- `ConfigureWarnings(Action<WarningsConfigurationBuilder>)` is the escalation seam beside the spine: per-event throw/log policy turns chosen runtime warnings into typed failures at the options row, not into log noise.

## provider-polymorphism axis

- The provider axis is one closed profile vocabulary: a profile row carries the provider-admission delegate (the `UseX` call), capability columns, and policy values.
- The relational base owns the shared knob set every profile inherits as row data: `MaxBatchSize`/`MinBatchSize` (save batching), `CommandTimeout`, `MigrationsAssembly` (string or `Assembly` overload), `MigrationsHistoryTable(name, schema)`, `UseRelationalNulls`, `UseQuerySplittingBehavior`, `ExecutionStrategy(Func<ExecutionStrategyDependencies, IExecutionStrategy>)`, and the parameterized-collection mode.
- Engine variance is case data, not code: the embedded-engine profile's options builder adds zero members beyond the relational base — its entire variance is migration SQL generation, type mapping, and a migration lock implementation.
- The server-engine profile adds engine rows: enum and range mapping (`MapEnum`, `MapRange`), identity strategy (`UseIdentityColumns` / `UseSerialColumns`), declared extensions (`HasPostgresExtension`), and the temporal plugin (`UseNodaTime`).
- A new engine is one profile row; interior code never branches on `IsSqlite`/`IsNpgsql` — those are boundary probes for assertions and tests only.
- Structural exclusion law: capability columns are option-typed delegate slots on the profile row; a lane the engine lacks is an absent slot, so the lane never composes for that profile — exclusion at composition with a typed explanation, never a runtime not-supported throw discovered under load.
- The model cache keys by context type plus design-time flag, not by provider: one context type composed against two engines silently serves the first-built model to the second.
- Three escapes from the cache-key trap, mutually exclusive: per-profile context types (preferred — the profile row carries its own context), one compiled model per profile selected at boot, or a custom `IModelCacheKeyFactory`.
- The third escape forecloses compiled models entirely — the trilemma is legislated at composition, never discovered as a wrong-model incident.
- Storage-contribution seam: store capabilities — second-level cache backing, serializer factory, execution strategy — enter as rows on one contribution at the composition root, never as a parallel store service family.
- The `ExecutionStrategy` knob is the contribution-seam proof shape: retry enters as a profile row and the suite's single-retry-owner law arrives settled; a second retry surface beside the row is the rejected form.
- Split-versus-single query is a two-level policy: profile default via `UseQuerySplittingBehavior`, per-query override via `AsSplitQuery`/`AsSingleQuery`.
- Split-query ordering is deterministic — the key is appended to the subquery ordering — so split is safe as a profile default where collection includes dominate.

## divergent

- compiled-model-fingerprint — the one startup-cost law: model acquisition is a three-route fold declared per profile row — compiled (`UseModel` + `HasPendingModelChanges` gate), cached-built (default, governed by the shared memory cache where a model costs 100 units), per-configuration (multiple compiled instances keyed by discriminant). The fingerprint gate is total only when both halves run: the runtime half catches migration/compiled-model divergence; the build half — regenerate-and-diff in CI, failing on a non-empty generated-source diff — closes the model-edit-without-migration hole no runtime check can see. Rejected forms this row forecloses: boot-time `EnsureCreated` "sync", schema probing at startup, and hash-of-DDL gates that re-derive what the snapshot diff already states. Quantitative posture: compiled models trade first-op latency for a regeneration obligation; below hundreds of entity types the obligation costs more than the latency buys.
- complex-types-converters — the one document law: a domain aggregate's storage shape is a single declaration choosing column-flattening or document mapping, with every interior value crossing through generated converters; the same declaration decides set-based-updatability (complex: yes, including document interiors; owned: no) and aliasing legality (value semantics: yes). The interaction worth legislating: a generated-type property inside a `ToJson` complex type rides converter-then-document — the converter produces the primitive, the document writer places it — so max-length policy and JSON property naming compose from two owners onto one property; declare both at the same model-building site or the drift stays invisible until a migration diff. Failure taxonomy with boundaries: struct collections (foreclosed at the shape), all-optional complex types (model-validation rejection), private converter factories under compiled models (model-compilation failure), owned-JSON aliasing (save-time identity error).
- interceptor-provider-axis — the one variance law: the interceptor spine is provider-invariant (the same registrations serve every profile) while the wire altitude is where engine variance becomes observable — cross-engine assertions live in command interceptors, never in domain code. Maximal unification: the profile row carries its interceptor set as a column — base spine rows plus engine-specific wire rows — folded into one `AddInterceptors` at composition; adding an engine adds wire rows, zero spine edits. Foreclosed forms: a per-engine context subclass overriding save behavior (the unit-of-work altitude owns it), provider `if`-branches inside an interceptor body (split into per-profile rows), exception-based read-only enforcement (suppression with a typed result is the gate). Edge with its law: suppressing at the unit-of-work altitude suppresses the save, not the tracker — entries stay dirty; a suppressing gate must also declare the tracker disposition (clear, detach, or hold-for-retry) or the next bracket inherits phantom state.
