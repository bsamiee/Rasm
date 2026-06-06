# [CSHARP_02_PLATFORM_CONTEXT]

## [TRANSCRIPT]

I operated as context / CSharp Platform Folders Context agent for `libs/csharp/Rasm.AppHost`, `libs/csharp/Rasm.AppUi`, `libs/csharp/Rasm.Compute`, `libs/csharp/Rasm.Persistence`, and `libs/csharp/Rasm.Materials`. I did not edit active files. I wrote only this requested `_reports/` report.

Read sequence:

- Read `CLAUDE.md`, which requires Markdown/docs work to use `docs/standards`, C# production context to use `coding-csharp`, current-file proof over memory, external-library-first dependency use, and central C# package version ownership in `Directory.Packages.props`; see `CLAUDE.md:14-30`, `CLAUDE.md:34-38`, `CLAUDE.md:45-56`, and `CLAUDE.md:108-112`.
- Read root `AGENTS.md`, which requires `CLAUDE.md` first, routes C# libraries through `libs/csharp/AGENTS.md` plus nearest project overlay, routes instruction-file work through `docs/standards/agents-md.md`, and keeps nested overlays delta-only; see `AGENTS.md:7-23`, `AGENTS.md:35-49`, and `AGENTS.md:53-69`.
- Read `docs/standards/README.md`, which makes current repository source/manifests/tool output the strongest documentation source and routes `AGENTS.md` work to the shared instruction-surface standard; see `docs/standards/README.md:14-23` and `docs/standards/README.md:68-79`.
- Read `docs/standards/agents-md.md`, which defines `AGENTS.md` as a scoped behavioral overlay, not README/architecture/roadmap/procedure/catalog duplication. It requires scope, trigger-driven reads, future-standard posture, owner contract, route-away, rejections, and local stop behavior only when parent guidance does not own it; see `docs/standards/agents-md.md:1-16`, `docs/standards/agents-md.md:30-41`, `docs/standards/agents-md.md:52-66`, and `docs/standards/agents-md.md:81-91`.
- Read `docs/standards/agentic-documentation.md`, which says `AGENTS.md` files are durable behavioral overlays that complement README files without duplicating them or publishing session state; see `docs/standards/agentic-documentation.md:88-94`.
- Read Codex and repo-local `coding-csharp` skill files. The governing C# posture is one polymorphic entrypoint per concern, strong typing, LanguageExt `Fin`/`Eff`/`Validation`, Thinktecture unions/smart enums/value objects, no wrapper-only APIs, and external libraries integrated as native implementation surfaces rather than forwarded facades.
- Read `libs/csharp/AGENTS.md`, then all target leaf overlays: `Rasm.AppHost/AGENTS.md`, `Rasm.AppUi/AGENTS.md`, `Rasm.Compute/AGENTS.md`, `Rasm.Persistence/AGENTS.md`, and `Rasm.Materials/AGENTS.md`.
- Read folder context for each target: README, `_ARCHITECTURE.md`, `ROADMAP.md` where present; `Rasm.AppUi.csproj`, `Rasm.Materials.csproj`, `Workspace.slnx`, `Directory.Packages.props`, `Directory.Build.props`; and the current `Rasm.Materials/Bricks` code and roadmap.
- Checked folder inventory with `fd`; target facts are: AppHost, Compute, and Persistence have docs/overlays but no `.csproj`; AppUi and Materials have `.csproj` files; Materials has active `Bricks/Brick.cs` and `Bricks/Layout.cs`.
- Checked package and graph facts with `rg`/numbered reads; AppUi is in `Workspace.slnx`, Materials is in `Workspace.slnx`, and central package management currently carries a concrete AppUi package matrix.

## [FOLDER_CONTEXT]

Parent C# library overlay:

