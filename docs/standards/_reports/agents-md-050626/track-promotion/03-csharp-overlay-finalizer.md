# [CSHARP_OVERLAY_FINALIZER]

Files read: `CLAUDE.md`, `docs/standards/README.md`, `docs/standards/agents-md.md`, `coding-csharp` skill, `track-source-scans/02-csharp-large-context.md`, `track-source-scans/03-csharp-platform-context.md`, `track-source-scans/06-poly-csharp.md`, current `libs/csharp/AGENTS.md`, and every current `libs/csharp/Rasm*/AGENTS.md`; `track-owner-routing/02-csharp-overlay-refiner.md` was not present.

## [1][TOPOLOGY_DECISION]

Keep the current topology: one parent `libs/csharp/AGENTS.md` plus one project-root overlay per `Rasm*` project. Do not add nested `AGENTS.md` files for `Rasm/Vectors`, `Rasm/Analysis`, `Rasm.Rhino/UI`, `Rasm.Rhino/Commands`, `Rasm.Rhino/Blocks`, or `Rasm.Grasshopper/UI` yet.

The right finalizer move is stronger project-root trigger grammar, because the weak point is not missing load locations; it is imprecise owner naming. Nested overlays would duplicate the same root/library/C# posture unless a subfolder has a distinct instruction authority, proof hazard, or runtime boundary that cannot fit in the project overlay.

Use this future rule for nested overlays:

- Add `libs/csharp/Rasm.Rhino/Construction/AGENTS.md` only after production `Construction/*.cs` lands and the folder owns `RhinoConstruction.Project<TOut>` / `ConstructionOp` beyond the roadmap route.
- Add `libs/csharp/Rasm.Grasshopper/UI/AGENTS.md` only if UI splits into independently maintained sub-surfaces with conflicting stop rules; until then, keep `WireOp`, `DocumentMutation`, `GrasshopperUiIntent<T>`, and motion rules in the project overlay.
- Add material-family nested overlays only when a material folder introduces a distinct generator, source-proof, or non-catalogue boundary; ordinary material catalogues stay governed by `Rasm.Materials/AGENTS.md`.
- Never add nested overlays merely because a folder is large. Size triggers exact owner rails in the project overlay, not another instruction file.

## [2][`libs/csharp/AGENTS.md`]

Edit spec:

- After the library contract, add a family-level internalization rule:
  `Approved packages and host APIs are internal implementation surfaces for the owning library rail. Public library surfaces expose Rasm intent, operations, receipts, projections, policies, capability records, or typed runtime records; they do not expose package facades, option bags, provider selectors, service locators, or native knob mirrors.`
- Add a large-folder trigger rule:
  `When a subfolder has multiple durable operation families, the nearest project overlay must name the exact rail to extend before new public types, files, folders, or entrypoints are added. If the rail cannot be named, stop and read source or architecture before editing.`
- Tighten `New project capability` wording to require owner proof before sibling rails:
  `New project capability: extend the owning project overlay, architecture/source owner, and existing operation/intent/receipt/policy rail before adding a sibling rail.`
- Keep project routing table. Do not add subfolder routing there; that belongs in project overlays.

Weak formulations to reject:

- `read existing sibling owners first`
- `collapse into one typed rail`
- `find the extension rail`
- `package adoption belongs in manifests`

Replace them with owner-action wording that says what rail receives the change and what shape is forbidden.

## [3][`libs/csharp/Rasm/AGENTS.md`]

Edit spec:

- Replace the broad `Analysis behavior` bullet with:
  `Analysis behavior: extend `IAspect`, `Operation<TGeometry,TOut>`, and `Analyze.Run` in `Analysis/Analyze.cs`; import `Domain` validation, statistics, coercion, and kind logic instead of creating analysis-local copies.`
