# [RASM_RHINO]

`Rasm.Rhino` is the single host boundary over RhinoCommon, Rhino UI, Eto, and the macOS native surface — the full Rhino host captured as typed, leased capability. Every host concern folds through one owner behind thread-affinity and capability gates, every native resource is retained only across its leased extent, and every outcome is a typed receipt. Its bar is total capture: an app root or an agent composes parameterized host capability without learning RhinoCommon's raw surface, and Rhino-native drafting, sheets, and file I/O stay rich rather than thinned toward a host-neutral floor.

It references no sibling package — every alignment travels down the one kernel seam, and it enters only at the app roots, never as an interior dependency of a host-neutral package.

## [01]-[ROUTER]

[DOCUMENT]:
- Route: [DOCUMENT_SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned sources behind fresh handle and capability evidence.
- Route: [DOCUMENT_GEOMETRY](.planning/Document/geometry.md): `GeometryHandle` owns geometry-custody crossing — inspection, motion, bounds, clipping, release.
- Route: [DOCUMENT_TABLES](.planning/Document/tables.md): `Tables.Commit` executes a shaped table transaction through the undo bracket and redraw compensation.
- Route: [DOCUMENT_EVENTS](.planning/Document/events.md): `DocumentStream.Observe` binds events into detached facts under scoped attach and symmetric release.
- Route: [DOCUMENT_LAYERS](.planning/Document/layers.md): `Layers.Commit` folds the layer-tree program under the shared undo bracket.

[PERSISTENCE]:
- Route: [PERSISTENCE_DICTIONARY](.planning/Persistence/dictionary.md): `ArchiveMap` closes the typed-value dictionary as one union with a detach/mint round trip.
- Route: [PERSISTENCE_SETTINGS](.planning/Persistence/settings.md): `Settings.Commit` carries the settings tree through pure reads, typed writes, and guards.
- Route: [PERSISTENCE_APPSETTINGS](.planning/Persistence/appsettings.md): `AppSettings.Commit` drives every application preference family through typed state.
- Route: [PERSISTENCE_USERDATA](.planning/Persistence/userdata.md): `ArchiveIo` frames attached custody; `TypedUserData` seals the participation template.
- Route: [PERSISTENCE_USERTEXT](.planning/Persistence/usertext.md): `Texts.Commit` owns document and per-object user strings with prior-value receipts.
- Route: [PERSISTENCE_PRESETS](.planning/Persistence/presets.md): `Presets.Commit` runs cplane, position, and layer-state presets under one mask vocabulary.
- Route: [PERSISTENCE_SNAPSHOTS](.planning/Persistence/snapshots.md): `Snapshots.Commit` scripts snapshot state; `SnapshotParticipant` adapts the plugin seam.

[OBJECTS]:
- Route: [OBJECTS_STATE](.planning/Objects/state.md): `Objects.Ask` owns the live-object window — snapshot, frames, component touch, detached section custody.
- Route: [OBJECTS_ATTRIBUTES](.planning/Objects/attributes.md): `AttributeProgram` closes attribute mutation as the typed payload of the table rail's `Amend`.
- Route: [OBJECTS_MATERIALS](.planning/Objects/materials.md): `Materials.Ask` resolves materials, mappings, and mesh caches behind one shared-bracket commit.
- Route: [OBJECTS_LIGHTS](.planning/Objects/lights.md): `Lights.Commit` runs the closed light-kind family under the shared bracket.
- Route: [OBJECTS_HISTORY](.planning/Objects/history.md): `HistoryScript` and `ReplayProgram` own the record/replay triad, linkage topology, and governance.
- Route: [OBJECTS_AUTHORING](.planning/Objects/authoring.md): `ObjectProgram` and `GripProgram` quarantine host subclassing behind adapters; `ObjectsTelemetry` is the one structured-log egress.

[COMMANDS]:
- Route: [COMMANDS_COMMAND](.planning/Commands/command.md): `CommandFlow<TState>.Drive` owns the bounded immutable command algebra behind the host lifecycle.
- Route: [COMMANDS_ACQUISITION](.planning/Commands/acquisition.md): `Acquisition.Get` owns each getter modality, native getter lifetime, and typed receipt.
- Route: [COMMANDS_OPTIONS](.planning/Commands/options.md): `OptionSet.Bind` makes command-line options data, leasing native carriers under scoped release.
- Route: [COMMANDS_SELECTION](.planning/Commands/selection.md): `Picks` owns eager picked-reference capture, geometry retention, and measured kernel re-entry.

[BLOCKS]:
- Route: [BLOCKS_MODEL](.planning/Blocks/model.md): `Definitions.Lens` resolves the Document-owned `ResourceRef`; `BlockSnapshot` owns the whole-state policy rows.
- Route: [BLOCKS_GRAPH](.planning/Blocks/graph.md): `BlockGraph.Ask` folds live and archived definitions into one transient topology and its closure evidence.
- Route: [BLOCKS_LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes ingress, preview vault, deferred refresh, eviction, and native disposal.
- Route: [BLOCKS_OPERATIONS](.planning/Blocks/operations.md): `Blocks.Commit` runs read and transaction rails through plan grants and additive receipts.

[MODELING]:
- Route: [MODELING_SOLIDS](.planning/Modeling/solids.md): `Solids.Build` runs Brep booleans, fillets, offsets, pipes, and joins on the `ModelGate` spine.
- Route: [MODELING_LOFTING](.planning/Modeling/lofting.md): `Lofts.Build` folds rail sweeps, lofts, patches, and developable lofting into one policy rail.
- Route: [MODELING_SURFACES](.planning/Modeling/surfaces.md): `Surfaces.Build` owns network, revolve, grid, geodesic, and analytic construction.
- Route: [MODELING_CURVES](.planning/Modeling/curves.md): `Curves.Build` owns the offset, refine, extend, split, boolean, and construction host ops.
- Route: [MODELING_MESHING](.planning/Modeling/meshing.md): `Meshes.Build` drives parameter-carried meshing, quad-remesh, shrink-wrap, booleans, and mesh edits.
- Route: [MODELING_SUBD](.planning/Modeling/subd.md): `SubDs.Build` owns SubD creation, crease authoring, value-semantic editing, and brep conversion.
- Route: [MODELING_DEFORM](.planning/Modeling/deform.md): `Deforms.Apply` dispatches the space-morph family and the unroll, squish, and unwrap flatteners.
- Route: [MODELING_PROJECTION](.planning/Modeling/projection.md): `Projections.Build` classifies Make2D hidden-line drawings on the `ModelGate` spine.

[ANNOTATION]:
- Route: [ANNOTATION_STYLE](.planning/Annotation/style.md): `StyleField` owns the drafting schema; its patch fold authors, amends, and override-mints styles.
- Route: [ANNOTATION_TEXT](.planning/Annotation/text.md): `Texts.Commit` owns text/leader construction, run edits, `TextFields` formulas, and the outlining family.
- Route: [ANNOTATION_DIMENSION](.planning/Annotation/dimension.md): `Dimensions.Commit` mints, adjusts, and restyles the six-kind family over one override algebra.
- Route: [ANNOTATION_HATCH](.planning/Annotation/hatch.md): `Hatches.Commit` owns hatch construction, the pattern line-definition model, and `.pat` interchange.
- Route: [ANNOTATION_LINETYPE](.planning/Annotation/linetype.md): `Linetypes.Commit` owns the segment/shape/taper stroke model and `.lin` interchange.
- Route: [ANNOTATION_TYPEFACE](.planning/Annotation/typeface.md): `Typefaces.Resolve` answers face evidence; `Sections.Commit` composes section-cut presentation.

[VIEWPORT]:
- Route: [VIEWPORT_CAMERA](.planning/Viewport/camera.md): `CameraPose` composes the kernel vector frame and intent owners behind the viewport lease.
- Route: [VIEWPORT_OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds camera operations across scalar or broadcast under one UI-thread policy.
- Route: [VIEWPORT_CAPTURE](.planning/Viewport/capture.md): `Captures` runs settings, transparent, and depth capture with frame-sequence custody.
- Route: [VIEWPORT_MOTION](.planning/Viewport/motion.md): `MotionPump.Drive` samples kernel motion into a leased drive under accessibility and display facts.

[DISPLAY]:
- Route: [DISPLAY_CONDUIT](.planning/Display/conduit.md): `Conduits.Mount` owns filtered display-pipeline participation and balanced render state.
- Route: [DISPLAY_DRAW](.planning/Display/draw.md): `Marks.Render` dispatches one mark vocabulary over the pipeline and Eto canvas backends.
- Route: [DISPLAY_INTERACTION](.planning/Display/interaction.md): `WidgetHost` owns pointer, gumball, and widget lifecycles while emitting detached facts.
- Route: [DISPLAY_RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch and realtime render custody beside the `SceneQueue` change reader.
- Route: [DISPLAY_MODES](.planning/Display/modes.md): `Modes.Configure` owns mode appearance, policy flags, viewport assignment, and analysis attachment.

[RENDER]:
- Route: [RENDER_CONTENT](.planning/Render/content.md): `ContentRef` addresses the RDK content graph behind the kind axis, change bracket, and snapshot.
- Route: [RENDER_KINDS](.planning/Render/kinds.md): `MaterialBridge` borrows baked material and PBR projections per window; mint verbs yield leased content.
- Route: [RENDER_FIELDS](.planning/Render/fields.md): `FieldValue` owns every typed content parameter through one polymorphic write, recover, and box dispatch.
- Route: [RENDER_REGISTRY](.planning/Render/registry.md): `Registry.Run` and `Registry.Read` run the content rail through factory vocabulary, receipts, and events.
- Route: [RENDER_SETTINGS](.planning/Render/settings.md): `Settings.Run` applies total render state across the document, archive, and free-floating duality.
- Route: [RENDER_MAPPING](.planning/Render/mapping.md): `MappingSpec` mints texture mappings; the `Mappings` rail binds and recovers them per object channel.

[EXCHANGE]:
- Route: [EXCHANGE_FORMATS](.planning/Exchange/formats.md): `FileCodec` is interchange authority from detection through engine dispatch and dialog registration.
- Route: [EXCHANGE_OPTIONS](.planning/Exchange/options.md): `FormatDial` closes every per-format option surface into one dial family at the codec boundary.
- Route: [EXCHANGE_ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` runs archive programs over one owned lease with exact-byte identity evidence.
- Route: [EXCHANGE_OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound programs, convert alone admitting headless concurrency.
- Route: [EXCHANGE_SHEETS](.planning/Exchange/sheets.md): `Sheets.Commit` unifies page and detail selection, arrangement, and undo/redraw settlement in one op.
- Route: [EXCHANGE_PUBLISH](.planning/Exchange/publish.md): `Publishing.Run` lands captured or blank pages as content-keyed artifacts or typed printer evidence.

[ETO]:
- Route: [ETO_PLATFORM](.planning/Eto/platform.md): `HostPlatform` owns ambient backend discovery, native-control crossings, and tracked theme rebroadcast.
- Route: [ETO_RUNTIME](.planning/Eto/runtime.md): `UiThread` owns control-tree affinity over displays, input, transfer, drag/drop, and system presence.
- Route: [ETO_ELEMENTS](.planning/Eto/elements.md): `Element.Realize` folds the typed control tree through the thread-identity gate.
- Route: [ETO_BINDING](.planning/Eto/binding.md): `Bind.Rig` wires typed state, context, or seed sources into a realized control's receipt ledger.
- Route: [ETO_CANVAS](.planning/Eto/canvas.md): `Surface` mounts the drawable and replays the mounted `PaintProgram` under the perceptual-color and `GlyphBlock` typography projection.
- Route: [ETO_CHROME](.planning/Eto/chrome.md): `IntentTable` projects one verb vocabulary into menus, toolbars, windows, and typed modals.

[HOSTUI]:
- Route: [HOSTUI_SHELL](.planning/HostUi/shell.md): `HostThread` owns Rhino command-thread affinity and every shell runtime surface.
- Route: [HOSTUI_PANELS](.planning/HostUi/panels.md): `PanelHost` owns the panel lifecycle and the consumable Rhino control library.
- Route: [HOSTUI_PAGES](.planning/HostUi/pages.md): `HostPage` owns page realization and kind-safe mounting behind the host base classes.
- Route: [HOSTUI_DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant.

## [02]-[DOMAIN_PACKAGES]

Host assemblies admitted by this folder bind as `Directory.Build.props` host references from the installed Rhino bundle — never manifest package rows — and this folder's `.api/` corroborates each surface.

[RHINO_HOST]:
- `RhinoCommon` — core Rhino host object model behind every document, command, geometry, and exchange surface.
- `Rhino.UI` — Rhino shell bridge for Eto hosting, panels, pages, dialogs, and RUI chrome.
- `System.Drawing.Common` — compile-time GDI carriers crossing host bitmap, icon, printer, and screen seams.

[NATIVE_UI]:
- `Eto` — cross-platform control, layout, binding, drawing, and window framework.
- `Eto.macOS` — AppKit backend, native control hosting, and platform conversions.
- `Microsoft.macOS` — AppKit, CoreAnimation, and Foundation bindings behind the gated motion and display owners.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the contracts, and `libs/csharp/.api/` holds the API evidence. This folder's observability axis is fault logging alone — `ObjectsTelemetry` on `Objects/authoring.md` is the one structured-log egress, every measured surface mints the typed receipt carrying its own run evidence, and instrument projection over those receipts is app-root altitude, never a second measurement truth inside the boundary.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry.Abstractions`

[GRAPH_ALGORITHM]:
- `QuikGraph` — transient block-graph topology, reachability, and source-first ordering.
