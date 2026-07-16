# [RASM_RHINO]

`Rasm.Rhino` is the single host boundary over RhinoCommon, Rhino UI, Eto, and the macOS native surface — the full Rhino host captured as typed, leased capability. Document sessions, geometry custody, table transactions, typed persistence and saved-state presets, live objects and their attributes, commands and acquisition, blocks, drafting annotation, native modeling compute, viewports and capture, display conduits, render content and settings, file exchange and publishing, Eto realization, and host chrome each fold through one owner behind thread-affinity and capability gates, every native resource retained only across its leased extent and every outcome a typed receipt. Its bar is total capture: an app root or an agent composes parameterized host capability without learning RhinoCommon's raw surface, and the rich Rhino-native features — drafting, sheets, native file IO — stay rich rather than thinned toward a host-neutral floor.

It references no sibling package — every alignment travels down the one kernel seam, and it enters only at the app roots, never as an interior dependency of a host-neutral package.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[SESSION](.planning/Document/session.md): `DocumentSession.Of` admits borrowed or owned sources behind fresh handle and capability evidence.
- [02]-[GEOMETRY](.planning/Document/geometry.md): `GeometryHandle` owns geometry-custody crossing — inspection, motion, bounds, clipping, release.
- [03]-[TABLES](.planning/Document/tables.md): `Tables.Commit` executes a shaped table transaction through the undo bracket and redraw compensation.
- [04]-[EVENTS](.planning/Document/events.md): `DocumentStream.Observe` binds events into detached facts under scoped attach and symmetric release.

[PERSISTENCE]:
- [05]-[DICTIONARY](.planning/Persistence/dictionary.md): `ArchiveMap` closes the typed-value dictionary as one union with a detach/mint round trip.
- [06]-[SETTINGS](.planning/Persistence/settings.md): `Settings.Commit` carries the settings tree through pure reads, typed writes, and guards.
- [07]-[USERDATA](.planning/Persistence/userdata.md): `ArchiveIo` frames attached custody; `TypedUserData` seals the participation template.
- [08]-[USERTEXT](.planning/Persistence/usertext.md): `Texts.Commit` owns document and per-object user strings with prior-value receipts.
- [09]-[PRESETS](.planning/Persistence/presets.md): `Presets.Commit` runs cplane, position, and layer-state presets under one mask vocabulary.
- [10]-[SNAPSHOTS](.planning/Persistence/snapshots.md): `Snapshots.Commit` scripts snapshot state; `SnapshotParticipant` adapts the plugin seam.

[OBJECTS]:
- [11]-[STATE](.planning/Objects/state.md): `Objects.Ask` owns the live-object window — snapshot, frames, component touch, detached section custody.
- [12]-[ATTRIBUTES](.planning/Objects/attributes.md): `AttributeProgram` closes attribute mutation as the typed payload of the table rail's `Amend`.
- [13]-[MATERIALS](.planning/Objects/materials.md): `Materials.Ask` resolves materials, mappings, and mesh caches behind one shared-bracket commit.
- [14]-[HISTORY](.planning/Objects/history.md): `HistoryScript` and `ReplayProgram` own the record/replay triad, linkage topology, and governance.
- [15]-[AUTHORING](.planning/Objects/authoring.md): `ObjectProgram` and `GripProgram` quarantine host subclassing behind adapters and widget grants.

[COMMANDS]:
- [16]-[COMMAND](.planning/Commands/command.md): `CommandFlow<TState>.Drive` owns the bounded immutable command algebra behind the host lifecycle.
- [17]-[ACQUISITION](.planning/Commands/acquisition.md): `Acquisition.Get` owns each getter modality, native getter lifetime, and typed receipt.
- [18]-[OPTIONS](.planning/Commands/options.md): `OptionSet.Bind` makes command-line options data, leasing native carriers under scoped release.
- [19]-[SELECTION](.planning/Commands/selection.md): `Picks` owns eager picked-reference capture, geometry retention, and measured kernel re-entry.