- `libs/csharp/AGENTS.md` is correctly delta-only. It says root owns universal C# policy and quality rails, while the library overlay adds family behavior for `Rasm.*` projects; see `libs/csharp/AGENTS.md:1-4`.
- It requires this parent plus nearest project overlay for library edits, sibling-owner discovery before new public rails/folders/operation algebras/state records/receipts/proof patterns, `docs/system-api-map` for `System.*`/package/global-using/host-reference/global-json changes, `docs/external-libs` for product-library or host SDK assumptions, and `docs/host-libraries.md` for host-composition packages; see `libs/csharp/AGENTS.md:5-12`.
- Its library contract is strong: library projects set capability ceilings; missing callers do not justify weak abstractions or wrapper-only APIs; native/domain capability should be captured deeply behind one small OOP boundary and typed FP/ROP rails; see `libs/csharp/AGENTS.md:13-18`.
- It routes the five target projects explicitly: Materials as host-free material catalogue, AppUi as product UI rail, AppHost as runtime platform, Compute as measured execution platform, and Persistence as local durable state; see `libs/csharp/AGENTS.md:27-40`.
- It rejects common duplicate surfaces and wrapper-only APIs; see `libs/csharp/AGENTS.md:49-55`.

AppHost:

- `libs/csharp/Rasm.AppHost` currently contains `AGENTS.md`, `README.md`, `ROADMAP.md`, and `_ARCHITECTURE.md`; no `.csproj` is present in the folder inventory.
- README states `Rasm.AppHost` owns runtime profiles, startup/drain/unload, scheduling, bounded in-process flow, telemetry correlation, external-hop policy, typed config, health/readiness, and lifecycle receipts; see `libs/csharp/Rasm.AppHost/README.md:3-12`.
- AppHost's current status says the project file is to be created in Phase 0 and package references are to be added centrally in Phase 0; see `libs/csharp/Rasm.AppHost/README.md:13-23` and `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:17-23`.
- The folder context has a high-risk invariant: Generic Host is companion/test/bridge only and never in-process; `PlugIn.OnLoad` constructs `RasmUiScheduler`, calls `AppHost.Boot`, receives `BootReceipt`, and hands the runtime record back to AppUi; see `libs/csharp/Rasm.AppHost/README.md:11-12`, `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:25-36`, and `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:48-58`.
- The architecture names integrated runtime fields and capability handles: `RasmRuntime`, `StoreDispatch`, `RasmUiScheduler`, `ObservabilitySlot`, `ILogger`, `ChannelWriter<ComputeRequest>`, and typed receipts; see `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:48-54` and `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:62-78`.
- Package context is external-lib-first but scoped by process mode: in-process uses LanguageExt, Channels/Dataflow where appropriate, DiagnosticSource, TimeProvider, OpenTelemetry API, logging abstractions, NodaTime, FluentValidation, and HTTP resilience; Generic Host, DI, Serilog, OTel SDK/exporters, Scrutor, and related packages are companion-only; see `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:155-200`.
- Leaf overlay correctly defines AppHost as unified runtime platform and excludes UI rendering, durable storage internals, compute substrate execution, domain logic, and host SDK behavior; see `libs/csharp/Rasm.AppHost/AGENTS.md:12-16`.

AppUi:

- `libs/csharp/Rasm.AppUi` has `AGENTS.md`, README, `_ARCHITECTURE.md`, ROADMAP, `Rasm.AppUi.csproj`, and `packages.lock.json`.
- `Workspace.slnx` includes `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj`; see `Workspace.slnx:9-11`.
- `Directory.Build.props` classifies `Rasm.AppUi` with `RasmAppUiRoot`/`IsAppUiProject` and makes it Rhino/GH/UI aware; see `Directory.Build.props:1-9` and `Directory.Build.props:41-48`.
- `Rasm.AppUi.csproj` references `Rasm`, `Rasm.Grasshopper`, and `Rasm.Rhino`, then references the active retained UI, live data/visuals, and product controls package matrix versionlessly; see `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj:6-10`, `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj:12-22`, `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj:24-33`, and `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj:35-40`.
- `Directory.Packages.props` carries central AppUi package pins and the SkiaSharp native hazard comment; see `Directory.Packages.props:25-54`.
- README states the integrated package surface directly: Avalonia, ReactiveUI, ReactiveUI.Avalonia, ReactiveUI.Validation, DynamicData, SkiaSharp, LiveCharts2, and adjacent packages integrate as one product UI rail; see `libs/csharp/Rasm.AppUi/README.md:1-9`.
- Architecture defines one public app-surface rail for shell, screen, command, live view, visuals, chart/dashboard, and diagnostic receipt; toolkit types stay internal; see `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:24-37`.
- Host delegation is explicit: AppUi lowers product intent to Rhino/GH rails and does not own dispatch, repaint, undo, lifecycle, or host teardown; see `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:38-63`.
- Type shapes internalize external packages into product concepts: `RasmUiScheduler`, `Screen<T>` over ReactiveUI.Validation, DynamicData-backed `LiveView<T>`, LiveCharts `ChartVm`, AppUi-owned `DiagnosticReceipt`, and AppState consumption from Persistence; see `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:113-218`.
- Composition is strong: `PlugIn.OnLoad` constructs `RasmUiScheduler`, calls AppHost boot, receives `BootReceipt`, and typed inbound contracts from Persistence/AppHost/Compute are ready even before siblings land; see `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:220-233`.
- Leaf overlay summarizes these boundaries, but it is thinner than the README/architecture on native gates and package internalization; see `libs/csharp/Rasm.AppUi/AGENTS.md:12-24` and `libs/csharp/Rasm.AppUi/AGENTS.md:25-43`.