- Replace the broad Vectors read rule with:
  `When changing Vectors projection, sampling, extraction, receipt, or algorithm entrypoints, read `Vectors/_ARCHITECTURE.md` and `Vectors/Intent.cs`; preserve `VectorIntent.Project<TOut>(Context, Op?)` as the consumer projection rail and `ExtractionDomain` + `SampleKind` as the sampling/extraction rail unless the architecture has already moved the owner.`
- Replace `Vector behavior` with a named rail list:
  `Vector behavior: extend `VectorIntent`, `FieldNabla`, `SampleKind`, `CloudKernel.MassOf`, `LaplacianCache`, `SpectralFilter`, `AtomProjection`, or the architecture-named owner before adding public projection, sampling, solver, mesh, spectral, or receipt surfaces.`
- Add a product-boundary rejection:
  `No Vectors UI, preview, command, GH2 parameter, bake, product workflow, or app receipt surface; host and product packages consume vector receipts and projections through their own rails.`
- Add future-only kernel wording:
  `Future concern categories are kernel rails first: one concern category, one consumer surface, typed intent/state, and algorithms over `Domain`; current missing consumers do not justify host-shaped APIs or narrow wrappers.`

Weak formulations to reject:

- `extend the analysis owner`
- `extend vector intent, support-space projection, fields...`
- `future category`
- `powerful operations with minimal ceremony`

Those are true but not self-executing.

## [4][`libs/csharp/Rasm.Rhino/AGENTS.md`]

Edit spec:

- Replace `case-row rail` with exact command/document rails:
  `Command input, option, prompt, selection, point event, gumball, text mode, or transform behavior extends `CommandInputPolicy`, `CommandInputRequest<T>`, `CommandPointEventPhase`, `CommandOption`, and the staged input rail in `Commands/Input.cs`; document mutation extends `DocumentOp`, `DocumentTransaction`, `DocumentEdit.Commit`, and `DocumentReceiptSlot` in `Commands/Document.cs`.`
- Add a construction read trigger:
  `When adding geometry creation, annotation, framed-bounds, transform/frame, block-ready, document-ready, or preview-ready output, read `Construction/ROADMAP.md` and route through the intended `RhinoConstruction.Project<TOut>` / `ConstructionOp` shape until production construction code supersedes the roadmap.`
- Add a Blocks owner rule:
  `Block definition, instance, linked archive, attribute, graph, preview, or cache behavior extends `BlockOp`, `BlockInstanceTask`, `LinkLifecycle`, `BlockAttributeTask`, `MutationReceipt`, and the existing snapshot/cache owners before adding another facade.`
- Add a UI owner rule:
  `Overlay, panel, dialog, retained resource, callback, motion, gumball, or viewport interaction behavior extends `UiIntent<T>`, `UiViewportPreview`, `UiViewportInteraction<TState>`, `MotionSpec`/motion owners, or the panel owner before adding a parallel executor.`
- Add native-obsolete boundary wording:
  `Obsolete native values may be projected only when live Rhino documents can still emit them; keep them inside the owning boundary conversion and expose canonical Rasm names, not compatibility aliases or stale public vocabulary.`
- Keep current API-proof stop rule, but make it reference the owner action:
  `If RhinoWIP XML, decompile evidence, or the API rail cannot prove semantics, stop before naming a public rail or replacement value; route runtime behavior to bridge scenarios.`

Weak formulations to reject:

- `extend the case-row rail`
- `apply redraw, commit, disposal, and UI-thread protection at the boundary edge`
- `UI owns Rhino/Eto integration`
- `no stale public names`

Replace with the rail names above.

## [5][`libs/csharp/Rasm.Grasshopper/AGENTS.md`]

Edit spec:

- Replace `typed UI rail` wording with:
  `UI operations implement or extend the existing `IUiOp<TResult>` case surface and flow through `GrasshopperUiIntent<T>`, `GhUi`, and `GrasshopperUi.Use`; do not add a second public executor, direct thread-marshalling surface, or caller-side GH2 operation switch.`
