# [RASM_RHINO]

`Rasm.Rhino` is the single host boundary over RhinoCommon, Rhino UI, Eto, and the macOS native surface — the full Rhino host captured as typed, leased capability. Every host concern folds through one owner behind thread-affinity and capability gates, every native resource is retained only across its leased extent, and every outcome is a typed receipt. Its bar is total capture: an app root or an agent composes parameterized host capability without learning RhinoCommon's raw surface, and Rhino-native drafting, sheets, and file I/O stay rich rather than thinned toward a host-neutral floor.

It references no sibling package — every alignment travels down the one kernel seam, and it enters only at the app roots, never as an interior dependency of a host-neutral package. Every measured surface mints the typed receipt carrying its own run evidence; instrument execution over those declarations is app-root altitude, never a second measurement truth inside the boundary.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned sources behind fresh handle and capability evidence.
- [02]-[GEOMETRY](.planning/Document/geometry.md): `GeometryHandle` owns geometry-custody crossing — inspection, motion, bounds, clipping, release.
- [03]-[TABLES](.planning/Document/tables.md): `Tables.Commit` executes a shaped table transaction through the undo bracket and redraw compensation.
- [04]-[EVENTS](.planning/Document/events.md): `DocumentStream.Observe` binds events into detached facts under scoped attach and symmetric release.
- [05]-[LAYERS](.planning/Document/layers.md): `Layers.Commit` folds the layer-tree program under the shared undo bracket.

[PERSISTENCE]:
- [06]-[DICTIONARY](.planning/Persistence/dictionary.md): `ArchiveMap` closes the typed-value dictionary as one union with a detach/mint round trip.
- [07]-[SETTINGS](.planning/Persistence/settings.md): `Settings.Commit` carries the settings tree through pure reads, typed writes, and guards.
- [08]-[APPSETTINGS](.planning/Persistence/appsettings.md): `AppSettings.Commit` drives every application preference family through typed state.
- [09]-[USERDATA](.planning/Persistence/userdata.md): `ArchiveIo` frames attached custody; `TypedUserData` seals the participation template.
- [10]-[USERTEXT](.planning/Persistence/usertext.md): `Texts.Commit` owns document and per-object user strings with prior-value receipts.
- [11]-[PRESETS](.planning/Persistence/presets.md): `Presets.Commit` runs cplane, position, and layer-state presets under one mask vocabulary.
- [12]-[SNAPSHOTS](.planning/Persistence/snapshots.md): `Snapshots.Commit` scripts snapshot state; `SnapshotParticipant` adapts the plugin seam.

[OBJECTS]:
- [13]-[STATE](.planning/Objects/state.md): `Objects.Ask` owns the live-object window — snapshot, frames, component touch, detached section custody.
- [14]-[ATTRIBUTES](.planning/Objects/attributes.md): `AttributeProgram` closes attribute mutation as the typed payload of the table rail's `Amend`.
- [15]-[MATERIALS](.planning/Objects/materials.md): `Materials.Ask` resolves materials, mappings, and mesh caches behind one shared-bracket commit.
- [16]-[LIGHTS](.planning/Objects/lights.md): `Lights.Commit` runs the closed light-kind family under the shared bracket.
- [17]-[HISTORY](.planning/Objects/history.md): `HistoryScript` and `ReplayProgram` own the record/replay triad, linkage topology, and governance.
- [18]-[AUTHORING](.planning/Objects/authoring.md): `ObjectProgram` and `GripProgram` quarantine host subclassing behind adapters.

[COMMANDS]:
- [19]-[COMMAND](.planning/Commands/command.md): `CommandFlow<TState>.Drive` owns the bounded immutable command algebra behind the host lifecycle.
- [20]-[ACQUISITION](.planning/Commands/acquisition.md): `Acquisition.Get` owns each getter modality, native getter lifetime, and typed receipt.
- [21]-[OPTIONS](.planning/Commands/options.md): `OptionSet.Bind` makes command-line options data, leasing native carriers under scoped release.
- [22]-[SELECTION](.planning/Commands/selection.md): `Picks` owns eager picked-reference capture, geometry retention, and measured kernel re-entry.