[BLOCKS]:
- [20]-[MODEL](.planning/Blocks/model.md): `BlockRef` owns instance-definition identity and the whole-state snapshot policy rows.
- [21]-[GRAPH](.planning/Blocks/graph.md): `BlockGraph.Ask` folds live and archived definitions into one transient topology and its closure evidence.
- [22]-[LIFECYCLE](.planning/Blocks/lifecycle.md): `BlockLifecycle` composes ingress, preview vault, deferred refresh, eviction, and native disposal.
- [23]-[OPERATIONS](.planning/Blocks/operations.md): `Blocks.Commit` runs read and transaction rails through plan grants and additive receipts.

[MODELING]:
- [24]-[SOLIDS](.planning/Modeling/solids.md): `Solids.Build` runs Brep booleans, fillets, offsets, pipes, and joins on the `ModelGate` spine.
- [25]-[LOFTING](.planning/Modeling/lofting.md): `Lofts.Build` folds rail sweeps, lofts, patches, and developable lofting into one policy rail.
- [26]-[SURFACES](.planning/Modeling/surfaces.md): `Surfaces.Build` owns network, revolve, grid, geodesic, and analytic construction.
- [27]-[CURVES](.planning/Modeling/curves.md): `Curves.Build` owns the offset, refine, extend, split, boolean, and construction host ops.
- [28]-[MESHING](.planning/Modeling/meshing.md): `Meshes.Build` drives parameter-carried meshing, quad-remesh, shrink-wrap, booleans, and mesh edits.
- [29]-[SUBD](.planning/Modeling/subd.md): `SubDs.Build` owns SubD creation, crease authoring, value-semantic editing, and brep conversion.
- [30]-[DEFORM](.planning/Modeling/deform.md): `Deforms.Apply` dispatches the space-morph family and the unroll, squish, and unwrap flatteners.

[ANNOTATION]:
- [31]-[STYLE](.planning/Annotation/style.md): `StyleField` owns the drafting schema; its patch fold authors, amends, and override-mints styles.
- [32]-[TEXT](.planning/Annotation/text.md): `Texts.Commit` owns text/leader construction, run edits, `TextFields` formulas, and the outlining family.
- [33]-[DIMENSION](.planning/Annotation/dimension.md): `Dimensions.Commit` mints, adjusts, and restyles the six-kind family over one override algebra.
- [34]-[HATCH](.planning/Annotation/hatch.md): `Hatches.Commit` owns hatch construction, the pattern line-definition model, and `.pat` interchange.
- [35]-[LINETYPE](.planning/Annotation/linetype.md): `Linetypes.Commit` owns the segment/shape/taper stroke model and `.lin` interchange.
- [36]-[TYPEFACE](.planning/Annotation/typeface.md): `Typefaces.Resolve` answers face evidence; `Sections.Commit` composes section-cut presentation.

[VIEWPORT]:
- [37]-[CAMERA](.planning/Viewport/camera.md): `CameraPose` composes the kernel vector frame and intent owners behind the viewport lease.
- [38]-[OPERATIONS](.planning/Viewport/operations.md): `Cameras.Apply` folds camera operations across scalar or broadcast under one UI-thread policy.
- [39]-[CAPTURE](.planning/Viewport/capture.md): `Captures` runs settings, transparent, and depth capture with frame-sequence custody.
- [40]-[MOTION](.planning/Viewport/motion.md): `MotionPump.Drive` samples kernel motion into a leased drive under accessibility and display facts.

[DISPLAY]:
- [41]-[CONDUIT](.planning/Display/conduit.md): `Conduits.Mount` owns filtered display-pipeline participation and balanced render state.
- [42]-[DRAW](.planning/Display/draw.md): `Marks.Render` dispatches one mark vocabulary over the pipeline and Eto canvas backends.
- [43]-[INTERACTION](.planning/Display/interaction.md): `WidgetHost` owns pointer, gumball, and widget lifecycles while emitting detached facts.
- [44]-[RENDER](.planning/Display/render.md): `RenderJob.Open` owns batch pipeline and window custody beside the realtime framebuffer participant.
- [45]-[MODES](.planning/Display/modes.md): `Modes.Configure` owns mode appearance, policy flags, viewport assignment, and analysis attachment.

