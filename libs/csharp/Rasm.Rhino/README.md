# [RASM_RHINO]

`Rasm.Rhino` is the single host boundary over RhinoCommon, Rhino UI, Eto, and the macOS native surface: every host concern folds through one owner behind thread-affinity and capability gates, every native resource retains only across its leased extent, and every outcome carries a typed receipt.

Host capability composes parameterized, so an app root never reaches RhinoCommon's raw surface; Rhino-native drafting, sheets, and file IO stay rich rather than thinned toward a host-neutral floor, and measurement declares here while executing at app-root altitude.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned sources behind fresh handle and capability evidence.
- [02]-[GEOMETRY](.planning/Document/geometry.md): `GeometryHandle` owns geometry-custody crossing — inspection, motion, bounds, clipping, release.
- [03]-[TABLES](.planning/Document/tables.md): `Tables.Commit` executes a shaped table transaction through the undo bracket and redraw compensation.
- [04]-[EVENTS](.planning/Document/events.md): `DocumentStream.Observe` binds events into detached facts under scoped attach and symmetric release.
- [05]-[LAYERS](.planning/Document/layers.md): `Layers.Commit` folds the managed layer domain and lowers the kernel drafting standards onto it.
- [06]-[FACTS](.planning/Document/facts.md): `FactStream` accumulates commit-scoped consequences under an undo stamp behind the readable kind gate.
- [07]-[COMMIT](.planning/Document/commit.md): `DocumentCommit.Sealed` frames every host mutation over `UndoBracket` custody and redraw compensation.
- [08]-[LIFETIME](.planning/Document/lifetime.md): `LifecycleGate` and `Subscription` own Rhino lifecycle admission and host-thread release.

[PERSISTENCE]:
- [09]-[DICTIONARY](.planning/Persistence/dictionary.md): `ArchiveMap` closes the typed-value dictionary as one union with a detach/mint round trip.
- [10]-[SETTINGS](.planning/Persistence/settings.md): `SettingStore.Commit` carries the settings tree through pure reads, typed writes, and guards.
- [11]-[APPSETTINGS](.planning/Persistence/appsettings.md): `AppSettings.Commit` drives every application preference family through typed state.
- [12]-[USERDATA](.planning/Persistence/userdata.md): `ArchiveIo` frames attached custody; `TypedUserData` seals the participation template.
- [13]-[USERTEXT](.planning/Persistence/usertext.md): `UserTexts.Commit` owns document and per-object user strings with prior-value receipts.
- [14]-[PRESETS](.planning/Persistence/presets.md): `Presets.Commit` runs cplane, position, and layer-state presets under one mask vocabulary.
- [15]-[SNAPSHOTS](.planning/Persistence/snapshots.md): `Snapshots.Commit` scripts snapshot state; `SnapshotParticipant` adapts the plugin seam.

[OBJECTS]:
- [16]-[STATE](.planning/Objects/state.md): `Objects.Ask` owns the live-object window — snapshot, frames, component touch, detached section custody.
- [17]-[ATTRIBUTES](.planning/Objects/attributes.md): `AttributeProgram` closes attribute mutation as the typed payload of the table rail's `Amend`.
- [18]-[MATERIALS](.planning/Objects/materials.md): `Materials.Ask` resolves materials, mappings, and mesh caches behind one shared-bracket commit.
- [19]-[LIGHTS](.planning/Objects/lights.md): `Lights.Commit` runs the closed light-kind family under the shared bracket.
- [20]-[HISTORY](.planning/Objects/history.md): `HistoryScript` and `ReplayProgram` own the record/replay triad, linkage topology, and governance.
- [21]-[AUTHORING](.planning/Objects/authoring.md): `ObjectProgram` and `GripProgram` quarantine host subclassing behind adapters.

[COMMANDS]:
- [22]-[COMMAND](.planning/Commands/command.md): `CommandFlow<TState>.Drive` owns the bounded immutable command algebra behind the host lifecycle.
- [23]-[ACQUISITION](.planning/Commands/acquisition.md): `Acquisition.Get` owns each getter modality, native getter lifetime, and typed receipt.
- [24]-[OPTIONS](.planning/Commands/options.md): `OptionSet.Bind` makes command-line options data, leasing native carriers under scoped release.
- [25]-[SELECTION](.planning/Commands/selection.md): `Picks` owns picked-reference capture, the `PartIndex` component owner, and kernel re-entry.