[BLOCKS]:
- [23]-[MODEL](.planning/Blocks/model.md): `Definitions.Lens` resolves the Document-owned `ResourceRef`.
- [24]-[GRAPH](.planning/Blocks/graph.md): `BlockGraph.Ask` folds live and archived definitions into one transient topology and its closure evidence.
- [25]-[LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes ingress, preview vault, deferred refresh, eviction, and native disposal.
- [26]-[OPERATIONS](.planning/Blocks/operations.md): `Blocks.Commit` runs read and transaction rails through plan grants and additive receipts.

[MODELING]:
- [27]-[SOLIDS](.planning/Modeling/solids.md): `Solids.Build` runs Brep booleans, fillets, offsets, pipes, and joins on the `ModelGate` spine.
- [28]-[LOFTING](.planning/Modeling/lofting.md): `Lofts.Build` folds rail sweeps, lofts, patches, and developable lofting into one policy rail.
- [29]-[SURFACES](.planning/Modeling/surfaces.md): `Surfaces.Build` owns network, revolve, grid, geodesic, and analytic construction.
- [30]-[CURVES](.planning/Modeling/curves.md): `Curves.Build` owns the offset, refine, extend, split, boolean, and construction host ops.
- [31]-[MESHING](.planning/Modeling/meshing.md): `Meshes.Build` drives parameter-carried meshing, quad-remesh, shrink-wrap, booleans, and mesh edits.
- [32]-[SUBD](.planning/Modeling/subd.md): `SubDs.Build` owns SubD creation, crease authoring, value-semantic editing, and brep conversion.
- [33]-[DEFORM](.planning/Modeling/deform.md): `Deforms.Apply` dispatches the space-morph family and the unroll, squish, and unwrap flatteners.
- [34]-[PROJECTION](.planning/Modeling/projection.md): `Projections.Build` classifies Make2D hidden-line drawings on the `ModelGate` spine.

[ANNOTATION]:
- [35]-[STYLE](.planning/Annotation/style.md): `StyleField` owns the drafting schema; its patch fold authors, amends, and override-mints styles.
- [36]-[TEXT](.planning/Annotation/text.md): `Texts.Commit` owns text/leader construction, run edits, `TextFields` formulas, and outlining.
- [37]-[DIMENSION](.planning/Annotation/dimension.md): `Dimensions.Commit` mints, adjusts, restyles the dimension family over one override algebra.
- [38]-[HATCH](.planning/Annotation/hatch.md): `Hatches.Commit` owns hatch construction, the pattern line-definition model, and `.pat` interchange.
- [39]-[LINETYPE](.planning/Annotation/linetype.md): `Linetypes.Commit` owns the segment/shape/taper stroke model and `.lin` interchange.
- [40]-[TYPEFACE](.planning/Annotation/typeface.md): `Typefaces.Resolve` answers face evidence; `Sections.Commit` composes section-cut presentation.

[VIEWPORT]:
- [41]-[CAMERA](.planning/Viewport/camera.md): `CameraPose` composes the kernel vector frame and intent owners behind the viewport lease.
- [42]-[OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds camera operations across scalar or broadcast under one UI-thread policy.
- [43]-[CAPTURE](.planning/Viewport/capture.md): `Captures` runs settings, transparent, and depth capture with frame-sequence custody.
- [44]-[MOTION](.planning/Viewport/motion.md): `MotionPump.Drive` samples kernel motion into a leased drive under accessibility and display facts.

[DISPLAY]:
- [45]-[CONDUIT](.planning/Display/conduit.md): `Conduits.Mount` owns filtered display-pipeline participation and balanced render state.
- [46]-[DRAW](.planning/Display/draw.md): `Marks.Render` dispatches one mark vocabulary over the pipeline and Eto canvas backends.
- [47]-[INTERACTION](.planning/Display/interaction.md): `WidgetHost` owns pointer, gumball, and widget lifecycles while emitting detached facts.
- [48]-[RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch and realtime render custody beside the `SceneQueue` change reader.
- [49]-[MODES](.planning/Display/modes.md): `Modes.Configure` owns mode appearance, policy flags, viewport assignment, and analysis attachment.

[RENDER]:
- [50]-[CONTENT](.planning/Render/content.md): `ContentRef` addresses the RDK content graph behind the kind axis, change bracket, and snapshot.
- [51]-[KINDS](.planning/Render/kinds.md): `MaterialBridge` borrows baked material and PBR projections per window; mint verbs yield leased content.
- [52]-[FIELDS](.planning/Render/fields.md): `FieldValue` owns every typed content parameter through polymorphic write, recover, and box dispatch.
- [53]-[REGISTRY](.planning/Render/registry.md): `Registry` runs the content rail through factory vocabulary, receipts, and events.
- [54]-[SETTINGS](.planning/Render/settings.md): `Settings.Run` applies total render state across the document, archive, and free-floating duality.
- [55]-[MAPPING](.planning/Render/mapping.md): `MappingSpec` mints texture mappings; the `Mappings` rail binds and recovers them per object channel.

[EXCHANGE]:
- [56]-[FORMATS](.planning/Exchange/formats.md): `FileCodec` is interchange authority from detection through engine dispatch and dialog registration.
- [57]-[OPTIONS](.planning/Exchange/options.md): `FormatDial` closes every per-format option surface into one dial family at the codec boundary.
- [58]-[ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` runs archive programs over one owned lease with exact-byte identity evidence.
- [59]-[OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound programs, convert alone admitting headless concurrency.
- [60]-[SHEETS](.planning/Exchange/sheets.md): `Sheets.Commit` unifies page and detail selection, arrangement, and undo/redraw settlement in one op.
- [61]-[PUBLISH](.planning/Exchange/publish.md): `Publishing.Run` lands captured or blank pages as content-keyed artifacts or typed printer evidence.

[ETO]:
- [62]-[PLATFORM](.planning/Eto/platform.md): `HostPlatform` owns ambient backend discovery, native-control crossings, and tracked theme rebroadcast.
- [63]-[RUNTIME](.planning/Eto/runtime.md): `UiThread` owns control-tree affinity over displays, input, transfer, drag/drop, and system presence.
- [64]-[ELEMENTS](.planning/Eto/elements.md): `Element.Realize` folds the typed control tree through the thread-identity gate.
- [65]-[BINDING](.planning/Eto/binding.md): `Bind.Rig` wires typed state, context, or seed sources into a realized control's receipt ledger.
- [66]-[CANVAS](.planning/Eto/canvas.md): `Surface` mounts the drawable and replays the mounted `PaintProgram`.
- [67]-[CHROME](.planning/Eto/chrome.md): `IntentTable` projects one verb vocabulary into menus, toolbars, windows, and typed modals.

[HOSTUI]:
- [68]-[SHELL](.planning/HostUi/shell.md): `HostThread` owns Rhino command-thread affinity and every shell runtime surface.
- [69]-[PANELS](.planning/HostUi/panels.md): `PanelHost` owns the panel lifecycle and the consumable Rhino control library.
- [70]-[PAGES](.planning/HostUi/pages.md): `HostPage` owns page realization and kind-safe mounting behind the host base classes.
- [71]-[DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant.

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

Shared substrate consumed from the C# registry; the registry and its charters own the full contracts, and `libs/csharp/.api/` holds the shared API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Telemetry.Abstractions`
- `Microsoft.Extensions.Compliance.Abstractions` — transitive `DataClassification` attribute surface; no direct manifest row.

[GRAPH_ALGORITHM]:
- `QuikGraph` — transient block-graph topology, reachability, and source-first ordering.