Compute:

- `libs/csharp/Rasm.Compute` currently contains `AGENTS.md`, README, ROADMAP, and `_ARCHITECTURE.md`; no `.csproj` is present.
- README states Compute wraps existing `Rasm.Vectors` tensor/numeric substrate in `Eff<RT,ExecutionReceipt>` and adds timing, allocation, cancellation, substrate selection, and progress without duplicating Vectors kernels; see `libs/csharp/Rasm.Compute/README.md:1-9`.
- Status says `.csproj` and package references are Phase 0 work; see `libs/csharp/Rasm.Compute/README.md:11-22` and `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:16-23`.
- Architecture defines public concepts: compute intent, substrate selection, execution receipt, model receipt, remote receipt, benchmark receipt, and cold progress stream; see `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:24-37`.
- It defines substrate escalation as an ordered predicate and requires `SubstrateKind` on `ExecutionReceipt`; see `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:44-56`.
- External-library behavior is meant to be internalized: Vectors/TensorPrimitives are called directly; ONNX Runtime/CoreML EP is model lane; gRPC is remote lane; CommunityToolkit.HighPerformance and UnitsNet are implementation surfaces; see `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:57-60`, `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:133-161`, and `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:178-190`.
- A shared-contracts project is required for `ComputeRequest`, `.proto`, and `Grpc.Tools`, but the leaf overlay does not currently route or stop on that owner; see `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:117-120`, `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:212-222`, and `libs/csharp/Rasm.Compute/AGENTS.md:5-12`.
- Leaf overlay correctly excludes scheduler/retry/channel/UI/persistence ownership and rejects `Task.Run`, PLINQ, generic receipt bases, and provider claims without proof; see `libs/csharp/Rasm.Compute/AGENTS.md:13-25` and `libs/csharp/Rasm.Compute/AGENTS.md:26-43`.

Persistence:

- `libs/csharp/Rasm.Persistence` currently contains `AGENTS.md`, README, ROADMAP, and `_ARCHITECTURE.md`; no `.csproj` is present.
- README states Persistence owns SQLite-backed app state, `StoreLifecycleOp`/`StoreQuery<T>` algebra, migrations, snapshots, live-state projection, and support artifacts; see `libs/csharp/Rasm.Persistence/README.md:1-9`.
- Status says `.csproj` and packages are Phase 0 work; see `libs/csharp/Rasm.Persistence/README.md:11-22`.
- Architecture identifies provider split and package roles: EF Core SQLite/Microsoft.Data.Sqlite as default local store, System.Text.Json source generation default, MessagePack compact snapshots, SQLitePCLRaw native init, NodaTime, FluentValidation, DynamicData internal projection, and support/redaction packages; see `libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:25-47`.
- Type shapes are deeply internalized: `StoreLifecycleOp`, `StoreQuery<TResult>`, `StoreReceipt`, `AppState`, `StoreProfile`, `SnapshotEnvelope`, and EF-to-DynamicData bridge; see `libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:49-93`.
- Public rail contract makes EF/SQLite internal: `DbContext` lives in an `Eff<RT,T>` `Bracket`, AppHost submits typed store operations, AppUi observes `IObservable<AppState>`, and DynamicData internals never cross the boundary; see `libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:94-107`.
- Leaf overlay is the strongest of the five on local stop behavior: if provider/native/encryption/corrupt-store/live-state/snapshot behavior cannot be proven statically, route the claim to architecture proof or runtime verification; see `libs/csharp/Rasm.Persistence/AGENTS.md:47-49`.