- Replace mutation wording with:
  `Document mutations extend `DocumentMutation` and run through `UiRail.RunDocumentMutation` so undo, repaint, `ActionList`, `DocumentMutationReceipt`, `DocumentMutationDelta`, and snapshots remain one rail.`
- Add wire capsule rule:
  `Wire behavior extends `WireOp`, `WireEdit`, `WireQuery`, `WireResult`, or `WireRepositoryRail`; no second reflective GH2 wire repository reader and no direct host-internal wire access outside that capsule.`
- Tighten component rule:
  `Component capability extends `ComponentSpec`, `SpecBuilder`, `OutputBinding`, `PortKind`, `Capability`, and ownership-transfer rails before adding one-off parameter, conversion, or diagnostic code.`
- Add motion rule:
  `Canvas motion, haptics, display-link pacing, spring configuration, cosmetic animation, and accessibility-driven motion reduction extend `UI/Motion.cs` owners; do not add per-feature timers or animation state.`
- Add future-only internalization wording:
  `GH2 APIs are internalized into component and UI operation rails. Public app code passes typed component specs, ports, outputs, UI intents, mutations, wire operations, and receipts; it never choreographs GH2 lifecycle or exposes raw native knobs.`

Weak formulations to reject:

- `read UI/ to find the typed UI rail`
- `route through the mutation rail`
- `new app behavior: keep App code a thin caller`
- `no duplicate host access paths`

Use rail names and forbidden replacement shapes.

## [6][`libs/csharp/Rasm.AppHost/AGENTS.md`]

Edit spec:

- Strengthen the lead:
  `Rasm.AppHost is the in-process runtime-record platform. Generic Host, DI containers, Scrutor, Serilog sinks, OpenTelemetry SDK/exporters, and companion service roots are companion/test/bridge concerns only unless architecture and manifest proof explicitly move them into a composition root.`
- Add package internalization:
  `Channels, LanguageExt schedules, TimeProvider/NodaTime clocks, logging abstractions, OpenTelemetry API signals, HTTP resilience, and configuration binding are internalized into `RasmRuntime`, lifecycle rails, scheduler/drain policy, and typed receipts before any config knob, service registry, or facade method is added.`
- Add stop rule:
  `If a change requires in-process `IServiceProvider`, Generic Host boot, exporter SDKs, raw host SDK calls, or unproved shutdown/drain behavior, stop and route to `_ARCHITECTURE.md` plus host proof before expanding the runtime rail.`

Weak formulations to reject:

- `unified runtime platform`
- `extend the runtime record and lifecycle rail`
- `no companion-service packages inside the in-process plugin path unless...`

Those need the runtime-record and companion-only boundary named up front.

## [7][`libs/csharp/Rasm.AppUi/AGENTS.md`]

Edit spec:

- Add package internalization after owner contract:
  `Avalonia, ReactiveUI, DynamicData, LiveCharts, Skia, DialogHost, icons, and behavior packages are internal implementation surfaces for product rails. Public API exposes product shell, screen, command, live-view, chart, visual, diagnostic, scheduler, and receipt concepts only.`
- Add native stop rules:
  `Stop before expanding UI surface when SkiaSharp native-major proof, GH2/Rhino panel-host API proof, `DisableAvaloniaAppDelegate`, `CreateEmbeddableTopLevel`, focus/disposal ordering, or software-rendering embedding proof is missing; route the gap to `_ARCHITECTURE.md` or runtime proof.`
- Add rejection:
  `No public toolkit settings bag, package option mirror, or floating-window fallback rail; encode behavior as typed product intent, `Screen<T>`, `CommandReceipt`, `DiagnosticReceipt`, or `RasmUiScheduler`.`

Weak formulations to reject:

- `package types stay internal unless exposing them carries real product semantics`
- `host embedding behavior: update local architecture proof`
- `no mixed UI paradigms`