[BLOCKS]:
- [26]-[MODEL](.planning/Blocks/model.md): `Definitions.Lens` resolves the Document-owned `ResourceRef` into one whole-state projection.
- [27]-[GRAPH](.planning/Blocks/graph.md): `BlockGraph.Ask` folds live and archived definitions into one transient topology and its closure evidence.
- [28]-[LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes ingress, preview vault, deferred refresh, eviction, and native disposal.
- [29]-[OPERATIONS](.planning/Blocks/operations.md): `Blocks.Commit` runs read and transaction rails through plan grants and additive receipts.

[MODELING]:
- [30]-[SOLIDS](.planning/Modeling/solids.md): `Solids.Build` runs Brep booleans, fillets, offsets, pipes, and joins on the `ModelGate` spine.
- [31]-[LOFTING](.planning/Modeling/lofting.md): `Lofts.Build` folds rail sweeps, lofts, patches, and developable lofting into one policy rail.
- [32]-[SURFACES](.planning/Modeling/surfaces.md): `HostSurfaces.Build` owns network, revolve, grid, geodesic, and analytic construction.
- [33]-[CURVES](.planning/Modeling/curves.md): `HostCurves.Build` owns curve host ops and seats the Modeling `ModelClaim` admission fold.
- [34]-[MESHING](.planning/Modeling/meshing.md): `HostMeshes.Build` drives parameter-carried meshing and every host mesh edit.
- [35]-[SUBD](.planning/Modeling/subd.md): `SubDs.Build` owns SubD creation, crease authoring, value-semantic editing, and brep conversion.
- [36]-[DEFORM](.planning/Modeling/deform.md): `Deforms.Build` dispatches the space-morph family and the unroll, squish, and unwrap flatteners.
- [37]-[PROJECTION](.planning/Modeling/projection.md): `Projections.Build` classifies Make2D hidden-line drawings on the `ModelGate` spine.

[ANNOTATION]:
- [38]-[STYLE](.planning/Annotation/style.md): `StyleField` owns the drafting schema; its patch fold authors, amends, and override-mints styles.
- [39]-[TEXT](.planning/Annotation/text.md): `Texts.Commit` owns text/leader construction, run edits, `TextFields` formulas, and outlining.
- [40]-[DIMENSION](.planning/Annotation/dimension.md): `Dimensions.Commit` mints, adjusts, restyles the dimension family over one override algebra.
- [41]-[HATCH](.planning/Annotation/hatch.md): `Hatches.Commit` owns hatch construction, the pattern line-definition model, and `.pat` interchange.
- [42]-[LINETYPE](.planning/Annotation/linetype.md): `Linetypes.Commit` owns the segment/shape/taper stroke model and `.lin` interchange.
- [43]-[TYPEFACE](.planning/Annotation/typeface.md): `Typefaces.Resolve` answers face evidence; `Sections.Commit` composes section-cut presentation.

[VIEWPORT]:
- [44]-[CAMERA](.planning/Viewport/camera.md): `CameraPose` composes the kernel vector frame and intent owners behind the viewport lease.
- [45]-[OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds camera operations across scalar or broadcast under one UI-thread policy.
- [46]-[CAPTURE](.planning/Viewport/capture.md): `Captures` runs settings, transparent, and depth capture with frame-sequence custody.
- [47]-[MOTION](.planning/Viewport/motion.md): `MotionPump.Drive` samples kernel motion into a leased drive under accessibility and display facts.

[DISPLAY]:
- [48]-[CONDUIT](.planning/Display/conduit.md): `Conduits.Mount` owns filtered display-pipeline participation and balanced render state.
- [49]-[DRAW](.planning/Display/draw.md): `Marks.Paint` draws one `DisplayMark` vocabulary over the pipeline, overlay, Eto, and page canvases.
- [50]-[INTERACTION](.planning/Display/interaction.md): `WidgetHost` owns pointer, gumball, and widget lifecycles while emitting detached facts.
- [51]-[RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch and gated render custody; `RealtimeEngines.Register` seats the engine.
- [52]-[MODES](.planning/Display/modes.md): `Modes.Configure` owns mode appearance, policy, table operations, viewport assignment, and analysis.

[RENDER]:
- [53]-[CONTENT](.planning/Render/content.md): `ContentRef` addresses the RDK content graph behind the kind axis, change bracket, and snapshot.
- [54]-[KINDS](.planning/Render/kinds.md): `MaterialBridge` borrows baked material and PBR projections per window; mint verbs yield leased content.
- [55]-[FIELDS](.planning/Render/fields.md): `ContentValue` owns every typed content parameter through polymorphic write, recover, and box dispatch.
- [56]-[REGISTRY](.planning/Render/registry.md): `Registry` runs the content rail through factory vocabulary, receipts, and events.
- [57]-[SETTINGS](.planning/Render/settings.md): `Settings.Run` applies total render state across the document, archive, and free-floating duality.
- [58]-[MAPPING](.planning/Render/mapping.md): `MappingSpec` mints texture mappings; the `Mappings` rail binds and recovers them per object channel.

[EXCHANGE]:
- [59]-[FORMATS](.planning/Exchange/formats.md): `FileCodec` owns codec identity, detection, filter projection, dispatch, and dialog registration.
- [60]-[OPTIONS](.planning/Exchange/options.md): `FormatDial` closes every per-format option surface, refusing seat and axis demands at one door.
- [61]-[ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` runs archive programs over one owned lease with exact-byte identity evidence.
- [62]-[OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound programs, convert alone admitting headless concurrency.
- [63]-[SHEETS](.planning/Exchange/sheets.md): `Sheets.Commit` unifies page and detail selection, arrangement, and undo/redraw settlement in one op.
- [64]-[PUBLISH](.planning/Exchange/publish.md): `Publishing.Run` lands captured or blank pages as content-keyed artifacts or typed printer evidence.

[HOSTUI]:
- [65]-[SHELL](.planning/HostUi/shell.md): `HostThread` owns command-thread affinity; `ShellCapsule` seats every process-lifetime registry.
- [66]-[PANELS](.planning/HostUi/panels.md): `PanelHost` owns the panel lifecycle and the consumable Rhino control library.
- [67]-[PAGES](.planning/HostUi/pages.md): `HostPage` owns page realization and kind-safe mounting behind the host base classes.
- [68]-[DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant.

[PLUGIN]:
- [69]-[LIFECYCLE](.planning/Plugin/lifecycle.md): `RasmPlugIn` seats the one host plug-in derivation and routes each override onto a typed phase.
- [70]-[CENSUS](.planning/Plugin/census.md): `PluginCensus.Ask` answers the installed registry; `PluginRegistry.Commit` owns load and protection.
- [71]-[LICENSING](.planning/Plugin/licensing.md): `Licenses.Ask` folds Zoo entitlement; its acquisition arm continues the plug-in derivation.
- [72]-[DOCUMENT](.planning/Plugin/document.md): `Participation.Cross` rides the archive frame; `PluginSettings.Commit` bridges the settings rail.

## [02]-[DOMAIN_PACKAGES]

Host assemblies admitted here bind as `Directory.Build.props` references off the installed Rhino bundle, corroborated by this folder's `.api/`.

[RHINO_HOST]:
- `RhinoCommon` — Core Rhino host object model behind every document, command, geometry, and exchange surface.
- `Rhino.UI` — Rhino shell bridge for Eto hosting, panels, pages, dialogs, and RUI chrome.

[NATIVE_UI]:
- `Eto` — Cross-platform control, layout, binding, drawing, and window framework.
- `Eto.macOS` — AppKit backend, native control hosting, and platform conversions.
- `Microsoft.macOS` — AppKit, CoreAnimation, and Foundation bindings behind the gated motion and display owners.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/csharp/.api/` holds the shared API evidence.

[CORE_SUBSTRATE]:
- `LanguageExt.Core`
- `NodaTime` — Semantic time on the render sun band.
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`
- `CommunityToolkit.HighPerformance` — Pooled span rentals behind snapshot staging.
- `QuikGraph` — Transient block-graph topology, reachability, and source-first ordering.
- `Riok.Mapperly` — Existing-target policy transcription onto host option objects and wire lowering.
- `Generator.Equals` — Structural equality on the command option, selection, acquisition, preset, and snapshot value families.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry.Abstractions`

[WIRE_CODEGEN]:
- `Google.Protobuf` — Runtime message surface behind the `rasm.organization.v1` and `rasm.scene.v1` egress.
- `Grpc.Tools` — Build-only `<Protobuf>` compile of the corpus-homed organization and scene sources.

[DEPENDENCY_FLOORS]:
- `Microsoft.Extensions.Compliance.Abstractions` — Transitive `DataClassification` attribute surface over kernel `Sensitivity` values.
- `System.Drawing.Common` — Compile-time GDI carriers crossing host bitmap, icon, printer, and screen seams.