Materials:

- `libs/csharp/Rasm.Materials` has `AGENTS.md`, `Rasm.Materials.csproj`, `packages.lock.json`, and active `Bricks/Brick.cs`, `Bricks/Layout.cs`, `Bricks/ROADMAP.md`.
- `Workspace.slnx` includes `libs/csharp/Rasm.Materials/Rasm.Materials.csproj`; see `Workspace.slnx:18-20`.
- `Rasm.Materials.csproj` is deliberately host-free and package-light; it declares only a description and a `System.Collections.Frozen` using; see `libs/csharp/Rasm.Materials/Rasm.Materials.csproj:1-9`.
- Leaf overlay states Materials is a host-free material catalogue plus scalar material-owned layout data, not geometry, Rhino, persistence, or generated-data dump; see `libs/csharp/Rasm.Materials/AGENTS.md:1-10`.
- `Brick.cs` implements closed vocabularies and unions with Thinktecture attributes and typed source routes: `CoringClass`, `BondKind`, `BrickType`, `Coring`, `JointProfile`, `BrickRegion`, `BondName`, `BrickDesignation`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `BrickSource`, and `DimensionBasis`; see `libs/csharp/Rasm.Materials/Bricks/Brick.cs:7-16`, `libs/csharp/Rasm.Materials/Bricks/Brick.cs:46-78`, `libs/csharp/Rasm.Materials/Bricks/Brick.cs:111-122`, `libs/csharp/Rasm.Materials/Bricks/Brick.cs:124-232`, `libs/csharp/Rasm.Materials/Bricks/Brick.cs:234-416`, and `libs/csharp/Rasm.Materials/Bricks/Brick.cs:418-520`.
- `Layout.cs` has one public scalar layout entrypoint, `BrickAssembly.Layout(BrickRun)`, returning `Fin<BrickAssembly>` and emitting scalar `BrickPlacement`; see `libs/csharp/Rasm.Materials/Bricks/Layout.cs:5-21`, `libs/csharp/Rasm.Materials/Bricks/Layout.cs:23-39`, and `libs/csharp/Rasm.Materials/Bricks/Layout.cs:150-160`.
- `Bricks/ROADMAP.md` confirms `Brick.cs` is the single catalogue source, `Layout.cs` is the single scalar layout source, generated bonds fail until a typed generator algebra exists, and host/native downstream work is deferred; see `libs/csharp/Rasm.Materials/Bricks/ROADMAP.md:3-18`, `libs/csharp/Rasm.Materials/Bricks/ROADMAP.md:33-77`, and `libs/csharp/Rasm.Materials/Bricks/ROADMAP.md:78-95`.

## [FINDINGS]

1. Strong baseline: the parent `libs/csharp/AGENTS.md` and all five leaf overlays already follow the main `AGENTS.md` shape. They are scoped, compact, route away to README/architecture/roadmap for state, avoid command catalogs, and mostly avoid duplicating root rules. This satisfies the core `agents-md.md` expectation that a leaf overlay carries only local behavior deltas; compare `docs/standards/agents-md.md:1-7` with `libs/csharp/AGENTS.md:3-12` and the leaf scope/read-order blocks.

2. The platform overlays make future-forward posture clear, but the clearest future-forward language often lives in README/architecture/roadmap rather than in the first edge of the leaf overlay. `agents-md.md` asks the lead to carry scope, parent relation, highest-risk invariant, and route-away rule; see `docs/standards/agents-md.md:18-22` and `docs/standards/agents-md.md:45-50`. AppHost's highest-risk invariant is "Generic Host never in-process," but the leaf lead only says state/sequence live in README/architecture/roadmap and does not state that invariant until weaker rejection language later; compare `libs/csharp/Rasm.AppHost/AGENTS.md:1-4` and `libs/csharp/Rasm.AppHost/AGENTS.md:38-45` with `libs/csharp/Rasm.AppHost/README.md:11-12` and `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:25-36`.