Keep the package names out of version prose, but name the product rails they must feed.

## [8][`libs/csharp/Rasm.Compute/AGENTS.md`]

Edit spec:

- Add shared-contracts read/stop rule:
  `Before changing `ComputeRequest`, `.proto`, gRPC codegen, remote payloads, or companion service contracts, read the shared-contracts owner where present. If no owner exists, stop and record the owner as a proof gap; do not define `ComputeRequest` inside Compute as a convenience.`
- Add package internalization:
  `ONNX Runtime/CoreML EP, gRPC, System.Reactive, CommunityToolkit.HighPerformance, UnitsNet, TensorPrimitives, and Vectors kernels become substrate, model, remote, progress, allocation, tolerance, and execution-receipt behavior inside `ComputeIntent` and `ExecutionReceipt`; package/provider shapes do not become public platform API.`
- Add stop rule:
  `Stop on native model-loader proof gaps, CoreML fp16 equivalence gaps, remote retry ownership gaps, missing shared-contracts project, or benchmark evidence gaps before broadening the execution rail.`

Weak formulations to reject:

- `new substrate or model lane`
- `remote service exists through companion contracts`
- `no provider API claims without proof`

Name the remote/shared-contract owner issue directly.

## [9][`libs/csharp/Rasm.Persistence/AGENTS.md`]

Edit spec:

- Add provider-boundary rejection:
  `No public `DbContext`, `DbContextOptions`, `IQueryable`, `SqliteConnection`, EF entity type, serializer option bag, codec selector, provider selector, or repository abstraction crosses the Persistence boundary; use `StoreLifecycleOp`, `StoreQuery<TResult>`, `StoreDispatch`, `AppState`, `SnapshotEnvelope`, and `StoreReceipt`.`
- Add package internalization:
  `EF Core SQLite, Microsoft.Data.Sqlite, SQLitePCLRaw, NodaTime, DynamicData/System.Reactive, MessagePack, LZ4, and redaction packages are interpreted inside lifecycle, query, snapshot, live-state, support-artifact, and receipt rails, not exposed as knobs.`
- Keep the existing stop rules; use them as the model for AppHost/AppUi/Compute.

Weak formulations to reject:

- `provider-specific behavior: put proof in architecture`
- `no generic repository wrapper`
- `No EF, SQLite, serializer...`

The final overlay should name the forbidden public shapes and replacement rails.

## [10][`libs/csharp/Rasm.Materials/AGENTS.md`]

Edit spec:

- Keep it host-free and source-standard-first. Do not add generic platform package-internalization language here.
- Add source authority wording:
  `Source standards are evidence surfaces, not instruction authority. New source facts enter typed catalogue records, closed vocabularies, code documentation, and citation routes; they do not become overlay prose or generated-data schemas.`
- Add generator/future-only wording:
  `Generated bonds, regional catalogues, or material datasets may land only through a typed generator/catalogue algebra that emits the same closed vocabulary and query rails; no JSON/SQL/schema dump is a public source of truth.`
- Add narrow stop rule:
  `If source-standard proof is missing for a regional policy, catalogue value, or layout rule, stop and route to source proof/code documentation instead of adding heuristic values.`

Weak formulations to reject:

- `read the source standards of record`
- `preserve typed citation routes`
- `No fabricated regional policy split`

Those should become proof-stop and typed-catalogue rules.

## [11][FINAL_WORDING_RULE]

Every final overlay addition should follow this shape:

`When <local change class>, extend <exact owner rail/symbol/file> before adding <forbidden duplicate shape>; route <proof/state/version> to <owner document or manifest>.`

Reject slogans even when they are directionally correct:

- `use advanced polymorphism`
- `avoid helpers`
- `keep package types internal`
- `extend the operation rail`
- `route through the mutation rail`
- `read source first`
- `no duplicate owners`

Accept only wording that names the rail an agent can edit.