[RENDER]:
- [46]-[CONTENT](.planning/Render/content.md): `ContentRef` addresses the RDK content graph behind the kind axis, change bracket, and snapshot.
- [47]-[KINDS](.planning/Render/kinds.md): `MaterialBridge` borrows baked material and PBR projections per window; mint verbs yield leased content.
- [48]-[FIELDS](.planning/Render/fields.md): `FieldValue` owns every typed content parameter through one polymorphic write, recover, and box dispatch.
- [49]-[REGISTRY](.planning/Render/registry.md): `Contents.Commit` runs the content rail through factory vocabulary, receipts, and events.
- [50]-[SETTINGS](.planning/Render/settings.md): `Settings.Commit` applies total render state across the document, archive, and free-floating duality.
- [51]-[MAPPING](.planning/Render/mapping.md): `MappingSpec` mints texture mappings; the `Mappings` rail binds and recovers them per object channel.

[EXCHANGE]:
- [52]-[FORMATS](.planning/Exchange/formats.md): `FileCodec` is interchange authority from detection through engine dispatch and dialog registration.
- [53]-[OPTIONS](.planning/Exchange/options.md): `FormatDial` closes every per-format option surface into one dial family at the codec boundary.
- [54]-[ARCHIVE](.planning/Exchange/archive.md): `Archives.Apply` runs archive programs over one owned lease with exact-byte identity evidence.
- [55]-[OPERATIONS](.planning/Exchange/operations.md): `Exchanges.Run` owns document-bound programs, convert alone admitting headless concurrency.
- [56]-[SHEETS](.planning/Exchange/sheets.md): `Sheets.Commit` unifies page and detail selection, arrangement, and undo/redraw settlement in one op.
- [57]-[PUBLISH](.planning/Exchange/publish.md): `Publishing.Run` lands captured or blank pages as content-keyed artifacts or typed printer evidence.

[ETO]:
- [58]-[PLATFORM](.planning/Eto/platform.md): `HostPlatform` owns ambient backend discovery, native-control crossings, and tracked theme rebroadcast.
- [59]-[RUNTIME](.planning/Eto/runtime.md): `UiThread` owns control-tree affinity over displays, input, transfer, drag/drop, and system presence.
- [60]-[ELEMENTS](.planning/Eto/elements.md): `Element.Realize` folds the typed control tree through specs, family rows, grids, and arrangements.
- [61]-[BINDING](.planning/Eto/binding.md): `Bind.Rig` wires typed state, context, or seed sources into a realized control's receipt ledger.
- [62]-[CANVAS](.planning/Eto/canvas.md): `Scene` renders and hit-tests one retained mark vocabulary under the perceptual-color projection.
- [63]-[CHROME](.planning/Eto/chrome.md): `IntentTable` projects one verb vocabulary into menus, toolbars, windows, and typed modals.

[HOSTUI]:
- [64]-[SHELL](.planning/HostUi/shell.md): `HostThread` owns Rhino command-thread affinity, application status, progress, and window adoption.
- [65]-[PANELS](.planning/HostUi/panels.md): `PanelHost` owns panel content, lifecycle, registration, placement, and toolbar-file composition.
- [66]-[PAGES](.planning/HostUi/pages.md): `HostPage` owns page realization and kind-safe mounting behind the host base classes.
- [67]-[DIALOGS](.planning/HostUi/dialogs.md): `Inquiries.Ask` folds native dialog intent into typed answers under the session dialog grant.

## [02]-[DOMAIN_PACKAGES]

Host assemblies admitted by this folder; versions centralize in the C# manifest and corroborate against this folder's `.api/`.

[RHINO_HOST]:
- `RhinoCommon` — core Rhino host object model behind every document, command, geometry, and exchange surface.
- `Rhino.UI` — Rhino shell bridge for Eto hosting, panels, pages, dialogs, and RUI chrome.
- `System.Drawing.Common` — compile-time GDI carriers crossing host bitmap, icon, printer, and screen seams.

[NATIVE_UI]:
- `Eto` — cross-platform control, layout, binding, drawing, and window framework.
- `Eto.macOS` — AppKit backend, native control hosting, and platform conversions.
- `Microsoft.macOS` — AppKit, CoreAnimation, and Foundation bindings behind the gated motion and display owners.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry; the registry and its charters own the contracts, and `libs/csharp/.api/` holds the API evidence.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `JetBrains.Annotations`

[GRAPH_ALGORITHM]:
- `QuikGraph` — transient block-graph topology, reachability, and source-first ordering.