3. External-lib-first is established at root and in architecture files, but the leaf overlays do not consistently translate it into local internalization grammar. Root says approved dependencies are primary implementation surfaces and forbids thin wrappers; see `CLAUDE.md:45-51`. The architectures name package roles in detail, but several overlays stop at "read architecture/package facts" rather than saying how to internalize each package family into the local rail. AppUi does this best with "toolkit types stay internal" and "no package-forwarding facade"; see `libs/csharp/Rasm.AppUi/AGENTS.md:14-23` and `libs/csharp/Rasm.AppUi/AGENTS.md:36-43`. Compute and AppHost would benefit from equally explicit package-internalization rules.

4. The "fspec-style internalized behavior, not knobs/params/facades" rule is present as an architectural pattern but not codified uniformly in overlays. The strongest examples are Persistence and Materials: Persistence makes EF/SQLite an internal `StoreLifecycleOp`/`StoreQuery<T>`/`StoreReceipt` algebra rather than repositories or provider knobs; see `libs/csharp/Rasm.Persistence/AGENTS.md:13-25` and `libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:94-107`. Materials makes brick facts and layout a typed in-memory catalogue/layout rail rather than data dump, source schema, or geometry facade; see `libs/csharp/Rasm.Materials/AGENTS.md:5-24`. AppHost, AppUi, and Compute should name this same internalization contract explicitly for runtime, UI, model/remote, and observable/progress behavior.

5. AppUi's overlay is accurate but misses local stop behavior for the native and host-embedding hazards that the architecture treats as host-fatal. Architecture requires `DisableAvaloniaAppDelegate`, `CreateEmbeddableTopLevel`, software-before-Metal validation, NSView reparenting, focus/disposal ordering, and a SkiaSharp native major gate; see `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:51-63`, `libs/csharp/Rasm.AppUi/_ARCHITECTURE.md:107-111`, and `libs/csharp/Rasm.AppUi/ROADMAP.md:73-107`. `agents-md.md` says stop rules are required when missing proof, unavailable host state, or unsafe runtime conditions make continuation harmful; see `docs/standards/agents-md.md:52-58`. AppUi has no `STOP_RULES` section.

6. Compute's overlay omits the shared-contracts route even though the architecture makes it load-bearing. `ComputeRequest` and `.proto` are owned by a shared-contracts project that must exist in `Workspace.slnx`; Compute consumes but does not define it; see `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:117-120` and `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:212-222`. The overlay read-order points to Vectors/AppHost/AppUi/Persistence, but not the shared-contracts owner; see `libs/csharp/Rasm.Compute/AGENTS.md:5-12`. That omission invites the wrong edit: defining `ComputeRequest` inside Compute or pushing gRPC codegen into the measured-execution folder.

7. AppHost's overlay correctly identifies the cross-folder boundary table but should more directly say that AppHost internalizes platform package behavior into runtime records, lifecycle rails, schedules, and typed receipts. The architecture already says `RasmRuntime` is the shared spine and that Generic Host/DI/SDK exporters are companion-only; see `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:48-58` and `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:155-200`. The overlay extension grammar says "telemetry/logging" and "configuration" but not the larger internalization rule for Generic Host, Channels, LanguageExt Schedule, TimeProvider/NodaTime, OTel API, logging abstractions, and HTTP resilience; see `libs/csharp/Rasm.AppHost/AGENTS.md:18-25`.

8. Persistence overlay is closest to complete for integrated behavior. It has owner contract, operation algebra, provider-specific proof routing, live-state emission, support artifact route, reference-boundary table, and stop rules; see `libs/csharp/Rasm.Persistence/AGENTS.md:13-49`. The one missing precision is a direct rejection of public provider knobs/facades: no public `DbContextOptions`, `SqliteConnection`, EF entity/queryable, serializer, codec option bag, or provider selector should cross the rail. Architecture implies this, but leaf grammar should make it action-changing.

9. Materials overlay is correct for a host-free catalogue but weaker on "external-lib-first" because the folder's first-class dependencies are mostly source standards and Thinktecture/LanguageExt inherited from the workspace. That is acceptable: this project should not force external packages for a catalogue that already uses typed closed vocabularies and pure scalar layout. The overlay should keep saying source facts go into code documentation/catalogue records, not overlay prose; see `libs/csharp/Rasm.Materials/AGENTS.md:18-24`.

10. The folder set has an important current-state split that overlays handle unevenly: AppUi and Materials are real projects in `Workspace.slnx`; AppHost, Compute, and Persistence are candidate folders awaiting project creation. AppUi and Materials project facts are verified at `Workspace.slnx:9-20`, `libs/csharp/Rasm.AppUi/Rasm.AppUi.csproj:1-42`, and `libs/csharp/Rasm.Materials/Rasm.Materials.csproj:1-9`. AppHost/Compute/Persistence docs state Phase 0 creates their project files; see `libs/csharp/Rasm.AppHost/_ARCHITECTURE.md:17-23`, `libs/csharp/Rasm.Compute/_ARCHITECTURE.md:16-23`, and `libs/csharp/Rasm.Persistence/_ARCHITECTURE.md:17-23`. Leaf overlays should avoid sounding like these three are already graph members while still enforcing future target behavior.

## [RECOMMENDED_OVERLAY_CHANGES]

Parent `libs/csharp/AGENTS.md`:

- Add one family-level platform sentence after `libs/csharp/AGENTS.md:17`: "Platform projects internalize approved package behavior into their owning rail; expose Rasm intent, typed operations, receipts, projections, and capability records, never package facades, option bags, or knobs." This is not root duplication because it translates root external-lib-first policy into C# library behavior.
- Add one extension grammar bullet near `libs/csharp/AGENTS.md:21-25`: "Approved package adoption: bind the package to the local operation algebra or runtime record first; route exact version/package proof to architecture/manifests." This would codify the cross-platform pattern without repeating package lists.

AppHost overlay:

- Strengthen the lead at `libs/csharp/Rasm.AppHost/AGENTS.md:3` with the highest-risk invariant: AppHost is runtime-record in-process; Generic Host/DI/Scrutor/Serilog/OTel SDK/exporters are companion/test/bridge roots only.
- Add an internalization rule under extension grammar: "Platform package behavior: internalize Channels, LanguageExt Schedule, TimeProvider/NodaTime, OTel API, logging abstractions, and HTTP resilience into `RasmRuntime`, lifecycle rails, and typed receipts before adding config knobs, service registries, or facade methods."
- Add a stop rule: if in-process Generic Host, `IServiceProvider`, OTel SDK/exporter, Serilog sink, raw host SDK call, or unproved shutdown/drain behavior is required to proceed, stop and route to `_ARCHITECTURE.md`/host proof rather than continuing.

AppUi overlay:

- Keep current owner contract, but add a "PACKAGE_INTERNALIZATION" or equivalent sentence after `libs/csharp/Rasm.AppUi/AGENTS.md:16`: Avalonia, ReactiveUI, DynamicData, LiveCharts, Skia, DialogHost, icons, and behaviors are internal implementation surfaces for product rails; public API exposes product shell/screen/command/live/chart/visual/diagnostic concepts only.
- Add stop rules for native/host hazards: missing SkiaSharp native-major proof, GH2 panel-host API absence, `DisableAvaloniaAppDelegate` uncertainty, `CreateEmbeddableTopLevel` uncertainty, focus/disposal ordering uncertainty, or software-rendering embedding proof gaps route to `_ARCHITECTURE.md`/runtime proof before expansion.
- Add a rejection paired with replacement: "No public toolkit settings bag; encode behavior as typed product intent, `Screen<T>`, `CommandReceipt`, `DiagnosticReceipt`, or `RasmUiScheduler`."

Compute overlay:

- Add read-order route for shared contracts before changing `ComputeRequest`, `.proto`, gRPC codegen, or remote-lane payloads. If no owner exists, stop and mark the shared-contracts owner as a proof gap rather than defining `ComputeRequest` in Compute.
- Add package-internalization grammar: ONNX Runtime/CoreML EP, gRPC, System.Reactive, CommunityToolkit.HighPerformance, UnitsNet, and TensorPrimitives become substrate/model/remote/progress/allocation/tolerance behavior inside `ExecutionReceipt` and `ComputeIntent`; never expose package API shapes or provider knobs as the public platform API.
- Add stop rules for native model load (`libonnxruntime.dylib` resolver), CoreML fp16 equivalence, remote retry ownership, missing shared-contracts project, or benchmark evidence gaps.

Persistence overlay:

- Add one explicit rejection: no public `DbContext`, `DbContextOptions`, `IQueryable`, `SqliteConnection`, serializer option bag, codec selector, provider selector, or EF entity type crosses the Persistence boundary; use `StoreLifecycleOp`, `StoreQuery<TResult>`, `StoreDispatch`, `AppState`, `SnapshotEnvelope`, and `StoreReceipt`.
- Add a package-internalization sentence: EF Core SQLite, Microsoft.Data.Sqlite, SQLitePCLRaw, NodaTime, DynamicData/System.Reactive, MessagePack, LZ4, and redaction packages are interpreted inside lifecycle/query/snapshot/support rails, not exposed as knobs.
- Keep the existing stop rules; they are the model for other platform overlays.

Materials overlay:

- Keep this overlay host-free and source-standard-first. Do not force package integration language that would weaken the catalogue boundary.
- Add one optional local vocabulary sentence: source standards are first-class evidence surfaces, but not instruction authority; new source facts enter typed catalogue records/code documentation and preserve source routes.
- Add one stop rule only if source-standard proof is missing for a new regional policy, catalogue value, or layout rule: stop and route to source proof/code documentation rather than adding heuristic values.

## [UNIVERSAL_PATTERNS_TO_CODIFY]

- Platform internalization pattern: "External library -> local rail -> typed Rasm surface." Package APIs are implementation detail; public surfaces are product/runtime/store/compute/material intent, typed operations, receipts, projections, or capability records.
- Fspec-style behavior pattern: integrated behavior is encoded as operation algebra, smart enum, union, fold, runtime record, receipt, projection, or source-owned table. Avoid knobs, params, facades, provider selectors, option bags, service locators, and package-forwarding wrappers.
- Candidate-folder pattern: a planned platform folder may enforce future target behavior, but present-state claims must route to README/architecture/roadmap and manifests. AppHost, Compute, and Persistence are not graph members until `.csproj`/solution adoption lands; AppUi and Materials are graph members now.
- Proof-stop pattern: runtime/native/provider claims need local stop behavior when static reading cannot prove them. AppUi native embedding, AppHost shutdown/drain, Compute ONNX/CoreML/gRPC, and Persistence native SQLite/encryption/corruption are the recurring hazards.
- Shared-spine pattern: AppHost owns runtime scheduling/correlation, AppUi owns UI scheduler/observation, Compute owns execution/progress/measurement, Persistence owns storage/query/live-state projection, Materials owns host-free facts/scalar layout. Overlays should route changes to the spine owner before allowing sibling rails.
- Receipt pattern: typed algorithm/proof receipts stay owner-local when fields carry route/status/native/model/benchmark/store evidence. Operational mutation receipt families can collapse into one fact stream only when repeated slot families share construction/count/status semantics. No generic `IReceipt` ledgers.
- Observable handoff pattern: producer owns cold typed observable and receipt failures; AppUi owns UI scheduling via `RasmUiScheduler`; no producer calls `ObserveOn`, no observable uses `OnError` for platform faults.
- Package proof route pattern: exact versions stay in `Directory.Packages.props`; active consumer references stay in `.csproj`; package/native/API proof lives in architecture/manifests/tool output, not root or leaf overlay prose.

## [CONFIDENCE]

High confidence on current repo/folder facts: I read the active root instructions, docs standards route, C# library parent overlay, all five target overlays, target README/architecture/roadmap files, current project files, solution membership, central package manifest, and current Materials source files.

Medium-high confidence on overlay recommendations: they are grounded in the active `agents-md.md` standard and direct folder context. The main judgment call is how much package-internalization language belongs in a leaf overlay versus architecture; the recommended changes keep package lists out of overlays and state only action-changing local grammar.

Medium confidence on external package freshness: I verified current repository manifests and architecture claims, not live package documentation. That is appropriate for this task because it asked for folder context and overlay behavior, not package-version research.
